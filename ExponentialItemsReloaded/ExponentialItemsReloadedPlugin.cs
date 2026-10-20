using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace ExponentialItemsReloaded;

public enum StackingMode
{
	Off,
	Linear,
	Sigmoid,
	Sqrt,
	Exponential
}

[BepInDependency("com.bepis.r2api.content_management", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin("com.cryptidlabs.exponentialitemsreloaded", "ExponentialItemsReloaded", "2.1.0")]
public class ExponentialItemsReloadedPlugin : BaseUnityPlugin
{
	public const string PluginGUID = "com.cryptidlabs.exponentialitemsreloaded";
	public const string PluginName = "ExponentialItemsReloaded";
	public const string PluginVersion = "2.1.0";

	private static ArtifactDef exponentialArtifactDef;

	private static ConfigEntry<bool> requireArtifactEnabled;
	private static ConfigEntry<StackingMode> stackingMode;
	private static ConfigEntry<float> sigmoidPeakBonus;
	private static ConfigEntry<float> sigmoidGrowthRate;
	private static ConfigEntry<float> sigmoidInflectionPoint;
	private static ConfigEntry<float> sigmoidSqrtFactor;
	private static ConfigEntry<int> linearMultiplier;
	private static ConfigEntry<float> sqrtFactor;
	private static ConfigEntry<int> maxStack;
	private static ConfigEntry<int> baseSize;

	private static ConfigEntry<bool> affectTier1;
	private static ConfigEntry<bool> affectTier2;
	private static ConfigEntry<bool> affectTier3;
	private static ConfigEntry<bool> affectBoss;
	private static ConfigEntry<bool> affectLunar;
	private static ConfigEntry<bool> affectVoid;
	private static ConfigEntry<bool> affectTierless;
	private static ConfigEntry<bool> affectTemporaryItems;
	private static ConfigEntry<bool> affectEquipment;

	private static ConfigEntry<bool> debugLogging;
	private static ConfigEntry<bool> debugLogArtifactList;

	private static bool suppressExponentialHook;

	public void Awake()
	{
		requireArtifactEnabled = Config.Bind<bool>("General", "Require Artifact Enabled", true, "If true, custom stacking only applies while Artifact of Exponents is enabled. If false, the mod is always active.");
		stackingMode = Config.Bind<StackingMode>("General", "Stacking Mode", StackingMode.Sigmoid, "Stacking calculation mode: Off (Vanilla 1x), Linear (y = M*x), Sigmoid (S-Curve hybrid with Sqrt tail: starts at 1, ramps smoothly to ~44 at pickup 10, then tapers into controlled sublinear scaling), Sqrt (sublinear root curve), or Exponential (2^n).");
		sigmoidPeakBonus = Config.Bind<float>("General", "Sigmoid Peak Bonus", 35.0f, "Bonus amplitude for Sigmoid mode. Default 35.0 gives ~44 stacks at pickup 10.");
		sigmoidGrowthRate = Config.Bind<float>("General", "Sigmoid Growth Rate", 0.6f, "Steepness of the Sigmoid ramp (default 0.6).");
		sigmoidInflectionPoint = Config.Bind<float>("General", "Sigmoid Inflection Point", 6.5f, "Pickup count at which acceleration peaks before tapering off towards pickup 10 (default 6.5).");
		sigmoidSqrtFactor = Config.Bind<float>("General", "Sigmoid Sqrt Factor", 2.0f, "Sublinear scaling factor for late-game pickups after item 10 (default 2.0).");
		linearMultiplier = Config.Bind<int>("General", "Linear Multiplier", 2, "Multiplier for Linear mode (e.g. 2 gives 2x stacks: 2, 4, 6, 8...).");
		sqrtFactor = Config.Bind<float>("General", "Sqrt Factor", 2.2f, "Sublinear bonus factor for Sqrt mode (e.g. 2.2 gives 3, 5, 7, 8, 10, 11, 13...).");
		maxStack = Config.Bind<int>("General", "Max Stack Size", 4096, "Maximum item stack count this mod will try to reach.");
		baseSize = Config.Bind<int>("General", "Base Size", 2, "Exponential base. 2 gives 1,2,4,8... 3 gives 1,3,9,27...");

		affectTier1 = Config.Bind<bool>("Item Filters", "Affect White Items", true, "Allows common/white items to stack with the chosen mode.");
		affectTier2 = Config.Bind<bool>("Item Filters", "Affect Green Items", true, "Allows uncommon/green items to stack with the chosen mode.");
		affectTier3 = Config.Bind<bool>("Item Filters", "Affect Red Items", true, "Allows legendary/red items to stack with the chosen mode.");
		affectBoss = Config.Bind<bool>("Item Filters", "Affect Boss Items", true, "Allows boss/yellow items to stack with the chosen mode.");
		affectLunar = Config.Bind<bool>("Item Filters", "Affect Lunar Items", true, "Allows lunar items to stack with the chosen mode.");
		affectVoid = Config.Bind<bool>("Item Filters", "Affect Void Items", true, "Allows void items to stack with the chosen mode.");
		affectTierless = Config.Bind<bool>("Item Filters", "Affect Tierless Items", false, "Allows tierless/hidden/internal items to stack. Usually safer off.");
		affectTemporaryItems = Config.Bind<bool>("Item Filters", "Affect Temporary Items", false, "Allows temporary items to stack with the chosen mode.");
		affectEquipment = Config.Bind<bool>("Item Filters", "Affect Equipment", false, "Reserved safety option.");

		debugLogging = Config.Bind<bool>("Debug", "Enable Debug Logging", false, "Logs item stack changes when the mod applies custom stacking.");
		debugLogArtifactList = Config.Bind<bool>("Debug", "Log Artifact List On Load", false, "Logs every loaded artifact on game load.");

		RegisterArtifact();

		On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
		On.RoR2.Inventory.GiveItem_ItemDef_int += Inventory_GiveItem_ItemDef_int;
		On.RoR2.Inventory.GiveItemTemp += Inventory_GiveItemTemp;
		On.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant;
		On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

		Logger.LogInfo($"{PluginName} v{PluginVersion} loaded (Mode: {stackingMode.Value}). Artifact registered: {exponentialArtifactDef != null}");

		if (debugLogArtifactList.Value)
		{
			RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, new Action(LogLoadedArtifacts));
		}
	}

	private void RegisterArtifact()
	{
		LanguageAPI.Add("ARTIFACT_EXPONENTIALITEMS_NAME", "Artifact of Exponents");
		LanguageAPI.Add("ARTIFACT_EXPONENTIALITEMS_DESC", "Items stack with advanced scaling (Sigmoid S-Curve, Linear, Sqrt, or Exponential) instead of one at a time.");
		exponentialArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
		exponentialArtifactDef.cachedName = "ExponentialItems";
		exponentialArtifactDef.nameToken = "ARTIFACT_EXPONENTIALITEMS_NAME";
		exponentialArtifactDef.descriptionToken = "ARTIFACT_EXPONENTIALITEMS_DESC";

		Sprite sel = LoadSprite("icon_selected.png");
		Sprite desel = LoadSprite("icon_deselected.png");
		if (sel != null) exponentialArtifactDef.smallIconSelectedSprite = sel;
		if (desel != null) exponentialArtifactDef.smallIconDeselectedSprite = desel;

		ContentAddition.AddArtifactDef(exponentialArtifactDef);
	}

	private Sprite LoadSprite(string fileName)
	{
		string text = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), fileName);
		if (!File.Exists(text))
		{
			Logger.LogWarning("Icon file not found: " + text);
			return null;
		}
		try
		{
			byte[] array = File.ReadAllBytes(text);
			Texture2D val = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			ImageConversion.LoadImage(val, array);
			val.name = System.IO.Path.GetFileNameWithoutExtension(fileName);
			return Sprite.Create(val, new Rect(0f, 0f, (float)val.width, (float)val.height), new Vector2(0.5f, 0.5f), 100f);
		}
		catch (Exception ex)
		{
			Logger.LogError($"Failed to load icon {fileName}: {ex}");
			return null;
		}
	}

	private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
	{
		try
		{
			if (suppressExponentialHook || !ShouldApply(self, itemIndex, count))
			{
				orig.Invoke(self, itemIndex, count);
				return;
			}
			int currentStack = GetCurrentStackForExponential(self, itemIndex);
			int nextTarget = GetNextTargetStack(currentStack);
			if (nextTarget <= currentStack)
			{
				orig.Invoke(self, itemIndex, count);
				return;
			}
			int num = Math.Max(1, nextTarget - currentStack);
			LogDebug($"GiveItem {itemIndex}: current={currentStack}, target={nextTarget}, adding={num}");
			suppressExponentialHook = true;
			try
			{
				orig.Invoke(self, itemIndex, num);
			}
			finally
			{
				suppressExponentialHook = false;
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
			orig.Invoke(self, itemIndex, count);
		}
	}

	private void Inventory_GiveItem_ItemDef_int(On.RoR2.Inventory.orig_GiveItem_ItemDef_int orig, Inventory self, ItemDef itemDef, int count)
	{
		if (itemDef == null)
		{
			orig.Invoke(self, itemDef, count);
			return;
		}
		Inventory_GiveItem_ItemIndex_int((inventory, index, itemCount) =>
		{
			orig.Invoke(inventory, itemDef, itemCount);
		}, self, itemDef.itemIndex, count);
	}

	private void Inventory_GiveItemTemp(On.RoR2.Inventory.orig_GiveItemTemp orig, Inventory self, ItemIndex itemIndex, float countToAdd)
	{
		try
		{
			if (suppressExponentialHook || countToAdd <= 0f || !ShouldApplyTemporaryGrant(self, itemIndex))
			{
				orig.Invoke(self, itemIndex, countToAdd);
				return;
			}
			int effective = self.GetItemCountEffective(itemIndex);
			int nextTarget = GetNextTargetStack(effective);
			if (nextTarget <= effective)
			{
				orig.Invoke(self, itemIndex, countToAdd);
				return;
			}
			float num = Math.Max(countToAdd, nextTarget - effective);
			LogTempGrant($"GiveItemTemp {itemIndex}: effectiveCurrent={effective}, incoming={countToAdd}, target={nextTarget}, adding={num}");
			suppressExponentialHook = true;
			try
			{
				orig.Invoke(self, itemIndex, num);
			}
			finally
			{
				suppressExponentialHook = false;
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
			orig.Invoke(self, itemIndex, countToAdd);
		}
	}

	private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, GenericPickupController self, CharacterBody body)
	{
		Inventory inv = (body != null) ? body.inventory : null;
		PickupIndex pIdx = self.pickupIndex;
		PickupDef pDef = PickupCatalog.GetPickupDef(pIdx);
		ItemIndex itemIndex = (pDef == null) ? ItemIndex.None : pDef.itemIndex;
		int before = (inv != null && itemIndex != ItemIndex.None) ? GetCurrentStackForExponential(inv, itemIndex) : 0;

		orig.Invoke(self, body);

		try
		{
			if (inv == null || itemIndex == ItemIndex.None || suppressExponentialHook || !ShouldApplyItem(inv, itemIndex))
			{
				return;
			}
			int currentStack = GetCurrentStackForExponential(inv, itemIndex);
			if (currentStack <= before)
			{
				return;
			}
			int nextTarget = GetNextTargetStack(before);
			if (nextTarget <= currentStack)
			{
				return;
			}
			int extra = nextTarget - currentStack;
			LogDebug($"Pickup {itemIndex}: before={before}, after={currentStack}, target={nextTarget}, adding={extra}");
			suppressExponentialHook = true;
			try
			{
				GiveExtraStacksForExponential(inv, itemIndex, extra);
			}
			finally
			{
				suppressExponentialHook = false;
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
		}
	}

	private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
	{
		orig.Invoke(self);
	}

	private static int GetCurrentStackForExponential(Inventory inventory, ItemIndex itemIndex)
	{
		if (inventory == null || itemIndex == ItemIndex.None)
		{
			return 0;
		}
		ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
		if (IsTemporaryItem(itemDef))
		{
			return inventory.GetItemCountEffective(itemIndex);
		}
		return inventory.GetItemCountPermanent(itemIndex);
	}

	private static void GiveExtraStacksForExponential(Inventory inventory, ItemIndex itemIndex, int amountToGive)
	{
		if (inventory != null && itemIndex != ItemIndex.None && amountToGive > 0)
		{
			ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
			if (IsTemporaryItem(itemDef))
			{
				inventory.GiveItemTemp(itemIndex, (float)amountToGive);
			}
			else
			{
				inventory.GiveItemPermanent(itemIndex, amountToGive);
			}
		}
	}

	private static int EvaluateSigmoidStack(int n, float L, float k, float n0, float alpha)
	{
		if (n <= 0) return 0;
		if (n == 1) return 1;

		double sigAt1 = L / (1.0 + Math.Exp(-k * (1.0 - n0)));
		double sqrtAt1 = alpha * Math.Sqrt(1.0);
		double offset = sigAt1 + sqrtAt1;

		double sig = L / (1.0 + Math.Exp(-k * (n - n0)));
		double sqrt = alpha * Math.Sqrt(n);
		double raw = n + (sig + sqrt - offset);

		return Math.Max(n, (int)Math.Round(raw));
	}

	private static int GetNextTargetStack(int current)
	{
		int maxCap = Math.Max(1, maxStack.Value);
		if (current < 0) return 1;
		if (current >= maxCap) return current;

		switch (stackingMode.Value)
		{
			case StackingMode.Off:
				return current + 1;

			case StackingMode.Linear:
			{
				int mult = Math.Max(1, linearMultiplier.Value);
				return Math.Min(maxCap, current + mult);
			}

			case StackingMode.Sigmoid:
			{
				if (current <= 0) return 1;
				float L = Math.Max(1f, sigmoidPeakBonus.Value);
				float k = Math.Max(0.05f, sigmoidGrowthRate.Value);
				float n0 = Math.Max(1f, sigmoidInflectionPoint.Value);
				float alpha = Math.Max(0.1f, sigmoidSqrtFactor.Value);

				int low = 1;
				int high = 5000;
				int currentN = 1;
				while (low <= high)
				{
					int mid = (low + high) / 2;
					int val = EvaluateSigmoidStack(mid, L, k, n0, alpha);
					if (val <= current)
					{
						currentN = mid;
						low = mid + 1;
					}
					else
					{
						high = mid - 1;
					}
				}
				int target = EvaluateSigmoidStack(currentN + 1, L, k, n0, alpha);
				return Math.Min(maxCap, Math.Max(current + 1, target));
			}

			case StackingMode.Sqrt:
			{
				float a = Math.Max(0.1f, sqrtFactor.Value);
				if (current <= 0)
				{
					return Math.Min(maxCap, 1 + (int)Math.Round(a));
				}
				// Invert current = n + a * sqrt(n)
				double u = (-a + Math.Sqrt(a * a + 4.0 * current)) / 2.0;
				int n = Math.Max(1, (int)Math.Round(u * u));
				int nextN = n + 1;
				int target = nextN + (int)Math.Round(a * Math.Sqrt(nextN));
				return Math.Min(maxCap, Math.Max(current + 1, target));
			}

			case StackingMode.Exponential:
			default:
			{
				int num = Math.Max(2, baseSize.Value);
				if (current == 0) return 1;
				double y = Math.Floor(Math.Log(current, num)) + 1.0;
				int val = (int)Math.Pow(num, y);
				return Math.Min(maxCap, Math.Max(current + 1, val));
			}
		}
	}

	private static void LogTempGrant(string message)
	{
		Debug.Log("[ExponentialItemsReloaded] [TempGrant] " + message);
	}

	private static bool ShouldApplyTemporaryGrant(Inventory self, ItemIndex itemIndex)
	{
		if (self == null || itemIndex == ItemIndex.None) return false;
		if (exponentialArtifactDef == null) return false;
		if (requireArtifactEnabled.Value)
		{
			if (RunArtifactManager.instance == null) return false;
			if (!RunArtifactManager.instance.IsArtifactEnabled(exponentialArtifactDef)) return false;
		}
		if (!affectTemporaryItems.Value) return false;
		return true;
	}

	private static bool ShouldApplyItem(Inventory self, ItemIndex itemIndex)
	{
		if (self == null || itemIndex == ItemIndex.None) return false;
		if (exponentialArtifactDef == null) return false;
		if (requireArtifactEnabled.Value)
		{
			if (RunArtifactManager.instance == null) return false;
			if (!RunArtifactManager.instance.IsArtifactEnabled(exponentialArtifactDef)) return false;
		}
		ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
		if (itemDef == null) return false;

		if (IsTemporaryItem(itemDef))
		{
			return affectTemporaryItems.Value;
		}
		if (!IsAllowedByTierConfig(itemDef)) return false;
		if (!affectTierless.Value && IsTierlessOrHidden(itemDef)) return false;

		return true;
	}

	private static bool ShouldApply(Inventory self, ItemIndex itemIndex, int count)
	{
		if (count != 1) return false;
		return ShouldApplyItem(self, itemIndex);
	}

	private static bool IsAllowedByTierConfig(ItemDef itemDef)
	{
		switch (itemDef.tier)
		{
			case ItemTier.Tier1:
				return affectTier1.Value;
			case ItemTier.Tier2:
				return affectTier2.Value;
			case ItemTier.Tier3:
				return affectTier3.Value;
			case ItemTier.Boss:
				return affectBoss.Value;
			case ItemTier.Lunar:
				return affectLunar.Value;
			case ItemTier.VoidTier1:
			case ItemTier.VoidTier2:
			case ItemTier.VoidTier3:
			case ItemTier.VoidBoss:
				return affectVoid.Value;
			default:
				return true;
		}
	}

	private static bool IsTierlessOrHidden(ItemDef itemDef)
	{
		return itemDef.tier == ItemTier.NoTier || itemDef.hidden;
	}

	private static bool IsTemporaryItem(ItemDef itemDef)
	{
		if (itemDef == null) return false;
		string text = itemDef.tier.ToString();
		string text2 = itemDef.name ?? string.Empty;
		string text3 = itemDef.nameToken ?? string.Empty;
		string text4 = text2.ToLowerInvariant();
		string text5 = text3.ToLowerInvariant();
		if (text.IndexOf("Food", StringComparison.OrdinalIgnoreCase) >= 0) return true;
		if (text.IndexOf("Temp", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("Temporary", StringComparison.OrdinalIgnoreCase) >= 0) return true;
		if (text4.Contains("temp") || text4.Contains("temporary") || text5.Contains("temp") || text5.Contains("temporary")) return true;
		if (itemDef.hidden) return true;
		return false;
	}

	private static void LogLoadedArtifacts()
	{
		LogDebug($"Total Artifacts Loaded: {ArtifactCatalog.artifactCount}");
	}

	private static void LogDebug(string message)
	{
		if (debugLogging != null && debugLogging.Value)
		{
			Debug.Log("[ExponentialItemsReloaded] " + message);
		}
	}
}

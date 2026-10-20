# ExponentialItemsReloaded v2.0 (Multi-Mode Scaling Edition)

A modernized Risk of Rain 2 mod that enhances item stacking mechanics with configurable scaling modes.

> **Credits & Acknowledgements:**  
> Originally developed by **CryptidLabs** on [Thunderstore](https://thunderstore.io/package/CryptidLabs/ExponentialItemsReloaded/).  
> Version 2.0 overhaul by [Brollix](https://github.com/Brollix) introduces multi-mode scaling algorithms (`Linear`, `Sqrt` / Sublinear, `Exponential`, and `Off`), configurable multipliers, and safety protections against uncontrollable stat spikes.

---

## What's New in v2.0?

The original mod only supported pure exponential base-2 stacking (`2^n`), which quickly ballooned items like *Soldier's Syringe* (Attack Speed) into game-breaking numbers. Version 2.0 introduces **4 selectable stacking modes**:

### 1. Sqrt Mode (Sublinear Square Root Curve) - Recommended
* **Formula:** `Stack(n) = n + round(A * sqrt(n))` (where `A` is configurable via `Sqrt Factor`, default: `2.2`).
* **How it feels:** 
  * 1st pickup: **3 stacks** (+45% attack speed / early power boost)
  * 2nd pickup: **5 stacks** (+75%)
  * 3rd pickup: **7 stacks** (+105%)
  * 4th pickup: **8 stacks** (+120%)
  * 5th pickup: **10 stacks** (+150%)
  * 10th pickup: **17 stacks** (+255%)
* **Why it's great:** Gives an immediate agile start (especially on survivors like Huntress) while gracefully tapering off so you never clip through walls or freeze the game with particle overload.

### 2. Linear Mode (y = M * x)
* **Formula:** Each pickup grants a configurable amount (default: `M = 2`).
* **How it feels:** `2, 4, 6, 8, 10, 12...` stacks.
* **Why it's great:** Clean, predictable "double loot" progression.

### 3. Exponential Mode (Base^x)
* **Formula:** Classic exponential scaling (`1, 2, 4, 8, 16, 32...`).
* **Why it's great:** The chaotic sandbox mode for explosive runs.

### 4. Off Mode (Vanilla 1x)
* Preserves the mod installed while reverting to 1x vanilla behavior.

---

## Configuration

Found in `BepInEx/config/com.cryptidlabs.exponentialitemsreloaded.cfg`:

```ini
[General]

## Stacking calculation mode: Off, Linear, Sqrt, or Exponential
# Setting type: StackingMode
# Default value: Sqrt
# Acceptable values: Off, Linear, Sqrt, Exponential
Stacking Mode = Sqrt

## Multiplier for Linear mode (e.g. 2 gives 2x stacks: 2, 4, 6, 8...)
# Setting type: Int32
# Default value: 2
Linear Multiplier = 2

## Sublinear bonus factor for Sqrt mode (e.g. 2.2 gives 3, 5, 7, 8, 10, 11, 13...)
# Setting type: Single
# Default value: 2.2
Sqrt Factor = 2.2

## If true, custom stacking only applies while Artifact of Exponents is enabled. If false, the mod is always active.
# Setting type: Boolean
# Default value: true
Require Artifact Enabled = true

## Maximum item stack count this mod will try to reach.
# Setting type: Int32
# Default value: 4096
Max Stack Size = 4096

## Exponential base for Exponential mode (e.g. 2 gives 1,2,4,8... 3 gives 1,3,9,27...)
# Setting type: Int32
# Default value: 2
Base Size = 2

[Item Filters]
Affect White Items = true
Affect Green Items = true
Affect Red Items = true
Affect Boss Items = true
Affect Lunar Items = true
Affect Void Items = true
Affect Tierless Items = false
Affect Temporary Items = false
Affect Equipment = false
```

---

## Building from Source

Requires [.NET 8+ / .NET SDK](https://dotnet.microsoft.com/):

```bash
dotnet build -c Release
```

The output DLL will be located at `bin/Release/netstandard2.1/ExponentialItemsReloaded.dll`.

---

## License

GPL-3.0 / Open Source.

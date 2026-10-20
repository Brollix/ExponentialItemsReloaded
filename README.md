# ExponentialItemsReloaded V2

A modernized Risk of Rain 2 mod that enhances item stacking mechanics with configurable scaling modes.

> **Credits:** Originally created by **CryptidLabs** on [Thunderstore](https://thunderstore.io/package/CryptidLabs/ExponentialItemsReloaded/).  
> Version 2.0 overhaul by [Brollix](https://github.com/Brollix) introduces multi-mode scaling algorithms (`Linear`, `Sqrt`, `Exponential`, and `Off`), customizable multipliers, and safety protections against runaway stat spikes.

---

## 4 Selectable Stacking Modes

Configure your preferred mode in `com.cryptidlabs.exponentialitemsreloaded.cfg`:

### 1. Sqrt Mode (Sublinear Square Root Curve) - Recommended
* **Formula:** `Stack(n) = n + round(A * sqrt(n))` (configurable via `Sqrt Factor`, default: `2.2`).
* **Progression:**
  * 1st pickup: **3 stacks** (+45% attack speed / early power boost)
  * 2nd pickup: **5 stacks** (+75%)
  * 3rd pickup: **7 stacks** (+105%)
  * 4th pickup: **8 stacks** (+120%)
  * 5th pickup: **10 stacks** (+150%)
  * 10th pickup: **17 stacks** (+255%)
* **Why it's great:** Gives an immediate agile start (especially on survivors like Huntress) while gracefully tapering off so you never clip through walls or freeze the game with particle overload.

### 2. Linear Mode (y = M * x)
* **Formula:** Each pickup grants a configurable amount (default: `M = 2`).
* **Progression:** `2, 4, 6, 8, 10, 12...` stacks.
* **Why it's great:** Clean, predictable "double loot" progression.

### 3. Exponential Mode (Base^x)
* **Formula:** Classic exponential scaling (`1, 2, 4, 8, 16, 32...`).
* **Why it's great:** The chaotic sandbox mode for explosive runs.

### 4. Off Mode (Vanilla 1x)
* Preserves the mod installed while reverting to standard 1x vanilla behavior.

---

## How to Use

1. **Artifact:** Enable the **Artifact of Exponents** in the character selection lobby (or set `Require Artifact Enabled = false` in the config to have it always active).
2. **Switching Modes:** Open the config in r2modman (`Config editor` -> `com.cryptidlabs.exponentialitemsreloaded.cfg`) and change `Stacking Mode` to `Sqrt`, `Linear`, `Exponential`, or `Off`.

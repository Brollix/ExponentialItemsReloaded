# ExponentialItemsReloaded V2

A modernized Risk of Rain 2 mod that enhances item stacking mechanics with configurable scaling modes.

> **Credits:** Originally created by **CryptidLabs** on [Thunderstore](https://thunderstore.io/package/CryptidLabs/ExponentialItemsReloaded/).  
> Version 2.0+ overhaul by [Brollix](https://github.com/Brollix) introduces multi-mode scaling algorithms (`Sigmoid`, `Linear`, `Sqrt`, `Exponential`, and `Off`), customizable multipliers, and safety protections against runaway stat spikes.

---

## 5 Selectable Stacking Modes

Configure your preferred mode in `com.cryptidlabs.exponentialitemsreloaded.cfg`:

### 1. Sigmoid Mode (S-Curve Hybrid with Sqrt Tail) - Recommended & Default
* **Philosophy:** Starts clean at **1 stack** on your first pickup (no overwhelming start), ramps up with an S-curve into satisfying chunky rewards through items 2-10, and then gently transitions into a sublinear square-root curve after item 10 to keep your game running smoothly without crashing or infinite stat loops.
* **Progression:**
  * 1st pickup: **1 stack** (+1, clean vanilla start)
  * 2nd pickup: **4 stacks** (+3)
  * 3rd pickup: **7 stacks** (+3)
  * 4th pickup: **11 stacks** (+4)
  * 5th pickup: **16 stacks** (+5)
  * 6th pickup: **23 stacks** (+7)
  * 7th pickup: **29 stacks** (+6)
  * 8th pickup: **35 stacks** (+6)
  * 9th pickup: **40 stacks** (+5)
  * 10th pickup: **44 stacks** (+4)
  * 11th pickup: **47 stacks** (+3, smooth braking begins)
  * 12th pickup: **49 stacks** (+2)
  * 15th pickup: **54 stacks** (+1)
  * 20th pickup: **61 stacks**
  * 50th pickup: **96 stacks**
  * 100th pickup: **152 stacks**
* **Why it's great:** Perfectly solves the early game (no instant 3x on item 1), rewards collecting items up to 10 with explosive power, and gracefully tapers off deep in loops so you never lag out.

### 2. Linear Mode (y = M * x)
* **Formula:** Each pickup grants a configurable amount (default: `M = 2`).
* **Progression:** `2, 4, 6, 8, 10, 12...` stacks.
* **Why it's great:** Clean, predictable "double loot" progression.

### 3. Sqrt Mode (Sublinear Square Root Curve)
* **Formula:** `Stack(n) = n + round(A * sqrt(n))` (configurable via `Sqrt Factor`, default: `2.2`).
* **Progression:** `3, 5, 7, 8, 10, 11, 13...` stacks.
* **Why it's great:** A faster early boost for survivors who need immediate stats.

### 4. Exponential Mode (Base^x)
* **Formula:** Classic exponential scaling (`1, 2, 4, 8, 16, 32...`).
* **Why it's great:** The chaotic sandbox mode for explosive runs.

### 5. Off Mode (Vanilla 1x)
* Preserves the mod installed while reverting to standard 1x vanilla behavior.

---

## How to Use

1. **Artifact:** Enable the **Artifact of Exponents** in the character selection lobby (or set `Require Artifact Enabled = false` in the config to have it always active).
2. **Switching Modes:** Open the config in r2modman (`Config editor` -> `com.cryptidlabs.exponentialitemsreloaded.cfg`) and change `Stacking Mode` to `Sigmoid`, `Linear`, `Sqrt`, `Exponential`, or `Off`.

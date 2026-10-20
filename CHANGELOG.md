# Changelog

All notable changes to **ExponentialItemsReloaded V2** will be documented in this file.

## [2.0.2] - 2026-10-20
### Documentation & Presentation
- Cleaned up `README.md` for r2modman and Thunderstore: removed redundant configuration dumps, technical build instructions, and repetitive license blocks.
- Added structured `CHANGELOG.md`.

## [2.0.1] - 2026-10-20
### Release
- Renamed Thunderstore package to `ExponentialItemsReloaded_V2` under `brollixTeam`.
- Added tags: `ai-generated`, `items`, `artifacts`, `tweaks`, `mods`.

## [2.0.0] - 2026-10-20
### Added
- **Multi-Mode Stacking Engine:**
  - `Sqrt Mode`: Sublinear progression formula `Stack(n) = n + round(A * sqrt(n))` providing strong early tempo without late-game game crashes.
  - `Linear Mode`: Configurable multiplicative step (`Linear Multiplier`, default: `2`).
  - `Exponential Mode`: Classic exponential doubling curve (`Base Size`, default: `2`).
  - `Off Mode`: Easily revert to vanilla 1x behavior without uninstalling the mod.
- **Safety caps:** Configurable `Max Stack Size` ceiling (default: 4096).
- **R2API & Modern RoR2 Compatibility:** Updated targeting .NET Standard 2.1 and current BepInEx / R2API dependencies.

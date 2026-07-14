# Mod specifics: Micro Engineer

Per-mod reference for the mod currently developed in this repo. See `CLAUDE.md` for
repo-wide/template conventions; this file is mod-specific and expected to change as the mod
evolves.

## What it is

A (heavily) KER-inspired information display mod. Shows orbital/surface/vessel/flight
parameters in flight, and stage info (deltaV, TWR, burn time) in the OAB (VAB), with a fully
user-customizable window/entry system (pop-out windows, custom windows, reorderable entries).

- Spacedock: https://spacedock.info/mod/3282/Micro%20Engineer
- Forum: https://forum.kerbalspaceprogram.com/index.php?/topic/215989-micro-engineer/
- Original (pre-Redux) author: [Micrologist](https://github.com/Micrologist); current author/maintainer: [Falki](https://github.com/Falki-git)

### Repository / source of truth

- **`Falki-git/MicroEngineer` (git remote `origin`) is the true, active source for the mod.**
  All work happens here. Push, branch, and open PRs against `origin`.
- **`Micrologist/MicroEngineer` (git remote `upstream`) is the original repo we forked from. It is
  ARCHIVED and read-only — the original author stopped maintaining the mod.** Do not target it:
  reference/history only.
- Gotcha for `gh`: with no `--repo`, `gh pr create` defaults the base to the archived upstream and
  fails with "Repository was archived so is read-only." Always pass
  `--repo Falki-git/MicroEngineer` (and base `redux/development`) when opening PRs.

## Location & metadata

- Mod source: `Assets/MicroEngineer/`
- `mod_id` / assembly: `MicroEngineer` / `MicroEngineer.dll`
- Entry point: `Assets/MicroEngineer/Code/MicroEngineerPlugin.cs` — class `MicroEngineerPlugin : KerbalMod`
- Current version: see `ModVer` const in `MicroEngineerPlugin.cs` and `Assets/MicroEngineer/swinfo.json` (keep both in sync when bumping)
- Depends on `SpaceWarp2 >= 2.0.0`; targets `ksp2_version` `0.2.8.3+` (see `swinfo.json`)
- Build output for in-editor testing: `Assets/Mods/__Testing/MicroEngineer/`

## Code layout (`Assets/MicroEngineer/Code/`)

| Folder | Contents |
|---|---|
| `Entries/` | Data-source "entry" definitions per category: `BodyEntries`, `FlightEntries`, `ManeuverEntries`, `MiscEntries`, `OabStageInfoEntries`, `OrbitalEntries`, `StageInfoEntries`, `SurfaceEntries`, `TargetEntries`, `VesselEntries`, plus `BaseEntry` base class |
| `Managers/` | `Manager` (window/entry registry + `DoFlightUpdate` driver), `MessageManager` (game message-bus subscriptions), `MicroCelestialBodies` |
| `UI/` | Scene controllers (`FlightSceneController`, `OABSceneController`), `MainGuiController`, `EntryWindowController`, `EditWindowsController`, `StageInfoEntriesBuilder`, `StageInfoOABController`, `NonStageableResourcesEntriesBuilder`, `Uxmls` (UXML/UI Toolkit asset refs), and `UI/Controls/` for individual UI Toolkit entry/control widgets |
| `Windows/` | Window classes: `BaseWindow`, `MainGuiWindow`, `EntryWindow`, `StageWindow`, `ManeuverWindow`, `TargetWindow`, `SettingsWIndow`, `StageInfoOabWindow` |
| `Utilities/` | `Settings` (SWConfiguration-backed config), `Utility`, `Enums`, `AeroForces`, `AltUnit`, `LatLonParsed`, `TimeParsed`, `TransferInfo`, `NonStageableResource`, `OrbitExtensions`, `Stage`, `UiToolkitExtensions` |

Other mod folders:
- `Copied/assets/bundles/` — `microengineer_flightui.bundle`, `microengineer_oabui.bundle` (UI Toolkit asset bundles, loaded via `AssetBundle.LoadFromFile` in `OnPreInitialized`)
- `Copied/assets/images/icon.png` — AppBar icon
- `Pipelines/` — ThunderKit "Build for Editor", "Build for Player", "Deploy to Zip File" pipelines/manifests
- No `Copied/Patches/` (PatchManager Lua patches) yet — add one if/when the mod needs to touch game content directly

## Design notes

- Extends `KerbalMod` (MonoBehaviour lifecycle) — uses `Update()` for the CTRL+E keybinding toggle and a `StartCoroutine` update loop (`DoFlightUpdate`, interval from `Settings.MainUpdateLoopUpdateFrequency`).
- Two UI surfaces with independent AppBar buttons and scene controllers: Flight/Map3D view (`ToolbarFlightButtonID` → `FlightSceneController`) and OAB/VAB (`ToolbarOabButtonID` → `OABSceneController`).
- Window/entry model: `Manager.Instance.Windows` holds all window instances; users can enable/disable entries, pop windows out, and build custom windows — see README "Usage" for the user-facing feature set.
- Currently uses `SWLogger` for logging in `MicroEngineerPlugin.cs` — new code should prefer the per-class `ReduxLib` logger convention described in `CLAUDE.md` instead.

## Build/test notes

- Build via the ThunderKit pipelines in the Unity Editor (`Assets/MicroEngineer/Pipelines/*.asset`) — ask the user to run these; Claude cannot drive the Unity editor.
- After "Build for Editor", output lands in `Assets/Mods/__Testing/MicroEngineer/` for in-editor testing.
- Check `Player.log` (path in `CLAUDE.md`) for runtime issues — this mod logs under prefixes emitted by `SWLogger`/`ReduxLib` loggers.

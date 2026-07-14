# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Keep this file current.** Whenever a script is added, removed, renamed, or has its
public API/responsibility changed, or a scene/prefab/LevelData asset is added or
rewired, update the relevant section below in the same change. This file is meant to be
the only context needed before making a change — don't make future work re-derive it by
re-reading every script from scratch.

## Project overview

Beam Split is a Unity 6 (Editor version **6000.3.8f1**) 2D URP mobile puzzle game: a
beam of light enters from a fixed Emitter, and the player places Mirrors (which split a
beam into two 45°-diagonal branches) and Filters (which additively tint a beam's color)
to route the correct color of light onto each Receiver simultaneously. Full game design
lives in the spec the user provided (20 levels across 4 phases, full UI, audio,
monetization) — **the core simulation layer plus objectives/economy/powerups/UI for 5
levels are implemented so far** (see "Build status" below); everything else (menus,
level-select, the other 15 levels, audio, ads, star rating, moving receivers) is
deferred to later phases.

Target platform is **mobile (touch)**, not desktop — this affects input handling (see
`PlacementController` below); keyboard-only interaction is not acceptable for any
player-facing control.

## Working with this repository

This is a Unity project, not a typical build-from-CLI codebase:

- No package.json/npm/Makefile. Building, running, and testing happens through the
  **Unity Editor** or Unity's command-line batch mode
  (`Unity.exe -batchmode -projectPath . -executeMethod ... -quit`).
- Unity's **Test Framework** (`com.unity.test-framework`) is installed. EditMode tests
  live under `Assets/Beam_Split/Beam_Split/Scripts/Tests/Editor/` and run via
  Window > General > Test Runner > EditMode, or batch mode:
  `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml -quit`.
- Do not hand-edit files under `Library/`, `Temp/`, `Logs/`, or `UserSettings/`
  (gitignored Unity caches/local state).
- Every asset has a paired `.meta` file holding its GUID — never delete or hand-edit a
  `.meta` file's GUID; it must move/rename with its asset.
- Scene/prefab/`.asset` files are YAML. They were hand-authored for this project's
  Phase 1 build (see below) since Unity Editor wasn't run during that build — treat them
  as fragile until opened and confirmed clean in the Editor once; after that, prefer
  Editor-driven changes over further hand-editing.

## Build status

**Phase 1 (core simulation layer) — implemented and manually verified working
end-to-end** (beam tracing, mirror splitting, filter color-mixing, receiver activation,
win-check, touch placement).

**Phase 2 (objectives, economy, powerups, UI, 5 levels) — implemented; NOT yet manually
verified in the Editor (no Editor run was available during this build).** All C# is
compile-sanity-reviewed (manual read-through of every new/modified file cross-checked
against the actual public API of every type it calls — see "Verification status"
below); the pure-logic classes (`CoinLedger`, `ObjectiveState`, `PlacementHistory`,
`SaveManager` JSON round-trip) have EditMode test coverage. The Canvas/EventSystem/
panel/HUD/tray/toast scene hierarchy and the 5 new `LevelData` assets are hand-authored
YAML, structurally self-consistent (every fileID reference resolves, no dangling refs,
no duplicate fileIDs — checked programmatically) but **not yet opened in the Editor**.
Treat the whole Phase 2 scene addition as fragile until opened once — see "Required
manual Editor steps" below for the exact checklist.

Not yet built: menus/level-select scene (Next Level / Menu buttons are documented
stubs — see `LevelLoader.OnMenuStub`/`OnNextLevelStub`), audio, ads, star rating, moving
receivers, the other 15 levels (of the planned 20), final art (all Phase 2 UI uses flat-
color placeholder `Image`s, no sliced sprites from `Art/BEAM SPLIT UI + ASSETS`).

## Project structure

```
Assets/Beam_Split/Beam_Split/
  Scripts/
    Data/             LevelData ScriptableObject schema (see below), ObjectiveType,
                       SolutionStepData/PlacementKind, EconomySaveData
    Gameplay/          Core simulation + placed-object components
      Rendering/       BeamRenderer (LineRenderer pooling)
      Economy/         CoinLedger (pure C#), EconomyManager (MonoBehaviour singleton)
      Objective/        IObjectiveClock/UnityDeltaTimeClock, MoveLimitState/TimeLimitState
                        (pure C#), ObjectiveController (MonoBehaviour)
      Powerups/         PowerupController (MonoBehaviour), PowerupCosts (const tunables)
      Placement/        PlacementHistory (pure C#, LIFO undo stack)
    Managers/          LevelLoader (level-flow state machine), SaveManager (static,
                       PlayerPrefs+JSON persistence)
    UI/                UIRaycastGate, PanelBase + 5 panels (Objective/Tutorial/Win/Lose/
                       Pause), HUD, PlacementTray, NotificationManager (toast pool),
                       HintHighlighter
    Utilities/         CanvasGroupFader, GameConstants
    Tests/Editor/      EditMode tests for the pure-logic classes
    BeamSplit.Runtime.asmdef   (references Unity.InputSystem, Unity.TextMeshPro)
  Prefabs/             Emitter, MirrorTile, FilterTile, Receiver, BeamLineRenderer, Wall
  Materials/           BeamAdditive.mat
  Data/Levels/         TestLevel_Core.asset (dev-test level, kept unmodified — see
                       "5 levels" below) + Level_01_FirstLight .. Level_05_DoubleSplit
  Art/                 UI source art (BEAM SPLIT UI + ASSETS) — not yet wired into any UI
                       (Phase 2 UI uses flat-color placeholder Images)
Assets/Scenes/
  CoreSimTest.unity    The only fully-built scene — GridManager/BeamSimulator/
                       LevelLoader/BeamRenderer/PlacementController/Main Camera (Phase
                       1, Inspector-wired) plus a Phase 2 Canvas/EventSystem hierarchy
                       and 5 new scene-resident singletons (EconomyManager/
                       ObjectiveController/PowerupController/NotificationManager/
                       HintHighlighter) — see "UI/Canvas hierarchy" below.
                       LevelLoader.currentLevel points at Level_01_FirstLight (was
                       TestLevel_Core in Phase 1).
  Gameplay.unity       Empty placeholder scene (Unity's default 2D template: Main
                       Camera + Global Light 2D only) — not wired to any gameplay
                       singleton and not referenced by LevelLoader or build settings.
                       Reserved for a future menu/level-select or production scene;
                       do not assume it has any of CoreSimTest's wiring.
Assets/Settings/       URP 2D renderer assets (UniversalRP.asset, Renderer2D.asset)
Assets/UI/             New art asset staging area, separate from
                       Assets/Beam_Split/Beam_Split/Art/ — currently just
                       Resource Vector Graphics/ (coin icon .eps + .png). Not yet
                       referenced by any prefab or script; HUD's coin display still
                       uses a placeholder Image. Update this line when it gets wired in.
```

When adding new scripts, follow the existing `Scripts/Data|Gameplay|Managers|UI|Utilities`
split. `Gameplay/Rendering/` was the first rendering-only subfolder under `Gameplay/`;
`Gameplay/Economy|Objective|Powerups|Placement/` follow the same one-concern-per-subfolder
pattern for Phase 2's systems.

## Render pipeline

URP with the **2D Renderer** (`Renderer2D.asset`). Target 2D URP conventions (e.g.
`Light2D`, 2D shader graph) rather than built-in or 3D URP. The beam uses
`Materials/BeamAdditive.mat` — URP Unlit shader, Transparent surface, Additive blend —
not a custom shader graph.

## Core simulation architecture

### Grid & direction model
The play field is a fixed-size logical grid (`GridManager`). All beam travel uses an
**8-direction compass** on `Vector2Int` (`GridDirection.cs`): 4 cardinals
(E/N/W/S, used only for an emitter's initial direction) + 4 true-45°-diagonals
(NE/NW/SW/SE, used only for post-mirror segments — diagonals step both x and y by ±1
per cell, so a mirror branch from row y=4 going NE passes through (x+1,y+1), (x+2,y+2),
etc., NOT an arbitrary diagonal). **When authoring level geometry, always check that a
receiver a mirror branch is meant to reach actually lies on that true 45° line** — this
was the root cause of a bug (see "Known issues fixed" below).

### BeamTracer (`Gameplay/BeamTracer.cs`)
Pure C# (no MonoBehaviour), unit-tested independently of the scene. Grid-intersection
walking (not continuous raycast): steps cell-by-cell along a direction, recursing into
two child rays at a mirror. Key rules:
- **Mirrors** split a beam into two new segments at ±45° (CCW/CW) from the *incoming*
  direction at that mirror — see the reflection lookup table in `GridDirection.Reflect`.
- **Filters** re-tint the beam's running color and do not change direction; the drawn
  segment is broken at the filter so `BeamRenderer` can show pre/post color distinctly.
- **Walls** block/terminate a beam outright (like a mirror with no reflection).
- **Receivers are pass-through, not obstacles** — a beam continues past a receiver it
  activates, so one beam can activate multiple receivers or reach components beyond one.
- A `(cell, direction)` visited-set guards against infinite loops from mirror cycles.
- Beams that exit grid bounds terminate harmlessly at the last in-bounds cell.

### Color mixing (`Gameplay/ColorMixer.cs`)
Beam color is **not** raw RGB — it's a discrete 7-value lattice
(`BeamColor`: White/Red/Blue/Yellow/Magenta/Green/Orange) represented during tracing as
a `ColorSet` flag-set of which base filter colors (Red/Blue/Yellow) have been hit,
capped at 2 distinct colors: a repeated color is a no-op, and a third distinct color
once 2 are already present is also a no-op (this is the source of truth — don't
reintroduce RGB float addition).

### Runtime wiring (MonoBehaviours)
- **`GridManager`** — static-`Instance` singleton. Grid dimensions set from `LevelData`
  at load; world↔grid conversion (`GetWorldPosition`, `GetGridPosition`); validates
  placement points by delegating to `BeamSimulator`.
- **`BeamSimulator`** — central authority. Holds the placement dictionary
  (`Vector2Int → IPlacedComponent`), registered `Emitter`/`Receiver` lists, and wall
  cells (from `LevelData.walls`). `Recalculate()` builds an occupancy map, calls
  `BeamTracer.Trace`, resolves world positions via `GridManager`, updates each
  `Receiver.IsActive`, and fires `OnBeamsUpdated` (consumed by `BeamRenderer`) and
  `OnAllReceiversActive` (win-check — all registered receivers active at once, checked
  every recalculation, not on a timer).
- **`Emitter`** — pure data: grid position + initial `Direction`. Multiple emitters are
  structurally supported (list-based) though only single-emitter levels exist so far.
- **`MirrorTile` / `FilterTile`** — implement `IPlacedComponent` (`GridPosition`,
  `ComponentType`). No simulation logic of their own; `BeamSimulator` reads their state
  directly. `FilterTile` also tints its own `SpriteRenderer` per `filterColor`.
- **`Receiver`** — stores `requiredColor` + `IsActive`, fires `OnActivated`/
  `OnDeactivated` UnityEvents only on state transition (wired in the prefab to toggle
  the child `Glow` sprite's `SetActive`). Not sealed / `SetActive` is protected, so a
  future `MovingReceiver : Receiver` can extend it (not implemented yet).
- **`PlacementController`** — input + placement logic. **Touch-first** (mobile), falls
  back to mouse for Editor testing (`Touchscreen.current` checked before
  `Mouse.current`). Mode selection (Mirror/FilterRed/FilterBlue/FilterYellow/Erase) is
  driven by `UI/PlacementTray.cs` calling the public `SetMode(PlacementMode)` (the old
  temporary OnGUI button row is gone — no more `OnGUI`/`ModeButtonRowHeight`). Toggles
  placement on repeated taps of an occupied cell; calls `BeamSimulator.Recalculate()`
  after any change. Gated by two independent mechanisms (see "UI input-blocking" below):
  a serialized `isInputEnabled` flag (level-flow gate, `SetInputEnabled(bool)`, defaults
  `false`) and `UI/UIRaycastGate.IsPointerOverUI()` (spatial gate for tray/panel taps).
  Owns a `PlacementHistory` (push on every successful `PlaceCurrentMode`, pop only via
  the new public `UndoLastPlacement()` — `TryErase` deliberately does not touch history).
  **Win/lose race-condition ordering** (see `LevelLoader` below): `PlaceCurrentMode`
  pushes history and calls `beamSimulator.Recalculate()` (which may synchronously fire a
  win) *before* calling `objectiveController.NotifyPlacementMade()`, and only calls it if
  `objectiveController.IsRunning` is still true afterward — this is the one subtle
  sequencing point in the whole codebase; do not reorder it.
- **`BeamRenderer`** (`Gameplay/Rendering/`) — fixed-size pool (20) of pooled
  `LineRenderer` instances (no growth), reused per `OnBeamsUpdated` pass; sets
  `startColor`/`endColor` per segment directly (no MaterialPropertyBlock needed since
  the material is a single simple additive unlit shader).
- **`LevelLoader`** (`Managers/`) — a small state machine: `LoadingData` →
  `ShowingObjectives` → `ShowingTutorial` → `Playing` → `Won`/`Lost`. Configures
  `GridManager`/`BeamSimulator`/`ObjectiveController`/`PlacementController`/`HUD`/
  `PlacementTray`, spawns Emitter/Receiver/Wall prefabs up front (so the grid is visible
  under the Objectives/Tutorial panels), shows `ObjectivePanel` then `TutorialPanel`
  (each gates `placementController.SetInputEnabled(false)` until closed), then on
  `Playing` enables input and calls `objectiveController.BeginRunning()`. `HandleWin`/
  `HandleLose` each guard on `state != Playing` (ignore late-arriving events after
  already won/lost), disable input, stop the objective clock, and show `WinPanel`
  (awards `currentLevel.coinReward` via `EconomyManager.Award`) or `LosePanel`
  ("Out of moves" / "Out of time"). `WinPanel`'s Next Level and `WinPanel`/`LosePanel`'s
  Menu buttons call documented stub methods (`OnMenuStub`/`OnNextLevelStub` — just log,
  no navigation exists yet). `LosePanel`'s Retry reloads the active scene (functional,
  not a stub).

### LevelData schema (`Data/LevelData.cs`, ScriptableObject)
Core (Phase 1, unchanged): `levelNumber`, `gridWidth`/`gridHeight`, `parMirrorCount`
(soft cap, unused beyond storage), `emitters: List<EmitterData>` (`gridPosition`,
`initialDirection`), `receivers: List<ReceiverData>` (`gridPosition`, `requiredColor`,
`isMoving`/`patrolWaypoints` — unused, future moving-receiver phase), `walls:
List<Vector2Int>`, `availableFilterColors: List<BeamColor>`, `mirrorBudget` (soft cap,
unused). Plain-field, engine-agnostic by design (no custom Unity types beyond
`Vector2Int`).

Phase 2 additions (purely additive — existing assets stay valid, new fields take their
C# defaults until hand-edited):
- **Objective**: `objectiveType` (`ObjectiveType.MoveLimit`/`TimeLimit`), `moveLimit`
  (int, used when `MoveLimit`), `timeLimitSeconds` (float, used when `TimeLimit`),
  `objectiveDescription` (free text shown on the Objectives panel).
- **Economy**: `coinReward` (flat int awarded on win).
- **Hint solution**: `solution: List<SolutionStepData>` — ordered list of
  `{gridPosition, kind: PlacementKind (Mirror/Filter), filterColor}`. The Hint powerup
  reveals the first entry whose cell doesn't yet hold a matching placement (checked via
  `BeamSimulator.TryGetPlacement`). `PlacementKind` is a small Data-layer enum, distinct
  from `PlacementController.PlacementMode`, because `LevelData` must stay engine/UI-
  agnostic (mirrors the existing `EmitterData`/`ReceiverData` plain-data-holder pattern).
- **Tutorial**: `tutorialText` (per-level override; empty = use
  `TutorialPanel.DefaultTutorialText`).

### Test level (`Data/Levels/TestLevel_Core.asset`)
Kept **unmodified** as a 6th dev-test level (not deleted, not wired into the 5 shipped
levels) — it's the worked example referenced by the mirror-reachability postmortem
below, and the new LevelData fields are additive so it still loads fine with C# defaults
(MoveLimit/10/50 coins/empty solution). 8×8 grid. Emitter at `(0,4)` facing Right.
Receiver A at `(7,4)` requiring Red — sits directly on the emitter's straight path,
reachable with just a Red filter anywhere upstream. Receiver B at `(6,7)` requiring
Orange — reachable via a mirror at `(3,4)` (reflects the row-4 beam NE:
`(4,5)→(5,6)→(6,7)`), with a Red filter placed upstream of the mirror and a Yellow
filter placed only on the NE branch before `(6,7)`. `availableFilterColors`:
Red/Blue/Yellow. `mirrorBudget`: 3.

### The 5 shipped levels (`Data/Levels/Level_01_FirstLight.asset` .. `Level_05_DoubleSplit.asset`)
`LevelLoader.currentLevel` in the scene points at `Level_01_FirstLight`. All reachability
was hand-derived against `GridDirection.Reflect`'s table and independently re-verified
(`|Δx| == |Δy|` on every diagonal step) — see "Known issues fixed" #1 for why this
matters.

| # | Name | Grid | Emitter | Mirrors | Receivers | Objective | Coins |
|---|------|------|---------|---------|-----------|-----------|-------|
| 1 | First Light | 6×6 | (0,3) E | none | (5,3) Red | MoveLimit 3 | 30 |
| 2 | Bent Path | 8×8 | (0,4) E | (1,4) | (4,7) Yellow | TimeLimit 60s | 50 |
| 3 | Two Targets | 8×8 | (0,4) E | (2,4) | (5,1) Red, (5,7) Blue | MoveLimit 6 | 75 |
| 4 | Wall Bounce | 8×8 | (0,2) E | (2,2), wall (4,2) | (6,6) Orange | TimeLimit 75s | 90 |
| 5 | Double Split | 8×8 | (0,4) E | (2,4), (3,5) | (7,5) Yellow, (3,7) Blue | MoveLimit 5 | 120 |

Level 3 deliberately puts both receivers on the *same* mirror's two diagonal outputs
(not one straight + one diverted) — a mirror terminates the incoming ray, so anything
placed on the emitter's own row to divert a second receiver would necessarily block the
first. Level 4's wall sits on the emitter's straight row *downstream* of the mirror, so
it blocks only the naive straight shot, not the intended NE-branch solution. Level 5's
second mirror receives a **diagonal** incoming direction (NE, not a cardinal) —
`GridDirection.Reflect` handles this via the same 8-entry table (`Reflect(NE)` → `(N,
E)`), confirming diagonal-incoming mirrors were already correctly supported by the
Phase 1 tracer without any changes.

### EditMode tests (`Scripts/Tests/Editor/`)
- `GridDirectionTests.cs` — the full 8-entry mirror reflection table + `Direction`→
  cardinal mapping.
- `ColorMixTests.cs` — every row of the color-mix table + the 2-color cap behavior.
- `BeamTracerTests.cs` — straight beam, single mirror, single filter, chained
  mirror+filter (no color leak between branches), wall blocking, pass-through receiver
  hits, multi-emitter convergence, infinite-loop guard.
- `CoinLedgerTests.cs` — `CanAfford`/`TrySpend` boundary cases (exact balance, over by
  1, negative-cost guard), `Add` accumulation.
- `ObjectiveStateTests.cs` — `MoveLimitState.ConsumeMove` down to 0 + `IsExhausted`,
  `AddMoves` un-exhausting; `TimeLimitState.Tick` clamping at 0 (never negative),
  `AddSeconds` un-expiring.
- `PlacementHistoryTests.cs` — push/pop LIFO order, `TryPop` on empty stack.
- `SaveManagerTests.cs` — `EconomySaveData` JSON round-trip, `Load()` default-when-
  absent. **Deletes the `"BeamSplit.Save"` PlayerPrefs key in `[TearDown]`** so running
  these tests doesn't pollute your real Editor's save data — if you add more
  PlayerPrefs-touching tests, follow the same cleanup pattern.

## Objectives / economy / powerups / UI architecture (Phase 2)

### Objective system (`Gameplay/Objective/`)
`ObjectiveController` (MonoBehaviour) owns exactly one of `MoveLimitState` or
`TimeLimitState` (pure C#, no Unity dependency), chosen from `LevelData.objectiveType`
at `Configure()`. Does not tick until `BeginRunning()` (called by `LevelLoader` once the
Objectives/Tutorial panels close). `PauseRunning()`/`ResumeRunning()` suspend/resume
`Tick()` without resetting `secondsRemaining` (used by `PauseMenu`, not
`Time.timeScale` — see "UI input-blocking" below for why). `NotifyPlacementMade()` /
`NotifyPlacementRemoved()` are called directly by `PlacementController` (not via a
`BeamSimulator` event, since move-count is about placement *actions*, not simulation
state) and are asymmetric: `NotifyPlacementMade` fires on every successful placement,
`NotifyPlacementRemoved` only on undo (both no-op on the objective type they don't
apply to). `TryAddTime(float)`/`TryAddMoves(int)` back the +Time/+Moves powerups and
return `false` if called against the wrong objective type. `IObjectiveClock` (with
production `UnityDeltaTimeClock`) is a seam so `TimeLimitState`'s countdown arithmetic
is unit-testable without real `Time.deltaTime`.

### Economy (`Gameplay/Economy/`)
`CoinLedger` (pure C#) wraps an int balance: `CanAfford`, `TrySpend` (no-ops on
insufficient/negative), `Add`. `EconomyManager` (MonoBehaviour, static-`Instance`
singleton like `GridManager`) owns one `CoinLedger`, loads it via `SaveManager.Load()`
in `Awake()`, and **persists immediately** (not batched) on every `Award()`/`TrySpend()`
via `SaveManager.Save()` — a mid-session crash/force-quit on mobile shouldn't lose a
coin change.

### Save (`Managers/SaveManager.cs`)
Static class, no scene presence. Single versioned JSON blob (`EconomySaveData{coins,
schemaVersion}`, `JsonUtility`) under one PlayerPrefs key (`"BeamSplit.Save"`).
`Save()` calls `PlayerPrefs.Save()` explicitly (doesn't rely on Unity's flush-on-quit).
`schemaVersion` is always written as `1` this phase; `Load()` doesn't branch on it yet
(no prior version to migrate from) but the field exists for a future format change.

### Powerups (`Gameplay/Powerups/`)
`PowerupController` (one MonoBehaviour owning all 4 actions — they share the same
spend-then-apply-then-notify pipeline): `TryUseAddTime()` (gated to `TimeLimit` levels),
`TryUseAddMoves()` (gated to `MoveLimit` levels), `TryUseHint()` (walks
`LevelData.solution` in order, highlights the first unsatisfied step via
`HintHighlighter`, checks satisfaction via `BeamSimulator.TryGetPlacement`),
`TryUseUndo()` (checked for "nothing to undo" *before* spending, so an empty-history
attempt costs nothing; on success calls `PlacementController.UndoLastPlacement()`).
Costs/grants live in `PowerupCosts.cs` (const tunables: AddTime 15 coins/+15s, AddMoves
15 coins/+3 moves, Hint 20 coins, Undo 10 coins).

### UI input-blocking (two independent mechanisms — see `PlacementController` above)
1. **`isInputEnabled`** (serialized bool, defaults `false` so a misconfigured scene
   fails safe) — the level-flow gate, set by `LevelLoader`'s state transitions and by
   `PauseMenu.Show()`/`Resume()`.
2. **`UI/UIRaycastGate.IsPointerOverUI()`** — the spatial gate (tray buttons, any open
   panel's clickable area), wraps `EventSystem.current.IsPointerOverGameObject()` with a
   **touch-ID overload** when a touch is active
   (`IsPointerOverGameObject(touchscreen.primaryTouch.touchId.ReadValue())`) — the
   parameterless overload only checks mouse position under the new Input System's UI
   integration, so the touch-ID overload is required for correct touch-over-UI
   detection (same touch-first requirement as `PlacementController`'s own input
   polling — see "Known issues fixed" #2).

Both are needed because they're single-purpose: `isInputEnabled` alone would leave tray
taps unprotected during normal `Playing` state (the tray isn't a blocking panel), and
`IsPointerOverUI()` alone wouldn't block input while a full-screen panel is open but the
tap lands on empty canvas space.

### UI/Canvas hierarchy (`Assets/Scenes/CoreSimTest.unity`)
One root `Canvas` (Screen Space - Overlay, `CanvasScaler` Scale With Screen Size,
reference resolution 1080×1920) added directly to the scene (not a prefab, matching how
`GridManager`/`BeamSimulator` are scene-resident). Children, in sibling/draw order:
`HUD` (pause button, level number, coins, live moves/time — `CanvasGroup`-driven
visibility, shown only once `Playing`), `PlacementTray` (5 mode buttons top row + 4
powerup buttons bottom row; `+Time`/`+Moves` buttons are mutually exclusive,
`SetActive` decided once at level start from `LevelData.objectiveType`), `ObjectivePanel`,
`TutorialPanel`, `WinPanel`, `LosePanel`, `PauseMenu` (all 5 use `PanelBase`'s
`CanvasGroup`-based `Show()`/`Hide()` — alpha/interactable/blocksRaycasts, not
`SetActive`/`Instantiate`-`Destroy` — so they can be pre-wired in the hierarchy), and
`ToastRoot` (holds `NotificationManager`'s fixed pool of 4 pooled `TextMeshProUGUI`
toasts, mirroring `BeamRenderer`'s fixed-pool-no-growth pattern; initially-inactive
`GameObject`s toggled via `SetActive` — the one place in this Canvas subtree that does
use `SetActive`, since toasts are genuinely transient, not modal). A sibling
`EventSystem` GameObject carries `InputSystemUIInputModule` (not the legacy
`StandaloneInputModule`, since this project uses `com.unity.inputsystem` exclusively)
with `actionsAsset` left unset — falls back to Unity's auto-generated default UI
actions. **This whole subtree is hand-authored YAML, not yet opened in the Editor —
see "Required manual Editor steps" below.**

Five new scene-resident logic singletons sit alongside the Canvas/EventSystem (plain
`Transform`, not UI): `EconomyManager`, `ObjectiveController`, `PowerupController`,
`NotificationManager`, `HintHighlighter` — same "scene-resident, not prefabbed" pattern
as `GridManager`/`BeamSimulator`.

## Required manual Editor steps (not yet verified — no Editor run was available)

1. **Open `CoreSimTest.unity` once.** Treat the entire Phase 2 Canvas/EventSystem/
   panel/HUD/tray/toast subtree and the 5 new `LevelData` assets as fragile until
   confirmed clean, per this file's existing hand-authored-YAML policy.
2. **Select the `EventSystem` GameObject and confirm `Input System UI Input Module`
   shows valid (non-error) default bindings** (Unity auto-populates these on first
   Inspector draw when the component was serialized with a null `actionsAsset`). If the
   Inspector shows a warning, click "Assign default actions" — a single click, not a
   rebuild. This is required for touch/mouse-over-UI detection
   (`UIRaycastGate.IsPointerOverUI()`) to work at all.
3. **Enter Play mode once per level** (switch `LevelLoader.currentLevel` between the 5
   assets in turn) and confirm: Objectives panel blocks grid taps → Tutorial panel
   blocks grid taps → gameplay becomes interactive on close → HUD shows level
   number/coins/moves-or-time → placing pieces updates the beam and HUD → win/lose
   panels appear at the right moment → coins persist across a Stop/Play cycle (closest
   in-Editor proxy for an app restart) → Undo/Hint/+Time/+Moves powerups work and
   correctly gate on cost/objective-type → Retry reloads the scene cleanly → Pause stops
   the timer and blocks input, Resume restores both.
4. **Confirm `TestLevel_Core.asset` still loads without error** when manually selected
   (regression check that the additive `LevelData` fields didn't break the pre-existing
   asset).
5. Run the EditMode test suite (`GridDirectionTests`, `ColorMixTests`,
   `BeamTracerTests`, `CoinLedgerTests`, `ObjectiveStateTests`, `PlacementHistoryTests`,
   `SaveManagerTests`) via Test Runner or batch mode and confirm all pass.

None of the above was run as part of this change — the C# was compile-sanity-reviewed
by reading every file against the actual public API it calls, and the scene/asset YAML
was checked programmatically for structural self-consistency (every fileID reference
resolves, no dangling references, no duplicate fileIDs, all `TextAlignmentOptions`
values valid), but neither is a substitute for an actual Editor run.

## Known issues fixed (context for future debugging)

1. **Mirror-branch geometry must land on a true 45° line.** A mirror's diagonal branch
   only visits cells where `|Δx| == |Δy|` from the mirror. `TestLevel_Core.asset`
   originally placed Receiver B at `(4,7)`, unreachable from a mirror at `(3,4)`'s NE
   branch (which only reaches `(4,5),(5,6),(6,7)`) — moved to `(6,7)`. When authoring
   any future level with mirror-routed receivers, verify reachability against this
   constraint before assuming a placement bug in the tracer itself. All 5 Phase 2 levels'
   emitter/mirror/receiver/solution coordinates were hand-derived against
   `GridDirection.Reflect`'s table and independently re-verified programmatically (every
   diagonal step satisfies `|Δx| == |Δy|`) specifically to avoid repeating this bug —
   see the "5 shipped levels" table above. Level 5 additionally exercises a mirror that
   receives a **diagonal** incoming direction (its second mirror sees `NE`, not a
   cardinal) — this was already correctly supported by `GridDirection.Reflect`'s
   8-entry table without any tracer changes, confirming the table (not just the 4
   cardinal-incoming rows) was already exhaustive.
2. **Input was desktop-only at first** (`Mouse.current` only) — this project targets
   **mobile/touch**, not desktop. `PlacementController.TryGetPointerDownPosition` now
   checks `Touchscreen.current` first, falling back to `Mouse.current` only for Editor
   convenience. Any future input-handling code must remain touch-first for the same
   reason — do not reintroduce mouse-only assumptions.

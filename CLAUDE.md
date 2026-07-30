# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Keep this file current.** Whenever a script is added, removed, renamed, or has its
public API/responsibility changed, or a scene/prefab/LevelData asset is added or
rewired, update the relevant section below in the same change. This file is meant to be
the only context needed before making a change — don't make future work re-derive it by
re-reading every script from scratch.

## Behavioral guidelines (CLAUDERULES.md)

The repo root also has a `CLAUDERULES.md` with general behavioral guidelines that apply
on top of everything below. Read it before implementing anything. Summary (biases
toward caution over speed; use judgment for trivial tasks):

1. **Think before coding** — state assumptions explicitly; if uncertain, ask. Present
   multiple interpretations rather than silently picking one. Say so if a simpler
   approach exists. Stop and ask if something is unclear.
2. **Simplicity first** — minimum code that solves the problem; no speculative
   features, abstractions, "flexibility," or error handling for impossible scenarios.
3. **Surgical changes** — touch only what the task requires; don't improve/refactor
   adjacent code; match existing style; remove only the imports/vars/functions your own
   change orphaned, not pre-existing dead code (mention it instead).
4. **Goal-driven execution** — turn tasks into verifiable success criteria (e.g. a
   failing test that then passes) and state a brief step → verify plan for multi-step
   work.

## Project overview

Beam Split is a Unity 6 (Editor version **6000.3.8f1**) 2D URP mobile puzzle game: a
beam of light enters from a fixed Emitter, and the player places Mirrors (which split a
beam into two 45°-diagonal branches) and Filters (which additively tint a beam's color)
to route the correct color of light onto each Receiver simultaneously. Full game design
lives in the spec the user provided (20 levels across 4 phases, full UI, audio,
monetization) — **the core simulation layer plus objectives/economy/powerups/UI for 10
levels are implemented so far**, plus audio (`AudioManager`) (see "Build status" below);
ad monetization was wired up (banner/interstitial/rewarded) but has since been removed
from the game scripts — see "Ads (removed)" below; everything else (menus, level-select,
the other 10 levels, star rating, moving receivers) is deferred to later phases.

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

Since implemented (post-Phase-2, not covered by the paragraph above): a working
`MainMenu.unity` with real Play/Quit/Next-Level/Retry/Menu navigation (no more stubs —
see `LevelLoader`'s "Runtime wiring" below), `AudioManager` (see "Audio"), and **5 more
levels (6–10), plus a redesign of Level 5's original geometry** (see "The 10 shipped
levels" below) — none of these 6 new/changed level assets have been opened in the Editor
either, same caveat as the original 5. Test-ad monetization (banner/interstitial/
rewarded) was also wired up post-Phase-2 but has since been **fully removed** at the
user's request (all `AdsManager` call sites deleted from game scripts — see "Ads
(removed)" below — plus the third-party `AdsPluginData` plugin folder, the Google Mobile
Ads SDK (`Assets/GoogleMobileAds/`, `Assets/Plugins/Android/`, `Assets/Plugins/iOS/`,
`Assets/ExternalDependencyManager/`), and `Assets/Scenes/SplashScreen.unity` itself were
all deleted from disk). **`MainMenu.unity` is now scene index 0 in Build Settings,
`CoreSimTest.unity` is index 1** (`SplashScreen.unity`'s dangling Build Settings entry —
left behind after the scene file was deleted, which would have broken any build — was
removed from `ProjectSettings/EditorBuildSettings.asset` in the same cleanup). Still not
built: a proper level-select scene (Next Level currently just walks
`LevelLoader.levels[]` in a fixed order), star rating, moving receivers, the other 10
levels (of the planned 20), and final art (all Phase 2 UI still uses flat-color
placeholder `Image`s, no sliced sprites from `Art/BEAM SPLIT UI + ASSETS`).

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
      Powerups/         PowerupController (MonoBehaviour; costs/grants are now its own
                       serialized Inspector fields, not a separate const class — see
                       "Powerups" below)
      Placement/        PlacementHistory (pure C#, LIFO undo stack)
    Managers/          LevelLoader (level-flow state machine), SaveManager (static,
                       PlayerPrefs+JSON persistence), MainMenuManager (Play/Quit wiring
                       for MainMenu.unity), LevelProgress (static hand-off for
                       Next Level's scene-reload progression), AudioManager
                       (DontDestroyOnLoad singleton, music+SFX, lives in MainMenu.unity)
    UI/                UIRaycastGate, PanelBase + 5 panels (Objective/Tutorial/Win/Lose/
                       Pause), HUD, PlacementTray, NotificationManager (toast pool),
                       HintHighlighter, CoinRewardEffect (win-screen coin-fly juice)
    Utilities/         CanvasGroupFader, GameConstants
    Tests/Editor/      EditMode tests for the pure-logic classes
    BeamSplit.Runtime.asmdef   (references Unity.InputSystem, Unity.TextMeshPro)
  Prefabs/             Emitter, MirrorTile, FilterTile, Receiver, BeamLineRenderer, Wall
  Materials/           BeamAdditive.mat
  Data/Levels/         Level_01_FirstLight .. Level_10_FinalConvergence (the 10 shipped
                       levels; TestLevel_Core.asset, the original dev-test level, was
                       deleted — see "10 shipped levels" below)
  Art/                 UI source art (BEAM SPLIT UI + ASSETS) — not yet wired into any UI
                       (Phase 2 UI uses flat-color placeholder Images)
Assets/Scenes/
  CoreSimTest.unity    The only fully-built scene — GridManager/BeamSimulator/
                       LevelLoader/BeamRenderer/PlacementController/Main Camera (Phase
                       1, Inspector-wired) plus a Phase 2 Canvas/EventSystem hierarchy
                       and 5 new scene-resident singletons (EconomyManager/
                       ObjectiveController/PowerupController/NotificationManager/
                       HintHighlighter) — see "UI/Canvas hierarchy" below.
                       LevelLoader.currentLevel points at Level_01_FirstLight;
                       LevelLoader.levels holds all 10 shipped levels in order for
                       real Next Level progression.
  MainMenu.unity       Minimal main-menu scene — Canvas (Bg image, PlayButton, QuitButton)
                       + EventSystem + a scene-resident `MainMenuManager` (plain
                       Transform, not UI, same "scene-resident singleton" pattern as
                       CoreSimTest's managers). `MainMenuManager` wires PlayButton to
                       `SceneManager.LoadScene("CoreSimTest")` and QuitButton to
                       `Application.Quit()` (`EditorApplication.isPlaying = false` in the
                       Editor) via `Button.onClick.AddListener` in `Awake()` — no
                       persistent `OnClick` calls in the scene YAML, matching
                       `PauseMenu`'s wiring style. No level-select exists yet, so Play
                       always loads CoreSimTest directly (see "Build status"). First
                       scene in Build Settings (index 0); CoreSimTest is index 1;
                       Gameplay.unity has been removed from the project. Also holds the
                       `AudioManager` GameObject (see "Audio" below) — it's placed here
                       specifically because this is the first scene loaded, and
                       `DontDestroyOnLoad` needs it to exist before CoreSimTest loads.
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
  future `MovingReceiver : Receiver` can extend it (not implemented yet). `Configure()`
  also calls `ApplyColorVisuals()`: picks the body sprite from the `colorSprites`
  reference array (`ColorSpriteEntry[]`, one slot per `BeamColor` — assign per-color
  sprites in the Inspector on `Receiver.prefab`; a color with no sprite assigned keeps
  whatever sprite is already on `bodySpriteRenderer`). **Only Red/Blue/Yellow/Green have
  actual receiver art** (White/Magenta/Orange slots are intentionally left empty) — so
  level design should avoid requiring White/Magenta/Orange receivers; see the Level 4
  postmortem below for the one place this already came up. `ApplyColorVisuals()` also
  tints `glowSpriteRenderer` to the required color via `BeamRenderer.ToUnityColor`
  (reused, not duplicated) with the
  glow's own alpha preserved.
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
  already won/lost), disable input, stop the objective clock, and show `WinPanel` or
  `LosePanel` ("Out of moves" / "Out of time"). On win, `HandleWin` first kicks off
  `CoinRewardEffect.PlayCoinReward` (if wired) to fly coin visuals into the HUD counter,
  awarding a slice of the level's flat `coinReward` via `EconomyManager.Award` as each
  one lands, and only calls `winPanel.Show(reward)` in that coroutine's `onComplete`
  callback — **`WinPanel` deliberately doesn't appear until every coin has landed**.
  Falls back to one immediate `Award(reward)` call followed immediately by
  `winPanel.Show(reward)` if `coinRewardEffect` isn't assigned.
  `WinPanel`'s Next Level button (`OnNextLevelClicked`) looks up `currentLevel`'s index
  in the `levels` array and, if a next entry exists, sets `LevelProgress.PendingLevel`
  and reloads this same scene (`Start()` picks up `LevelProgress.PendingLevel` before
  falling back to the last-saved level — see below) — past the last level, or with no
  `levels` array configured, falls through to the main menu instead. `WinPanel`/
  `LosePanel`/`PauseMenu`'s Menu buttons (`GoToMainMenu`) all call
  `SceneManager.LoadScene(mainMenuSceneName)` (defaults to `"MainMenu"`).
  `PauseMenu.IsLevelPlaying` is wired to `LevelLoader.IsPlaying` in `Start()` so Resume
  doesn't race a win/lose that happened while paused. `LosePanel`'s Retry reloads the
  active scene — `HandleLose` passes `currentLevel` into `LosePanel.Show(reason, level)`,
  which `Retry()` stashes and sets as `LevelProgress.PendingLevel` right before the
  reload, the same hand-off `OnNextLevelClicked` uses; without this the reload would
  fall through to whichever level is last-saved/serialized instead of the one just
  failed (this was a real bug — Retry was reloading Level 1 regardless of which level
  the player was on, since it never told `Start()` which level to resume).
  `Start()`'s fallback-when-there's-no-`PendingLevel` case (a genuine fresh scene entry
  — app launch or Main Menu → Play, not a Retry/Next-Level-triggered reload, both of
  which always set `PendingLevel`) now resumes from `SaveManager.Load().currentLevelIndex`
  instead of always resetting to the serialized `currentLevel` (Level 1) — see "Save"
  below. `Start()` also calls a new `PersistCurrentLevel()` right after resolving
  `currentLevel`, so every level start (fresh, Retry, or Next Level) keeps the saved
  index in sync.

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

### The 10 shipped levels (`Data/Levels/Level_01_FirstLight.asset` .. `Level_10_FinalConvergence.asset`)
`LevelLoader.currentLevel` in the scene points at `Level_01_FirstLight` (the starting
level); `LevelLoader.levels` holds all 10 in order for `OnNextLevelClicked` to walk
through — see "Runtime wiring" below. `TestLevel_Core.asset` (the original dev-test
6th level, worked example for the mirror-reachability postmortem in "Known issues
fixed" #1) has been deleted now that the 10 shipped levels are the only ones in play —
its geometry/solution notes live only in that postmortem entry now, not as a loadable
asset. All 10 levels' reachability was hand-derived against `GridDirection.Reflect`'s
table and independently re-verified (`|Δx| == |Δy|` on every diagonal step).

| # | Name | Grid | Emitter | Mirrors | Receivers | Objective | Coins |
|---|------|------|---------|---------|-----------|-----------|-------|
| 1 | First Light | 6×6 | (0,3) E | none | (5,3) Red | MoveLimit 3 | 30 |
| 2 | Bent Path | 8×8 | (0,4) E | (1,4) | (4,7) Yellow | TimeLimit 60s | 50 |
| 3 | Two Targets | 8×8 | (0,4) E | (2,4) | (5,1) Red, (5,7) Blue | MoveLimit 6 | 75 |
| 4 | Wall Bounce | 8×8 | (0,2) E | (2,2), wall (4,2) | (6,6) Green | TimeLimit 75s | 90 |
| 5 | Double Split | 8×8 | (0,3) E | (2,3), (3,4) | (3,7) Red, (7,4) Blue | MoveLimit 5 | 120 |
| 6 | Triple Threat | 8×8 | (0,2) E | (2,2), (4,4) | (4,0) Yellow, (4,7) Red, (7,4) Blue | TimeLimit 75s | 140 |
| 7 | Wall & Blend | 8×8 | (0,3) E | (2,3), wall (3,3) | (6,7) Green, (5,0) Red | MoveLimit 5 | 160 |
| 8 | Cross Current | 8×8 | (0,5) E | (1,5), (3,3), wall (2,5) | (3,7) Yellow, (7,3) Blue | TimeLimit 70s | 180 |
| 9 | Color Lab | 8×8 | (0,4) E | (2,4), (4,2) | (5,7) Green, (7,2) Red, (4,0) Yellow | MoveLimit 7 | 200 |
| 10 | Final Convergence | 8×8 | (0,3) E | (2,3), (4,5), wall (3,3) | (4,7) Blue, (7,5) Yellow, (5,0) Red | TimeLimit 100s | 240 |

Level 5 was redesigned from its original geometry (new mirror/receiver coordinates,
still MoveLimit 5, 120 coins) after it was reported unsolvable in play — the original
recorded solution data traced out as internally consistent by hand, so the cause wasn't
visible without an Editor run; rather than chase it, the level was rebuilt from scratch
with fresh, independently-verified coordinates on the same "one mirror splits into a
dead branch + a live branch, second mirror splits the live branch into the two
receivers" pattern. Levels 6–10 continue the odd/even `MoveLimit`/`TimeLimit`
alternation established by 1–5, escalate to 2 mirrors and up to 3 receivers, and
introduce two new patterns: a decorative wall blocking only the naive straight shot
(matching Level 4's, on 7/8/10) and a Blue+Yellow→Green mixed-color receiver (on 7/9) —
Green is used because it's the only 2-filter mix with receiver art (see `Receiver`
above; Magenta/Orange are never used as `requiredColor` for the same reason). Level 8's
second mirror receives a diagonal (`SE`) incoming direction, same pattern already
proven by Level 5's original design (see "Known issues fixed" #1).

Level 3 deliberately puts both receivers on the *same* mirror's two diagonal outputs
(not one straight + one diverted) — a mirror terminates the incoming ray, so anything
placed on the emitter's own row to divert a second receiver would necessarily block the
first. Level 4's wall sits on the emitter's straight row *downstream* of the mirror, so
it blocks only the naive straight shot, not the intended NE-branch solution — originally
required Orange (Red+Yellow) but was changed to Green (Blue+Yellow) since only
Red/Blue/Yellow/Green have receiver art (see `Receiver` above); the solution's first
filter changed from Red to Blue to match, mirror/second-filter unchanged. Level 5's
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
schemaVersion, currentLevelIndex}`, `JsonUtility`) under one PlayerPrefs key
(`"BeamSplit.Save"`). `Save()` calls `PlayerPrefs.Save()` explicitly (doesn't rely on
Unity's flush-on-quit). `schemaVersion` is always written as `1` this phase; `Load()`
doesn't branch on it yet (no prior version to migrate from) but the field exists for a
future format change. `currentLevelIndex` (index into `LevelLoader.levels`) was added
so level progress survives an app restart, not just coins — `LevelLoader.Start()`
writes it via `PersistCurrentLevel()` on every level start, and reads it back as the
fallback when there's no `LevelProgress.PendingLevel` (see `LevelLoader` above). Because
this is one shared blob, **both writers must read-modify-write, not construct a fresh
`EconomySaveData` from scratch** — `EconomyManager.Persist()` was changed from
constructing `new EconomySaveData { coins = ..., schemaVersion = 1 }` (which would have
silently zeroed `currentLevelIndex` back to Level 1 on every coin change) to
`SaveManager.Load()` → mutate `coins` → `SaveManager.Save()`; `LevelLoader.
PersistCurrentLevel()` follows the same load-mutate-save pattern for
`currentLevelIndex`. Any future field added to this blob must follow the same pattern.

### Powerups (`Gameplay/Powerups/`)
`PowerupController` (one MonoBehaviour owning all 4 actions — they share the same
spend-then-apply-then-notify pipeline): `TryUseAddTime()` (gated to `TimeLimit` levels),
`TryUseAddMoves()` (gated to `MoveLimit` levels), `TryUseHint()` (walks
`LevelData.solution` in order, highlights the first unsatisfied step via
`HintHighlighter`, checks satisfaction via `BeamSimulator.TryGetPlacement`),
`TryUseUndo()` (checked for "nothing to undo" *before* spending, so an empty-history
attempt costs nothing; on success calls `PlacementController.UndoLastPlacement()`).
Costs/grants are `PowerupController`'s own serialized fields (`addTimeCost`/
`addTimeSeconds`/`addMovesCost`/`addMovesGrant`/`hintCost`/`undoCost`, defaults matching
the old constants: AddTime 15 coins/+15s, AddMoves 15 coins/+3 moves, Hint 20 coins,
Undo 10 coins) — **edit them directly on the `PowerupController` component's Inspector**
(the `PowerupController` GameObject in `CoreSimTest.unity`). The standalone
`PowerupCosts.cs` const class this replaced has been deleted; there's no other tunables
file for these values now.

### Audio (`Managers/AudioManager.cs`)
Static-`Instance` singleton like `GridManager`/`EconomyManager`, but additionally
`DontDestroyOnLoad` since it must survive the `MainMenu` → `CoreSimTest` scene load (and
back via any Menu button). Lives on an `AudioManager` GameObject in `MainMenu.unity`
(the first scene loaded); guards against a duplicate in `Awake()` (`Destroy(gameObject)`
if `Instance` is already set — matters if `MainMenu` gets reloaded later while one
already persists). Owns two `AudioSource`s (`musicSource`: looping,
`sfxSource`: one-shots via `PlaySfx`/`AudioSource.PlayOneShot`) and a serialized
`AudioClip` slot per sound: `mainMenuMusic`/`gameplayMusic` (auto-switched via
`SceneManager.sceneLoaded`, keyed on scene name — `"MainMenu"` vs. anything else) and
`buttonClickClip`/`placeClip`/`eraseClip`/`winClip`/`loseClip`/`coinClip`/
`powerupClip`, each with a `PlayX()` convenience wrapper. **All `AudioClip` slots are
left unassigned** — every call site (`MainMenuManager` Play/Quit,
`PlacementController` place/erase, `LevelLoader` win/lose, `CoinRewardEffect` per coin
landed, `PowerupController` all 4 actions, `PlacementTray` mode-select) calls
`AudioManager.Instance?.PlayX()` unconditionally, which safely no-ops on a null clip —
assign clips on the `AudioManager` GameObject's Inspector (`MainMenu.unity`) to hear
them; no code changes needed.

### Ads (fully removed)
Google AdMob test ads (banner, interstitial, rewarded) were previously wired via a
third-party asset pack under `Assets/Plugins/Services/AdsPluginData/` (`AdsManager.cs`,
`AdmobManager.cs`, `Age.cs`, `SplashLoader.cs`, global namespace) plus the Google Mobile
Ads Unity SDK. **The whole ads system has since been removed, in two passes:**
1. All calls into `AdsManager` were removed from BeamSplit's own game scripts, reverting
   each call site to its pre-ads, no-ad behavior:
   - `LevelLoader`: Next Level (`WinPanel.OnNextLevelClicked`) subscribes directly to
     `AdvanceToNextLevel()` — the interstitial-then-advance wrapper (`OnNextLevelClicked`)
     was deleted.
   - `LosePanel.Retry()` reloads the scene immediately — no interstitial-before-reload.
   - `MainMenuManager` no longer calls `ShowBanner()` (its now-empty `Start()` override
     was removed entirely).
   - `PowerupController`'s 4 methods (`TryUseAddTime`/`TryUseAddMoves`/`TryUseHint`/
     `TryUseUndo`) no longer fall back to a rewarded ad when `economyManager.TrySpend`
     fails — they just show "Not enough coins" via `NotificationManager`. The `Grant()`
     local-function structure in each method is unchanged (still used for the success
     path).
   - `BeamSplit.Runtime.asmdef`'s `references` array no longer lists `"AdsPluginData"`.
2. The user then deleted, entirely from disk: `Assets/Plugins/Services/AdsPluginData/`
   (all 4 scripts, `Age_Screen.prefab`, its art), `Assets/GoogleMobileAds/` (SDK + Editor
   tooling), `Assets/Plugins/Android/` (AdMob's Android manifest/gradle templates),
   `Assets/Plugins/iOS/` (AdMob's iOS native templates), `Assets/ExternalDependencyManager/`
   (Google's dependency-resolution package used only by the ads SDK), and
   `Assets/Scenes/SplashScreen.unity` itself (the age-gate + ad-init scene). Confirmed via
   project-wide grep: no script anywhere references `AdsManager`/`AdmobManager`/
   `GoogleMobileAds`/`SplashScreen` post-deletion.

**Fallout fixed in the same cleanup**: deleting `SplashScreen.unity` left a dangling
entry in `ProjectSettings/EditorBuildSettings.asset` (Build Settings still listed it as
scene index 0, pointing at a file that no longer existed) — this would break any build.
Removed that entry; `MainMenu.unity` is now index 0, `CoreSimTest.unity` is index 1 (see
"Project structure" above). All remaining `SceneManager.LoadScene(...)` calls in the
codebase already use scene **names**, not build indices, so nothing else needed updating.

The user then also deleted `Assets/Plugins/Demigiant/DOTween/` (the whole plugin,
including `DOTweenModules.asmdef`) entirely — it only existed so `SplashLoader.cs`
(deleted along with the rest of `AdsPluginData`) could call `DOFillAmount`; nothing
under `BeamSplit.*` ever used DOTween. Confirmed via project-wide grep: no remaining
`DOTween`/`DG.Tweening` references anywhere in `Assets/` except one harmless orphan —
`Assets/Resources/DOTweenSettings.asset` (DOTween's own auto-generated config asset,
now pointing at a deleted script type). It isn't loaded by anything since the plugin
that would read it is gone; left in place since deleting it wasn't asked for, but it's
safe to delete whenever.

**Note on asmdef files generally**: `.asmdef` files (`BeamSplit.Runtime.asmdef`,
`BeamSplit.Tests.Editor.asmdef` — the only two left in the project) are a Unity
Editor-only compile-time construct — they control which `.dll` a script compiles into
and have no runtime-visible effect. They are not a plausible cause of an App Store "still in testing"
rejection; that phrasing is almost always about visible in-app content (e.g. the AdMob
test-ad watermark this cleanup already removed), not project structure. Don't remove
asmdefs as an App-Store-rejection fix without first confirming the actual rejection
reason.

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

A separate `Canvas` (`Camera Space`, `sortingOrder: -10`, so it renders behind the main
Canvas) holds a single full-screen decorative `bg` `Image` (art from `Art/BGs/`). Its
`Raycast Target` must stay disabled — see "Known issues fixed" #4 for why a raycastable
full-screen background silently blocks all grid input regardless of its render-behind
sorting order.

Five new scene-resident logic singletons sit alongside the Canvas/EventSystem (plain
`Transform`, not UI): `EconomyManager`, `ObjectiveController`, `PowerupController`,
`NotificationManager`, `HintHighlighter` — same "scene-resident, not prefabbed" pattern
as `GridManager`/`BeamSimulator`.

## Required manual Editor steps (not yet verified — no Editor run was available)

1. **Open `CoreSimTest.unity` once.** Treat the entire Phase 2 Canvas/EventSystem/
   panel/HUD/tray/toast subtree and all 10 `LevelData` assets (the original 5 plus the
   redesigned Level 5 and new Levels 6–10) as fragile until confirmed clean, per this
   file's existing hand-authored-YAML policy.
2. **Select the `EventSystem` GameObject and confirm `Input System UI Input Module`
   shows valid (non-error) default bindings** (Unity auto-populates these on first
   Inspector draw when the component was serialized with a null `actionsAsset`). If the
   Inspector shows a warning, click "Assign default actions" — a single click, not a
   rebuild. This is required for touch/mouse-over-UI detection
   (`UIRaycastGate.IsPointerOverUI()`) to work at all.
3. **Enter Play mode once per level** (switch `LevelLoader.currentLevel` between the 10
   assets in turn) and confirm: Objectives panel blocks grid taps → Tutorial panel
   blocks grid taps → gameplay becomes interactive on close → HUD shows level
   number/coins/moves-or-time → placing pieces updates the beam and HUD → win/lose
   panels appear at the right moment → coins persist across a Stop/Play cycle (closest
   in-Editor proxy for an app restart) → Undo/Hint/+Time/+Moves powerups work and
   correctly gate on cost/objective-type → Retry reloads the scene cleanly → Pause stops
   the timer and blocks input, Resume restores both. Pay particular attention to the
   redesigned Level 5 and new Levels 6–10 — their solutions are hand-traced, not
   in-Editor verified.
4. **Confirm Next Level (WinPanel) actually advances** through `LevelLoader.levels` in
   order and lands on the main menu after Level 10 (see `LevelProgress`/
   `OnNextLevelClicked` in "Runtime wiring" below).
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
   constraint before assuming a placement bug in the tracer itself. All 10 Phase 2
   levels' emitter/mirror/receiver/solution coordinates were hand-derived against
   `GridDirection.Reflect`'s table and independently re-verified programmatically (every
   diagonal step satisfies `|Δx| == |Δy|`) specifically to avoid repeating this bug —
   see the "10 shipped levels" table above. Levels 5 and 8 additionally exercise a
   mirror that receives a **diagonal** incoming direction (not a cardinal) — this was
   already correctly supported by `GridDirection.Reflect`'s 8-entry table without any
   tracer changes, confirming the table (not just the 4 cardinal-incoming rows) was
   already exhaustive.
2. **Input was desktop-only at first** (`Mouse.current` only) — this project targets
   **mobile/touch**, not desktop. `PlacementController.TryGetPointerDownPosition` now
   checks `Touchscreen.current` first, falling back to `Mouse.current` only for Editor
   convenience. Any future input-handling code must remain touch-first for the same
   reason — do not reintroduce mouse-only assumptions.
3. **All 5 panels' body/message text was white TMP text on a fully opaque white
   `PanelBackground` `Image`** (`ObjectivePanel`, `TutorialPanel`, `WinPanel`,
   `LosePanel`'s text components all had `m_fontColor`/`m_Color` at `{1,1,1,1}` against
   a `PanelBackground` also at `{1,1,1,1}`) — the text was being set and shown correctly
   (fading in with the panel, `text` string correct) but was invisible, camouflaged
   against its own background. Fixed by setting each panel's body text color to a dark
   near-black (`{0.1, 0.1, 0.1, 1}`) instead of touching the (presumably intentional)
   white card background. If a future panel's text still doesn't show despite the
   `CanvasGroup` alpha and `.text` value both being confirmed correct via logging, check
   for this same contrast issue before assuming a script/wiring bug.
4. **A full-screen decorative background `Image` blocked all grid taps.** A `bg`
   GameObject (using `Art/BGs/bg (1).png`) was added under a new low-sorting-order
   Canvas (`sortingOrder: -10`, so it renders behind everything) but kept
   `m_RaycastTarget: 1` — since `UIRaycastGate.IsPointerOverUI()` uses
   `EventSystem.IsPointerOverGameObject()`, which doesn't care about visual sorting
   order, this full-screen raycast target made *every* tap anywhere on screen register
   as "over UI", silently blocking all mirror/filter placement. Fixed by setting
   `m_RaycastTarget: 0` on it. Any future purely-decorative full-screen UI element
   (background art, vignettes, etc.) must have Raycast Target disabled, or it will
   block input the same way regardless of its Canvas sorting order/visual depth.
5. **`Receiver.prefab`'s original sprite GUID (`a86470a33a6bf42c4b3595704624658b`, used
   by both its body `SpriteRenderer` and the `Glow` child) is dangling — it does not
   resolve to any asset anywhere in the project**, so Receivers rendered invisible. This
   is why Receivers appeared to "never spawn" even though `LevelLoader.SpawnReceivers`
   and the simulation logic were both correct — the GameObjects existed, they just had
   no visible sprite. `Receiver`'s new `colorSprites` reference array (see `Receiver`
   above) exists so a real sprite can be assigned per color in the Editor to fix this —
   as of this note that assignment is still outstanding (`bodySpriteRenderer` still
   falls back to whatever sprite is already on the prefab's SpriteRenderer until a
   `colorSprites` entry is filled in). If a similarly "nothing appears" symptom shows up
   again on a different prefab, check for a dangling sprite/material GUID (`grep` the
   GUID across `Assets/` — a legitimate built-in material like `Sprites-Default` still
   won't have a matching `.meta`, but a legitimate *project* asset will) before assuming
   a spawn/logic bug.
6. **HUD's live objective value (`objectiveLiveValueText`) showed the prefab's raw
   placeholder text ("New Text") instead of the actual move count / time on level
   start.** `ObjectiveController.Configure()` fires the initial `OnMovesChanged`/
   `OnTimeChanged` event synchronously, but `LevelLoader.Start()` calls
   `objectiveController.Configure(currentLevel)` *before* `hud.Configure(...)` —
   so that first event fired before `HUD` had subscribed, and nothing else fires one
   until the player's first placement (`MoveLimit`) or the first `Update()` tick after
   `BeginRunning()` (`TimeLimit`, which happens to self-correct within a frame — this
   bug was really only visible on `MoveLimit` levels like Level 1). Fixed by having
   `HUD.Configure()` pull the current value directly from
   `objectiveController.MovesRemaining`/`SecondsRemaining` right after subscribing,
   instead of relying solely on the Configure-time event. Any future
   subscribe-then-expect-an-initial-event pattern between two `Configure()`-style
   methods needs to either guarantee subscription happens first or, like this fix,
   pull the current value explicitly after subscribing.

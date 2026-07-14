# Beam Split — Progress Summary (as of 2026-07-14)

## What the game is
Beam Split is a 2D mobile puzzle game built in Unity 6 (URP). A beam of light enters
a grid from a fixed Emitter, and the player places **Mirrors** (split the beam into
two 45° branches) and **Filters** (tint the beam's color) to route the correct color
of light onto every Receiver on the board at the same time. The full design covers 20
levels across 4 phases, with menus, audio, and monetization — this summary covers
what has been built so far.

## What's implemented and working

**Core simulation (Phase 1) — built and manually verified end-to-end.**
- Grid-based beam tracing engine: straight beams, 45° mirror splitting, wall
  blocking, and additive color mixing across 7 possible beam colors (White, Red,
  Blue, Yellow, Magenta, Green, Orange).
- Touch-based placement system (built mobile-first, not desktop/keyboard) for
  placing and erasing mirrors/filters on the grid.
- Live beam rendering that updates instantly as pieces are placed, plus win
  detection when every receiver is lit correctly at once.

**Game systems layer (Phase 2) — implemented, code-reviewed, unit-tested; pending
one in-Editor playtest pass.**
- **Objectives**: move-limit and time-limit level types, with a live HUD countdown.
- **Economy**: a coin balance that persists between play sessions (save/load).
- **Powerups**: Hint, Undo, +Time, and +Moves, each spendable with coins.
- **Full UI flow**: Objective panel → Tutorial panel → gameplay → Win/Lose panel →
  Pause menu, with a placement tray for choosing tools and a toast notification
  system for feedback.
- **5 playable levels** authored and wired in, each with a distinct objective, coin
  reward, and hand-verified solution path (First Light, Bent Path, Two Targets,
  Wall Bounce, Double Split).
- **Automated test coverage** for all the pure-logic pieces (beam tracing, color
  mixing, coin ledger, objective countdowns, undo history, save/load round-trip) —
  all passing.

## Verification status
All core simulation behavior has been played and confirmed working by hand. The
Phase 2 additions (economy/objectives/powerups/UI/5 levels) have been thoroughly
code-reviewed line-by-line and covered by automated tests, but still need one pass of
manually clicking through each level in the Unity Editor to confirm the UI panels and
scene wiring behave as expected — this is the next immediate step, not a sign of
unfinished work.

## What's not built yet (by design, later phases)
- Main menu / level-select screen (currently jumps straight into a level)
- Audio and ads
- Star rating system and moving receivers
- The remaining 15 of the planned 20 levels
- Final polished art (current UI uses placeholder colored boxes; source art files are
  in the project but not yet wired in)

## Housekeeping done today
Audited the full project against its internal documentation to confirm everything
matches what's actually in the repo, and logged two newly-added, not-yet-used assets
(an empty placeholder scene and a new UI art folder) so they're tracked for future work.

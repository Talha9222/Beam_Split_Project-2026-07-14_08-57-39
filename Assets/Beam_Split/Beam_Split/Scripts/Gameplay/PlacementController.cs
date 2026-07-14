using BeamSplit.Data;
using BeamSplit.Gameplay.Objective;
using BeamSplit.Gameplay.Placement;
using BeamSplit.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeamSplit.Gameplay
{
    /// <summary>
    /// Polls the New Input System directly (no .inputactions asset this phase). Mode
    /// selection is driven by UI/PlacementTray.cs (public SetMode) with keyboard digit
    /// keys 1-5 kept for Editor convenience. Input is gated by isInputEnabled (level-flow
    /// state) and UIRaycastGate.IsPointerOverUI() (spatial UI gate) — see CLAUDE.md/plan
    /// Section 6 for why both are needed.
    /// </summary>
    public class PlacementController : MonoBehaviour
    {
        public enum PlacementMode
        {
            Mirror,
            FilterRed,
            FilterBlue,
            FilterYellow,
            Erase
        }

        [SerializeField] private GridManager gridManager;
        [SerializeField] private BeamSimulator beamSimulator;
        [SerializeField] private ObjectiveController objectiveController;
        [SerializeField] private MirrorTile mirrorTilePrefab;
        [SerializeField] private FilterTile filterTilePrefab;
        [SerializeField] private Camera targetCamera;

        // Defaults false: a scene that somehow starts without LevelLoader calling
        // Configure() fails safe rather than accepting taps before a level is ready.
        [SerializeField] private bool isInputEnabled = false;

        private PlacementMode currentMode = PlacementMode.Mirror;
        private readonly PlacementHistory history = new PlacementHistory();

        public PlacementMode CurrentMode => currentMode;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        public void Configure(GridManager grid, BeamSimulator simulator, ObjectiveController objController)
        {
            gridManager = grid;
            beamSimulator = simulator;
            objectiveController = objController;
        }

        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;
        }

        private void Update()
        {
            HandleModeSelection();
            HandleClick();
        }

        public void SetMode(PlacementMode mode)
        {
            currentMode = mode;
            Debug.Log($"[PlacementController] Mode set to {currentMode}");
        }

        private void HandleModeSelection()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame) SetMode(PlacementMode.Mirror);
            else if (keyboard.digit2Key.wasPressedThisFrame) SetMode(PlacementMode.FilterRed);
            else if (keyboard.digit3Key.wasPressedThisFrame) SetMode(PlacementMode.FilterBlue);
            else if (keyboard.digit4Key.wasPressedThisFrame) SetMode(PlacementMode.FilterYellow);
            else if (keyboard.digit5Key.wasPressedThisFrame) SetMode(PlacementMode.Erase);
        }

        private void HandleClick()
        {
            if (!isInputEnabled)
            {
                return;
            }

            if (!TryGetPointerDownPosition(out Vector2 screenPosition))
            {
                return;
            }

            if (UIRaycastGate.IsPointerOverUI())
            {
                return;
            }

            if (gridManager == null || beamSimulator == null || targetCamera == null)
            {
                return;
            }

            Vector3 screenPos = screenPosition;
            screenPos.z = -targetCamera.transform.position.z;
            Vector3 worldPos = targetCamera.ScreenToWorldPoint(screenPos);

            Vector2Int? gridPos = gridManager.GetGridPosition(worldPos);
            if (!gridPos.HasValue)
            {
                return;
            }

            Vector2Int cell = gridPos.Value;

            if (currentMode == PlacementMode.Erase)
            {
                TryErase(cell);
                return;
            }

            if (beamSimulator.HasPlacement(cell))
            {
                // Toggle: clicking an occupied cell (that isn't valid to place onto) removes it.
                TryErase(cell);
                return;
            }

            if (!gridManager.IsValidPlacementPoint(cell))
            {
                Debug.Log($"[PlacementController] Rejected placement at {cell}: not a valid placement point.");
                return;
            }

            PlaceCurrentMode(cell);
        }

        /// <summary>
        /// Touch-first (mobile), falls back to mouse so the same build still works for
        /// in-Editor testing. Only considers a press that begins this frame, matching the
        /// prior mouse-only behavior (no drag/hold repeats).
        /// </summary>
        private static bool TryGetPointerDownPosition(out Vector2 screenPosition)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        /// <summary>
        /// Erase is a distinct manual action, not consumed by undo — deliberately does NOT
        /// push or pop PlacementHistory, and does NOT call NotifyPlacementMade/Removed.
        /// </summary>
        private void TryErase(Vector2Int cell)
        {
            if (!beamSimulator.HasPlacement(cell))
            {
                return;
            }

            var placedObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var obj in placedObjects)
            {
                if (obj is IPlacedComponent placed && placed.GridPosition == cell)
                {
                    beamSimulator.RemovePlacement(cell);
                    Destroy(obj.gameObject);
                    beamSimulator.Recalculate();
                    return;
                }
            }
        }

        private void PlaceCurrentMode(Vector2Int cell)
        {
            IPlacedComponent component = null;
            Vector3 worldPos = gridManager.GetWorldPosition(cell);
            BeamColor placedFilterColor = BeamColor.White;
            PlacementKind placedKind = PlacementKind.Mirror;

            switch (currentMode)
            {
                case PlacementMode.Mirror:
                    if (mirrorTilePrefab != null)
                    {
                        var mirror = Instantiate(mirrorTilePrefab, worldPos, Quaternion.identity);
                        mirror.SetGridPosition(cell);
                        component = mirror;
                        placedKind = PlacementKind.Mirror;
                    }
                    break;

                case PlacementMode.FilterRed:
                case PlacementMode.FilterBlue:
                case PlacementMode.FilterYellow:
                    if (filterTilePrefab != null)
                    {
                        var filter = Instantiate(filterTilePrefab, worldPos, Quaternion.identity);
                        filter.SetGridPosition(cell);
                        placedFilterColor = ModeToColor(currentMode);
                        filter.SetFilterColor(placedFilterColor);
                        component = filter;
                        placedKind = PlacementKind.Filter;
                    }
                    break;
            }

            if (component == null)
            {
                return;
            }

            if (!beamSimulator.TryPlace(cell, component))
            {
                Destroy(((Component)component).gameObject);
                return;
            }

            // Win/lose race: Recalculate() may synchronously fire OnAllReceiversActive ->
            // HandleWin, which sets level-flow state to Won BEFORE we get a chance to
            // consume a move below. Push history and recalc first; only consume a move if
            // the objective clock is still running afterward (i.e. the level didn't just end).
            history.Push(new PlacementRecord { cell = cell, kind = placedKind, filterColor = placedFilterColor });
            beamSimulator.Recalculate();

            if (objectiveController != null && objectiveController.IsRunning)
            {
                objectiveController.NotifyPlacementMade();
            }
        }

        public bool HasHistory()
        {
            return history.Count > 0;
        }

        /// <summary>
        /// Pops PlacementHistory, destroys the corresponding spawned object (same
        /// IPlacedComponent-scan-and-Destroy pattern TryErase uses), removes it from the
        /// simulator, recalculates, and gives the move back via NotifyPlacementRemoved
        /// (no-op on TimeLimit levels). Does not consume a move to remove.
        /// </summary>
        public void UndoLastPlacement()
        {
            if (!history.TryPop(out var record))
            {
                return;
            }

            if (beamSimulator.HasPlacement(record.cell))
            {
                var placedObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var obj in placedObjects)
                {
                    if (obj is IPlacedComponent placed && placed.GridPosition == record.cell)
                    {
                        beamSimulator.RemovePlacement(record.cell);
                        Destroy(obj.gameObject);
                        break;
                    }
                }
            }

            beamSimulator.Recalculate();
            objectiveController?.NotifyPlacementRemoved();
        }

        private static BeamColor ModeToColor(PlacementMode mode)
        {
            switch (mode)
            {
                case PlacementMode.FilterRed: return BeamColor.Red;
                case PlacementMode.FilterBlue: return BeamColor.Blue;
                case PlacementMode.FilterYellow: return BeamColor.Yellow;
                default: return BeamColor.White;
            }
        }
    }
}

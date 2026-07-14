using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BeamSplit.UI
{
    /// <summary>
    /// Single source of truth for "is the pointer currently over UI". Touch-aware: the
    /// fingerId-taking overload of IsPointerOverGameObject is required for correct
    /// touch-over-UI detection under the new Input System's UI integration (the
    /// parameterless overload only checks mouse position).
    /// </summary>
    public static class UIRaycastGate
    {
        public static bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
            {
                return EventSystem.current.IsPointerOverGameObject(touchscreen.primaryTouch.touchId.ReadValue());
            }

            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}

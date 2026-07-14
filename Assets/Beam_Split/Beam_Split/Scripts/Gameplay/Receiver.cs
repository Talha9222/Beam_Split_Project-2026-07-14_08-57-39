using BeamSplit.Data;
using UnityEngine;
using UnityEngine.Events;

namespace BeamSplit.Gameplay
{
    /// <summary>
    /// Not sealed, SetActive protected — designed so a later MovingReceiver : Receiver can
    /// extend it; not implemented this phase.
    /// </summary>
    public class Receiver : MonoBehaviour
    {
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private BeamColor requiredColor;

        public UnityEvent OnActivated;
        public UnityEvent OnDeactivated;

        public Vector2Int GridPosition => gridPosition;
        public BeamColor RequiredColor => requiredColor;
        public bool IsActive { get; private set; }

        public void Configure(Vector2Int position, BeamColor color)
        {
            gridPosition = position;
            requiredColor = color;
        }

        /// <summary>
        /// Called by BeamSimulator. Fires OnActivated/OnDeactivated only on transition.
        /// </summary>
        protected void SetActive(bool active)
        {
            if (IsActive == active)
            {
                return;
            }

            IsActive = active;

            if (active)
            {
                OnActivated?.Invoke();
            }
            else
            {
                OnDeactivated?.Invoke();
            }
        }

        public void ApplyBeamColor(BeamColor? arrivingColor)
        {
            bool matches = arrivingColor.HasValue && arrivingColor.Value == requiredColor;
            SetActive(matches);
        }
    }
}

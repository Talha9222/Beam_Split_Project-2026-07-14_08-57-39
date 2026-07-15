using BeamSplit.Data;
using BeamSplit.Gameplay.Rendering;
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
        [System.Serializable]
        public class ColorSpriteEntry
        {
            public BeamColor color;
            public Sprite sprite;
        }

        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private BeamColor requiredColor;

        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [SerializeField] private SpriteRenderer glowSpriteRenderer;
        [SerializeField] private ColorSpriteEntry[] colorSprites;

        public UnityEvent OnActivated;
        public UnityEvent OnDeactivated;

        public Vector2Int GridPosition => gridPosition;
        public BeamColor RequiredColor => requiredColor;
        public bool IsActive { get; private set; }

        public void Configure(Vector2Int position, BeamColor color)
        {
            gridPosition = position;
            requiredColor = color;
            ApplyColorVisuals();
        }

        /// <summary>
        /// Body sprite comes from the colorSprites reference array (assign per-color sprites
        /// in the Inspector); Glow is tinted to the same required color via BeamRenderer's
        /// shared BeamColor-to-RGB mapping rather than needing its own per-color sprite.
        /// </summary>
        private void ApplyColorVisuals()
        {
            if (bodySpriteRenderer != null && colorSprites != null)
            {
                foreach (var entry in colorSprites)
                {
                    if (entry.color == requiredColor && entry.sprite != null)
                    {
                        bodySpriteRenderer.sprite = entry.sprite;
                        break;
                    }
                }
            }

            if (glowSpriteRenderer != null)
            {
                Color tint = BeamRenderer.ToUnityColor(requiredColor);
                tint.a = glowSpriteRenderer.color.a;
                glowSpriteRenderer.color = tint;
            }
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

using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay
{
    public class FilterTile : MonoBehaviour, IPlacedComponent
    {
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private BeamColor filterColor = BeamColor.Red;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Vector2Int GridPosition => gridPosition;
        public PlacedComponentType ComponentType => PlacedComponentType.Filter;
        public BeamColor FilterColor => filterColor;

        private static readonly Color RedTint = new Color(1f, 0f, 0f, 0.7f);
        private static readonly Color BlueTint = new Color(0f, 0f, 1f, 0.7f);
        private static readonly Color YellowTint = new Color(1f, 1f, 0f, 0.7f);

        public void SetGridPosition(Vector2Int position)
        {
            gridPosition = position;
        }

        public void SetFilterColor(BeamColor color)
        {
            filterColor = color;
            ApplyTint();
        }

        private void Awake()
        {
            ApplyTint();
        }

        private void ApplyTint()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                return;
            }

            switch (filterColor)
            {
                case BeamColor.Red:
                    spriteRenderer.color = RedTint;
                    break;
                case BeamColor.Blue:
                    spriteRenderer.color = BlueTint;
                    break;
                case BeamColor.Yellow:
                    spriteRenderer.color = YellowTint;
                    break;
                default:
                    spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
                    break;
            }
        }
    }
}

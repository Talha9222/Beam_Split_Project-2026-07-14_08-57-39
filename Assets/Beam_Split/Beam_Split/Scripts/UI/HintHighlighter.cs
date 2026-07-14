using BeamSplit.Gameplay;
using UnityEngine;

namespace BeamSplit.UI
{
    /// <summary>
    /// Purely presentational — never touches BeamSimulator. Draws a temporary
    /// semi-transparent placeholder square at the target world position, self-destroying
    /// after a few seconds.
    /// </summary>
    public class HintHighlighter : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private GameObject highlightPrefab;

        private static readonly Color HighlightColor = new Color(1f, 1f, 1f, 0.45f);

        public void Show(Vector2Int gridPosition)
        {
            if (gridManager == null)
            {
                return;
            }

            Vector3 worldPos = gridManager.GetWorldPosition(gridPosition);

            GameObject highlight;
            if (highlightPrefab != null)
            {
                highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity);
            }
            else
            {
                // No prefab wired: fall back to a plain-color placeholder square, matching
                // the placeholder-UI convention used everywhere else in this project.
                highlight = new GameObject("HintHighlight");
                highlight.transform.position = worldPos;
                var sr = highlight.AddComponent<SpriteRenderer>();
                sr.color = HighlightColor;
                sr.drawMode = SpriteDrawMode.Simple;
            }

            Destroy(highlight, Utilities.GameConstants.HintHighlightDuration);
        }
    }
}

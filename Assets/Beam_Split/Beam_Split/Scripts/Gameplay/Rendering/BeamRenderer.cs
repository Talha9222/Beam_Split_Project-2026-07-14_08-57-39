using System.Collections.Generic;
using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay.Rendering
{
    /// <summary>
    /// Fixed-size LineRenderer pool, no growth. Subscribes to BeamSimulator.OnBeamsUpdated.
    /// </summary>
    public class BeamRenderer : MonoBehaviour
    {
        [SerializeField] private BeamSimulator beamSimulator;
        [SerializeField] private LineRenderer lineRendererPrefab;
        [SerializeField] private int poolSize = 20;
        [SerializeField] private Transform poolParent;

        private readonly List<LineRenderer> pool = new List<LineRenderer>();

        private void Awake()
        {
            if (poolParent == null)
            {
                var beamsGo = new GameObject("Beams");
                beamsGo.transform.SetParent(transform, false);
                poolParent = beamsGo.transform;
            }

            for (int i = 0; i < poolSize; i++)
            {
                LineRenderer instance = lineRendererPrefab != null
                    ? Instantiate(lineRendererPrefab, poolParent)
                    : CreateFallbackLineRenderer();

                instance.gameObject.SetActive(false);
                pool.Add(instance);
            }
        }

        private LineRenderer CreateFallbackLineRenderer()
        {
            var go = new GameObject("BeamLine");
            go.transform.SetParent(poolParent, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.widthMultiplier = 0.1f;
            return lr;
        }

        private void OnEnable()
        {
            if (beamSimulator != null)
            {
                beamSimulator.OnBeamsUpdated += HandleBeamsUpdated;
            }
        }

        private void OnDisable()
        {
            if (beamSimulator != null)
            {
                beamSimulator.OnBeamsUpdated -= HandleBeamsUpdated;
            }
        }

        private void HandleBeamsUpdated(List<BeamSegment> segments)
        {
            int count = segments.Count;
            if (count > poolSize)
            {
                Debug.LogWarning($"[BeamRenderer] {count} segments exceeds pool size {poolSize}; truncating.");
                count = poolSize;
            }

            for (int i = 0; i < count; i++)
            {
                var segment = segments[i];
                var lr = pool[i];
                lr.gameObject.SetActive(true);
                lr.positionCount = 2;
                lr.SetPosition(0, segment.startWorld);
                lr.SetPosition(1, segment.endWorld);
                Color color = ToUnityColor(segment.color);
                lr.startColor = color;
                lr.endColor = color;
            }

            for (int i = count; i < pool.Count; i++)
            {
                pool[i].gameObject.SetActive(false);
            }
        }

        public static Color ToUnityColor(BeamColor color)
        {
            switch (color)
            {
                case BeamColor.White: return new Color(1f, 1f, 1f, 1f);
                case BeamColor.Red: return new Color(1f, 0f, 0f, 1f);
                case BeamColor.Blue: return new Color(0f, 0f, 1f, 1f);
                case BeamColor.Yellow: return new Color(1f, 1f, 0f, 1f);
                case BeamColor.Magenta: return new Color(1f, 0f, 1f, 1f);
                case BeamColor.Green: return new Color(0f, 1f, 0f, 1f);
                case BeamColor.Orange: return new Color(1f, 0.5f, 0f, 1f);
                default: return Color.white;
            }
        }
    }
}

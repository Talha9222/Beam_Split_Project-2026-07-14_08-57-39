using System.Collections;
using System.Collections.Generic;
using BeamSplit.Utilities;
using TMPro;
using UnityEngine;

namespace BeamSplit.UI
{
    /// <summary>
    /// Toast system. Scene singleton (static Instance, same pattern as GridManager/
    /// EconomyManager). Fixed pool of 4 pre-instantiated, initially-inactive toast
    /// instances (mirrors BeamRenderer's fixed-pool-no-growth pattern). Multiple concurrent
    /// messages stack upward with a fixed vertical offset per active toast.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI[] toastPool = new TextMeshProUGUI[GameConstants.ToastPoolSize];

        private readonly Queue<TextMeshProUGUI> available = new Queue<TextMeshProUGUI>();
        private readonly List<TextMeshProUGUI> active = new List<TextMeshProUGUI>();

        private void Awake()
        {
            Instance = this;

            foreach (var toast in toastPool)
            {
                if (toast == null)
                {
                    continue;
                }

                toast.gameObject.SetActive(false);
                var group = toast.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = 0f;
                }

                available.Enqueue(toast);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ShowMessage(string text)
        {
            if (available.Count == 0)
            {
                // Fixed pool, no growth — silently drop if all 4 slots are busy.
                return;
            }

            var toast = available.Dequeue();
            active.Add(toast);
            toast.text = text;
            toast.gameObject.SetActive(true);

            RepositionActiveToasts();
            StartCoroutine(RunToastLifecycle(toast));
        }

        private void RepositionActiveToasts()
        {
            for (int i = 0; i < active.Count; i++)
            {
                var rect = active[i].rectTransform;
                rect.anchoredPosition = new Vector2(0f, i * GameConstants.ToastStackOffsetY);
            }
        }

        private IEnumerator RunToastLifecycle(TextMeshProUGUI toast)
        {
            var group = toast.GetComponent<CanvasGroup>();

            yield return CanvasGroupFader.FadeTo(group, 1f, GameConstants.ToastFadeInDuration);
            yield return new WaitForSecondsRealtime(GameConstants.ToastHoldDuration);
            yield return CanvasGroupFader.FadeTo(group, 0f, GameConstants.ToastFadeOutDuration);

            toast.gameObject.SetActive(false);
            active.Remove(toast);
            available.Enqueue(toast);
            RepositionActiveToasts();
        }
    }
}

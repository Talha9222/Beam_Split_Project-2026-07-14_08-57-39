using System;
using System.Collections;
using BeamSplit.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace BeamSplit.UI
{
    /// <summary>
    /// Small abstract base for the 5 modal-ish panels (Objectives, Tutorial, Win, Lose,
    /// Pause). Show()/Hide() toggle a CanvasGroup (alpha/interactable/blocksRaycasts)
    /// rather than SetActive, so panels can be pre-wired in the Canvas hierarchy without
    /// repeated Instantiate/Destroy.
    /// </summary>
    public abstract class PanelBase : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Button closeButton;

        public event Action OnClosed;

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseButtonClicked);
            }

            SetVisible(false, immediate: true);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            SetVisible(true, immediate: false);
        }

        public virtual void Hide()
        {
            SetVisible(false, immediate: false);
        }

        protected void CloseButtonClicked()
        {
            Hide();
            OnClosed?.Invoke();
        }

        private void SetVisible(bool visible, bool immediate)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            StopAllCoroutines();
            if (immediate)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
            }
            else
            {
                StartCoroutine(CanvasGroupFader.FadeTo(canvasGroup, visible ? 1f : 0f, GameConstants.PanelFadeDuration));
            }
        }
    }
}

using System.Collections;
using UnityEngine;

namespace BeamSplit.Utilities
{
    /// <summary>Tiny reusable coroutine helper for fading a CanvasGroup's alpha.</summary>
    public static class CanvasGroupFader
    {
        public static IEnumerator FadeTo(CanvasGroup group, float target, float duration)
        {
            if (group == null)
            {
                yield break;
            }

            float start = group.alpha;
            if (duration <= 0f)
            {
                group.alpha = target;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            group.alpha = target;
        }
    }
}

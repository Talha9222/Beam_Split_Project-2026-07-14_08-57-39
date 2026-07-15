using System.Collections;
using BeamSplit.Managers;
using BeamSplit.Utilities;
using UnityEngine;

namespace BeamSplit.UI
{
    /// <summary>
    /// Win-screen juice: spawns a handful of coin visuals at screen center and flies them
    /// one by one to the HUD coin readout, calling back with a partial award as each one
    /// lands so the HUD counter ticks up instead of jumping straight to the total.
    /// coinImagePrefab is intentionally left for hand-authored art (see CLAUDE.md) — if it's
    /// unassigned, PlayCoinReward degrades to an instant single award with no visual.
    /// </summary>
    public class CoinRewardEffect : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private RectTransform coinSpawnPoint;
        [SerializeField] private RectTransform coinTargetPoint;
        [SerializeField] private RectTransform coinImagePrefab;

        /// <summary>
        /// Spawns min(totalCoins, CoinFlyMaxCount) coin visuals, staggered, each awarding a
        /// slice of totalCoins (remainder folded into the last one) via onCoinLanded as it
        /// arrives at coinTargetPoint. Invokes onComplete once every coin has landed.
        /// </summary>
        public IEnumerator PlayCoinReward(int totalCoins, System.Action<int> onCoinLanded, System.Action onComplete)
        {
            if (coinImagePrefab == null || coinTargetPoint == null || canvasRoot == null || totalCoins <= 0)
            {
                onCoinLanded?.Invoke(totalCoins);
                onComplete?.Invoke();
                yield break;
            }

            int coinCount = Mathf.Clamp(totalCoins, 1, GameConstants.CoinFlyMaxCount);
            int baseValue = totalCoins / coinCount;
            int remainder = totalCoins - baseValue * coinCount;

            Vector3 spawnPosition = coinSpawnPoint != null ? coinSpawnPoint.position : canvasRoot.position;

            int remainingCoins = coinCount;
            for (int i = 0; i < coinCount; i++)
            {
                int value = baseValue + (i == coinCount - 1 ? remainder : 0);
                StartCoroutine(FlyCoin(spawnPosition, value, onCoinLanded, () =>
                {
                    remainingCoins--;
                    if (remainingCoins == 0)
                    {
                        onComplete?.Invoke();
                    }
                }));

                yield return new WaitForSecondsRealtime(GameConstants.CoinFlyStagger);
            }
        }

        private IEnumerator FlyCoin(Vector3 spawnPosition, int value, System.Action<int> onLanded, System.Action onArrived)
        {
            var coin = Instantiate(coinImagePrefab, canvasRoot);
            coin.position = spawnPosition;

            Vector3 start = spawnPosition;
            float elapsed = 0f;

            while (elapsed < GameConstants.CoinFlyDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / GameConstants.CoinFlyDuration);
                float eased = t * t * (3f - 2f * t); // smoothstep
                coin.position = Vector3.Lerp(start, coinTargetPoint.position, eased);
                yield return null;
            }

            Destroy(coin.gameObject);
            AudioManager.Instance?.PlayCoin();
            onLanded?.Invoke(value);
            onArrived?.Invoke();
        }
    }
}

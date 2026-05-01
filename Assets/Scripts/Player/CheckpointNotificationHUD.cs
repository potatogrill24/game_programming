using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameProgramming.Player
{
    public class CheckpointNotificationHUD : MonoBehaviour
    {
        [SerializeField] private Text notificationText;
        [SerializeField] private string message = "Checkpoint reached";
        [SerializeField] private float visibleDuration = 1.6f;
        [SerializeField] private float fadeDuration = 0.4f;

        private Coroutine activeRoutine;

        private void Awake()
        {
            SetAlpha(0f);
        }

        public void Show()
        {
            if (notificationText == null)
            {
                return;
            }

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            notificationText.text = message;
            SetAlpha(1f);

            yield return new WaitForSeconds(visibleDuration);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsed / fadeDuration));
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(0f);
            activeRoutine = null;
        }

        private void SetAlpha(float alpha)
        {
            if (notificationText == null)
            {
                return;
            }

            Color color = notificationText.color;
            color.a = alpha;
            notificationText.color = color;
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using GameProgramming.Core;

namespace GameProgramming.Game
{
    public class GameStateController : MonoBehaviour
    {
        [SerializeField] private GameObject victoryRoot;
        [SerializeField] private GameObject gameplayRoot;
        [SerializeField] private bool allowKeyboardRestart = true;
        [SerializeField] private KeyCode restartKey = KeyCode.R;

        public static GameStateController Instance { get; private set; }

        public bool IsLevelCompleted { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ApplyState();
        }

        private void Update()
        {
            if (!allowKeyboardRestart)
            {
                return;
            }

            if (GameInput.WasPressed(restartKey))
            {
                RestartLevel();
            }
        }

        public void CompleteLevel()
        {
            if (IsLevelCompleted)
            {
                return;
            }

            IsLevelCompleted = true;
            ApplyState();
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }

        private void ApplyState()
        {
            if (victoryRoot != null)
            {
                victoryRoot.SetActive(IsLevelCompleted);
            }

            if (gameplayRoot != null)
            {
                gameplayRoot.SetActive(!IsLevelCompleted);
            }
        }
    }
}

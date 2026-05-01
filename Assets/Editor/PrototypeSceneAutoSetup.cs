using System;
using GameProgramming.Game;
using GameProgramming.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameProgramming.EditorTools
{
    [InitializeOnLoad]
    public static class PrototypeSceneAutoSetup
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

        private static bool isEnsureQueued;

        static PrototypeSceneAutoSetup()
        {
            EditorApplication.delayCall += QueueEnsurePrototypeLevel;
            EditorSceneManager.sceneOpened += HandleSceneOpened;
        }

        private static void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            QueueEnsurePrototypeLevel();
        }

        private static void QueueEnsurePrototypeLevel()
        {
            if (isEnsureQueued)
            {
                return;
            }

            isEnsureQueued = true;
            EditorApplication.delayCall += EnsurePrototypeLevelIfNeeded;
        }

        private static void EnsurePrototypeLevelIfNeeded()
        {
            isEnsureQueued = false;

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!IsTargetScene(activeScene))
            {
                return;
            }

            if (PrototypeLevelBuilder.HasDemoLevel())
            {
                return;
            }

            if (UnityEngine.Object.FindFirstObjectByType<AstronautController>() != null ||
                UnityEngine.Object.FindFirstObjectByType<GameStateController>() != null)
            {
                return;
            }

            bool created = PrototypeLevelBuilder.TryCreateDemoLevelIfMissing();
            if (created)
            {
                Debug.Log("SampleScene was empty, so the prototype 3D puzzle level was generated automatically.");
            }
        }

        private static bool IsTargetScene(Scene scene)
        {
            return scene.IsValid() &&
                   string.Equals(scene.path, SampleScenePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}

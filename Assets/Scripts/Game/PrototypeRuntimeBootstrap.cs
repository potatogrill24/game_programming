using System;
using System.Reflection;
using GameProgramming.CameraSystem;
using GameProgramming.Core;
using GameProgramming.Player;
using GameProgramming.World.Cubes;
using GameProgramming.World.Doors;
using GameProgramming.World.Hazards;
using GameProgramming.World.Mechanisms;
using GameProgramming.World.Panels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;

namespace GameProgramming.Game
{
    public static class PrototypeRuntimeBootstrap
    {
        private const string RootName = "PrototypeLevel_Runtime";
        private const string SampleSceneName = "SampleScene";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildSampleSceneIfNeeded()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!IsTargetScene(activeScene))
            {
                return;
            }

            if (UnityObject.FindFirstObjectByType<AstronautController>() != null)
            {
                return;
            }

            BuildLevel();
        }

        private static bool IsTargetScene(Scene scene)
        {
            return scene.IsValid() &&
                   (scene.name == SampleSceneName || scene.path.EndsWith("/" + SampleSceneName + ".unity"));
        }

        private static void BuildLevel()
        {
            GameObject root = new GameObject(RootName);
            root.SetActive(false);

            GameObject systemsRoot = CreateEmpty("Systems", root.transform, Vector3.zero);
            GameObject geometryRoot = CreateEmpty("Geometry", root.transform, Vector3.zero);
            GameObject mechanicsRoot = CreateEmpty("Mechanics", root.transform, Vector3.zero);

            GameStateController gameState = CreateGameState(systemsRoot.transform);
            CreateRespawnZone(systemsRoot.transform);
            EnsureDirectionalLight(root.transform);

            AstronautEnergy playerEnergy = CreatePlayer(root.transform);
            Camera camera = CreateOrConfigureCamera(root.transform, playerEnergy.transform);
            CreateOrConfigureUi(root.transform, playerEnergy, gameState);

            AssignField(playerEnergy.GetComponent<AstronautController>(), "cameraReference", camera.transform);

            CreateGeometry(geometryRoot.transform);
            CreateBlueDoor(mechanicsRoot.transform);
            BridgeMechanism bridge = CreateBridge(mechanicsRoot.transform);
            CreateColorPanel(
                "PurpleBridgePanel",
                mechanicsRoot.transform,
                new Vector3(5.4f, 1.2f, 3.8f),
                EnergyColor.Purple,
                bridge,
                new Color(0.72f, 0.35f, 1f));

            LaserBarrier laserBarrier = CreateLaserBarrier(mechanicsRoot.transform);
            CreateColorPanel(
                "YellowLaserPanel",
                mechanicsRoot.transform,
                new Vector3(11.5f, 1.2f, 3.8f),
                EnergyColor.Yellow,
                laserBarrier,
                new Color(1f, 0.85f, 0.2f));

            CreateWaveZone(mechanicsRoot.transform);
            CreateCheckpoint(mechanicsRoot.transform);

            CreatePurpleCube(mechanicsRoot.transform);
            PassageMechanism finalBlock = CreateFinalBlock(mechanicsRoot.transform);
            CreatePedestal(mechanicsRoot.transform, finalBlock);
            CreateGoal(mechanicsRoot.transform, gameState);

            root.SetActive(true);
        }

        private static GameStateController CreateGameState(Transform parent)
        {
            GameObject gameStateObject = CreateEmpty("GameState", parent, Vector3.zero);
            return gameStateObject.AddComponent<GameStateController>();
        }

        private static void CreateRespawnZone(Transform parent)
        {
            GameObject respawnZone = CreateEmpty("RespawnZone", parent, new Vector3(18f, -8f, 0f));
            BoxCollider collider = respawnZone.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(80f, 1f, 40f);
            respawnZone.AddComponent<RespawnZone>();
        }

        private static AstronautEnergy CreatePlayer(Transform parent)
        {
            GameObject player = CreateEmpty("Player", parent, new Vector3(-3f, 1f, 0f));

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.height = 2f;
            characterController.radius = 0.5f;

            player.AddComponent<AstronautController>();
            AstronautEnergy astronautEnergy = player.AddComponent<AstronautEnergy>();
            AstronautInteractor astronautInteractor = player.AddComponent<AstronautInteractor>();
            player.AddComponent<PlayerRespawn>();

            GameObject graphics = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            graphics.name = "Graphics";
            graphics.transform.SetParent(player.transform, false);
            graphics.transform.localPosition = new Vector3(0f, 1f, 0f);
            UnityObject.Destroy(graphics.GetComponent<Collider>());
            ApplyRendererColor(graphics.GetComponent<Renderer>(), new Color(0.92f, 0.92f, 0.92f), 0.15f);

            GameObject carryAnchor = CreateEmpty("CarryAnchor", player.transform, new Vector3(0f, 1.2f, 1f));

            AssignField(astronautEnergy, "suitRenderers", new[] { graphics.GetComponent<Renderer>() });
            AssignField(astronautInteractor, "carryAnchor", carryAnchor.transform);
            AssignField(astronautInteractor, "interactionOrigin", carryAnchor.transform);
            AssignField(astronautInteractor, "carriedCubeEulerOffset", Vector3.zero);

            astronautEnergy.RefreshVisuals();
            return astronautEnergy;
        }

        private static Camera CreateOrConfigureCamera(Transform parent, Transform target)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = UnityObject.FindFirstObjectByType<Camera>();
            }

            GameObject cameraObject;
            if (camera == null)
            {
                cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
            }
            else
            {
                cameraObject = camera.gameObject;
            }

            cameraObject.transform.SetParent(parent, true);
            cameraObject.transform.position = new Vector3(-8f, 7f, -8f);
            cameraObject.transform.rotation = Quaternion.Euler(28f, 35f, 0f);
            camera.orthographic = false;

            SimpleCameraFollow follow = cameraObject.GetComponent<SimpleCameraFollow>();
            if (follow == null)
            {
                follow = cameraObject.AddComponent<SimpleCameraFollow>();
            }

            AssignField(follow, "target", target);
            AssignField(follow, "offset", new Vector3(-2.5f, 6f, -7f));
            AssignField(follow, "useTargetSpaceOffset", false);

            return camera;
        }

        private static void CreateOrConfigureUi(Transform parent, AstronautEnergy playerEnergy, GameStateController gameState)
        {
            Canvas canvas = UnityObject.FindFirstObjectByType<Canvas>();
            GameObject canvasObject;

            if (canvas == null)
            {
                canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvasObject = canvas.gameObject;
            }

            canvasObject.transform.SetParent(parent, true);

            GameObject hudRoot = new GameObject("HUD", typeof(RectTransform));
            hudRoot.transform.SetParent(canvasObject.transform, false);

            RectTransform hudRootRect = hudRoot.GetComponent<RectTransform>();
            hudRootRect.anchorMin = Vector2.zero;
            hudRootRect.anchorMax = Vector2.one;
            hudRootRect.offsetMin = Vector2.zero;
            hudRootRect.offsetMax = Vector2.zero;

            Image glowFrame = CreateUiImage("GlowFrame", hudRoot.transform, new Vector2(80f, -80f), new Vector2(110f, 110f), new Color(1f, 0.9f, 0.35f, 0.35f));
            Image colorIcon = CreateUiImage("ColorIcon", hudRoot.transform, new Vector2(80f, -80f), new Vector2(72f, 72f), new Color(1f, 0.85f, 0.2f, 1f));

            CreateUiText(
                "Instructions",
                hudRoot.transform,
                new Vector2(24f, -24f),
                new Vector2(420f, 96f),
                "WASD - move\n1/2/3 - switch color\nE - interact / carry cube\nR - restart level",
                20,
                TextAnchor.UpperLeft);
            hudRoot.transform.Find("Instructions").GetComponent<Text>().color = Color.black;

            GameObject checkpointNotice = CreateUiText(
                "CheckpointNotice",
                hudRoot.transform,
                new Vector2(-180f, -24f),
                new Vector2(360f, 40f),
                "Checkpoint reached",
                24,
                TextAnchor.MiddleCenter);
            Text checkpointNoticeText = checkpointNotice.GetComponent<Text>();
            checkpointNoticeText.color = new Color(0f, 0f, 0f, 0f);

            RectTransform checkpointRect = checkpointNotice.GetComponent<RectTransform>();
            checkpointRect.anchorMin = new Vector2(0.5f, 1f);
            checkpointRect.anchorMax = new Vector2(0.5f, 1f);
            checkpointRect.pivot = new Vector2(0.5f, 1f);
            checkpointRect.anchoredPosition = new Vector2(0f, -24f);

            GameObject victoryPanel = CreateUiPanel("VictoryPanel", canvasObject.transform, new Color(0f, 0f, 0f, 0.78f));
            victoryPanel.SetActive(false);
            CreateUiText(
                "VictoryText",
                victoryPanel.transform,
                Vector2.zero,
                new Vector2(580f, 180f),
                "Level Complete\nPress R to restart",
                34,
                TextAnchor.MiddleCenter);

            RectTransform victoryTextRect = victoryPanel.transform.Find("VictoryText").GetComponent<RectTransform>();
            victoryTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            victoryTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            victoryTextRect.pivot = new Vector2(0.5f, 0.5f);
            victoryTextRect.anchoredPosition = Vector2.zero;

            AstronautHUD hud = hudRoot.AddComponent<AstronautHUD>();
            CheckpointNotificationHUD checkpointNotificationHud = hudRoot.AddComponent<CheckpointNotificationHUD>();
            AssignField(hud, "targetEnergy", playerEnergy);
            AssignField(hud, "colorIcon", colorIcon);
            AssignField(hud, "glowFrame", glowFrame);
            AssignField(checkpointNotificationHud, "notificationText", checkpointNoticeText);
            AssignField(gameState, "victoryRoot", victoryPanel);
        }

        private static void CreateGeometry(Transform parent)
        {
            CreateColoredCubePrimitive("StartPlatform", parent, new Vector3(0f, 0f, 0f), new Vector3(12f, 1f, 10f), new Color(0.38f, 0.68f, 0.42f));
            CreateColoredCubePrimitive("MidPlatform", parent, new Vector3(18f, 0f, 0f), new Vector3(16f, 1f, 10f), new Color(0.38f, 0.68f, 0.42f));
            CreateColoredCubePrimitive("FinalPlatform", parent, new Vector3(34f, 0f, 0f), new Vector3(16f, 1f, 10f), new Color(0.38f, 0.68f, 0.42f));
            CreateColoredCubePrimitive("LeftWall", parent, new Vector3(17f, 1.5f, -5.5f), new Vector3(52f, 3f, 1f), new Color(0.27f, 0.48f, 0.29f));
            CreateColoredCubePrimitive("RightWall", parent, new Vector3(17f, 1.5f, 5.5f), new Vector3(52f, 3f, 1f), new Color(0.27f, 0.48f, 0.29f));
        }

        private static void CreateBlueDoor(Transform parent)
        {
            GameObject doorRoot = CreateEmpty("StartDoorRoot", parent, new Vector3(4f, 0f, 0f));
            BoxCollider trigger = doorRoot.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 2f, 0f);
            trigger.size = new Vector3(2f, 4f, 4f);

            GameObject doorVisual = CreateColoredCubePrimitive("DoorVisual", doorRoot.transform, new Vector3(0f, 2f, 0f), new Vector3(1f, 4f, 4f), EnergyColorPalette.ToColor(EnergyColor.Blue));
            ColorDoor colorDoor = doorRoot.AddComponent<ColorDoor>();

            AssignField(colorDoor, "requiredColor", EnergyColor.Blue);
            AssignField(colorDoor, "doorVisual", doorVisual.transform);
            AssignField(colorDoor, "accentRenderers", new[] { doorVisual.GetComponent<Renderer>() });
        }

        private static BridgeMechanism CreateBridge(Transform parent)
        {
            GameObject bridgeController = CreateEmpty("BridgeController", parent, Vector3.zero);
            BridgeMechanism bridge = bridgeController.AddComponent<BridgeMechanism>();

            GameObject bridgeVisual = CreateColoredCubePrimitive("BridgeVisual", bridgeController.transform, new Vector3(8f, 0f, 0f), new Vector3(4f, 1f, 4f), new Color(0.4f, 0.65f, 0.45f));
            bridgeVisual.SetActive(false);

            AssignField(bridge, "bridgeRoot", bridgeVisual);
            AssignField(bridge, "startPowered", false);
            return bridge;
        }

        private static void CreateColorPanel(string panelName, Transform parent, Vector3 position, EnergyColor requiredColor, MechanismReceiver target, Color previewColor)
        {
            GameObject panel = CreateColoredCubePrimitive(panelName, parent, position, new Vector3(0.3f, 0.8f, 0.8f), previewColor);
            Collider panelCollider = panel.GetComponent<Collider>();
            if (panelCollider != null)
            {
                panelCollider.isTrigger = true;
            }

            ColorPanel colorPanel = panel.AddComponent<ColorPanel>();

            AssignField(colorPanel, "requiredColor", requiredColor);
            AssignField(colorPanel, "panelRenderers", new[] { panel.GetComponent<Renderer>() });
            AssignField(colorPanel, "targets", new[] { target });
            AssignField(colorPanel, "activateOnTouch", true);
            AssignField(colorPanel, "toggleMode", true);
            AssignField(colorPanel, "singleUse", true);
        }

        private static LaserBarrier CreateLaserBarrier(Transform parent)
        {
            GameObject laserRoot = CreateEmpty("LaserBarrierRoot", parent, Vector3.zero);
            Rigidbody rigidbody = laserRoot.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            LaserBarrier laserBarrier = laserRoot.AddComponent<LaserBarrier>();

            GameObject laserBeam = CreateColoredCubePrimitive("LaserBeam", laserRoot.transform, new Vector3(15.5f, 1.5f, 0f), new Vector3(0.2f, 3f, 6f), new Color(1f, 0.25f, 0.25f));
            UnityObject.Destroy(laserBeam.GetComponent<Collider>());

            GameObject laserTrigger = CreateEmpty("LaserTrigger", laserRoot.transform, new Vector3(15.5f, 1.5f, 0f));
            BoxCollider trigger = laserTrigger.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1f, 3f, 6f);

            AssignField(laserBarrier, "visualRoots", new[] { laserBeam });
            AssignField(laserBarrier, "damageColliders", new Collider[] { trigger });
            AssignField(laserBarrier, "invertSignal", true);
            AssignField(laserBarrier, "startPowered", false);
            return laserBarrier;
        }

        private static void CreateWaveZone(Transform parent)
        {
            GameObject waveZone = CreateEmpty("WaveZone", parent, new Vector3(21f, 1f, 0f));
            BoxCollider boxCollider = waveZone.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(8f, 3f, 8f);

            GameObject waveVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            waveVisual.name = "WaveVisual";
            waveVisual.transform.SetParent(waveZone.transform, false);
            waveVisual.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            waveVisual.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
            UnityObject.Destroy(waveVisual.GetComponent<Collider>());
            ApplyRendererColor(waveVisual.GetComponent<Renderer>(), EnergyColorPalette.ToColor(EnergyColor.Purple), 2f);

            WaveHazardZone waveHazardZone = waveZone.AddComponent<WaveHazardZone>();
            AssignField(waveHazardZone, "waveVisual", waveVisual.transform);
            AssignField(waveHazardZone, "waveRenderers", new[] { waveVisual.GetComponent<Renderer>() });
            AssignField(waveHazardZone, "startupDelay", 1f);
            AssignField(waveHazardZone, "timeBetweenWaves", 2.5f);
            AssignField(waveHazardZone, "waveTravelTime", 1.2f);

            waveVisual.SetActive(false);
        }

        private static void CreateCheckpoint(Transform parent)
        {
            GameObject checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            checkpoint.name = "Checkpoint01";
            checkpoint.transform.SetParent(parent, false);
            checkpoint.transform.localPosition = new Vector3(25f, 0.5f, -3f);

            Collider collider = checkpoint.GetComponent<Collider>();
            collider.isTrigger = true;

            Checkpoint checkpointComponent = checkpoint.AddComponent<Checkpoint>();
            AssignField(checkpointComponent, "checkpointRenderers", new[] { checkpoint.GetComponent<Renderer>() });
        }

        private static void CreatePurpleCube(Transform parent)
        {
            GameObject cube = CreateColoredCubePrimitive("PurpleCube", parent, new Vector3(24f, 1f, 2f), new Vector3(1f, 1f, 1f), EnergyColorPalette.ToColor(EnergyColor.Purple));
            Rigidbody rigidbody = cube.AddComponent<Rigidbody>();
            rigidbody.mass = 1.5f;

            ColoredCube coloredCube = cube.AddComponent<ColoredCube>();
            cube.AddComponent<ResettableTransform>();

            AssignField(coloredCube, "cubeColor", EnergyColor.Purple);
            AssignField(coloredCube, "cubeRenderers", new[] { cube.GetComponent<Renderer>() });
        }

        private static void CreatePedestal(Transform parent, PassageMechanism finalBlock)
        {
            GameObject pedestal = CreateColoredCubePrimitive("PurplePedestal", parent, new Vector3(27f, 0.5f, 2f), new Vector3(1.5f, 1f, 1.5f), EnergyColorPalette.ToColor(EnergyColor.Purple));
            BoxCollider boxCollider = pedestal.GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.center = new Vector3(0f, 0.4f, 0f);
            boxCollider.size = new Vector3(1.6f, 1.8f, 1.6f);

            GameObject snapPoint = CreateEmpty("SnapPoint", pedestal.transform, new Vector3(0f, 1f, 0f));
            PedestalSocket pedestalSocket = pedestal.AddComponent<PedestalSocket>();

            AssignField(pedestalSocket, "requiredColor", EnergyColor.Purple);
            AssignField(pedestalSocket, "pedestalRenderers", new[] { pedestal.GetComponent<Renderer>() });
            AssignField(pedestalSocket, "targets", new MechanismReceiver[] { finalBlock });
            AssignField(pedestalSocket, "snapPoint", snapPoint.transform);
            AssignField(pedestalSocket, "snapCubeOnActivate", true);
        }

        private static PassageMechanism CreateFinalBlock(Transform parent)
        {
            GameObject controller = CreateEmpty("FinalBlockController", parent, Vector3.zero);
            PassageMechanism passageMechanism = controller.AddComponent<PassageMechanism>();

            GameObject finalBlock = CreateColoredCubePrimitive("FinalBlock", controller.transform, new Vector3(29f, 1.5f, 0f), new Vector3(1f, 3f, 6f), new Color(0.48f, 0.2f, 0.74f));

            AssignField(passageMechanism, "passageRoot", finalBlock);
            AssignField(passageMechanism, "invertSignal", true);
            AssignField(passageMechanism, "startPowered", false);
            return passageMechanism;
        }

        private static void CreateGoal(Transform parent, GameStateController gameState)
        {
            GameObject goal = CreateEmpty("Goal", parent, new Vector3(39f, 1f, 0f));
            BoxCollider goalCollider = goal.AddComponent<BoxCollider>();
            goalCollider.isTrigger = true;
            goalCollider.size = new Vector3(2f, 2f, 4f);

            LevelGoal levelGoal = goal.AddComponent<LevelGoal>();
            AssignField(levelGoal, "gameStateController", gameState);

            GameObject goalMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goalMarker.name = "GoalMarker";
            goalMarker.transform.SetParent(goal.transform, false);
            goalMarker.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            goalMarker.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
            UnityObject.Destroy(goalMarker.GetComponent<Collider>());
            ApplyRendererColor(goalMarker.GetComponent<Renderer>(), new Color(0.3f, 1f, 0.6f), 2f);
        }

        private static void EnsureDirectionalLight(Transform parent)
        {
            if (UnityObject.FindFirstObjectByType<Light>() != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }

        private static GameObject CreateEmpty(string name, Transform parent, Vector3 localPosition)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }

        private static GameObject CreateColoredCubePrimitive(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = name;
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = localScale;
            ApplyRendererColor(gameObject.GetComponent<Renderer>(), color, 0.2f);
            return gameObject;
        }

        private static void ApplyRendererColor(Renderer renderer, Color color, float emissionIntensity)
        {
            if (renderer == null)
            {
                return;
            }

            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                }
                else if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", color);
                }

                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", color * emissionIntensity);
                }
            }
        }

        private static Image CreateUiImage(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static GameObject CreateUiText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string value, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return textObject;
        }

        private static GameObject CreateUiPanel(string name, Transform parent, Color color)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            return panelObject;
        }

        private static void AssignField(object target, string fieldName, object value)
        {
            FieldInfo fieldInfo = FindField(target.GetType(), fieldName);
            if (fieldInfo == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            fieldInfo.SetValue(target, value);
        }

        private static FieldInfo FindField(Type targetType, string fieldName)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            while (targetType != null)
            {
                FieldInfo fieldInfo = targetType.GetField(fieldName, Flags);
                if (fieldInfo != null)
                {
                    return fieldInfo;
                }

                targetType = targetType.BaseType;
            }

            return null;
        }
    }
}

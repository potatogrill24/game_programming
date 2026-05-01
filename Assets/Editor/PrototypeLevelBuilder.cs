using System.Reflection;
using GameProgramming.CameraSystem;
using GameProgramming.Core;
using GameProgramming.Game;
using GameProgramming.Player;
using GameProgramming.World.Cubes;
using GameProgramming.World.Doors;
using GameProgramming.World.Hazards;
using GameProgramming.World.Mechanisms;
using GameProgramming.World.Panels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameProgramming.EditorTools
{
    public static class PrototypeLevelBuilder
    {
        private const string RootName = "PrototypeLevel_Auto";

        [MenuItem("Tools/Game Programming/Create Demo Level")]
        public static void CreateDemoLevel()
        {
            if (!PrepareSceneForBuild(promptBeforeReplace: true))
            {
                return;
            }

            BuildDemoLevel();
        }

        public static bool TryCreateDemoLevelIfMissing()
        {
            if (HasDemoLevel() || HasSceneGameplay())
            {
                return false;
            }

            if (!PrepareSceneForBuild(promptBeforeReplace: false))
            {
                return false;
            }

            BuildDemoLevel();
            return true;
        }

        public static bool HasDemoLevel()
        {
            return GameObject.Find(RootName) != null;
        }

        private static bool HasSceneGameplay()
        {
            return UnityEngine.Object.FindFirstObjectByType<AstronautController>() != null ||
                   UnityEngine.Object.FindFirstObjectByType<GameStateController>() != null;
        }

        private static bool PrepareSceneForBuild(bool promptBeforeReplace)
        {
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot == null)
            {
                return true;
            }

            if (promptBeforeReplace)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Replace Existing Level",
                    "A generated demo level already exists in this scene. Replace it?",
                    "Replace",
                    "Cancel");

                if (!replace)
                {
                    return false;
                }
            }

            Undo.DestroyObjectImmediate(existingRoot);
            return true;
        }

        private static void BuildDemoLevel()
        {
            GameObject root = CreateEmpty(RootName, null, Vector3.zero);
            try
            {
                GameObject systemsRoot = CreateEmpty("Systems", root.transform, Vector3.zero);
                GameObject geometryRoot = CreateEmpty("Geometry", root.transform, Vector3.zero);
                GameObject mechanicsRoot = CreateEmpty("Mechanics", root.transform, Vector3.zero);

                DestroyAllEventSystems();
                GameStateController gameState = CreateGameState(systemsRoot.transform);
                CreateRespawnZone(systemsRoot.transform);
                EnsureDirectionalLight(root.transform);

                AstronautEnergy playerEnergy = CreatePlayer(root.transform);
                Camera camera = CreateOrConfigureCamera(root.transform, playerEnergy.transform);
                CreateOrConfigureUi(root.transform, playerEnergy, gameState);

                ApplySerializedField(playerEnergy.GetComponent<AstronautController>(), "cameraReference", camera.transform);

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

                Selection.activeGameObject = root;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

                Debug.Log("Demo level created. Press Play to test the full mechanic flow.");
            }
            catch
            {
                if (root != null)
                {
                    Undo.DestroyObjectImmediate(root);
                }

                throw;
            }
        }

        private static void CreateGeometry(Transform parent)
        {
            CreateColoredCubePrimitive("StartPlatform", parent, new Vector3(0f, 0f, 0f), new Vector3(12f, 1f, 10f), new Color(0.38f, 0.68f, 0.42f));
            CreateColoredCubePrimitive("MidPlatform", parent, new Vector3(18f, 0f, 0f), new Vector3(16f, 1f, 10f), new Color(0.38f, 0.68f, 0.42f));
            CreateColoredCubePrimitive("FinalPlatform", parent, new Vector3(34f, 0f, 0f), new Vector3(16f, 1f, 10f), new Color(0.38f, 0.68f, 0.42f));

            CreateColoredCubePrimitive("LeftWall", parent, new Vector3(17f, 1.5f, -5.5f), new Vector3(52f, 3f, 1f), new Color(0.27f, 0.48f, 0.29f));
            CreateColoredCubePrimitive("RightWall", parent, new Vector3(17f, 1.5f, 5.5f), new Vector3(52f, 3f, 1f), new Color(0.27f, 0.48f, 0.29f));
        }

        private static GameStateController CreateGameState(Transform parent)
        {
            GameObject gameStateObject = CreateEmpty("GameState", parent, Vector3.zero);
            GameStateController controller = Undo.AddComponent<GameStateController>(gameStateObject);
            return controller;
        }

        private static void CreateRespawnZone(Transform parent)
        {
            GameObject respawnZone = CreateEmpty("RespawnZone", parent, new Vector3(18f, -8f, 0f));
            BoxCollider collider = Undo.AddComponent<BoxCollider>(respawnZone);
            collider.isTrigger = true;
            collider.size = new Vector3(80f, 1f, 40f);
            Undo.AddComponent<RespawnZone>(respawnZone);
        }

        private static AstronautEnergy CreatePlayer(Transform parent)
        {
            GameObject player = CreateEmpty("Player", parent, new Vector3(-3f, 1f, 0f));

            CharacterController characterController = Undo.AddComponent<CharacterController>(player);
            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.height = 2f;
            characterController.radius = 0.5f;

            AstronautController astronautController = Undo.AddComponent<AstronautController>(player);
            AstronautEnergy astronautEnergy = Undo.AddComponent<AstronautEnergy>(player);
            AstronautInteractor astronautInteractor = Undo.AddComponent<AstronautInteractor>(player);
            Undo.AddComponent<PlayerRespawn>(player);

            GameObject graphics = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Undo.RegisterCreatedObjectUndo(graphics, "Create Player Graphics");
            graphics.name = "Graphics";
            graphics.transform.SetParent(player.transform, false);
            graphics.transform.localPosition = new Vector3(0f, 1f, 0f);
            graphics.transform.localScale = new Vector3(1f, 1f, 1f);
            UnityEngine.Object.DestroyImmediate(graphics.GetComponent<Collider>());
            ApplyRendererColor(graphics.GetComponent<Renderer>(), new Color(0.92f, 0.92f, 0.92f), 0.15f);

            GameObject carryAnchor = CreateEmpty("CarryAnchor", player.transform, new Vector3(0f, 1.2f, 1f));

            ApplySerializedField(astronautEnergy, "suitRenderers", new[] { graphics.GetComponent<Renderer>() });
            ApplySerializedField(astronautInteractor, "carryAnchor", carryAnchor.transform);
            ApplySerializedField(astronautInteractor, "interactionOrigin", carryAnchor.transform);
            ApplySerializedField(astronautInteractor, "carriedCubeEulerOffset", new Vector3(0f, 0f, 0f));

            astronautEnergy.RefreshVisuals();
            EditorUtility.SetDirty(astronautController);
            EditorUtility.SetDirty(astronautEnergy);
            EditorUtility.SetDirty(astronautInteractor);

            return astronautEnergy;
        }

        private static Camera CreateOrConfigureCamera(Transform parent, Transform target)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }

            GameObject cameraObject;
            if (camera == null)
            {
                cameraObject = new GameObject("Main Camera");
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Main Camera");
                camera = Undo.AddComponent<Camera>(cameraObject);
                camera.tag = "MainCamera";
                Undo.AddComponent<AudioListener>(cameraObject);
            }
            else
            {
                cameraObject = camera.gameObject;
                Undo.RecordObject(cameraObject.transform, "Configure Main Camera");
                if (!camera.CompareTag("MainCamera"))
                {
                    camera.tag = "MainCamera";
                }
            }

            cameraObject.transform.SetParent(parent, true);
            cameraObject.transform.position = new Vector3(-8f, 7f, -8f);
            cameraObject.transform.rotation = Quaternion.Euler(28f, 35f, 0f);

            SimpleCameraFollow follow = cameraObject.GetComponent<SimpleCameraFollow>();
            if (follow == null)
            {
                follow = Undo.AddComponent<SimpleCameraFollow>(cameraObject);
            }

            ApplySerializedField(follow, "target", target);
            ApplySerializedField(follow, "offset", new Vector3(-2.5f, 6f, -7f));
            ApplySerializedField(follow, "useTargetSpaceOffset", false);

            EditorUtility.SetDirty(cameraObject);
            return camera;
        }

        private static void CreateOrConfigureUi(Transform parent, AstronautEnergy playerEnergy, GameStateController gameState)
        {
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            GameObject canvasObject;

            if (canvas == null)
            {
                canvasObject = new GameObject("Canvas");
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");
                canvas = Undo.AddComponent<Canvas>(canvasObject);
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Undo.AddComponent<CanvasScaler>(canvasObject);
                Undo.AddComponent<GraphicRaycaster>(canvasObject);
            }
            else
            {
                canvasObject = canvas.gameObject;
            }

            canvasObject.transform.SetParent(parent, true);

            GameObject hudRoot = new GameObject("HUD", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(hudRoot, "Create HUD");
            hudRoot.transform.SetParent(canvasObject.transform, false);

            RectTransform hudRootRect = hudRoot.GetComponent<RectTransform>();
            hudRootRect.anchorMin = Vector2.zero;
            hudRootRect.anchorMax = Vector2.one;
            hudRootRect.offsetMin = Vector2.zero;
            hudRootRect.offsetMax = Vector2.zero;

            Image glowFrame = CreateUiImage("GlowFrame", hudRoot.transform, new Vector2(80f, -80f), new Vector2(110f, 110f), new Color(1f, 0.9f, 0.35f, 0.35f));
            Image colorIcon = CreateUiImage("ColorIcon", hudRoot.transform, new Vector2(80f, -80f), new Vector2(72f, 72f), new Color(1f, 0.85f, 0.2f, 1f));

            GameObject instructions = CreateUiText(
                "Instructions",
                hudRoot.transform,
                new Vector2(24f, -24f),
                new Vector2(420f, 96f),
                "WASD - move\n1/2/3 - switch color\nE - interact / carry cube\nR - restart level",
                20,
                TextAnchor.UpperLeft);
            instructions.GetComponent<Text>().color = Color.black;

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

            AstronautHUD hud = hudRoot.GetComponent<AstronautHUD>();
            if (hud == null)
            {
                hud = Undo.AddComponent<AstronautHUD>(hudRoot);
            }

            CheckpointNotificationHUD checkpointNotificationHud = hudRoot.GetComponent<CheckpointNotificationHUD>();
            if (checkpointNotificationHud == null)
            {
                checkpointNotificationHud = Undo.AddComponent<CheckpointNotificationHUD>(hudRoot);
            }

            ApplySerializedField(hud, "targetEnergy", playerEnergy);
            ApplySerializedField(hud, "colorIcon", colorIcon);
            ApplySerializedField(hud, "glowFrame", glowFrame);
            ApplySerializedField(checkpointNotificationHud, "notificationText", checkpointNoticeText);
            ApplySerializedField(gameState, "victoryRoot", victoryPanel);

            EditorUtility.SetDirty(hudRoot);
            EditorUtility.SetDirty(gameState);
            EditorUtility.SetDirty(instructions);
        }

        private static void CreateBlueDoor(Transform parent)
        {
            GameObject doorRoot = CreateEmpty("StartDoorRoot", parent, new Vector3(4f, 0f, 0f));
            BoxCollider trigger = Undo.AddComponent<BoxCollider>(doorRoot);
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 2f, 0f);
            trigger.size = new Vector3(2f, 4f, 4f);

            ColorDoor colorDoor = Undo.AddComponent<ColorDoor>(doorRoot);

            GameObject doorVisual = CreateColoredCubePrimitive("DoorVisual", doorRoot.transform, new Vector3(0f, 2f, 0f), new Vector3(1f, 4f, 4f), EnergyColorPalette.ToColor(EnergyColor.Blue));

            ApplySerializedField(colorDoor, "requiredColor", EnergyColor.Blue);
            ApplySerializedField(colorDoor, "doorVisual", doorVisual.transform);
            ApplySerializedField(colorDoor, "accentRenderers", new[] { doorVisual.GetComponent<Renderer>() });

            EditorUtility.SetDirty(colorDoor);
        }

        private static BridgeMechanism CreateBridge(Transform parent)
        {
            GameObject bridgeController = CreateEmpty("BridgeController", parent, Vector3.zero);
            BridgeMechanism bridge = Undo.AddComponent<BridgeMechanism>(bridgeController);

            GameObject bridgeVisual = CreateColoredCubePrimitive("BridgeVisual", bridgeController.transform, new Vector3(8f, 0f, 0f), new Vector3(4f, 1f, 4f), new Color(0.4f, 0.65f, 0.45f));
            bridgeVisual.SetActive(false);

            ApplySerializedField(bridge, "bridgeRoot", bridgeVisual);
            ApplySerializedField(bridge, "startPowered", false);

            EditorUtility.SetDirty(bridge);
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

            ColorPanel colorPanel = Undo.AddComponent<ColorPanel>(panel);

            ApplySerializedField(colorPanel, "requiredColor", requiredColor);
            ApplySerializedField(colorPanel, "panelRenderers", new[] { panel.GetComponent<Renderer>() });
            ApplySerializedField(colorPanel, "targets", new[] { target });
            ApplySerializedField(colorPanel, "activateOnTouch", true);
            ApplySerializedField(colorPanel, "toggleMode", true);
            ApplySerializedField(colorPanel, "singleUse", true);

            EditorUtility.SetDirty(colorPanel);
        }

        private static LaserBarrier CreateLaserBarrier(Transform parent)
        {
            GameObject laserRoot = CreateEmpty("LaserBarrierRoot", parent, Vector3.zero);
            Rigidbody rigidbody = Undo.AddComponent<Rigidbody>(laserRoot);
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            LaserBarrier laserBarrier = Undo.AddComponent<LaserBarrier>(laserRoot);

            GameObject laserBeam = CreateColoredCubePrimitive("LaserBeam", laserRoot.transform, new Vector3(15.5f, 1.5f, 0f), new Vector3(0.2f, 3f, 6f), new Color(1f, 0.25f, 0.25f));
            UnityEngine.Object.DestroyImmediate(laserBeam.GetComponent<Collider>());

            GameObject laserTrigger = CreateEmpty("LaserTrigger", laserRoot.transform, new Vector3(15.5f, 1.5f, 0f));
            BoxCollider trigger = Undo.AddComponent<BoxCollider>(laserTrigger);
            trigger.isTrigger = true;
            trigger.size = new Vector3(1f, 3f, 6f);

            ApplySerializedField(laserBarrier, "visualRoots", new[] { laserBeam });
            ApplySerializedField(laserBarrier, "damageColliders", new Collider[] { trigger });
            ApplySerializedField(laserBarrier, "invertSignal", true);
            ApplySerializedField(laserBarrier, "startPowered", false);

            EditorUtility.SetDirty(laserBarrier);
            return laserBarrier;
        }

        private static void CreateWaveZone(Transform parent)
        {
            GameObject waveZone = CreateEmpty("WaveZone", parent, new Vector3(21f, 1f, 0f));
            BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(waveZone);
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(8f, 3f, 8f);

            WaveHazardZone waveHazardZone = Undo.AddComponent<WaveHazardZone>(waveZone);

            GameObject waveVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(waveVisual, "Create Wave Visual");
            waveVisual.name = "WaveVisual";
            waveVisual.transform.SetParent(waveZone.transform, false);
            waveVisual.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            waveVisual.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
            UnityEngine.Object.DestroyImmediate(waveVisual.GetComponent<Collider>());
            ApplyRendererColor(waveVisual.GetComponent<Renderer>(), EnergyColorPalette.ToColor(EnergyColor.Purple), 2f);

            ApplySerializedField(waveHazardZone, "waveVisual", waveVisual.transform);
            ApplySerializedField(waveHazardZone, "waveRenderers", new[] { waveVisual.GetComponent<Renderer>() });
            ApplySerializedField(waveHazardZone, "startupDelay", 1f);
            ApplySerializedField(waveHazardZone, "timeBetweenWaves", 2.5f);
            ApplySerializedField(waveHazardZone, "waveTravelTime", 1.2f);

            waveVisual.SetActive(false);
            EditorUtility.SetDirty(waveHazardZone);
        }

        private static void CreateCheckpoint(Transform parent)
        {
            GameObject checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(checkpoint, "Create Checkpoint");
            checkpoint.name = "Checkpoint01";
            checkpoint.transform.SetParent(parent, false);
            checkpoint.transform.localPosition = new Vector3(25f, 0.5f, -3f);
            checkpoint.transform.localScale = new Vector3(1f, 1f, 1f);

            Collider collider = checkpoint.GetComponent<Collider>();
            collider.isTrigger = true;

            Checkpoint checkpointComponent = Undo.AddComponent<Checkpoint>(checkpoint);
            ApplySerializedField(checkpointComponent, "checkpointRenderers", new[] { checkpoint.GetComponent<Renderer>() });

            EditorUtility.SetDirty(checkpointComponent);
        }

        private static ColoredCube CreatePurpleCube(Transform parent)
        {
            GameObject cube = CreateColoredCubePrimitive("PurpleCube", parent, new Vector3(24f, 1f, 2f), new Vector3(1f, 1f, 1f), EnergyColorPalette.ToColor(EnergyColor.Purple));
            Rigidbody rigidbody = Undo.AddComponent<Rigidbody>(cube);
            rigidbody.mass = 1.5f;

            ColoredCube coloredCube = Undo.AddComponent<ColoredCube>(cube);
            Undo.AddComponent<ResettableTransform>(cube);

            ApplySerializedField(coloredCube, "cubeColor", EnergyColor.Purple);
            ApplySerializedField(coloredCube, "cubeRenderers", new[] { cube.GetComponent<Renderer>() });

            EditorUtility.SetDirty(coloredCube);
            return coloredCube;
        }

        private static void CreatePedestal(Transform parent, PassageMechanism finalBlock)
        {
            GameObject pedestal = CreateColoredCubePrimitive("PurplePedestal", parent, new Vector3(27f, 0.5f, 2f), new Vector3(1.5f, 1f, 1.5f), EnergyColorPalette.ToColor(EnergyColor.Purple));
            BoxCollider boxCollider = pedestal.GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.center = new Vector3(0f, 0.4f, 0f);
            boxCollider.size = new Vector3(1.6f, 1.8f, 1.6f);

            GameObject snapPoint = CreateEmpty("SnapPoint", pedestal.transform, new Vector3(0f, 1f, 0f));
            PedestalSocket pedestalSocket = Undo.AddComponent<PedestalSocket>(pedestal);

            ApplySerializedField(pedestalSocket, "requiredColor", EnergyColor.Purple);
            ApplySerializedField(pedestalSocket, "pedestalRenderers", new[] { pedestal.GetComponent<Renderer>() });
            ApplySerializedField(pedestalSocket, "targets", new MechanismReceiver[] { finalBlock });
            ApplySerializedField(pedestalSocket, "snapPoint", snapPoint.transform);
            ApplySerializedField(pedestalSocket, "snapCubeOnActivate", true);

            EditorUtility.SetDirty(pedestalSocket);
        }

        private static PassageMechanism CreateFinalBlock(Transform parent)
        {
            GameObject controller = CreateEmpty("FinalBlockController", parent, Vector3.zero);
            PassageMechanism passageMechanism = Undo.AddComponent<PassageMechanism>(controller);

            GameObject finalBlock = CreateColoredCubePrimitive("FinalBlock", controller.transform, new Vector3(29f, 1.5f, 0f), new Vector3(1f, 3f, 6f), new Color(0.48f, 0.2f, 0.74f));

            ApplySerializedField(passageMechanism, "passageRoot", finalBlock);
            ApplySerializedField(passageMechanism, "invertSignal", true);
            ApplySerializedField(passageMechanism, "startPowered", false);

            EditorUtility.SetDirty(passageMechanism);
            return passageMechanism;
        }

        private static void CreateGoal(Transform parent, GameStateController gameState)
        {
            GameObject goal = CreateEmpty("Goal", parent, new Vector3(39f, 1f, 0f));
            BoxCollider goalCollider = Undo.AddComponent<BoxCollider>(goal);
            goalCollider.isTrigger = true;
            goalCollider.size = new Vector3(2f, 2f, 4f);

            LevelGoal levelGoal = Undo.AddComponent<LevelGoal>(goal);
            ApplySerializedField(levelGoal, "gameStateController", gameState);

            GameObject goalMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(goalMarker, "Create Goal Marker");
            goalMarker.name = "GoalMarker";
            goalMarker.transform.SetParent(goal.transform, false);
            goalMarker.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            goalMarker.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
            UnityEngine.Object.DestroyImmediate(goalMarker.GetComponent<Collider>());
            ApplyRendererColor(goalMarker.GetComponent<Renderer>(), new Color(0.3f, 1f, 0.6f), 2f);

            EditorUtility.SetDirty(levelGoal);
        }

        private static GameObject CreateEmpty(string name, Transform parent, Vector3 localPosition)
        {
            GameObject gameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }

        private static GameObject CreateColoredCubePrimitive(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
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

            Material[] materials = Application.isPlaying ? renderer.materials : GetWritableEditorMaterials(renderer);

            foreach (Material material in materials)
            {
                if (material == null)
                {
                    continue;
                }

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

        private static Material[] GetWritableEditorMaterials(Renderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            bool clonedAny = false;

            for (int index = 0; index < materials.Length; index++)
            {
                Material material = materials[index];
                if (material == null || !EditorUtility.IsPersistent(material))
                {
                    continue;
                }

                materials[index] = new Material(material)
                {
                    name = material.name + " (Scene)"
                };
                clonedAny = true;
            }

            if (clonedAny)
            {
                renderer.sharedMaterials = materials;
            }

            return renderer.sharedMaterials;
        }

        private static void DestroyAllEventSystems()
        {
            var eventSystems = UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
            foreach (var eventSystem in eventSystems)
            {
                if (eventSystem != null)
                {
                    Undo.DestroyObjectImmediate(eventSystem.gameObject);
                }
            }
        }

        private static void EnsureDirectionalLight(Transform parent)
        {
            Light existingLight = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (existingLight != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Directional Light");
            Undo.RegisterCreatedObjectUndo(lightObject, "Create Directional Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light light = Undo.AddComponent<Light>(lightObject);
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }

        private static Image CreateUiImage(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(imageObject, "Create " + name);
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
            Undo.RegisterCreatedObjectUndo(textObject, "Create " + name);
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
            Undo.RegisterCreatedObjectUndo(panelObject, "Create " + name);
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

        private static void ApplySerializedField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                return;
            }

            FieldInfo fieldInfo = FindField(target.GetType(), fieldName);
            if (fieldInfo == null)
            {
                throw new System.MissingFieldException(target.GetType().Name, fieldName);
            }

            fieldInfo.SetValue(target, value);

            if (target is UnityEngine.Object unityObject)
            {
                EditorUtility.SetDirty(unityObject);
            }
        }

        private static FieldInfo FindField(System.Type targetType, string fieldName)
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

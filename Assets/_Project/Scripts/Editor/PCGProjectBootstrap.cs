using System;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using Platformer;
using Platformer.PCG;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PCGProjectBootstrap {
    const string Root = "Assets/_Project/PCG";
    const string PrefabFolder = Root + "/Prefabs";
    const string DataFolder = Root + "/Data";
    const string ConfigPath = Root + "/LevelGenerationConfig.asset";
    const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player 2.prefab";
    const string InputReaderPath = "Assets/_Project/ScriptableObjects/InputReader.asset";
    const string ScenePath = "Assets/_Project/Scenes/PCG_Lab.unity";
    const int BootstrapVersion = 8;
    const float LabCameraSensitivity = 2f;
    const float HorizontalLayoutScale = 1.25f;
    const float PlatformFootprintScale = 1.05f;
    const string TutorialGroundMaterialPath = "Assets/_Project/_Shaders/NoiseGround.mat";
    const string TutorialTimedMaterialPath = "Assets/_Project/Materials/TimedPlatform.mat";

    static string BootstrapVersionKey =>
        $"Platformer.PCG.Bootstrap.{Application.dataPath.GetHashCode()}";

    [InitializeOnLoadMethod]
    static void ScheduleFirstRun() {
        if (Application.isBatchMode) return;
        if (EditorPrefs.GetInt(BootstrapVersionKey, 0) >= BootstrapVersion &&
            AssetDatabase.LoadAssetAtPath<LevelGenerationConfig>(ConfigPath) != null &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return;

        EditorApplication.update -= TryRunFirstBatch;
        EditorApplication.update += TryRunFirstBatch;
    }

    static void TryRunFirstBatch() {
        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode) return;

        EditorApplication.update -= TryRunFirstBatch;
        CreateFirstBatch();
    }

    [MenuItem("Platformer/PCG/Create First Batch")]
    public static void CreateFirstBatch() {
        if (EditorApplication.isPlayingOrWillChangePlaymode) {
            Debug.LogWarning("PCG asset generation is only available in Edit Mode. Exit Play Mode and try again.");
            return;
        }

        EnsureFolder("Assets/_Project", "PCG");
        EnsureFolder(Root, "Prefabs");
        EnsureFolder(Root, "Data");

        var definitions = new[] {
            new ChunkDefinition("basic_01", ChunkCategory.Basic, AbilityRequirement.None, 0.12f, 0f, 0.1f,
                new[] { new PlatformSpec(new Vector3(0f, 0f, 2.5f), new Vector3(5f, 1f, 5f)) },
                new Vector3(0f, 0.5f, 5f), horizontalReach: 0f, verticalReach: 0f),
            new ChunkDefinition("rising_01", ChunkCategory.Basic, AbilityRequirement.None, 0.32f, 0f, 0.35f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 2.5f), new Vector3(5f, 1f, 5f)),
                    new PlatformSpec(new Vector3(0f, 1.2f, 7.5f), new Vector3(5f, 1f, 5f))
                }, new Vector3(0f, 1.7f, 10f), horizontalReach: 2.5f, verticalReach: 1.2f),
            new ChunkDefinition("turn_left_01", ChunkCategory.Exploration, AbilityRequirement.None, 0.3f, 0f, 0.35f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 2.5f), new Vector3(5f, 1f, 5f)),
                    new PlatformSpec(new Vector3(-2.5f, 0f, 5f), new Vector3(5f, 1f, 5f)),
                    new PlatformSpec(new Vector3(-5f, 0f, 7.5f), new Vector3(5f, 1f, 5f))
                }, new Vector3(-7.5f, 0.5f, 7.5f), exitYaw: -90f),
            new ChunkDefinition("turn_right_01", ChunkCategory.Exploration, AbilityRequirement.None, 0.3f, 0f, 0.35f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 2.5f), new Vector3(5f, 1f, 5f)),
                    new PlatformSpec(new Vector3(2.5f, 0f, 5f), new Vector3(5f, 1f, 5f)),
                    new PlatformSpec(new Vector3(5f, 0f, 7.5f), new Vector3(5f, 1f, 5f))
                }, new Vector3(7.5f, 0.5f, 7.5f), exitYaw: 90f),
            new ChunkDefinition("offset_left_01", ChunkCategory.Exploration, AbilityRequirement.None, 0.36f, 0f, 0.4f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 2f), new Vector3(4f, 1f, 4f)),
                    new PlatformSpec(new Vector3(-2f, 0.4f, 5.5f), new Vector3(3.5f, 0.7f, 3f)),
                    new PlatformSpec(new Vector3(-3.5f, 0f, 9f), new Vector3(4f, 1f, 4f))
                }, new Vector3(-3.5f, 0.5f, 11f), horizontalReach: 2.5f, verticalReach: 0.4f),
            new ChunkDefinition("climb_01", ChunkCategory.Basic, AbilityRequirement.None, 0.44f, 0f, 0.48f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 2f), new Vector3(4.5f, 1f, 4f)),
                    new PlatformSpec(new Vector3(0f, 1f, 5f), new Vector3(4f, 1f, 3f)),
                    new PlatformSpec(new Vector3(0f, 2f, 8f), new Vector3(4.5f, 1f, 4f))
                }, new Vector3(0f, 2.5f, 10f), horizontalReach: 2f, verticalReach: 1f),
            new ChunkDefinition("descend_01", ChunkCategory.Recovery, AbilityRequirement.None, 0.34f, 0f, 0.32f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 2f), new Vector3(4.5f, 1f, 4f)),
                    new PlatformSpec(new Vector3(0f, -1f, 5f), new Vector3(4f, 1f, 3f)),
                    new PlatformSpec(new Vector3(0f, -2f, 8f), new Vector3(4.5f, 1f, 4f))
                }, new Vector3(0f, -1.5f, 10f), horizontalReach: 2f),
            new ChunkDefinition("climb_turn_left_01", ChunkCategory.Exploration, AbilityRequirement.None, 0.52f, 0f, 0.55f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 2f), new Vector3(4f, 1f, 4f)),
                    new PlatformSpec(new Vector3(-2f, 1f, 4.5f), new Vector3(4f, 1f, 3.5f)),
                    new PlatformSpec(new Vector3(-4.5f, 2f, 6.5f), new Vector3(5f, 1f, 4f))
                }, new Vector3(-7f, 2.5f, 6.5f), horizontalReach: 2.5f, verticalReach: 1f, exitYaw: -90f),
            new ChunkDefinition("moving_01", ChunkCategory.Moving, AbilityRequirement.None, 0.42f, 0f, 0.45f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 1.5f), new Vector3(4f, 1f, 3f)),
                    new PlatformSpec(new Vector3(1.5f, 0.4f, 5f), new Vector3(3f, 0.7f, 2.5f)),
                    new PlatformSpec(new Vector3(0f, 0f, 8.5f), new Vector3(4f, 1f, 3f))
                }, new Vector3(0f, 0.5f, 10f), horizontalReach: 2.5f, verticalReach: 0.4f),
            new ChunkDefinition("timed_01", ChunkCategory.Timed, AbilityRequirement.None, 0.48f, 0f, 0.55f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 1.5f), new Vector3(4f, 1f, 3f)),
                    new PlatformSpec(new Vector3(0f, 0f, 5f), new Vector3(2.5f, 0.6f, 2.5f)),
                    new PlatformSpec(new Vector3(0f, 0f, 8.5f), new Vector3(4f, 1f, 3f))
                }, new Vector3(0f, 0.5f, 10f), horizontalReach: 2.25f, verticalReach: 0f),
            new ChunkDefinition("double_jump_01", ChunkCategory.AbilityGate, AbilityRequirement.DoubleJump, 0.68f, 0f, 0.75f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 1.5f), new Vector3(4f, 1f, 3f)),
                    new PlatformSpec(new Vector3(0f, 2.2f, 8.5f), new Vector3(4f, 1f, 3f))
                }, new Vector3(0f, 2.7f, 10f), 3, 5f, 2.2f),
            new ChunkDefinition("dash_gap_01", ChunkCategory.AbilityGate, AbilityRequirement.Dash, 0.78f, 0f, 0.72f,
                new[] {
                    new PlatformSpec(new Vector3(0f, 0f, 1.5f), new Vector3(4f, 1f, 3f)),
                    new PlatformSpec(new Vector3(0f, 0f, 10.5f), new Vector3(4f, 1f, 3f))
                }, new Vector3(0f, 0.5f, 12f), 5, 8f, 0f),
            new ChunkDefinition("combat_01", ChunkCategory.Combat, AbilityRequirement.None, 0.28f, 0.65f, 0.2f,
                new[] { new PlatformSpec(new Vector3(0f, 0f, 5f), new Vector3(10f, 1f, 10f)) },
                new Vector3(0f, 0.5f, 10f), horizontalReach: 0f, verticalReach: 0f),
            new ChunkDefinition("recovery_01", ChunkCategory.Recovery, AbilityRequirement.None, 0.05f, 0f, 0.05f,
                new[] { new PlatformSpec(new Vector3(0f, 0f, 3.5f), new Vector3(7f, 1f, 7f)) },
                new Vector3(0f, 0.5f, 7f), horizontalReach: 0f, verticalReach: 0f)
        };

        var groundMaterial = AssetDatabase.LoadAssetAtPath<Material>(TutorialGroundMaterialPath);
        var timedMaterial = AssetDatabase.LoadAssetAtPath<Material>(TutorialTimedMaterialPath);
        if (groundMaterial == null || timedMaterial == null)
            throw new InvalidOperationException("Tutorial platform materials are missing.");

        var dataAssets = new List<PlatformChunkData>();
        foreach (var definition in definitions)
            dataAssets.Add(CreateChunk(definition, groundMaterial, timedMaterial));

        var config = AssetDatabase.LoadAssetAtPath<LevelGenerationConfig>(ConfigPath);
        if (config == null) {
            config = ScriptableObject.CreateInstance<LevelGenerationConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }
        config.Configure(dataAssets.ToArray(), 16);
        EditorUtility.SetDirty(config);

        CreateLabScene(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorPrefs.SetInt(BootstrapVersionKey, BootstrapVersion);
        Debug.Log($"PCG first batch created. Open {ScenePath} and press Play.");
    }

    public static void CreateFirstBatchBatchMode() {
        CreateFirstBatch();
        EditorApplication.Exit(0);
    }

    static PlatformChunkData CreateChunk(
        ChunkDefinition definition,
        Material groundMaterial,
        Material timedMaterial) {
        var root = new GameObject(definition.Id);
        var chunk = root.AddComponent<PlatformChunk>();

        var entry = CreateSocket(root.transform, "Entry", new Vector3(0f, 0.5f, 0f));
        var scaledExit = ScaleHorizontal(definition.Exit, HorizontalLayoutScale);
        var exit = CreateSocket(root.transform, "Exit", scaledExit, definition.ExitYaw);

        for (var i = 0; i < definition.Platforms.Length; i++) {
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = $"Platform_{i:00}";
            platform.transform.SetParent(root.transform);
            platform.transform.localPosition =
                ScaleHorizontal(definition.Platforms[i].Position, HorizontalLayoutScale);
            platform.transform.localScale =
                ScaleFootprint(definition.Platforms[i].Scale, PlatformFootprintScale);
            platform.layer = LayerMask.NameToLayer("Ground");
            platform.GetComponent<Renderer>().sharedMaterial =
                definition.Category == ChunkCategory.Timed && i == 1
                    ? timedMaterial
                    : groundMaterial;

            if (definition.Category == ChunkCategory.Moving && i == 1) {
                platform.tag = "MovingPlatform";
                platform.AddComponent<PCGOscillatingPlatform>()
                    .Configure(new Vector3(-3f * HorizontalLayoutScale, 0f, 0f), 1.8f, 0.4f);
            }

            if (definition.Category == ChunkCategory.Timed && i == 1) {
                platform.AddComponent<PCGTimedPlatform>()
                    .Configure(2.5f, 0.8f, 1.4f, 0.4f);
            }
        }

        Transform[] enemySlots = Array.Empty<Transform>();
        if (definition.Category == ChunkCategory.Combat) {
            enemySlots = new[] {
                CreateSocket(root.transform, "EnemySlot_A", new Vector3(-2.5f, 1f, 5f)),
                CreateSocket(root.transform, "EnemySlot_B", new Vector3(2.5f, 1f, 6f))
            };
        }

        Transform[] collectibleSlots = definition.Category == ChunkCategory.Recovery
            ? new[] { CreateSocket(root.transform, "CollectibleSlot", new Vector3(0f, 1f, 3.5f)) }
            : Array.Empty<Transform>();

        chunk.Configure(entry, new[] { exit }, enemySlots, collectibleSlots);

        var prefabPath = $"{PrefabFolder}/{definition.Id}.prefab";
        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        UnityEngine.Object.DestroyImmediate(root);

        var dataPath = $"{DataFolder}/{definition.Id}.asset";
        var data = AssetDatabase.LoadAssetAtPath<PlatformChunkData>(dataPath);
        if (data == null) {
            data = ScriptableObject.CreateInstance<PlatformChunkData>();
            AssetDatabase.CreateAsset(data, dataPath);
        }

        data.Configure(
            definition.Id,
            savedPrefab.GetComponent<PlatformChunk>(),
            definition.Category,
            definition.Ability,
            definition.Traversal,
            definition.Combat,
            definition.Precision,
            1f,
            definition.MinimumProgress,
            definition.HorizontalReach * HorizontalLayoutScale,
            definition.VerticalReach,
            scaledExit.y - 0.5f,
            definition.ExitYaw,
            scaledExit.x);
        EditorUtility.SetDirty(data);
        return data;
    }

    static void CreateLabScene(LevelGenerationConfig config) {
        var previousActiveScene = SceneManager.GetActiveScene();
        var scene = SceneManager.GetSceneByPath(ScenePath);
        var reuseLoadedScene = scene.IsValid() && scene.isLoaded;
        if (reuseLoadedScene) {
            foreach (var rootObject in scene.GetRootGameObjects())
                UnityEngine.Object.DestroyImmediate(rootObject);
        } else {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            scene.name = "PCG_Lab";
        }
        SceneManager.SetActiveScene(scene);

        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        var start = new GameObject("Start Platform");
        start.transform.position = new Vector3(0f, 0f, -3.5f);
        var startVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        startVisual.name = "Platform";
        startVisual.transform.SetParent(start.transform);
        startVisual.transform.localPosition = Vector3.zero;
        startVisual.transform.localScale = new Vector3(8f, 1f, 7f);
        startVisual.layer = LayerMask.NameToLayer("Ground");
        startVisual.GetComponent<Renderer>().sharedMaterial =
            AssetDatabase.LoadAssetAtPath<Material>(TutorialGroundMaterialPath);
        var anchor = CreateSocket(start.transform, "Start Anchor", new Vector3(0f, 0.5f, 3.5f));

        var spawn = CreateSocket(start.transform, "Player Spawn", new Vector3(0f, 1.5f, 0f));
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        var inputReader = AssetDatabase.LoadAssetAtPath<InputReader>(InputReaderPath);
        if (playerPrefab == null || inputReader == null)
            throw new InvalidOperationException("Player 2 prefab or InputReader asset is missing.");

        GameObject playerObject;
        var loggingEnabled = Debug.unityLogger.logEnabled;
        try {
            // ValidatedMonoBehaviour validates immediately when instantiated/added. Suppress the
            // transient "missing ref" messages until both scene references have been assigned.
            Debug.unityLogger.logEnabled = false;
            playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            playerObject.name = "Player 2";
            playerObject.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            var playerController = playerObject.GetComponent<PlayerController>();
            var freeLook = CreateTutorialCameraRig(inputReader, playerObject.transform);
            SetObjectReferences(
                playerController,
                ("input", inputReader),
                ("freeLookVCam", freeLook));
            playerObject.AddComponent<PCGPlayerAbilityBridge>();
        } finally {
            Debug.unityLogger.logEnabled = loggingEnabled;
        }

        var generatedRoot = new GameObject("Generated Chunks").transform;
        var system = new GameObject("PCG System");
        var generator = system.AddComponent<LevelGenerator>();
        generator.Configure(config, anchor, generatedRoot, 82431);
        generator.SetTraversalCapabilities(PlayerTraversalCapabilities.LabDefaults);
        var runController = system.AddComponent<PCGRunController>();
        runController.Configure(playerObject.transform, spawn);
        var telemetry = system.AddComponent<PCGRunTelemetry>();
        telemetry.Configure(generator, runController, playerObject.transform);
        var panel = system.AddComponent<PCGDebugPanel>();
        panel.Configure(generator, playerObject, runController, telemetry);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
            SceneManager.SetActiveScene(previousActiveScene);
        if (!reuseLoadedScene) EditorSceneManager.CloseScene(scene, true);
    }

    static Transform CreateSocket(
        Transform parent,
        string name,
        Vector3 localPosition,
        float localYaw = 0f) {
        var socket = new GameObject(name).transform;
        socket.SetParent(parent);
        socket.localPosition = localPosition;
        socket.localRotation = Quaternion.Euler(0f, localYaw, 0f);
        return socket;
    }

    static Vector3 ScaleHorizontal(Vector3 value, float scale) =>
        new Vector3(value.x * scale, value.y, value.z * scale);

    static Vector3 ScaleFootprint(Vector3 value, float scale) =>
        new Vector3(value.x * scale, value.y, value.z * scale);

    static CinemachineFreeLook CreateTutorialCameraRig(InputReader inputReader, Transform player) {
        var cameraSystem = new GameObject("CameraSystem");

        var mainCameraObject = new GameObject("Main Camera");
        mainCameraObject.tag = "MainCamera";
        mainCameraObject.transform.SetParent(cameraSystem.transform);
        mainCameraObject.transform.position = player.position + new Vector3(0f, 4f, -8f);
        var camera = mainCameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        mainCameraObject.AddComponent<AudioListener>();
        mainCameraObject.AddComponent<CinemachineBrain>();

        var freeLookObject = new GameObject("FreeLook Camera");
        freeLookObject.transform.SetParent(cameraSystem.transform);
        var freeLook = freeLookObject.AddComponent<CinemachineFreeLook>();
        freeLook.m_Lens.FieldOfView = 80f;
        freeLook.m_BindingMode = CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp;
        var xAxis = freeLook.m_XAxis;
        xAxis.m_InputAxisName = string.Empty;
        xAxis.m_InvertInput = false;
        freeLook.m_XAxis = xAxis;
        var yAxis = freeLook.m_YAxis;
        yAxis.m_InputAxisName = string.Empty;
        yAxis.m_InvertInput = true;
        freeLook.m_YAxis = yAxis;
        freeLook.m_Orbits = new[] {
            new CinemachineFreeLook.Orbit(4.5f, 3f),
            new CinemachineFreeLook.Orbit(2.5f, 10f),
            new CinemachineFreeLook.Orbit(0.4f, 6f)
        };
        freeLook.Follow = player;
        freeLook.LookAt = player;

        var cameraManager = cameraSystem.AddComponent<CameraManager>();
        SetObjectReferences(
            cameraManager,
            ("input", inputReader),
            ("freeLookVCam", freeLook));
        SetFloat(cameraManager, "speedMultiplier", LabCameraSensitivity);
        return freeLook;
    }

    static void SetObjectReferences(
        UnityEngine.Object target,
        params (string fieldName, UnityEngine.Object value)[] references) {
        var serializedObject = new SerializedObject(target);
        foreach (var reference in references) {
            var property = serializedObject.FindProperty(reference.fieldName);
            if (property == null) throw new InvalidOperationException(
                $"{target.GetType().Name}.{reference.fieldName} was not found.");
            property.objectReferenceValue = reference.value;
        }
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetFloat(UnityEngine.Object target, string fieldName, float value) {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(fieldName);
        if (property == null) throw new InvalidOperationException(
            $"{target.GetType().Name}.{fieldName} was not found.");
        property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureFolder(string parent, string child) {
        var path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    static void AddSceneToBuildSettings(string scenePath) {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(item => item.path == scenePath)) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    readonly struct PlatformSpec {
        public readonly Vector3 Position;
        public readonly Vector3 Scale;

        public PlatformSpec(Vector3 position, Vector3 scale) {
            Position = position;
            Scale = scale;
        }
    }

    readonly struct ChunkDefinition {
        public readonly string Id;
        public readonly ChunkCategory Category;
        public readonly AbilityRequirement Ability;
        public readonly float Traversal;
        public readonly float Combat;
        public readonly float Precision;
        public readonly PlatformSpec[] Platforms;
        public readonly Vector3 Exit;
        public readonly int MinimumProgress;
        public readonly float HorizontalReach;
        public readonly float VerticalReach;
        public readonly float ExitYaw;

        public ChunkDefinition(
            string id,
            ChunkCategory category,
            AbilityRequirement ability,
            float traversal,
            float combat,
            float precision,
            PlatformSpec[] platforms,
            Vector3 exit,
            int minimumProgress = 0,
            float horizontalReach = 0f,
            float verticalReach = 0f,
            float exitYaw = 0f) {
            Id = id;
            Category = category;
            Ability = ability;
            Traversal = traversal;
            Combat = combat;
            Precision = precision;
            Platforms = platforms;
            Exit = exit;
            MinimumProgress = minimumProgress;
            HorizontalReach = horizontalReach;
            VerticalReach = verticalReach;
            ExitYaw = exitYaw;
        }
    }
}

using System;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGDebugPanel : MonoBehaviour {
        [SerializeField] LevelGenerator generator;
        [SerializeField] GameObject player;
        [SerializeField] PCGRunController runController;
        [SerializeField] PCGRunTelemetry telemetry;
        [SerializeField] PCGAdaptiveDifficultyDirector difficultyDirector;
        [SerializeField] PCGGameAIObservationSensor observationSensor;
        [SerializeField] PCGMultimodalDatasetRecorder datasetRecorder;
        [SerializeField] MonoBehaviour trainingControllerBehaviour;

        string seedText = "82431";
        bool doubleJump;
        bool dash;
        bool adaptiveDifficulty = true;
        bool trainingMode;
        Vector2 manifestScroll;

        public bool HideLegacyGui { get; set; }
        public string SeedText => seedText;
        public bool DoubleJumpEnabled => doubleJump;
        public bool DashEnabled => dash;
        public bool AdaptiveDifficultyEnabled => adaptiveDifficulty;
        public bool TrainingModeEnabled => trainingMode;
        public MonoBehaviour TrainingControllerBehaviour => trainingControllerBehaviour;

        IPCGTrainingController TrainingController =>
            trainingControllerBehaviour as IPCGTrainingController;

        void Awake() {
            if (generator == null) generator = FindObjectOfType<LevelGenerator>();
            if (generator != null) seedText = generator.Seed.ToString();
            PCGLabExperience.EnsureInstalled(this);
        }

        public void Configure(
            LevelGenerator levelGenerator,
            GameObject labPlayer = null,
            PCGRunController labRunController = null,
            PCGRunTelemetry runTelemetry = null,
            PCGAdaptiveDifficultyDirector adaptiveDirector = null,
            PCGGameAIObservationSensor gameAIObservationSensor = null,
            PCGMultimodalDatasetRecorder multimodalDatasetRecorder = null,
            MonoBehaviour mlTrainingController = null) {
            generator = levelGenerator;
            player = labPlayer;
            runController = labRunController;
            telemetry = runTelemetry;
            difficultyDirector = adaptiveDirector;
            observationSensor = gameAIObservationSensor;
            datasetRecorder = multimodalDatasetRecorder;
            trainingControllerBehaviour = mlTrainingController;
            trainingMode = TrainingController != null && TrainingController.TrainingMode;
            if (generator != null) seedText = generator.Seed.ToString();
            var experience = GetComponent<PCGLabExperience>();
            experience?.Bind(this);
        }

        public void SetDoubleJump(bool value) => doubleJump = value;

        public void SetDash(bool value) => dash = value;

        public void SetAdaptiveDifficulty(bool value) {
            adaptiveDifficulty = value;
            difficultyDirector?.SetAdaptiveDifficultyEnabled(adaptiveDifficulty);
        }

        public void SetTrainingMode(bool value) {
            trainingMode = value;
            TrainingController?.SetTrainingMode(trainingMode);
        }

        public void GenerateFromSeed(string seed) {
            seedText = seed;
            Generate();
        }

        public void GenerateRandomSeed() {
            seedText = Environment.TickCount.ToString();
            Generate();
        }

        public void CopySeed() {
            if (generator != null) GUIUtility.systemCopyBuffer = generator.Seed.ToString();
        }

        public void CopyManifest() {
            if (generator != null && generator.LastManifest != null)
                GUIUtility.systemCopyBuffer = generator.LastManifest.ToJson();
        }

        public void CopyTelemetry() {
            if (telemetry != null) GUIUtility.systemCopyBuffer = telemetry.ToJson();
        }

        public void CopyObservation() {
            if (observationSensor != null)
                GUIUtility.systemCopyBuffer = observationSensor.LatestObservationToJson();
        }

        public void StartDatasetRecording() => datasetRecorder?.StartRecording();

        public void StopDatasetRecording() => datasetRecorder?.StopRecording(false);

        public void CopyDatasetPath() {
            if (datasetRecorder != null && !string.IsNullOrEmpty(datasetRecorder.CurrentEpisodeDirectory))
                GUIUtility.systemCopyBuffer = datasetRecorder.CurrentEpisodeDirectory;
        }

        void OnGUI() {
            if (HideLegacyGui || generator == null) return;

            GUILayout.BeginArea(new Rect(16f, 16f, 390f, Screen.height - 32f), GUI.skin.box);
            GUILayout.Label("PCG Lab");
            GUILayout.Label("Move: WASD / Jump: Space / Dash: Shift");
            if (runController != null)
                GUILayout.Label($"Checkpoint: {runController.FurthestCheckpoint + 1}   Resets: {runController.ResetCount}");
            if (telemetry != null)
                GUILayout.Label($"Telemetry events: {telemetry.Events.Count}");
            if (difficultyDirector != null)
                GUILayout.Label(
                    $"Game AI skill: {difficultyDirector.SkillEstimate:F2}   " +
                    $"PCG bias: {difficultyDirector.DifficultyBias:+0.00;-0.00;0.00}");
            if (observationSensor != null)
                GUILayout.Label(
                    $"Observation: {PCGGameAIObservation.VectorSize}D + " +
                    $"{PCGGameAIObservationSensor.VisualWidth}x" +
                    $"{PCGGameAIObservationSensor.VisualHeight} RGB");
            if (datasetRecorder != null)
                GUILayout.Label(datasetRecorder.IsRecording
                    ? "Dataset: RECORDING"
                    : datasetRecorder.LastSummary != null
                        ? $"Dataset: {datasetRecorder.LastSummary.sampleCount} samples saved"
                        : "Dataset: ready");
            if (TrainingController != null)
                GUILayout.Label(
                    $"ML Episodes: {TrainingController.CompletedEpisodes} completed / " +
                    $"{TrainingController.FailedEpisodes} failed   " +
                    $"Reward: {TrainingController.LastEpisodeReward:F2}");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed", GUILayout.Width(55f));
            seedText = GUILayout.TextField(seedText, GUILayout.Width(120f));
            if (GUILayout.Button("Generate")) Generate();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            doubleJump = GUILayout.Toggle(doubleJump, "Double Jump");
            dash = GUILayout.Toggle(dash, "Dash");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var nextAdaptiveDifficulty = GUILayout.Toggle(
                adaptiveDifficulty,
                "Adaptive Difficulty");
            if (nextAdaptiveDifficulty != adaptiveDifficulty) {
                adaptiveDifficulty = nextAdaptiveDifficulty;
                difficultyDirector?.SetAdaptiveDifficultyEnabled(adaptiveDifficulty);
            }
            GUILayout.EndHorizontal();

            if (TrainingController != null) {
                var nextTrainingMode = GUILayout.Toggle(
                    trainingMode,
                    "ML-Agents Training Mode");
                if (nextTrainingMode != trainingMode) {
                    trainingMode = nextTrainingMode;
                    TrainingController.SetTrainingMode(trainingMode);
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Random Seed")) {
                seedText = Environment.TickCount.ToString();
                Generate();
            }
            if (GUILayout.Button("Copy Seed")) GUIUtility.systemCopyBuffer = generator.Seed.ToString();
            if (GUILayout.Button("Copy Manifest") && generator.LastManifest != null)
                GUIUtility.systemCopyBuffer = generator.LastManifest.ToJson();
            if (GUILayout.Button("Copy Telemetry") && telemetry != null)
                GUIUtility.systemCopyBuffer = telemetry.ToJson();
            GUILayout.EndHorizontal();

            if (observationSensor != null &&
                GUILayout.Button("Copy Latest Game AI Observation"))
                GUIUtility.systemCopyBuffer = observationSensor.LatestObservationToJson();

            if (datasetRecorder != null) {
                GUILayout.BeginHorizontal();
                GUI.enabled = !datasetRecorder.IsRecording;
                if (GUILayout.Button("Start Dataset Recording"))
                    datasetRecorder.StartRecording();
                GUI.enabled = datasetRecorder.IsRecording;
                if (GUILayout.Button("Stop Recording"))
                    datasetRecorder.StopRecording(false);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                if (!string.IsNullOrEmpty(datasetRecorder.CurrentEpisodeDirectory) &&
                    GUILayout.Button("Copy Dataset Path"))
                    GUIUtility.systemCopyBuffer = datasetRecorder.CurrentEpisodeDirectory;
            }

            var manifest = generator.LastManifest;
            if (manifest != null) {
                GUILayout.Space(8f);
                GUILayout.Label(manifest.completed
                    ? $"Generated {manifest.chunks.Count} chunks"
                    : $"Failed: {manifest.failureReason}");

                manifestScroll = GUILayout.BeginScrollView(manifestScroll);
                GUILayout.TextArea(manifest.ToJson(), GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();
            }

            GUILayout.EndArea();
        }

        void Generate() {
            if (!int.TryParse(seedText, out var parsedSeed)) parsedSeed = 82431;
            generator.Seed = parsedSeed;
            generator.SetCapabilities(doubleJump, dash);
            if (player != null)
                player.SendMessage(
                    "ApplyPCGTraversalAbilities",
                    new PlayerAbilityProfile(doubleJump, dash),
                    SendMessageOptions.DontRequireReceiver);
            generator.Generate();
            seedText = generator.Seed.ToString();
        }
    }
}

using UnityEngine;

namespace Platformer.PCG {
    [DisallowMultipleComponent]
    public sealed class PCGLabExperience : MonoBehaviour {
        [SerializeField] PCGDebugPanel panel;
        [SerializeField] LevelGenerator generator;
        [SerializeField] PCGRunController runController;
        [SerializeField] PCGRunTelemetry telemetry;
        [SerializeField] PCGAdaptiveDifficultyDirector difficultyDirector;
        [SerializeField] PCGGameAIObservationSensor observationSensor;
        [SerializeField] PCGMultimodalDatasetRecorder datasetRecorder;
        [SerializeField] MonoBehaviour trainingControllerBehaviour;

        PCGLabVisualStyler styler;
        PCGLabAudio audio;
        PCGLabHud hud;
        bool worldApplied;

        IPCGTrainingController TrainingController =>
            trainingControllerBehaviour as IPCGTrainingController;

        public static PCGLabExperience EnsureInstalled(PCGDebugPanel debugPanel) {
            if (debugPanel == null) return null;
            var experience = debugPanel.GetComponent<PCGLabExperience>();
            if (experience == null) experience = debugPanel.gameObject.AddComponent<PCGLabExperience>();
            experience.Bind(debugPanel);
            return experience;
        }

        public void Bind(PCGDebugPanel debugPanel) {
            panel = debugPanel;
            if (generator == null) generator = FindObjectOfType<LevelGenerator>();
            if (runController == null) runController = FindObjectOfType<PCGRunController>();
            if (telemetry == null) telemetry = FindObjectOfType<PCGRunTelemetry>();
            if (difficultyDirector == null)
                difficultyDirector = FindObjectOfType<PCGAdaptiveDifficultyDirector>();
            if (observationSensor == null)
                observationSensor = FindObjectOfType<PCGGameAIObservationSensor>();
            if (datasetRecorder == null)
                datasetRecorder = FindObjectOfType<PCGMultimodalDatasetRecorder>();
            if (trainingControllerBehaviour == null && panel != null)
                trainingControllerBehaviour = panel.TrainingControllerBehaviour;
        }

        void Awake() {
            if (panel == null) panel = GetComponent<PCGDebugPanel>();
            Bind(panel);
        }

        void OnEnable() {
            Bind(panel);
            Subscribe();
        }

        void Start() {
            Unsubscribe();
            Bind(panel);
            Subscribe();
            ApplyWorld();
            EnsurePresentation();
            if (generator != null && generator.LastManifest != null) {
                PCGPlatformFeel.CloseWalkableSeams(generator);
                styler.StyleGeneratedLevel(generator);
            }
        }

        void OnDisable() {
            Unsubscribe();
        }

        void Subscribe() {
            if (generator != null) generator.GenerationFinished += HandleGenerationFinished;
            if (runController != null) {
                runController.CheckpointReached += HandleCheckpoint;
                runController.PlayerRespawned += HandleRespawn;
            }
            PCGTimedPlatform.StateChanged += HandleTimedState;
            PCGLabSignals.JumpStarted += HandleJump;
            PCGLabSignals.DashStarted += HandleDash;
        }

        void Unsubscribe() {
            if (generator != null) generator.GenerationFinished -= HandleGenerationFinished;
            if (runController != null) {
                runController.CheckpointReached -= HandleCheckpoint;
                runController.PlayerRespawned -= HandleRespawn;
            }
            PCGTimedPlatform.StateChanged -= HandleTimedState;
            PCGLabSignals.JumpStarted -= HandleJump;
            PCGLabSignals.DashStarted -= HandleDash;
        }

        void OnDestroy() {
            styler?.Dispose();
        }

        void ApplyWorld() {
            if (worldApplied) return;
            var sun = Object.FindObjectOfType<Light>();
            var start = GameObject.Find("Start Platform");
            styler = new PCGLabVisualStyler();
            styler.ApplyWorld(start != null ? start.transform : null, sun);
            worldApplied = true;
        }

        void EnsurePresentation() {
            if (audio == null) {
                audio = gameObject.GetComponent<PCGLabAudio>();
                if (audio == null) audio = gameObject.AddComponent<PCGLabAudio>();
                audio.Configure();
            }

            if (hud == null) {
                hud = gameObject.GetComponent<PCGLabHud>();
                if (hud == null) hud = gameObject.AddComponent<PCGLabHud>();
                hud.Configure(
                    panel,
                    generator,
                    runController,
                    telemetry,
                    difficultyDirector,
                    observationSensor,
                    datasetRecorder,
                    TrainingController);
            }
        }

        void HandleGenerationFinished(GeneratedLevelManifest manifest) {
            EnsurePresentation();
            PCGPlatformFeel.CloseWalkableSeams(generator);
            styler?.StyleGeneratedLevel(generator);
            audio?.PlayGenerate();
            if (manifest != null && manifest.completed)
                hud?.ShowToast($"ROUTE SEEDED  {manifest.seed}   {manifest.chunks.Count} CHUNKS");
            else
                hud?.ShowToast(manifest != null ? $"GENERATION FAILED  {manifest.failureReason}" : "GENERATION FAILED");
        }

        void HandleCheckpoint(int index, Vector3 position) {
            var total = generator != null && generator.LastManifest != null
                ? generator.LastManifest.chunks.Count
                : 16;
            var finished = index >= total - 1;
            if (finished) {
                audio?.PlayFinish();
                hud?.ShowToast("ROUTE COMPLETE", 2.6f);
            } else {
                audio?.PlayCheckpoint();
                hud?.ShowToast($"CHECKPOINT  {index + 1} / {total}");
            }
        }

        void HandleRespawn(int resetCount, Vector3 position) {
            audio?.PlayRespawn();
            hud?.ShowToast($"RESPAWN  #{resetCount}");
        }

        void HandleTimedState(PCGTimedPlatform platform, TimedPlatformState state) {
            styler?.PulseTimedPlatform(platform, state);
            if (state == TimedPlatformState.Warning) audio?.PlayTimedWarning();
        }

        void HandleJump() => audio?.PlayJump();
        void HandleDash() => audio?.PlayDash();
    }
}

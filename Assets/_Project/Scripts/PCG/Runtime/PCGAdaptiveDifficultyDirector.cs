using System;
using UnityEngine;

namespace Platformer.PCG {
    [Serializable]
    public struct PCGAdaptiveDifficultyModel {
        [SerializeField, Range(0f, 1f)] float skillEstimate;

        public float SkillEstimate => skillEstimate;

        public PCGAdaptiveDifficultyModel(float initialSkill) {
            skillEstimate = Mathf.Clamp01(initialSkill);
        }

        public float Update(float checkpointSeconds, int resetDelta, float smoothing) {
            var sample = ScorePerformance(checkpointSeconds, resetDelta);
            skillEstimate = Mathf.Lerp(
                skillEstimate,
                sample,
                Mathf.Clamp01(smoothing));
            return skillEstimate;
        }

        public static float ScorePerformance(float checkpointSeconds, int resetDelta) {
            var timeScore = Mathf.InverseLerp(45f, 10f, Mathf.Max(0f, checkpointSeconds));
            return Mathf.Clamp01(timeScore - Mathf.Max(0, resetDelta) * 0.2f);
        }
    }

    public sealed class PCGAdaptiveDifficultyDirector : MonoBehaviour {
        [SerializeField] LevelGenerator generator;
        [SerializeField] PCGRunController runController;
        [SerializeField] PCGRunTelemetry telemetry;
        [SerializeField] bool adaptiveDifficultyEnabled = true;
        [SerializeField, Range(0.05f, 1f)] float smoothing = 0.25f;
        [SerializeField, Range(0f, 0.5f)] float maximumDifficultyBias = 0.2f;
        [SerializeField] PCGAdaptiveDifficultyModel model =
            new PCGAdaptiveDifficultyModel(0.5f);

        float checkpointStartTime;
        int resetCountAtCheckpointStart;

        public float SkillEstimate => model.SkillEstimate;
        public float DifficultyBias =>
            adaptiveDifficultyEnabled
                ? (SkillEstimate - 0.5f) * maximumDifficultyBias * 2f
                : 0f;
        public event Action<float, float> DifficultyAdjusted;

        void Awake() {
            if (generator == null) generator = FindObjectOfType<LevelGenerator>();
            if (runController == null) runController = FindObjectOfType<PCGRunController>();
            if (telemetry == null) telemetry = FindObjectOfType<PCGRunTelemetry>();
            checkpointStartTime = Time.unscaledTime;
            resetCountAtCheckpointStart =
                runController != null ? runController.ResetCount : 0;
        }

        void OnEnable() {
            Subscribe();
        }

        void Start() {
            ApplyDifficulty();
        }

        void OnDisable() {
            Unsubscribe();
        }

        public void Configure(
            LevelGenerator levelGenerator,
            PCGRunController controller,
            PCGRunTelemetry runTelemetry) {
            Unsubscribe();
            generator = levelGenerator;
            runController = controller;
            telemetry = runTelemetry;
            checkpointStartTime = Time.unscaledTime;
            resetCountAtCheckpointStart =
                runController != null ? runController.ResetCount : 0;
            if (isActiveAndEnabled) Subscribe();
        }

        public void SetAdaptiveDifficultyEnabled(bool value) {
            adaptiveDifficultyEnabled = value;
            ApplyDifficulty();
        }

        public void ApplyPerformanceSample(float checkpointSeconds, int resetDelta) {
            if (adaptiveDifficultyEnabled)
                model.Update(checkpointSeconds, resetDelta, smoothing);
            ApplyDifficulty();
        }

        void HandleCheckpointReached(int chunkIndex, Vector3 position) {
            var now = Time.unscaledTime;
            var resetCount = runController != null ? runController.ResetCount : 0;
            ApplyPerformanceSample(
                now - checkpointStartTime,
                resetCount - resetCountAtCheckpointStart);
            checkpointStartTime = now;
            resetCountAtCheckpointStart = resetCount;
        }

        void ApplyDifficulty() {
            var bias = DifficultyBias;
            if (generator != null) generator.SetDifficultyBias(bias);

            foreach (var platform in FindObjectsOfType<PCGOscillatingPlatform>())
                platform.ApplyDifficulty(SkillEstimate);
            foreach (var platform in FindObjectsOfType<PCGTimedPlatform>())
                platform.ApplyDifficulty(SkillEstimate);

            telemetry?.Record(
                PCGTelemetryEventType.DifficultyAdjusted,
                runController != null ? runController.FurthestCheckpoint : -1,
                Vector3.zero,
                $"skill={SkillEstimate:F3};bias={bias:F3}");
            DifficultyAdjusted?.Invoke(SkillEstimate, bias);
        }

        void Subscribe() {
            if (runController != null)
                runController.CheckpointReached += HandleCheckpointReached;
            if (generator != null)
                generator.GenerationFinished += HandleGenerationFinished;
        }

        void Unsubscribe() {
            if (runController != null)
                runController.CheckpointReached -= HandleCheckpointReached;
            if (generator != null)
                generator.GenerationFinished -= HandleGenerationFinished;
        }

        void HandleGenerationFinished(GeneratedLevelManifest manifest) {
            if (manifest != null && manifest.completed) ApplyDifficulty();
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Platformer.PCG {
    public enum PCGBehaviorLabel {
        Idle,
        Traversing,
        Airborne,
        Falling,
        Recovery
    }

    [Serializable]
    public sealed class PCGDatasetSample {
        public int sampleIndex;
        public float episodeTime;
        public float[] observation;
        public PCGBehaviorLabel behavior;
        public float reward;
        public string visualFrame;
    }

    [Serializable]
    public sealed class PCGDatasetEpisodeSummary {
        public string schemaVersion = "1.0";
        public string episodeId;
        public int seed;
        public string startedUtc;
        public string finishedUtc;
        public float durationSeconds;
        public int sampleCount;
        public int visualFrameCount;
        public int resetCount;
        public int furthestCheckpoint;
        public bool completed;
        public float episodeReturn;
        public float finalSkillEstimate;
        public float finalDifficultyBias;

        public string ToJson(bool prettyPrint = true) =>
            JsonUtility.ToJson(this, prettyPrint);
    }

    public sealed class PCGMultimodalDatasetRecorder : MonoBehaviour {
        [SerializeField] LevelGenerator generator;
        [SerializeField] PCGRunController runController;
        [SerializeField] PCGGameAIObservationSensor observationSensor;
        [SerializeField] PCGAdaptiveDifficultyDirector difficultyDirector;
        [SerializeField] PCGRunTelemetry telemetry;
        [SerializeField] bool saveVisualFrames = true;
        [SerializeField, Range(1, 20)] int saveEveryNthVisualFrame = 1;

        StreamWriter observationWriter;
        string outputRootOverride;
        float episodeStartTime;
        DateTime episodeStartedUtc;
        int resetCountAtStart;
        int sampleCount;
        int visualFrameCount;
        int lastVisualSequence;
        float previousProgress;
        int previousResetCount;
        float episodeReturn;

        public bool IsRecording { get; private set; }
        public string CurrentEpisodeDirectory { get; private set; } = string.Empty;
        public PCGDatasetEpisodeSummary LastSummary { get; private set; }

        void OnDisable() {
            if (IsRecording) StopRecording(false);
        }

        public void Configure(
            LevelGenerator levelGenerator,
            PCGRunController controller,
            PCGGameAIObservationSensor sensor,
            PCGAdaptiveDifficultyDirector director,
            PCGRunTelemetry runTelemetry) {
            Unsubscribe();
            generator = levelGenerator;
            runController = controller;
            observationSensor = sensor;
            difficultyDirector = director;
            telemetry = runTelemetry;
            if (IsRecording) Subscribe();
        }

        public void SetOutputRoot(string path) {
            if (IsRecording)
                throw new InvalidOperationException(
                    "Cannot change the dataset output directory while recording.");
            outputRootOverride = path;
        }

        public bool StartRecording() {
            if (IsRecording || observationSensor == null) return false;

            var episodeId = CreateEpisodeId(
                generator != null ? generator.Seed : 0,
                DateTime.UtcNow);
            var root = string.IsNullOrWhiteSpace(outputRootOverride)
                ? Path.Combine(Application.persistentDataPath, "PCGDatasets")
                : outputRootOverride;
            CurrentEpisodeDirectory = Path.Combine(root, episodeId);
            Directory.CreateDirectory(CurrentEpisodeDirectory);
            if (saveVisualFrames)
                Directory.CreateDirectory(Path.Combine(CurrentEpisodeDirectory, "frames"));

            observationWriter = new StreamWriter(
                Path.Combine(CurrentEpisodeDirectory, "observations.jsonl"),
                false);
            observationWriter.AutoFlush = true;
            episodeStartTime = Time.unscaledTime;
            episodeStartedUtc = DateTime.UtcNow;
            resetCountAtStart = runController != null ? runController.ResetCount : 0;
            previousResetCount = resetCountAtStart;
            previousProgress = observationSensor.LatestObservation.normalizedProgress;
            sampleCount = 0;
            visualFrameCount = 0;
            lastVisualSequence = 0;
            episodeReturn = 0f;
            LastSummary = null;
            IsRecording = true;
            Subscribe();
            telemetry?.Record(
                PCGTelemetryEventType.DatasetRecordingStarted,
                runController != null ? runController.FurthestCheckpoint : -1,
                Vector3.zero,
                episodeId);
            return true;
        }

        public PCGDatasetEpisodeSummary StopRecording(bool completed) {
            if (!IsRecording) return LastSummary;
            Unsubscribe();
            IsRecording = false;
            observationWriter?.Flush();
            observationWriter?.Dispose();
            observationWriter = null;

            LastSummary = new PCGDatasetEpisodeSummary {
                episodeId = Path.GetFileName(CurrentEpisodeDirectory),
                seed = generator != null ? generator.Seed : 0,
                startedUtc = episodeStartedUtc.ToString("O", CultureInfo.InvariantCulture),
                finishedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                durationSeconds = Mathf.Max(0f, Time.unscaledTime - episodeStartTime),
                sampleCount = sampleCount,
                visualFrameCount = visualFrameCount,
                resetCount = runController != null
                    ? runController.ResetCount - resetCountAtStart
                    : 0,
                furthestCheckpoint = runController != null
                    ? runController.FurthestCheckpoint
                    : -1,
                completed = completed,
                episodeReturn = episodeReturn,
                finalSkillEstimate = difficultyDirector != null
                    ? difficultyDirector.SkillEstimate
                    : 0.5f,
                finalDifficultyBias = difficultyDirector != null
                    ? difficultyDirector.DifficultyBias
                    : 0f
            };
            File.WriteAllText(
                Path.Combine(CurrentEpisodeDirectory, "episode.json"),
                LastSummary.ToJson());
            telemetry?.Record(
                PCGTelemetryEventType.DatasetRecordingFinished,
                LastSummary.furthestCheckpoint,
                Vector3.zero,
                $"episode={LastSummary.episodeId};completed={completed};samples={sampleCount}");
            return LastSummary;
        }

        public static PCGBehaviorLabel ClassifyBehavior(
            Vector3 velocity,
            bool recentlyRespawned) {
            if (recentlyRespawned) return PCGBehaviorLabel.Recovery;
            if (velocity.y < -2f) return PCGBehaviorLabel.Falling;
            if (Mathf.Abs(velocity.y) > 0.5f) return PCGBehaviorLabel.Airborne;
            var planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            return planarSpeed < 0.25f
                ? PCGBehaviorLabel.Idle
                : PCGBehaviorLabel.Traversing;
        }

        public static float CalculateReward(
            float previousNormalizedProgress,
            float normalizedProgress,
            int resetDelta) {
            var progressReward =
                (normalizedProgress - previousNormalizedProgress) * 10f;
            return progressReward - Mathf.Max(0, resetDelta);
        }

        public static string CreateEpisodeId(int seed, DateTime utcTime) =>
            $"episode_{utcTime:yyyyMMdd_HHmmss_fff}_seed_{seed}";

        void Subscribe() {
            if (observationSensor != null) {
                observationSensor.ObservationReady -= HandleObservation;
                observationSensor.VisualFrameReady -= HandleVisualFrame;
                observationSensor.ObservationReady += HandleObservation;
                observationSensor.VisualFrameReady += HandleVisualFrame;
            }
            if (runController != null) {
                runController.CheckpointReached -= HandleCheckpointReached;
                runController.CheckpointReached += HandleCheckpointReached;
            }
        }

        void Unsubscribe() {
            if (observationSensor != null) {
                observationSensor.ObservationReady -= HandleObservation;
                observationSensor.VisualFrameReady -= HandleVisualFrame;
            }
            if (runController != null)
                runController.CheckpointReached -= HandleCheckpointReached;
        }

        void HandleObservation(PCGGameAIObservation observation) {
            if (!IsRecording || observationWriter == null || observation == null) return;
            var resetDelta = Mathf.Max(0, observation.resetCount - previousResetCount);
            var reward = CalculateReward(
                previousProgress,
                observation.normalizedProgress,
                resetDelta);
            var sample = new PCGDatasetSample {
                sampleIndex = sampleCount,
                episodeTime = Mathf.Max(0f, Time.unscaledTime - episodeStartTime),
                observation = observation.ToVector(),
                behavior = ClassifyBehavior(
                    observation.playerVelocity,
                    resetDelta > 0),
                reward = reward,
                visualFrame = lastVisualSequence > 0
                    ? $"frames/frame_{lastVisualSequence:000000}.png"
                    : string.Empty
            };
            observationWriter.WriteLine(JsonUtility.ToJson(sample));
            sampleCount++;
            episodeReturn += reward;
            previousProgress = observation.normalizedProgress;
            previousResetCount = observation.resetCount;
        }

        void HandleVisualFrame(int sequence, RenderTexture frame) {
            if (!IsRecording || !saveVisualFrames || frame == null) return;
            if (sequence % Mathf.Max(1, saveEveryNthVisualFrame) != 0) return;

            var previousActive = RenderTexture.active;
            var texture = new Texture2D(
                frame.width,
                frame.height,
                TextureFormat.RGB24,
                false);
            try {
                RenderTexture.active = frame;
                texture.ReadPixels(new Rect(0f, 0f, frame.width, frame.height), 0, 0);
                texture.Apply(false);
                File.WriteAllBytes(
                    Path.Combine(
                        CurrentEpisodeDirectory,
                        "frames",
                        $"frame_{sequence:000000}.png"),
                    texture.EncodeToPNG());
                lastVisualSequence = sequence;
                visualFrameCount++;
            } finally {
                RenderTexture.active = previousActive;
                Destroy(texture);
            }
        }

        void HandleCheckpointReached(int chunkIndex, Vector3 position) {
            if (!IsRecording || generator == null) return;
            if (chunkIndex >= generator.SpawnedChunks.Count - 1)
                StopRecording(true);
        }
    }
}

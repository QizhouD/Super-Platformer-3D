using System;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGGameAIObservationSensor : MonoBehaviour {
        public const int VisualWidth = 84;
        public const int VisualHeight = 84;

        [SerializeField] LevelGenerator generator;
        [SerializeField] PCGRunController runController;
        [SerializeField] Transform player;
        [SerializeField] Camera observationCamera;
        [SerializeField] PCGAdaptiveDifficultyDirector difficultyDirector;
        [SerializeField, Range(0.05f, 1f)] float structuredObservationInterval = 0.2f;
        [SerializeField, Range(0.1f, 2f)] float visualObservationInterval = 0.5f;
        [SerializeField] bool captureVisualFrames = true;

        Rigidbody playerBody;
        float nextStructuredObservationTime;
        float nextVisualObservationTime;
        int visualFrameSequence;

        public PCGGameAIObservation LatestObservation { get; private set; } =
            new PCGGameAIObservation();
        public RenderTexture LatestVisualFrame { get; private set; }
        public event Action<PCGGameAIObservation> ObservationReady;
        public event Action<int, RenderTexture> VisualFrameReady;

        void Awake() {
            ResolveReferences();
            EnsureVisualTarget();
        }

        void Update() {
            if (Time.unscaledTime < nextStructuredObservationTime) return;
            nextStructuredObservationTime = Time.unscaledTime + structuredObservationInterval;
            CaptureStructuredObservation();
        }

        void LateUpdate() {
            if (!captureVisualFrames ||
                observationCamera == null ||
                Time.unscaledTime < nextVisualObservationTime) return;

            nextVisualObservationTime = Time.unscaledTime + visualObservationInterval;
            CaptureVisualFrame();
        }

        void OnDestroy() {
            if (LatestVisualFrame == null) return;
            LatestVisualFrame.Release();
            Destroy(LatestVisualFrame);
        }

        public void Configure(
            LevelGenerator levelGenerator,
            PCGRunController controller,
            Transform playerTransform,
            Camera sourceCamera,
            PCGAdaptiveDifficultyDirector director) {
            generator = levelGenerator;
            runController = controller;
            player = playerTransform;
            observationCamera = sourceCamera;
            difficultyDirector = director;
            playerBody = player != null ? player.GetComponent<Rigidbody>() : null;
            EnsureVisualTarget();
        }

        public PCGGameAIObservation CaptureStructuredObservation() {
            ResolveReferences();
            var chunkCount = generator != null ? generator.SpawnedChunks.Count : 0;
            var currentChunk = runController != null ? runController.FurthestCheckpoint + 1 : 0;
            var nextDifficulty = 0f;
            if (generator != null &&
                generator.LastManifest != null &&
                currentChunk >= 0 &&
                currentChunk < generator.LastManifest.chunks.Count)
                nextDifficulty = generator.LastManifest.chunks[currentChunk].targetDifficulty;

            var timedPlatforms = FindObjectsOfType<PCGTimedPlatform>();
            var visibleTimedPlatforms = 0;
            foreach (var timedPlatform in timedPlatforms)
                if (timedPlatform.State != TimedPlatformState.Hidden) visibleTimedPlatforms++;

            LatestObservation = new PCGGameAIObservation {
                timestamp = Time.unscaledTime,
                playerPosition = player != null ? player.position : Vector3.zero,
                playerVelocity = playerBody != null ? playerBody.velocity : Vector3.zero,
                cameraForward = observationCamera != null
                    ? observationCamera.transform.forward
                    : Vector3.forward,
                normalizedProgress = chunkCount > 0
                    ? Mathf.Clamp01((float)Mathf.Max(0, currentChunk) / chunkCount)
                    : 0f,
                currentChunkIndex = currentChunk,
                resetCount = runController != null ? runController.ResetCount : 0,
                nextChunkDifficulty = nextDifficulty,
                adaptiveSkill = difficultyDirector != null ? difficultyDirector.SkillEstimate : 0.5f,
                difficultyBias = difficultyDirector != null ? difficultyDirector.DifficultyBias : 0f,
                timedPlatformVisibleRatio = timedPlatforms.Length > 0
                    ? (float)visibleTimedPlatforms / timedPlatforms.Length
                    : 1f,
                movingPlatformCount = FindObjectsOfType<PCGOscillatingPlatform>().Length,
                visualFrameSequence = visualFrameSequence
            };
            ObservationReady?.Invoke(LatestObservation);
            return LatestObservation;
        }

        public string LatestObservationToJson(bool prettyPrint = true) =>
            JsonUtility.ToJson(LatestObservation, prettyPrint);

        public RenderTexture GetOrCreateVisualFrame() {
            EnsureVisualTarget();
            return LatestVisualFrame;
        }

        void CaptureVisualFrame() {
            EnsureVisualTarget();
            if (LatestVisualFrame == null) return;

            var previousTarget = observationCamera.targetTexture;
            try {
                observationCamera.targetTexture = LatestVisualFrame;
                observationCamera.Render();
                visualFrameSequence++;
                VisualFrameReady?.Invoke(visualFrameSequence, LatestVisualFrame);
            } finally {
                observationCamera.targetTexture = previousTarget;
            }
        }

        void ResolveReferences() {
            if (generator == null) generator = FindObjectOfType<LevelGenerator>();
            if (runController == null) runController = FindObjectOfType<PCGRunController>();
            if (difficultyDirector == null)
                difficultyDirector = FindObjectOfType<PCGAdaptiveDifficultyDirector>();
            if (player == null) {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }
            if (observationCamera == null) observationCamera = Camera.main;
            if (player != null && playerBody == null) playerBody = player.GetComponent<Rigidbody>();
        }

        void EnsureVisualTarget() {
            if (!captureVisualFrames || LatestVisualFrame != null) return;
            LatestVisualFrame = new RenderTexture(
                VisualWidth,
                VisualHeight,
                16,
                RenderTextureFormat.ARGB32) {
                name = "PCG Game AI Visual Observation",
                filterMode = FilterMode.Bilinear,
                useMipMap = false
            };
            LatestVisualFrame.Create();
        }
    }
}

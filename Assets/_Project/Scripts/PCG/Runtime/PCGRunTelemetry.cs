using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.PCG {
    public enum PCGTelemetryEventType {
        GenerationFinished,
        CheckpointReached,
        PlayerRespawned,
        TimedPlatformStateChanged
    }

    [Serializable]
    public sealed class PCGTelemetryEvent {
        public float timestamp;
        public PCGTelemetryEventType type;
        public int chunkIndex;
        public Vector3 position;
        public string detail;
    }

    [Serializable]
    public sealed class PCGTelemetrySnapshot {
        public float timestamp;
        public Vector3 playerPosition;
        public Vector3 playerVelocity;
        public int furthestCheckpoint;
        public int resetCount;
        public int generatedChunkCount;
    }

    [Serializable]
    sealed class PCGTelemetryExport {
        public PCGTelemetrySnapshot latest;
        public List<PCGTelemetryEvent> events;
    }

    public sealed class PCGRunTelemetry : MonoBehaviour {
        [SerializeField] LevelGenerator generator;
        [SerializeField] PCGRunController runController;
        [SerializeField] Transform player;
        [SerializeField, Range(16, 2048)] int eventCapacity = 256;
        [SerializeField, Range(0.05f, 2f)] float snapshotInterval = 0.25f;

        readonly List<PCGTelemetryEvent> events = new List<PCGTelemetryEvent>();
        Rigidbody playerBody;
        float nextSnapshotTime;

        public IReadOnlyList<PCGTelemetryEvent> Events => events;
        public PCGTelemetrySnapshot LatestSnapshot { get; private set; } = new PCGTelemetrySnapshot();

        void Awake() {
            if (generator == null) generator = FindObjectOfType<LevelGenerator>();
            if (runController == null) runController = FindObjectOfType<PCGRunController>();
            if (player == null) {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }
            if (player != null) playerBody = player.GetComponent<Rigidbody>();
        }

        void OnEnable() {
            if (generator != null) generator.GenerationFinished += HandleGenerationFinished;
            if (runController != null) {
                runController.CheckpointReached += HandleCheckpointReached;
                runController.PlayerRespawned += HandlePlayerRespawned;
            }
            PCGTimedPlatform.StateChanged += HandleTimedPlatformStateChanged;
        }

        void OnDisable() {
            if (generator != null) generator.GenerationFinished -= HandleGenerationFinished;
            if (runController != null) {
                runController.CheckpointReached -= HandleCheckpointReached;
                runController.PlayerRespawned -= HandlePlayerRespawned;
            }
            PCGTimedPlatform.StateChanged -= HandleTimedPlatformStateChanged;
        }

        void Update() {
            if (Time.unscaledTime < nextSnapshotTime) return;
            nextSnapshotTime = Time.unscaledTime + snapshotInterval;
            CaptureSnapshot();
        }

        public void Configure(
            LevelGenerator levelGenerator,
            PCGRunController controller,
            Transform playerTransform) {
            generator = levelGenerator;
            runController = controller;
            player = playerTransform;
            playerBody = player != null ? player.GetComponent<Rigidbody>() : null;
        }

        public void Record(
            PCGTelemetryEventType type,
            int chunkIndex,
            Vector3 position,
            string detail = "") {
            events.Add(new PCGTelemetryEvent {
                timestamp = Time.unscaledTime,
                type = type,
                chunkIndex = chunkIndex,
                position = position,
                detail = detail ?? string.Empty
            });
            while (events.Count > Mathf.Max(16, eventCapacity)) events.RemoveAt(0);
        }

        public string ToJson(bool prettyPrint = true) {
            return JsonUtility.ToJson(new PCGTelemetryExport {
                latest = LatestSnapshot,
                events = new List<PCGTelemetryEvent>(events)
            }, prettyPrint);
        }

        void CaptureSnapshot() {
            LatestSnapshot = new PCGTelemetrySnapshot {
                timestamp = Time.unscaledTime,
                playerPosition = player != null ? player.position : Vector3.zero,
                playerVelocity = playerBody != null ? playerBody.velocity : Vector3.zero,
                furthestCheckpoint = runController != null ? runController.FurthestCheckpoint : -1,
                resetCount = runController != null ? runController.ResetCount : 0,
                generatedChunkCount = generator != null ? generator.SpawnedChunks.Count : 0
            };
        }

        void HandleGenerationFinished(GeneratedLevelManifest manifest) {
            Record(
                PCGTelemetryEventType.GenerationFinished,
                -1,
                Vector3.zero,
                manifest.completed
                    ? $"seed={manifest.seed};chunks={manifest.chunks.Count}"
                    : manifest.failureReason);
        }

        void HandleCheckpointReached(int chunkIndex, Vector3 position) {
            Record(PCGTelemetryEventType.CheckpointReached, chunkIndex, position);
        }

        void HandlePlayerRespawned(int resetCount, Vector3 position) {
            Record(PCGTelemetryEventType.PlayerRespawned, -1, position, $"reset={resetCount}");
        }

        void HandleTimedPlatformStateChanged(PCGTimedPlatform platform, TimedPlatformState state) {
            var chunkIndex = ResolveChunkIndex(platform);
            if (chunkIndex < 0) return;
            Record(
                PCGTelemetryEventType.TimedPlatformStateChanged,
                chunkIndex,
                platform.transform.position,
                state.ToString());
        }

        int ResolveChunkIndex(Component component) {
            if (generator == null || component == null) return -1;
            var chunk = component.GetComponentInParent<PlatformChunk>();
            if (chunk == null) return -1;
            for (var i = 0; i < generator.SpawnedChunks.Count; i++)
                if (generator.SpawnedChunks[i] == chunk) return i;
            return -1;
        }
    }
}

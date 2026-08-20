using System;
using UnityEngine;

namespace Platformer.PCG {
    [CreateAssetMenu(menuName = "Platformer/PCG/Generation Config", fileName = "LevelGenerationConfig")]
    public sealed class LevelGenerationConfig : ScriptableObject {
        [SerializeField] PlatformChunkData[] chunks = Array.Empty<PlatformChunkData>();
        [SerializeField, Min(1)] int initialChunkCount = 12;
        [SerializeField, Min(1)] int chunksAhead = 4;
        [SerializeField, Min(1)] int maximumGenerationAttempts = 20;
        [SerializeField] AnimationCurve difficultyByProgress =
            new AnimationCurve(new Keyframe(0f, 0.15f), new Keyframe(1f, 0.7f));
        [SerializeField, Min(1)] int checkpointInterval = 5;
        [SerializeField, Min(0)] int recoveryChunkCooldown = 3;
        [SerializeField, Min(0f)] float overlapPadding = 0.08f;
        [SerializeField, Min(1)] int maximumConsecutiveCategory = 2;
        [Header("Reachability")]
        [SerializeField, Range(0.6f, 1f)] float reachSafetyFactor = 0.82f;
        [SerializeField] bool syncReachFromPlayer = true;
        [Header("Rhythm")]
        [SerializeField] bool useRhythmPlan = true;
        [SerializeField] bool spawnSafeFallback = true;
        [SerializeField] bool decorateWithProjectAssets = true;

        public PlatformChunkData[] Chunks => chunks;
        public int InitialChunkCount => initialChunkCount;
        public int ChunksAhead => chunksAhead;
        public int MaximumGenerationAttempts => maximumGenerationAttempts;
        public int CheckpointInterval => checkpointInterval;
        public int RecoveryChunkCooldown => recoveryChunkCooldown;
        public float OverlapPadding => overlapPadding;
        public int MaximumConsecutiveCategory => maximumConsecutiveCategory;
        public float ReachSafetyFactor => reachSafetyFactor;
        public bool SyncReachFromPlayer => syncReachFromPlayer;
        public bool UseRhythmPlan => useRhythmPlan;
        public bool SpawnSafeFallback => spawnSafeFallback;
        public bool DecorateWithProjectAssets => decorateWithProjectAssets;

        public float DifficultyAt(int chunkIndex, int totalChunks) {
            var progress = totalChunks <= 1 ? 0f : chunkIndex / (float)(totalChunks - 1);
            return Mathf.Clamp01(difficultyByProgress.Evaluate(progress));
        }

        public void Configure(PlatformChunkData[] chunkLibrary, int chunkCount = 12) {
            chunks = chunkLibrary ?? Array.Empty<PlatformChunkData>();
            initialChunkCount = Mathf.Max(1, chunkCount);
        }
    }
}

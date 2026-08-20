using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class LevelGenerator : MonoBehaviour {
        [Header("Generation")]
        [SerializeField] LevelGenerationConfig config;
        [SerializeField] Transform startAnchor;
        [SerializeField] Transform generatedRoot;
        [SerializeField] int seed = 82431;
        [SerializeField, Min(0)] int chunkCountOverride;
        [SerializeField] bool generateOnStart = true;
        [SerializeField, Range(-0.5f, 0.5f)] float difficultyBias;

        [Header("Spatial grammar")]
        [SerializeField, Range(1, 6)] int maximumConsecutiveFlatChunks = 3;
        [SerializeField, Range(1, 6)] int maximumConsecutiveStraightChunks = 3;
        [SerializeField] float minimumRelativeElevation = -4f;
        [SerializeField] float maximumRelativeElevation = 8f;

        [Header("Player capabilities")]
        [SerializeField] bool hasDoubleJump;
        [SerializeField] bool hasDash;
        [SerializeField] PlayerTraversalCapabilities traversalCapabilities =
            new PlayerTraversalCapabilities(6.5f, 1.8f, 5f, 2.5f);

        readonly List<PlatformChunk> spawnedChunks = new List<PlatformChunk>();
        readonly ChunkSelector selector = new ChunkSelector();
        readonly BoundsOverlapValidator overlapValidator = new BoundsOverlapValidator();

        public int Seed {
            get => seed;
            set => seed = value;
        }

        public GeneratedLevelManifest LastManifest { get; private set; }
        public IReadOnlyList<PlatformChunk> SpawnedChunks => spawnedChunks;
        public LevelGenerationConfig Config => config;
        public float DifficultyBias => difficultyBias;
        public event Action<GeneratedLevelManifest> GenerationFinished;

        void Start() {
            if (generateOnStart) Generate();
        }

        [ContextMenu("Generate Level")]
        public void Generate() {
            Clear();
            var abilities = new PlayerAbilityProfile(hasDoubleJump, hasDash);
            LastManifest = new GeneratedLevelManifest {
                seed = seed,
                hasDoubleJump = abilities.HasDoubleJump,
                hasDash = abilities.HasDash
            };

            if (config == null || config.Chunks == null || config.Chunks.Length == 0) {
                Fail("No LevelGenerationConfig or chunk library assigned.");
                return;
            }

            if (startAnchor == null) {
                Fail("No start anchor assigned.");
                return;
            }

            if (generatedRoot == null) generatedRoot = transform;

            var reach = traversalCapabilities;
            var random = new DeterministicRandom(seed);
            var count = chunkCountOverride > 0 ? chunkCountOverride : config.InitialChunkCount;
            var currentAnchor = startAnchor;
            ChunkCategory? previousCategory = null;
            var consecutiveCategoryCount = 0;
            var consecutiveFlatCount = 0;
            var consecutiveStraightCount = 0;
            var startElevation = startAnchor.position.y;
            var currentElevation = 0f;

            for (var index = 0; index < count; index++) {
                var placed = false;
                var targetDifficulty = Mathf.Clamp01(
                    config.DifficultyAt(index, count) + difficultyBias);
                var role = config.UseRhythmPlan
                    ? PCGRhythmPlanner.RoleAt(index, count)
                    : (PCGRhythmRole?)null;

                for (var attempt = 0; attempt < config.MaximumGenerationAttempts; attempt++) {
                    var data = selector.Select(
                        config.Chunks,
                        index,
                        targetDifficulty,
                        abilities,
                        random,
                        previousCategory,
                        consecutiveCategoryCount,
                        config.MaximumConsecutiveCategory,
                        reach,
                        consecutiveFlatCount >= maximumConsecutiveFlatChunks,
                        consecutiveStraightCount >= maximumConsecutiveStraightChunks,
                        currentElevation,
                        minimumRelativeElevation,
                        maximumRelativeElevation,
                        role);

                    if (data == null) break;

                    var candidate = Instantiate(data.Prefab, generatedRoot);
                    candidate.name = $"{index:00}_{data.ChunkId}";
                    Align(candidate, currentAnchor);

                    if (overlapValidator.OverlapsAny(candidate, spawnedChunks, config.OverlapPadding)) {
                        DestroyGeneratedObject(candidate.gameObject);
                        continue;
                    }

                    spawnedChunks.Add(candidate);
                    LastManifest.chunks.Add(new GeneratedChunkRecord {
                        index = index,
                        chunkId = data.ChunkId,
                        position = candidate.transform.position,
                        rotation = candidate.transform.rotation,
                        targetDifficulty = targetDifficulty,
                        actualDifficulty = data.CompositeDifficulty
                    });

                    if (previousCategory == data.Category) {
                        consecutiveCategoryCount++;
                    } else {
                        previousCategory = data.Category;
                        consecutiveCategoryCount = 1;
                    }

                    consecutiveFlatCount = data.ChangesElevation ? 0 : consecutiveFlatCount + 1;
                    consecutiveStraightCount = data.ChangesDirection ? 0 : consecutiveStraightCount + 1;

                    currentAnchor = candidate.Exits[random.Range(0, candidate.Exits.Count)];
                    currentElevation = currentAnchor.position.y - startElevation;
                    CreateCheckpoint(candidate, currentAnchor, index);
                    placed = true;
                    break;
                }

                if (!placed && config.SpawnSafeFallback)
                    placed = TryPlaceSafeFallback(
                        index,
                        abilities,
                        traversalCapabilities,
                        ref currentAnchor,
                        ref previousCategory,
                        ref consecutiveCategoryCount,
                        ref consecutiveFlatCount,
                        ref consecutiveStraightCount,
                        startElevation,
                        ref currentElevation,
                        targetDifficulty);

                if (!placed) {
                    Fail($"Unable to place chunk {index} after {config.MaximumGenerationAttempts} attempts.");
                    return;
                }
            }

            if (config.DecorateWithProjectAssets)
                PCGExistingAssetPlacer.Decorate(this, new DeterministicRandom(seed ^ 0x51ED));

            LastManifest.completed = true;
            GenerationFinished?.Invoke(LastManifest);
        }

        bool TryPlaceSafeFallback(
            int index,
            PlayerAbilityProfile abilities,
            PlayerTraversalCapabilities reach,
            ref Transform currentAnchor,
            ref ChunkCategory? previousCategory,
            ref int consecutiveCategoryCount,
            ref int consecutiveFlatCount,
            ref int consecutiveStraightCount,
            float startElevation,
            ref float currentElevation,
            float targetDifficulty) {
            var data = selector.SelectSafest(config.Chunks, index, abilities, reach);
            if (data == null) return false;

            var candidate = Instantiate(data.Prefab, generatedRoot);
            candidate.name = $"{index:00}_{data.ChunkId}_safe";
            Align(candidate, currentAnchor);
            if (overlapValidator.OverlapsAny(candidate, spawnedChunks, config.OverlapPadding)) {
                DestroyGeneratedObject(candidate.gameObject);
                return false;
            }

            spawnedChunks.Add(candidate);
            LastManifest.chunks.Add(new GeneratedChunkRecord {
                index = index,
                chunkId = data.ChunkId,
                position = candidate.transform.position,
                rotation = candidate.transform.rotation,
                targetDifficulty = targetDifficulty,
                actualDifficulty = data.CompositeDifficulty
            });
            previousCategory = data.Category;
            consecutiveCategoryCount = 1;
            consecutiveFlatCount = data.ChangesElevation ? 0 : consecutiveFlatCount + 1;
            consecutiveStraightCount = data.ChangesDirection ? 0 : consecutiveStraightCount + 1;
            currentAnchor = candidate.Exits[0];
            currentElevation = currentAnchor.position.y - startElevation;
            CreateCheckpoint(candidate, currentAnchor, index);
            return true;
        }

        public void SetCapabilities(bool doubleJump, bool dash) {
            hasDoubleJump = doubleJump;
            hasDash = dash;
        }

        public void SetTraversalCapabilities(PlayerTraversalCapabilities capabilities) {
            traversalCapabilities = capabilities;
        }

        public void SetDifficultyBias(float value) {
            difficultyBias = Mathf.Clamp(value, -0.5f, 0.5f);
        }

        public void Configure(
            LevelGenerationConfig generationConfig,
            Transform firstAnchor,
            Transform outputRoot,
            int initialSeed = 82431) {
            config = generationConfig;
            startAnchor = firstAnchor;
            generatedRoot = outputRoot;
            seed = initialSeed;
        }

        [ContextMenu("Clear Generated Level")]
        public void Clear() {
            for (var i = spawnedChunks.Count - 1; i >= 0; i--) {
                if (spawnedChunks[i] != null) DestroyGeneratedObject(spawnedChunks[i].gameObject);
            }
            spawnedChunks.Clear();
        }

        static void Align(PlatformChunk chunk, Transform target) {
            var entry = chunk.Entry;
            var rotationDelta = target.rotation * Quaternion.Inverse(entry.rotation);
            chunk.transform.rotation = rotationDelta * chunk.transform.rotation;
            chunk.transform.position += target.position - entry.position;
        }

        static void CreateCheckpoint(PlatformChunk chunk, Transform exit, int index) {
            var checkpointObject = new GameObject($"Checkpoint_{index:00}");
            checkpointObject.transform.SetParent(chunk.transform);
            checkpointObject.transform.SetPositionAndRotation(exit.position + Vector3.up * 1.25f, exit.rotation);
            var trigger = checkpointObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(4f, 2.5f, 1.2f);
            checkpointObject.AddComponent<ChunkCheckpoint>().Configure(index);
        }

        void Fail(string reason) {
            LastManifest.completed = false;
            LastManifest.failureReason = reason;
            Debug.LogError($"PCG generation failed: {reason}", this);
            GenerationFinished?.Invoke(LastManifest);
        }

        static void DestroyGeneratedObject(UnityEngine.Object target) {
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }

        void OnDrawGizmos() {
            if (spawnedChunks.Count == 0) return;
            for (var i = 0; i < spawnedChunks.Count; i++) {
                var chunk = spawnedChunks[i];
                if (chunk == null) continue;
                var difficulty = LastManifest != null && i < LastManifest.chunks.Count
                    ? LastManifest.chunks[i].actualDifficulty
                    : 0.3f;
                Gizmos.color = difficulty < 0.35f
                    ? new Color(0.3f, 0.85f, 0.35f, 0.35f)
                    : difficulty < 0.6f
                        ? new Color(0.95f, 0.82f, 0.2f, 0.35f)
                        : new Color(0.9f, 0.25f, 0.2f, 0.35f);
                var bounds = chunk.CalculateBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
                if (chunk.Entry != null) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(chunk.Entry.position, 0.18f);
                }
                if (chunk.Exits != null) {
                    Gizmos.color = Color.cyan;
                    foreach (var exit in chunk.Exits) {
                        if (exit == null) continue;
                        Gizmos.DrawSphere(exit.position, 0.18f);
                        if (chunk.Entry != null)
                            Gizmos.DrawLine(chunk.Entry.position, exit.position);
                    }
                }
            }

            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.2f);
            var reach = traversalCapabilities.HorizontalReach(new PlayerAbilityProfile(hasDoubleJump, hasDash));
            if (startAnchor != null)
                Gizmos.DrawWireSphere(startAnchor.position, Mathf.Max(1f, reach));
        }
    }
}

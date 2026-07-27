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

        [Header("Player capabilities")]
        [SerializeField] bool hasDoubleJump;
        [SerializeField] bool hasDash;
        [SerializeField] PlayerTraversalCapabilities traversalCapabilities =
            new PlayerTraversalCapabilities(6f, 1.8f, 5f, 2.5f);

        readonly List<PlatformChunk> spawnedChunks = new List<PlatformChunk>();
        readonly ChunkSelector selector = new ChunkSelector();
        readonly BoundsOverlapValidator overlapValidator = new BoundsOverlapValidator();

        public int Seed {
            get => seed;
            set => seed = value;
        }

        public GeneratedLevelManifest LastManifest { get; private set; }
        public IReadOnlyList<PlatformChunk> SpawnedChunks => spawnedChunks;

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

            var random = new DeterministicRandom(seed);
            var count = chunkCountOverride > 0 ? chunkCountOverride : config.InitialChunkCount;
            var currentAnchor = startAnchor;
            ChunkCategory? previousCategory = null;
            var consecutiveCategoryCount = 0;

            for (var index = 0; index < count; index++) {
                var placed = false;
                var targetDifficulty = config.DifficultyAt(index, count);

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
                        traversalCapabilities);

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

                    currentAnchor = candidate.Exits[random.Range(0, candidate.Exits.Count)];
                    CreateCheckpoint(candidate, currentAnchor, index);
                    placed = true;
                    break;
                }

                if (!placed) {
                    Fail($"Unable to place chunk {index} after {config.MaximumGenerationAttempts} attempts.");
                    return;
                }
            }

            LastManifest.completed = true;
        }

        public void SetCapabilities(bool doubleJump, bool dash) {
            hasDoubleJump = doubleJump;
            hasDash = dash;
        }

        public void SetTraversalCapabilities(PlayerTraversalCapabilities capabilities) {
            traversalCapabilities = capabilities;
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
        }

        static void DestroyGeneratedObject(Object target) {
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }
    }
}

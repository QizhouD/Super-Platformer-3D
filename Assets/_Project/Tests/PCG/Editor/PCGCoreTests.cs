using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Platformer.PCG.Tests {
    public sealed class PCGCoreTests {
        readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown() {
            for (var i = cleanup.Count - 1; i >= 0; i--) {
                if (cleanup[i] != null) Object.DestroyImmediate(cleanup[i]);
            }
            cleanup.Clear();
        }

        [Test]
        public void DeterministicRandom_SameSeedProducesSameSequence() {
            var first = new DeterministicRandom(82431);
            var second = new DeterministicRandom(82431);

            for (var i = 0; i < 20; i++) Assert.That(first.Range(0, 10000), Is.EqualTo(second.Range(0, 10000)));
        }

        [Test]
        public void AbilityProfile_RejectsLockedAbilities() {
            var basic = new PlayerAbilityProfile(false, false);
            var advanced = new PlayerAbilityProfile(true, true);

            Assert.That(basic.Supports(AbilityRequirement.None), Is.True);
            Assert.That(basic.Supports(AbilityRequirement.DoubleJump), Is.False);
            Assert.That(basic.Supports(AbilityRequirement.Dash), Is.False);
            Assert.That(advanced.Supports(AbilityRequirement.DoubleJump | AbilityRequirement.Dash), Is.True);
        }

        [Test]
        public void ChunkSelector_FiltersChunksByAbility() {
            var basic = CreateData("basic", AbilityRequirement.None);
            var dash = CreateData("dash", AbilityRequirement.Dash);
            var selector = new ChunkSelector();

            for (var seed = 0; seed < 10; seed++) {
                var selected = selector.Select(
                    new[] { basic, dash },
                    10,
                    0.5f,
                    new PlayerAbilityProfile(false, false),
                    new DeterministicRandom(seed));
                Assert.That(selected, Is.SameAs(basic));
            }
        }

        [Test]
        public void ChunkSelector_SameSeedProducesSameChoice() {
            var first = CreateData("first", AbilityRequirement.None);
            var second = CreateData("second", AbilityRequirement.None);
            var library = new[] { first, second };
            var selectorA = new ChunkSelector();
            var selectorB = new ChunkSelector();

            var selectedA = selectorA.Select(
                library, 0, 0.2f, new PlayerAbilityProfile(), new DeterministicRandom(42));
            var selectedB = selectorB.Select(
                library, 0, 0.2f, new PlayerAbilityProfile(), new DeterministicRandom(42));

            Assert.That(selectedA.ChunkId, Is.EqualTo(selectedB.ChunkId));
        }

        [Test]
        public void ChunkSelector_RespectsMinimumProgress() {
            var available = CreateData("available", AbilityRequirement.None);
            var gated = CreateData("gated", AbilityRequirement.None, ChunkCategory.Basic, 5);
            var selector = new ChunkSelector();

            var selected = selector.Select(
                new[] { available, gated },
                2,
                0.2f,
                new PlayerAbilityProfile(),
                new DeterministicRandom(7));

            Assert.That(selected, Is.SameAs(available));
        }

        [Test]
        public void ChunkSelector_BreaksLongCategoryStreaks() {
            var repeated = CreateData("basic", AbilityRequirement.None, ChunkCategory.Basic);
            var recovery = CreateData("recovery", AbilityRequirement.None, ChunkCategory.Recovery);
            var selector = new ChunkSelector();

            var selected = selector.Select(
                new[] { repeated, recovery },
                4,
                0.2f,
                new PlayerAbilityProfile(),
                new DeterministicRandom(7),
                ChunkCategory.Basic,
                2,
                2);

            Assert.That(selected, Is.SameAs(recovery));
        }

        [Test]
        public void ReachabilityValidator_RejectsGapOutsideBaseMovement() {
            var wideGap = CreateData(
                "wide-gap",
                AbilityRequirement.None,
                horizontalReach: 8f);

            var result = ChunkReachabilityValidator.CanTraverse(
                wideGap,
                new PlayerAbilityProfile(false, false),
                PlayerTraversalCapabilities.LabDefaults);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ReachabilityValidator_AppliesDashAndDoubleJumpBonuses() {
            var abilityGate = CreateData(
                "ability-gate",
                AbilityRequirement.Dash | AbilityRequirement.DoubleJump,
                horizontalReach: 8f,
                verticalReach: 2.2f);

            var result = ChunkReachabilityValidator.CanTraverse(
                abilityGate,
                new PlayerAbilityProfile(true, true),
                PlayerTraversalCapabilities.LabDefaults);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ChunkSelector_FiltersPhysicallyUnreachableChunks() {
            var reachable = CreateData("reachable", AbilityRequirement.None, horizontalReach: 4f);
            var unreachable = CreateData("unreachable", AbilityRequirement.None, horizontalReach: 20f);
            var selector = new ChunkSelector();

            for (var seed = 0; seed < 10; seed++) {
                var selected = selector.Select(
                    new[] { reachable, unreachable },
                    10,
                    0.5f,
                    new PlayerAbilityProfile(false, false),
                    new DeterministicRandom(seed),
                    traversalCapabilities: PlayerTraversalCapabilities.LabDefaults);
                Assert.That(selected, Is.SameAs(reachable));
            }
        }

        [Test]
        public void Manifest_RoundTripsThroughJson() {
            var manifest = new GeneratedLevelManifest {
                seed = 123,
                hasDash = true,
                completed = true,
                chunks = new List<GeneratedChunkRecord> {
                    new GeneratedChunkRecord {
                        index = 0,
                        chunkId = "basic_01",
                        position = new Vector3(1f, 2f, 3f),
                        rotation = Quaternion.identity,
                        targetDifficulty = 0.2f,
                        actualDifficulty = 0.25f
                    }
                }
            };

            var restored = GeneratedLevelManifest.FromJson(manifest.ToJson());

            Assert.That(restored.seed, Is.EqualTo(123));
            Assert.That(restored.hasDash, Is.True);
            Assert.That(restored.chunks, Has.Count.EqualTo(1));
            Assert.That(restored.chunks[0].chunkId, Is.EqualTo("basic_01"));
        }

        [Test]
        public void Manifest_PreservesGenerationFailure() {
            var manifest = new GeneratedLevelManifest {
                seed = 99,
                completed = false,
                failureReason = "No valid chunk"
            };

            var restored = GeneratedLevelManifest.FromJson(manifest.ToJson());

            Assert.That(restored.completed, Is.False);
            Assert.That(restored.failureReason, Is.EqualTo("No valid chunk"));
        }

        [Test]
        public void BoundsValidator_AllowsTouchingChunksButRejectsOverlap() {
            var existing = CreateChunkObject("existing", Vector3.zero, new Vector3(4f, 1f, 4f));
            var touching = CreateChunkObject("touching", new Vector3(0f, 0f, 4f), new Vector3(4f, 1f, 4f));
            var overlapping = CreateChunkObject("overlap", new Vector3(0f, 0f, 3f), new Vector3(4f, 1f, 4f));
            var validator = new BoundsOverlapValidator();

            Assert.That(validator.OverlapsAny(touching, new[] { existing }, 0.05f), Is.False);
            Assert.That(validator.OverlapsAny(overlapping, new[] { existing }, 0.05f), Is.True);
        }

        [Test]
        public void GeneratedLibrary_CreatesTwelveDeterministicChunksWithoutLockedAbilities() {
            var config = AssetDatabase.LoadAssetAtPath<LevelGenerationConfig>(
                "Assets/_Project/PCG/LevelGenerationConfig.asset");
            Assert.That(config, Is.Not.Null, "Run Platformer/PCG/Create First Batch before this integration test.");

            var system = new GameObject("PCG Integration Test");
            cleanup.Add(system);
            var anchor = new GameObject("Start Anchor").transform;
            anchor.SetParent(system.transform);
            var output = new GameObject("Output").transform;
            output.SetParent(system.transform);

            var generator = system.AddComponent<LevelGenerator>();
            generator.Configure(config, anchor, output, 82431);
            generator.SetCapabilities(false, false);
            generator.Generate();

            Assert.That(generator.LastManifest.completed, Is.True, generator.LastManifest.failureReason);
            Assert.That(generator.LastManifest.chunks, Has.Count.EqualTo(12));
            Assert.That(generator.LastManifest.chunks.Exists(record =>
                record.chunkId == "dash_gap_01" || record.chunkId == "double_jump_01"), Is.False);

            var firstManifest = generator.LastManifest.ToJson();
            generator.Generate();
            var secondManifest = generator.LastManifest.ToJson();

            Assert.That(secondManifest, Is.EqualTo(firstManifest));
        }

        PlatformChunkData CreateData(
            string id,
            AbilityRequirement requirement,
            ChunkCategory category = ChunkCategory.Basic,
            int minimumProgress = 0,
            float horizontalReach = 0f,
            float verticalReach = 0f) {
            var chunk = CreateChunkObject(id, Vector3.zero, Vector3.one);
            var data = ScriptableObject.CreateInstance<PlatformChunkData>();
            cleanup.Add(data);
            data.Configure(
                id,
                chunk,
                category,
                requirement,
                0.2f,
                0f,
                0.2f,
                1f,
                minimumProgress,
                horizontalReach,
                verticalReach);
            return data;
        }

        PlatformChunk CreateChunkObject(string name, Vector3 position, Vector3 scale) {
            var root = new GameObject(name);
            cleanup.Add(root);
            root.transform.position = position;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = scale;

            var entry = new GameObject("Entry").transform;
            entry.SetParent(root.transform);
            var exit = new GameObject("Exit").transform;
            exit.SetParent(root.transform);
            exit.localPosition = Vector3.forward * scale.z;

            var chunk = root.AddComponent<PlatformChunk>();
            chunk.Configure(entry, new[] { exit });
            return chunk;
        }
    }
}

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
        public void ChunkSelector_EnforcesRequestedSpatialVariation() {
            var flat = CreateData("flat", AbilityRequirement.None);
            var climbTurn = CreateData(
                "climb-turn",
                AbilityRequirement.None,
                elevationDelta: 2f,
                headingDelta: -90f,
                lateralDelta: -5f);
            var selector = new ChunkSelector();

            var selected = selector.Select(
                new[] { flat, climbTurn },
                4,
                0.5f,
                new PlayerAbilityProfile(),
                new DeterministicRandom(12),
                traversalCapabilities: PlayerTraversalCapabilities.LabDefaults,
                requireElevationChange: true,
                requireDirectionChange: true);

            Assert.That(selected, Is.SameAs(climbTurn));
        }

        [Test]
        public void ChunkSelector_RejectsChunksOutsideElevationEnvelope() {
            var climb = CreateData(
                "climb",
                AbilityRequirement.None,
                elevationDelta: 2f);
            var descend = CreateData(
                "descend",
                AbilityRequirement.None,
                elevationDelta: -2f);
            var selector = new ChunkSelector();

            var selected = selector.Select(
                new[] { climb, descend },
                8,
                0.5f,
                new PlayerAbilityProfile(),
                new DeterministicRandom(4),
                traversalCapabilities: PlayerTraversalCapabilities.LabDefaults,
                requireElevationChange: true,
                currentElevation: 5f,
                minimumElevation: -2.5f,
                maximumElevation: 6f);

            Assert.That(selected, Is.SameAs(descend));
        }

        [Test]
        public void OscillatingPlatform_EvaluatesPauseTravelAndReturnPhases() {
            const float duration = 2f;
            const float pause = 0.5f;

            Assert.That(
                PCGOscillatingPlatform.EvaluateNormalizedPosition(0.25f, duration, pause),
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                PCGOscillatingPlatform.EvaluateNormalizedPosition(1.5f, duration, pause),
                Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(
                PCGOscillatingPlatform.EvaluateNormalizedPosition(2.75f, duration, pause),
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                PCGOscillatingPlatform.EvaluateNormalizedPosition(4f, duration, pause),
                Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void TimedPlatform_EvaluatesTelegraphedCycle() {
            Assert.That(
                PCGTimedPlatform.EvaluateState(1f, 2.5f, 0.8f, 1.4f),
                Is.EqualTo(TimedPlatformState.Visible));
            Assert.That(
                PCGTimedPlatform.EvaluateState(2.7f, 2.5f, 0.8f, 1.4f),
                Is.EqualTo(TimedPlatformState.Warning));
            Assert.That(
                PCGTimedPlatform.EvaluateState(4f, 2.5f, 0.8f, 1.4f),
                Is.EqualTo(TimedPlatformState.Hidden));
            Assert.That(
                PCGTimedPlatform.EvaluateState(4.8f, 2.5f, 0.8f, 1.4f),
                Is.EqualTo(TimedPlatformState.Visible));
        }

        [Test]
        public void AdaptiveDifficultyModel_RanksFastCleanRunAboveSlowFailedRun() {
            var fastClean = PCGAdaptiveDifficultyModel.ScorePerformance(12f, 0);
            var slowFailed = PCGAdaptiveDifficultyModel.ScorePerformance(40f, 2);

            Assert.That(fastClean, Is.GreaterThan(0.8f));
            Assert.That(slowFailed, Is.LessThan(0.1f));
            Assert.That(fastClean, Is.GreaterThan(slowFailed));
        }

        [Test]
        public void AdaptiveDifficultyModel_SmoothsPerformanceSamples() {
            var model = new PCGAdaptiveDifficultyModel(0.5f);

            var updated = model.Update(10f, 0, 0.25f);

            Assert.That(updated, Is.EqualTo(0.625f).Within(0.0001f));
        }

        [Test]
        public void GameAIObservation_ProducesStableTwentyValueVector() {
            var observation = new PCGGameAIObservation {
                playerPosition = new Vector3(1f, 2f, 3f),
                playerVelocity = new Vector3(4f, 0f, 0f),
                cameraForward = Vector3.forward,
                normalizedProgress = 0.5f,
                currentChunkIndex = 7,
                resetCount = 2,
                nextChunkDifficulty = 0.6f,
                adaptiveSkill = 0.7f,
                difficultyBias = 0.08f,
                timedPlatformVisibleRatio = 1f,
                movingPlatformCount = 3,
                visualFrameSequence = 4,
                timestamp = 12f
            };

            var vector = observation.ToVector();

            Assert.That(vector, Has.Length.EqualTo(PCGGameAIObservation.VectorSize));
            Assert.That(vector[0], Is.EqualTo(1f));
            Assert.That(vector[9], Is.EqualTo(0.5f));
            Assert.That(vector[18], Is.EqualTo(4f));
        }

        [Test]
        public void Telemetry_UsesBoundedEventBufferAndExportsJson() {
            var telemetryObject = new GameObject("Telemetry");
            cleanup.Add(telemetryObject);
            var telemetry = telemetryObject.AddComponent<PCGRunTelemetry>();

            for (var i = 0; i < 300; i++)
                telemetry.Record(
                    PCGTelemetryEventType.CheckpointReached,
                    i,
                    new Vector3(0f, 0f, i));

            Assert.That(telemetry.Events, Has.Count.EqualTo(256));
            Assert.That(telemetry.Events[0].chunkIndex, Is.EqualTo(44));
            Assert.That(telemetry.ToJson(), Does.Contain("\"events\""));
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
        public void GeneratedLibrary_CreatesSixteenDeterministicChunksWithoutLockedAbilities() {
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
            Assert.That(generator.LastManifest.chunks, Has.Count.EqualTo(16));
            Assert.That(generator.LastManifest.chunks.Exists(record =>
                record.chunkId == "dash_gap_01" || record.chunkId == "double_jump_01"), Is.False);

            var firstPosition = generator.LastManifest.chunks[0].position;
            var hasLateralVariation = false;
            var hasElevationVariation = false;
            var hasHeadingVariation = false;
            foreach (var record in generator.LastManifest.chunks) {
                hasLateralVariation |= Mathf.Abs(record.position.x - firstPosition.x) > 0.1f;
                hasElevationVariation |= Mathf.Abs(record.position.y - firstPosition.y) > 0.1f;
                hasHeadingVariation |= Mathf.Abs(Mathf.DeltaAngle(
                    record.rotation.eulerAngles.y,
                    generator.LastManifest.chunks[0].rotation.eulerAngles.y)) > 1f;
            }
            Assert.That(hasLateralVariation, Is.True);
            Assert.That(hasElevationVariation, Is.True);
            Assert.That(hasHeadingVariation, Is.True);

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
            float verticalReach = 0f,
            float elevationDelta = 0f,
            float headingDelta = 0f,
            float lateralDelta = 0f) {
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
                verticalReach,
                elevationDelta,
                headingDelta,
                lateralDelta);
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

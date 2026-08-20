using NUnit.Framework;
using UnityEngine;

namespace Platformer.PCG.Tests {
    public sealed class PCGLabGenerationTests {
        [Test]
        public void ReachModel_AppliesSafetyFactorBelowRawJumpEnvelope() {
            var rawHeight = PCGPlayerReachModel.MaxJumpHeight(10f, 0.5f, 3f);
            var comfortable = PCGPlayerReachModel.ComfortableJumpHeight(10f, 3f);
            var reach = PCGPlayerReachModel.FromJumpProfile(300f, 10f, 0.5f, 3f, 5f, 1f, 0.82f);

            Assert.That(rawHeight, Is.GreaterThan(comfortable));
            Assert.That(reach.BaseVerticalReach, Is.LessThan(comfortable));
            Assert.That(reach.BaseHorizontalReach, Is.GreaterThan(4f));
            Assert.That(reach.BaseHorizontalReach, Is.LessThan(12f));
        }

        [Test]
        public void RhythmPlanner_CreatesStartChallengeAndRecoveryBeats() {
            var sawStart = false;
            var sawChallenge = false;
            var sawRecovery = false;
            for (var i = 0; i < 16; i++) {
                var role = PCGRhythmPlanner.RoleAt(i, 16);
                sawStart |= role == PCGRhythmRole.Start || role == PCGRhythmRole.Easy;
                sawChallenge |= role == PCGRhythmRole.Challenge;
                sawRecovery |= role == PCGRhythmRole.Recovery || role == PCGRhythmRole.Reward;
            }

            Assert.That(PCGRhythmPlanner.RoleAt(0, 16), Is.EqualTo(PCGRhythmRole.Start));
            Assert.That(sawStart, Is.True);
            Assert.That(sawChallenge, Is.True);
            Assert.That(sawRecovery, Is.True);
        }

        [Test]
        public void RhythmPlanner_MakesEasyPreferBasicOverChallengeTypes() {
            var basic = PCGRhythmPlanner.CategoryMultiplier(PCGRhythmRole.Easy, ChunkCategory.Basic, 0.2f);
            var timed = PCGRhythmPlanner.CategoryMultiplier(PCGRhythmRole.Easy, ChunkCategory.Timed, 0.2f);
            var challengeTimed = PCGRhythmPlanner.CategoryMultiplier(PCGRhythmRole.Challenge, ChunkCategory.Timed, 0.7f);

            Assert.That(basic, Is.GreaterThan(timed));
            Assert.That(challengeTimed, Is.GreaterThan(timed));
        }

        [Test]
        public void ChunkSelector_SelectSafestPrefersEasyReachableChunk() {
            var recovery = ScriptableObject.CreateInstance<PlatformChunkData>();
            var hard = ScriptableObject.CreateInstance<PlatformChunkData>();
            var recoveryChunk = CreateChunk("recovery");
            var hardChunk = CreateChunk("hard");
            recovery.Configure("recovery", recoveryChunk, ChunkCategory.Recovery, AbilityRequirement.None, 0.05f, 0f, 0.05f);
            hard.Configure("hard", hardChunk, ChunkCategory.Combat, AbilityRequirement.None, 0.8f, 0.6f, 0.5f);

            var safest = new ChunkSelector().SelectSafest(
                new[] { hard, recovery },
                0,
                new PlayerAbilityProfile());

            Assert.That(safest, Is.SameAs(recovery));
            Object.DestroyImmediate(recovery);
            Object.DestroyImmediate(hard);
            Object.DestroyImmediate(recoveryChunk.gameObject);
            Object.DestroyImmediate(hardChunk.gameObject);
        }

        [Test]
        public void ChunkSelector_RhythmWeightsStayDeterministic() {
            var first = ScriptableObject.CreateInstance<PlatformChunkData>();
            var second = ScriptableObject.CreateInstance<PlatformChunkData>();
            var a = CreateChunk("a");
            var b = CreateChunk("b");
            first.Configure("a", a, ChunkCategory.Basic, AbilityRequirement.None, 0.2f, 0f, 0.2f);
            second.Configure("b", b, ChunkCategory.Timed, AbilityRequirement.None, 0.5f, 0f, 0.5f);
            var library = new[] { first, second };

            var selectedA = new ChunkSelector().Select(
                library, 8, 0.5f, new PlayerAbilityProfile(), new DeterministicRandom(77),
                rhythmRole: PCGRhythmRole.Challenge);
            var selectedB = new ChunkSelector().Select(
                library, 8, 0.5f, new PlayerAbilityProfile(), new DeterministicRandom(77),
                rhythmRole: PCGRhythmRole.Challenge);

            Assert.That(selectedA.ChunkId, Is.EqualTo(selectedB.ChunkId));
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        static PlatformChunk CreateChunk(string name) {
            var root = new GameObject(name);
            var entry = new GameObject("Entry").transform;
            entry.SetParent(root.transform);
            var exit = new GameObject("Exit").transform;
            exit.SetParent(root.transform);
            exit.localPosition = Vector3.forward * 4f;
            var chunk = root.AddComponent<PlatformChunk>();
            chunk.Configure(entry, new[] { exit });
            return chunk;
        }
    }
}

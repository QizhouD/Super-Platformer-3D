using System.Collections.Generic;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class ChunkSelector {
        readonly List<PlatformChunkData> candidates = new List<PlatformChunkData>();
        readonly List<float> weights = new List<float>();

        public PlatformChunkData Select(
            IReadOnlyList<PlatformChunkData> library,
            int progress,
            float targetDifficulty,
            PlayerAbilityProfile abilities,
            DeterministicRandom random,
            ChunkCategory? previousCategory = null,
            int consecutiveCategoryCount = 0,
            int maximumConsecutiveCategory = 2,
            PlayerTraversalCapabilities? traversalCapabilities = null,
            bool requireElevationChange = false,
            bool requireDirectionChange = false,
            float currentElevation = 0f,
            float minimumElevation = float.NegativeInfinity,
            float maximumElevation = float.PositiveInfinity,
            PCGRhythmRole? rhythmRole = null) {
            CollectCandidates(
                library,
                progress,
                targetDifficulty,
                abilities,
                previousCategory,
                consecutiveCategoryCount,
                maximumConsecutiveCategory,
                traversalCapabilities,
                requireElevationChange,
                requireDirectionChange,
                currentElevation,
                minimumElevation,
                maximumElevation,
                rhythmRole);

            if (candidates.Count == 0) return null;

            var totalWeight = 0f;
            for (var i = 0; i < weights.Count; i++) totalWeight += weights[i];

            var roll = (float)random.Value() * totalWeight;
            for (var i = 0; i < candidates.Count; i++) {
                roll -= weights[i];
                if (roll <= 0f) return candidates[i];
            }

            return candidates[candidates.Count - 1];
        }

        public PlatformChunkData SelectSafest(
            IReadOnlyList<PlatformChunkData> library,
            int progress,
            PlayerAbilityProfile abilities,
            PlayerTraversalCapabilities? traversalCapabilities = null) {
            PlatformChunkData safest = null;
            var safestScore = float.PositiveInfinity;
            for (var i = 0; i < library.Count; i++) {
                var chunk = library[i];
                if (!IsLegal(chunk, progress, abilities, traversalCapabilities)) continue;
                var score = chunk.CompositeDifficulty;
                if (chunk.Category == ChunkCategory.Recovery) score -= 0.15f;
                if (chunk.Category == ChunkCategory.Basic) score -= 0.08f;
                if (score < safestScore) {
                    safestScore = score;
                    safest = chunk;
                }
            }

            return safest;
        }

        void CollectCandidates(
            IReadOnlyList<PlatformChunkData> library,
            int progress,
            float targetDifficulty,
            PlayerAbilityProfile abilities,
            ChunkCategory? previousCategory,
            int consecutiveCategoryCount,
            int maximumConsecutiveCategory,
            PlayerTraversalCapabilities? traversalCapabilities,
            bool requireElevationChange,
            bool requireDirectionChange,
            float currentElevation,
            float minimumElevation,
            float maximumElevation,
            PCGRhythmRole? rhythmRole) {
            candidates.Clear();
            weights.Clear();

            for (var i = 0; i < library.Count; i++) {
                var chunk = library[i];
                if (!IsLegal(chunk, progress, abilities, traversalCapabilities)) continue;
                if (requireElevationChange && !chunk.ChangesElevation) continue;
                if (requireDirectionChange && !chunk.ChangesDirection) continue;
                var predictedElevation = currentElevation + chunk.ElevationDelta;
                if (predictedElevation < minimumElevation || predictedElevation > maximumElevation) continue;
                if (previousCategory.HasValue &&
                    chunk.Category == previousCategory.Value &&
                    consecutiveCategoryCount >= maximumConsecutiveCategory) continue;

                var difficultyAffinity = Mathf.Max(0.05f, 1f - Mathf.Abs(chunk.CompositeDifficulty - targetDifficulty));
                var rhythm = rhythmRole.HasValue
                    ? PCGRhythmPlanner.CategoryMultiplier(rhythmRole.Value, chunk.Category, targetDifficulty)
                    : 1f;
                candidates.Add(chunk);
                weights.Add(chunk.Weight * difficultyAffinity * rhythm);
            }
        }

        static bool IsLegal(
            PlatformChunkData chunk,
            int progress,
            PlayerAbilityProfile abilities,
            PlayerTraversalCapabilities? traversalCapabilities) {
            if (chunk == null || chunk.Prefab == null || !chunk.Prefab.IsConfigured) return false;
            if (chunk.MinimumProgress > progress) return false;
            if (!abilities.Supports(chunk.RequiredAbility)) return false;
            if (traversalCapabilities.HasValue &&
                !ChunkReachabilityValidator.CanTraverse(chunk, abilities, traversalCapabilities.Value))
                return false;
            return true;
        }

    }
}

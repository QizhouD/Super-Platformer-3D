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
            float maximumElevation = float.PositiveInfinity) {
            candidates.Clear();
            weights.Clear();

            for (var i = 0; i < library.Count; i++) {
                var chunk = library[i];
                if (chunk == null || chunk.Prefab == null || !chunk.Prefab.IsConfigured) continue;
                if (chunk.MinimumProgress > progress) continue;
                if (!abilities.Supports(chunk.RequiredAbility)) continue;
                if (traversalCapabilities.HasValue &&
                    !ChunkReachabilityValidator.CanTraverse(chunk, abilities, traversalCapabilities.Value)) continue;
                if (requireElevationChange && !chunk.ChangesElevation) continue;
                if (requireDirectionChange && !chunk.ChangesDirection) continue;
                var predictedElevation = currentElevation + chunk.ElevationDelta;
                if (predictedElevation < minimumElevation || predictedElevation > maximumElevation) continue;
                if (previousCategory.HasValue &&
                    chunk.Category == previousCategory.Value &&
                    consecutiveCategoryCount >= maximumConsecutiveCategory) continue;

                var difficultyAffinity = Mathf.Max(0.05f, 1f - Mathf.Abs(chunk.CompositeDifficulty - targetDifficulty));
                candidates.Add(chunk);
                weights.Add(chunk.Weight * difficultyAffinity);
            }

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
    }
}

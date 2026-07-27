using System.Collections.Generic;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class BoundsOverlapValidator {
        public bool OverlapsAny(PlatformChunk candidate, IReadOnlyList<PlatformChunk> existing, float padding) {
            var candidateBounds = Shrink(candidate.CalculateBounds(), padding);
            for (var i = 0; i < existing.Count; i++) {
                if (existing[i] == null) continue;
                var existingBounds = Shrink(existing[i].CalculateBounds(), padding);
                if (candidateBounds.Intersects(existingBounds)) return true;
            }
            return false;
        }

        static Bounds Shrink(Bounds bounds, float amount) {
            var reduction = Vector3.one * Mathf.Max(0f, amount * 2f);
            bounds.size = Vector3.Max(Vector3.zero, bounds.size - reduction);
            return bounds;
        }
    }
}

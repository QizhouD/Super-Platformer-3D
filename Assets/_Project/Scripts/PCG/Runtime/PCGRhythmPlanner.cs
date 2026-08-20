using UnityEngine;

namespace Platformer.PCG {
    public enum PCGRhythmRole {
        Start,
        Easy,
        Normal,
        Challenge,
        Recovery,
        Reward,
        Landmark
    }

    public static class PCGRhythmPlanner {
        public static PCGRhythmRole RoleAt(int index, int total) {
            if (total <= 1) return PCGRhythmRole.Easy;
            var progress = index / (float)Mathf.Max(1, total - 1);
            if (index <= 1) return PCGRhythmRole.Start;
            if (progress < 0.22f) return PCGRhythmRole.Easy;
            if (index == total - 1) return PCGRhythmRole.Reward;
            if (index == Mathf.Max(3, total / 2)) return PCGRhythmRole.Recovery;
            if (index == Mathf.Max(2, (total * 3) / 4)) return PCGRhythmRole.Reward;
            if (index == Mathf.Max(4, total / 3)) return PCGRhythmRole.Landmark;
            if (progress < 0.4f) return PCGRhythmRole.Normal;
            if (progress < 0.55f) return PCGRhythmRole.Challenge;
            if (progress < 0.65f) return PCGRhythmRole.Recovery;
            if (progress < 0.85f) return PCGRhythmRole.Challenge;
            return PCGRhythmRole.Normal;
        }

        public static float CategoryMultiplier(PCGRhythmRole role, ChunkCategory category, float difficulty) {
            var challengeBias = Mathf.Lerp(0.7f, 1.35f, Mathf.Clamp01(difficulty));
            switch (role) {
                case PCGRhythmRole.Start:
                case PCGRhythmRole.Easy:
                    return category == ChunkCategory.Basic ? 2.4f :
                        category == ChunkCategory.Recovery ? 1.3f :
                        category == ChunkCategory.Exploration ? 1.1f : 0.35f;
                case PCGRhythmRole.Recovery:
                    return category == ChunkCategory.Recovery ? 2.8f :
                        category == ChunkCategory.Basic ? 1.4f : 0.28f;
                case PCGRhythmRole.Reward:
                    return category == ChunkCategory.Recovery ? 2.2f :
                        category == ChunkCategory.Basic ? 1.3f :
                        category == ChunkCategory.Exploration ? 1.1f : 0.4f;
                case PCGRhythmRole.Landmark:
                    return category == ChunkCategory.Exploration ? 1.8f :
                        category == ChunkCategory.Combat ? 1.2f :
                        category == ChunkCategory.Recovery ? 1.1f : 0.7f;
                case PCGRhythmRole.Challenge:
                    return category == ChunkCategory.Moving ? 1.5f * challengeBias :
                        category == ChunkCategory.Timed ? 1.45f * challengeBias :
                        category == ChunkCategory.AbilityGate ? 1.25f * challengeBias :
                        category == ChunkCategory.Combat ? 1.15f * challengeBias :
                        category == ChunkCategory.Exploration ? 1.05f :
                        0.55f;
                default:
                    return category == ChunkCategory.Basic ? 1.15f :
                        category == ChunkCategory.Exploration ? 1.2f :
                        category == ChunkCategory.Moving ? 0.95f * challengeBias :
                        category == ChunkCategory.Timed ? 0.8f * challengeBias :
                        1f;
            }
        }
    }
}

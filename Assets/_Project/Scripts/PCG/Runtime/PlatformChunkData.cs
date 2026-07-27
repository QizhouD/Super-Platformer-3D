using UnityEngine;

namespace Platformer.PCG {
    [CreateAssetMenu(menuName = "Platformer/PCG/Chunk Data", fileName = "ChunkData")]
    public sealed class PlatformChunkData : ScriptableObject {
        [SerializeField] string chunkId;
        [SerializeField] PlatformChunk prefab;
        [SerializeField] ChunkCategory category;
        [SerializeField] AbilityRequirement requiredAbility;
        [SerializeField, Range(0f, 1f)] float traversalDifficulty = 0.2f;
        [SerializeField, Range(0f, 1f)] float combatDifficulty;
        [SerializeField, Range(0f, 1f)] float precisionDifficulty;
        [SerializeField, Range(0f, 1f)] float explorationScore;
        [SerializeField, Min(0.01f)] float weight = 1f;
        [SerializeField, Min(0)] int minimumProgress;
        [SerializeField, Min(0f)] float requiredHorizontalReach;
        [SerializeField, Min(0f)] float requiredVerticalReach;
        [SerializeField] bool allowRotation;
        [SerializeField] string[] tags = new string[0];

        public string ChunkId => chunkId;
        public PlatformChunk Prefab => prefab;
        public ChunkCategory Category => category;
        public AbilityRequirement RequiredAbility => requiredAbility;
        public float TraversalDifficulty => traversalDifficulty;
        public float CombatDifficulty => combatDifficulty;
        public float PrecisionDifficulty => precisionDifficulty;
        public float ExplorationScore => explorationScore;
        public float Weight => weight;
        public int MinimumProgress => minimumProgress;
        public float RequiredHorizontalReach => requiredHorizontalReach;
        public float RequiredVerticalReach => requiredVerticalReach;
        public bool AllowRotation => allowRotation;
        public string[] Tags => tags;

        public float CompositeDifficulty =>
            Mathf.Clamp01(traversalDifficulty * 0.55f + precisionDifficulty * 0.3f + combatDifficulty * 0.15f);

        public void Configure(
            string id,
            PlatformChunk chunkPrefab,
            ChunkCategory chunkCategory,
            AbilityRequirement ability,
            float traversal,
            float combat,
            float precision,
            float selectionWeight = 1f,
            int minProgress = 0,
            float horizontalReach = 0f,
            float verticalReach = 0f) {
            chunkId = id;
            prefab = chunkPrefab;
            category = chunkCategory;
            requiredAbility = ability;
            traversalDifficulty = Mathf.Clamp01(traversal);
            combatDifficulty = Mathf.Clamp01(combat);
            precisionDifficulty = Mathf.Clamp01(precision);
            weight = Mathf.Max(0.01f, selectionWeight);
            minimumProgress = Mathf.Max(0, minProgress);
            requiredHorizontalReach = Mathf.Max(0f, horizontalReach);
            requiredVerticalReach = Mathf.Max(0f, verticalReach);
        }
    }
}

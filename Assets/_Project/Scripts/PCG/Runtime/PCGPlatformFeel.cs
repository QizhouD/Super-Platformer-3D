using UnityEngine;

namespace Platformer.PCG {
    public static class PCGPlatformFeel {
        public const float WalkableOverlap = 0.22f;

        public static void CloseWalkableSeams(LevelGenerator generator) {
            if (generator == null) return;
            foreach (var chunk in generator.SpawnedChunks) {
                if (chunk != null) ExpandColliders(chunk.transform);
            }

            var start = GameObject.Find("Start Platform");
            if (start != null) ExpandColliders(start.transform);
        }

        public static Vector3 ExpandBoxSize(Vector3 localSize, Vector3 lossyScale, float worldPadding) {
            return new Vector3(
                localSize.x + WorldPaddingToLocal(worldPadding, lossyScale.x),
                localSize.y,
                localSize.z + WorldPaddingToLocal(worldPadding, lossyScale.z));
        }

        static void ExpandColliders(Transform root) {
            var boxes = root.GetComponentsInChildren<BoxCollider>();
            foreach (var box in boxes) {
                if (box == null || box.isTrigger) continue;
                if (box.GetComponent<PCGWalkableExpanded>() != null) continue;
                box.size = ExpandBoxSize(box.size, box.transform.lossyScale, WalkableOverlap);
                box.gameObject.AddComponent<PCGWalkableExpanded>();
            }
        }

        static float WorldPaddingToLocal(float worldPadding, float scale) =>
            worldPadding / Mathf.Max(0.01f, Mathf.Abs(scale));
    }

    sealed class PCGWalkableExpanded : MonoBehaviour { }
}

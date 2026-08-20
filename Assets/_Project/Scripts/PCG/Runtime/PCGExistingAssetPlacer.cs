using UnityEngine;

namespace Platformer.PCG {
    public static class PCGExistingAssetPlacer {
        const string CratePath = "Assets/_Project/Prefabs/crate-box.prefab";
        const string ChestPath = "Assets/_Project/Prefabs/chest.prefab";

        public static void Decorate(LevelGenerator generator, DeterministicRandom random) {
            if (generator == null) return;
            var crate = LoadPrefab(CratePath);
            var chest = LoadPrefab(ChestPath);
            if (crate == null && chest == null) return;

            foreach (var chunk in generator.SpawnedChunks) {
                if (chunk == null) continue;
                foreach (var slot in chunk.EnemySlots) {
                    if (slot == null || crate == null || random.Value() > 0.7d) continue;
                    Spawn(crate, slot, chunk.transform);
                }

                foreach (var slot in chunk.CollectibleSlots) {
                    if (slot == null || chest == null || random.Value() > 0.85d) continue;
                    Spawn(chest, slot, chunk.transform);
                }
            }
        }

        static void Spawn(GameObject prefab, Transform slot, Transform parent) {
            var instance = Object.Instantiate(prefab, slot.position, slot.rotation, parent);
            instance.name = prefab.name;
            foreach (var agent in instance.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                agent.enabled = false;
        }

        static GameObject LoadPrefab(string path) {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
            return null;
#endif
        }
    }
}

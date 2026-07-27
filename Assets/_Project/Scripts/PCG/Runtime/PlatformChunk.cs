using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class PlatformChunk : MonoBehaviour {
        [SerializeField] Transform entry;
        [SerializeField] Transform[] exits = Array.Empty<Transform>();
        [SerializeField] Transform[] enemySlots = Array.Empty<Transform>();
        [SerializeField] Transform[] collectibleSlots = Array.Empty<Transform>();

        public Transform Entry => entry;
        public IReadOnlyList<Transform> Exits => exits;
        public IReadOnlyList<Transform> EnemySlots => enemySlots;
        public IReadOnlyList<Transform> CollectibleSlots => collectibleSlots;

        public bool IsConfigured => entry != null && exits != null && exits.Length > 0;

        public Bounds CalculateBounds() {
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0) {
                var bounds = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                return bounds;
            }

            var colliders = GetComponentsInChildren<Collider>();
            if (colliders.Length > 0) {
                var bounds = colliders[0].bounds;
                for (var i = 1; i < colliders.Length; i++) bounds.Encapsulate(colliders[i].bounds);
                return bounds;
            }

            return new Bounds(transform.position, Vector3.zero);
        }

        public void Configure(
            Transform entryPoint,
            Transform[] exitPoints,
            Transform[] enemySpawnPoints = null,
            Transform[] collectibleSpawnPoints = null) {
            entry = entryPoint;
            exits = exitPoints ?? Array.Empty<Transform>();
            enemySlots = enemySpawnPoints ?? Array.Empty<Transform>();
            collectibleSlots = collectibleSpawnPoints ?? Array.Empty<Transform>();
        }

        void OnDrawGizmosSelected() {
            if (entry != null) {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(entry.position, 0.2f);
                Gizmos.DrawRay(entry.position, entry.forward);
            }

            if (exits == null) return;
            Gizmos.color = Color.cyan;
            foreach (var exit in exits) {
                if (exit == null) continue;
                Gizmos.DrawSphere(exit.position, 0.2f);
                Gizmos.DrawRay(exit.position, exit.forward);
            }
        }
    }
}

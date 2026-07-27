using System;
using UnityEngine;

namespace Platformer.PCG {
    [RequireComponent(typeof(Collider))]
    public sealed class ChunkCheckpoint : MonoBehaviour {
        public static event Action<int, Vector3> Reached;

        [SerializeField] int chunkIndex;
        bool activated;

        public void Configure(int index) {
            chunkIndex = index;
        }

        void OnTriggerEnter(Collider other) {
            if (activated || !other.transform.root.CompareTag("Player")) return;
            activated = true;
            Reached?.Invoke(chunkIndex, transform.position);
        }
    }
}

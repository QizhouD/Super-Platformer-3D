using UnityEngine;

namespace Platformer {
    /// <summary>
    /// Authored-level checkpoint. Player 2 on PCG_LAB still uses ChunkCheckpoint + PCGRunController.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class GameplayCheckpoint : MonoBehaviour {
        [SerializeField] string playerTag = "Player";
        [SerializeField] Transform spawnPoint;
        [SerializeField] float fallThreshold = -12f;

        static Vector3 respawnPosition;
        static Quaternion respawnRotation;
        static bool hasRespawn;
        static Rigidbody trackedBody;
        static Transform trackedPlayer;

        void Reset() {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other) {
            if (!other.transform.root.CompareTag(playerTag)) return;
            var root = other.transform.root;
            trackedPlayer = root;
            trackedBody = root.GetComponent<Rigidbody>();
            var point = spawnPoint != null ? spawnPoint : transform;
            respawnPosition = point.position;
            respawnRotation = point.rotation;
            hasRespawn = true;
        }

        void LateUpdate() {
            if (!hasRespawn || trackedPlayer == null) return;
            if (trackedPlayer.position.y > fallThreshold) return;
            Teleport(trackedPlayer, trackedBody, respawnPosition, respawnRotation);
        }

        static void Teleport(Transform player, Rigidbody body, Vector3 position, Quaternion rotation) {
            if (body != null) {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.position = position;
                body.rotation = rotation;
            } else {
                player.SetPositionAndRotation(position, rotation);
            }
        }
    }
}

using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGRunController : MonoBehaviour {
        [SerializeField] Transform player;
        [SerializeField] Transform initialSpawn;
        [SerializeField] float fallThreshold = -12f;

        Rigidbody playerBody;
        Vector3 respawnPosition;
        Quaternion respawnRotation;

        public int FurthestCheckpoint { get; private set; } = -1;
        public int ResetCount { get; private set; }

        void Awake() {
            if (player == null) {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }
            if (player != null) playerBody = player.GetComponent<Rigidbody>();
            if (initialSpawn != null) {
                respawnPosition = initialSpawn.position;
                respawnRotation = initialSpawn.rotation;
            } else if (player != null) {
                respawnPosition = player.position;
                respawnRotation = player.rotation;
            }
        }

        void OnEnable() {
            ChunkCheckpoint.Reached += HandleCheckpointReached;
        }

        void OnDisable() {
            ChunkCheckpoint.Reached -= HandleCheckpointReached;
        }

        void Update() {
            if (player != null && player.position.y < fallThreshold) Respawn();
        }

        public void Configure(Transform labPlayer, Transform spawn) {
            player = labPlayer;
            initialSpawn = spawn;
            playerBody = player != null ? player.GetComponent<Rigidbody>() : null;
        }

        public void Respawn() {
            if (player == null) return;
            ResetCount++;
            if (playerBody != null) {
                playerBody.velocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
                playerBody.position = respawnPosition;
                playerBody.rotation = respawnRotation;
            } else {
                player.SetPositionAndRotation(respawnPosition, respawnRotation);
            }
        }

        void HandleCheckpointReached(int chunkIndex, Vector3 position) {
            if (chunkIndex <= FurthestCheckpoint) return;
            FurthestCheckpoint = chunkIndex;
            respawnPosition = position + Vector3.up * 1.5f;
            respawnRotation = Quaternion.identity;
        }
    }
}

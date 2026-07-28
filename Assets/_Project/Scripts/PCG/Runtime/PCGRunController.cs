using System;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGRunController : MonoBehaviour {
        [SerializeField] Transform player;
        [SerializeField] Transform initialSpawn;
        [SerializeField] float fallThreshold = -12f;

        Rigidbody playerBody;
        Vector3 initialSpawnPosition;
        Quaternion initialSpawnRotation;
        Vector3 respawnPosition;
        Quaternion respawnRotation;

        public int FurthestCheckpoint { get; private set; } = -1;
        public int ResetCount { get; private set; }
        public event Action<int, Vector3> CheckpointReached;
        public event Action<int, Vector3> PlayerRespawned;

        void Awake() {
            if (player == null) {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }
            if (player != null) playerBody = player.GetComponent<Rigidbody>();
            if (initialSpawn != null) {
                initialSpawnPosition = initialSpawn.position;
                initialSpawnRotation = initialSpawn.rotation;
            } else if (player != null) {
                initialSpawnPosition = player.position;
                initialSpawnRotation = player.rotation;
            }
            respawnPosition = initialSpawnPosition;
            respawnRotation = initialSpawnRotation;
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
            if (initialSpawn != null) {
                initialSpawnPosition = initialSpawn.position;
                initialSpawnRotation = initialSpawn.rotation;
                respawnPosition = initialSpawnPosition;
                respawnRotation = initialSpawnRotation;
            }
        }

        public void RestartRun(bool resetCounters = true) {
            FurthestCheckpoint = -1;
            if (resetCounters) ResetCount = 0;
            respawnPosition = initialSpawnPosition;
            respawnRotation = initialSpawnRotation;
            TeleportPlayer(initialSpawnPosition, initialSpawnRotation);
        }

        public void Respawn() {
            if (player == null) return;
            ResetCount++;
            TeleportPlayer(respawnPosition, respawnRotation);
            PlayerRespawned?.Invoke(ResetCount, respawnPosition);
        }

        void TeleportPlayer(Vector3 position, Quaternion rotation) {
            if (player == null) return;
            if (playerBody != null) {
                playerBody.velocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
                playerBody.position = position;
                playerBody.rotation = rotation;
            } else {
                player.SetPositionAndRotation(position, rotation);
            }
        }

        void HandleCheckpointReached(int chunkIndex, Vector3 position) {
            if (chunkIndex <= FurthestCheckpoint) return;
            FurthestCheckpoint = chunkIndex;
            respawnPosition = position + Vector3.up * 1.5f;
            respawnRotation = Quaternion.identity;
            CheckpointReached?.Invoke(chunkIndex, position);
        }
    }
}

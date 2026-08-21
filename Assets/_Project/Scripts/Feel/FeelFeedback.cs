using System.Collections;
using UnityEngine;

namespace Platformer {
    /// <summary>
    /// Lightweight jump / land / collect juice. Attach to the player or a scene object.
    /// </summary>
    public sealed class FeelFeedback : MonoBehaviour {
        public static FeelFeedback Instance { get; private set; }

        [SerializeField] Transform shakeTarget;
        [SerializeField] GameObject jumpBurst;
        [SerializeField] GameObject landBurst;
        [SerializeField] GameObject collectBurst;
        [SerializeField, Min(0f)] float landShake = 0.08f;
        [SerializeField, Min(0f)] float collectShake = 0.05f;
        [SerializeField, Min(0.01f)] float shakeTime = 0.12f;

        PlayerController player;
        Vector3 shakeRest;
        Coroutine shakeRoutine;

        void Awake() {
            Instance = this;
            player = GetComponent<PlayerController>();
        }

        void OnEnable() {
            if (player == null) player = GetComponent<PlayerController>();
            if (player == null) return;
            player.Jumped += HandleJump;
            player.Landed += HandleLand;
        }

        void OnDisable() {
            if (player == null) return;
            player.Jumped -= HandleJump;
            player.Landed -= HandleLand;
        }

        void OnDestroy() {
            if (Instance == this) Instance = null;
        }

        public static void CollectAt(Vector3 position) {
            if (Instance != null) Instance.PlayCollect(position);
        }

        void HandleJump() {
            Spawn(jumpBurst, transform.position);
        }

        void HandleLand() {
            Spawn(landBurst, transform.position);
            Punch(landShake);
        }

        void PlayCollect(Vector3 position) {
            Spawn(collectBurst, position);
            Punch(collectShake);
        }

        void Spawn(GameObject prefab, Vector3 position) {
            if (prefab == null) return;
            Instantiate(prefab, position, Quaternion.identity);
        }

        void Punch(float amount) {
            if (shakeTarget == null || amount <= 0f) return;
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(Shake(amount));
        }

        IEnumerator Shake(float amount) {
            shakeRest = shakeTarget.localPosition;
            var elapsed = 0f;
            while (elapsed < shakeTime) {
                elapsed += Time.unscaledDeltaTime;
                var falloff = 1f - elapsed / shakeTime;
                shakeTarget.localPosition = shakeRest + Random.insideUnitSphere * (amount * falloff);
                yield return null;
            }
            shakeTarget.localPosition = shakeRest;
            shakeRoutine = null;
        }
    }
}

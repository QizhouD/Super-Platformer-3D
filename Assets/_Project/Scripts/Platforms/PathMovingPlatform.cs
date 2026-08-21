using UnityEngine;

namespace Platformer {
    /// <summary>
    /// Moves along world waypoints. Tag the object MovingPlatform so PlatformCollisionHandler can parent the player.
    /// </summary>
    public sealed class PathMovingPlatform : MonoBehaviour {
        [SerializeField] Transform[] waypoints = new Transform[0];
        [SerializeField, Min(0.01f)] float speed = 2.5f;
        [SerializeField, Min(0f)] float endPause = 0.35f;
        [SerializeField] bool pingPong = true;
        [SerializeField] bool startOnAwake = true;

        Rigidbody body;
        int index;
        int direction = 1;
        float pauseUntil;
        bool running;

        void Awake() {
            body = GetComponent<Rigidbody>();
            if (body != null) {
                body.isKinematic = true;
                body.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        void OnEnable() {
            if (startOnAwake) running = waypoints != null && waypoints.Length >= 2;
        }

        void FixedUpdate() {
            if (!running || waypoints == null || waypoints.Length < 2) return;
            if (Time.time < pauseUntil) return;

            var target = waypoints[index];
            if (target == null) return;

            var next = Vector3.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
            if (body != null) body.MovePosition(next);
            else transform.position = next;

            if ((next - target.position).sqrMagnitude > 0.0004f) return;

            pauseUntil = Time.time + endPause;
            if (pingPong) {
                if (index == 0) direction = 1;
                else if (index == waypoints.Length - 1) direction = -1;
                index += direction;
            } else {
                index = (index + 1) % waypoints.Length;
            }
        }
    }
}

using UnityEngine;

namespace Platformer {
    public sealed class RotatingPlatform : MonoBehaviour {
        [SerializeField] Vector3 eulerPerSecond = new Vector3(0f, 40f, 0f);
        [SerializeField] bool useRigidbody = true;

        Rigidbody body;

        void Awake() {
            body = GetComponent<Rigidbody>();
            if (body != null && useRigidbody) {
                body.isKinematic = true;
                body.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        void FixedUpdate() {
            var delta = eulerPerSecond * Time.fixedDeltaTime;
            if (useRigidbody && body != null)
                body.MoveRotation(body.rotation * Quaternion.Euler(delta));
            else
                transform.Rotate(delta, Space.Self);
        }
    }
}

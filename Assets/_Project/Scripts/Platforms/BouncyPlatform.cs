using UnityEngine;

namespace Platformer {
    public sealed class BouncyPlatform : MonoBehaviour {
        [SerializeField, Min(0f)] float bounceSpeed = 14f;
        [SerializeField] float minUpNormal = 0.45f;
        [SerializeField] string playerTag = "Player";

        void OnCollisionEnter(Collision collision) {
            if (!collision.transform.root.CompareTag(playerTag)) return;
            if (collision.contactCount == 0) return;
            if (collision.GetContact(0).normal.y < minUpNormal) return;

            var player = collision.transform.root.GetComponent<PlayerController>();
            if (player != null) {
                player.AddVerticalImpulse(bounceSpeed);
                return;
            }

            var rb = collision.rigidbody;
            if (rb == null) return;
            var velocity = rb.velocity;
            velocity.y = bounceSpeed;
            rb.velocity = velocity;
        }
    }
}

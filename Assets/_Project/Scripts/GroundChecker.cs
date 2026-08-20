using UnityEngine;

namespace Platformer {
    public class GroundChecker : MonoBehaviour {
        [SerializeField] float groundDistance = 0.35f;
        [SerializeField] float probeRadius = 0.28f;
        [SerializeField] float coyoteTime = 0.14f;
        [SerializeField] LayerMask groundLayers;

        int supportContacts;
        int wallContacts;
        float coyoteUntil;

        public bool IsGrounded { get; private set; }
        public bool CanJump =>
            IsGrounded || Time.time <= coyoteUntil || wallContacts > 0;

        void FixedUpdate() {
            var probed = ProbeGround();
            IsGrounded = probed || supportContacts > 0;
            if (supportContacts > 0) supportContacts--;
            if (wallContacts > 0) wallContacts--;
            if (IsGrounded) coyoteUntil = Time.time + coyoteTime;
        }

        public bool ProbeGround() {
            var origin = transform.position + Vector3.up * (probeRadius + 0.08f);
            var distance = groundDistance + probeRadius + 0.08f;
            if (Physics.SphereCast(
                    origin,
                    probeRadius,
                    Vector3.down,
                    out _,
                    distance,
                    groundLayers,
                    QueryTriggerInteraction.Ignore))
                return true;

            return Physics.CheckSphere(
                transform.position + Vector3.up * 0.1f,
                probeRadius,
                groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        void OnCollisionStay(Collision collision) {
            if (!IsGroundLayer(collision.gameObject)) return;
            var count = collision.contactCount;
            for (var i = 0; i < count; i++) {
                var normalY = collision.GetContact(i).normal.y;
                if (normalY > 0.2f) supportContacts = 2;
                else if (normalY > -0.4f) wallContacts = 2;
            }
        }

        bool IsGroundLayer(GameObject target) =>
            groundLayers == 0 || ((1 << target.layer) & groundLayers) != 0;
    }
}

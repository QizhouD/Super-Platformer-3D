using UnityEngine;

namespace Platformer {
    public class GroundChecker : MonoBehaviour {
        [SerializeField] float groundDistance = 0.28f;
        [SerializeField] float probeRadius = 0.22f;
        [SerializeField] float skinWidth = 0.08f;
        [SerializeField] float coyoteTime = 0.1f;
        [SerializeField] int extraRays = 4;
        [SerializeField] LayerMask groundLayers;
        [SerializeField] bool drawGizmos = true;

        int supportContacts;
        int wallContacts;
        float coyoteUntil;
        bool groundedLastFixed;

        public bool IsGrounded { get; private set; }
        public bool BecameGrounded { get; private set; }
        public bool CanJump =>
            IsGrounded || Time.time <= coyoteUntil || wallContacts > 0;

        public void SetCoyoteTime(float seconds) =>
            coyoteTime = Mathf.Max(0f, seconds);

        void FixedUpdate() {
            var probed = ProbeGround();
            IsGrounded = probed || supportContacts > 0;
            BecameGrounded = IsGrounded && !groundedLastFixed;
            groundedLastFixed = IsGrounded;
            if (supportContacts > 0) supportContacts--;
            if (wallContacts > 0) wallContacts--;
            if (IsGrounded) coyoteUntil = Time.time + coyoteTime;
        }

        public bool ProbeGround() {
            var origin = transform.position + Vector3.up * (probeRadius + skinWidth);
            var distance = groundDistance + skinWidth;
            if (SphereHits(origin, distance)) return true;

            var radius = extraRays <= 0 ? 0f : probeRadius * 0.65f;
            for (var i = 0; i < extraRays; i++) {
                var angle = i * Mathf.PI * 2f / extraRays;
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (Physics.Raycast(
                        origin + offset,
                        Vector3.down,
                        distance + probeRadius,
                        groundLayers,
                        QueryTriggerInteraction.Ignore))
                    return true;
            }

            return Physics.CheckSphere(
                transform.position + Vector3.up * skinWidth,
                probeRadius * 0.85f,
                groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        bool SphereHits(Vector3 origin, float distance) =>
            Physics.SphereCast(
                origin,
                probeRadius,
                Vector3.down,
                out _,
                distance,
                groundLayers,
                QueryTriggerInteraction.Ignore);

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

        void OnDrawGizmosSelected() {
            if (!drawGizmos) return;
            var origin = transform.position + Vector3.up * (probeRadius + skinWidth);
            var distance = groundDistance + skinWidth;
            Gizmos.color = Application.isPlaying && IsGrounded
                ? new Color(0.3f, 0.9f, 0.35f, 0.85f)
                : new Color(0.95f, 0.75f, 0.2f, 0.85f);
            Gizmos.DrawWireSphere(origin, probeRadius);
            Gizmos.DrawLine(origin, origin + Vector3.down * distance);
            Gizmos.DrawWireSphere(origin + Vector3.down * distance, probeRadius);
        }
    }
}

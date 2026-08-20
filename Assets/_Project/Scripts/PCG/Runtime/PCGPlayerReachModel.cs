using UnityEngine;

namespace Platformer.PCG {
    public static class PCGPlayerReachModel {
        public const float DefaultSafetyFactor = 0.82f;
        const float DefaultFixedDeltaTime = 0.02f;
        const float MinimumJumpHold = 0.1f;

        public static float HorizontalSpeed(float moveSpeed) =>
            moveSpeed > 40f ? moveSpeed * DefaultFixedDeltaTime : Mathf.Max(0.01f, moveSpeed);

        public static float MaxJumpHeight(float jumpForce, float jumpDuration, float gravityMultiplier) {
            var gravity = Mathf.Abs(Physics.gravity.y) * Mathf.Max(0.01f, gravityMultiplier);
            var hold = Mathf.Max(0f, jumpForce) * Mathf.Max(0f, jumpDuration);
            var extra = jumpForce * jumpForce / (2f * gravity);
            return hold + extra;
        }

        public static float ComfortableJumpHeight(float jumpForce, float gravityMultiplier) {
            var gravity = Mathf.Abs(Physics.gravity.y) * Mathf.Max(0.01f, gravityMultiplier);
            var hold = Mathf.Max(0f, jumpForce) * MinimumJumpHold;
            var extra = jumpForce * jumpForce / (2f * gravity);
            return hold + extra;
        }

        public static float MaxAirTime(float jumpForce, float jumpDuration, float gravityMultiplier) {
            var gravity = Mathf.Abs(Physics.gravity.y) * Mathf.Max(0.01f, gravityMultiplier);
            var height = MaxJumpHeight(jumpForce, jumpDuration, gravityMultiplier);
            var timeToApexAfterHold = jumpForce / gravity;
            var fallTime = Mathf.Sqrt(Mathf.Max(0f, 2f * height / gravity));
            return Mathf.Max(0f, jumpDuration) + timeToApexAfterHold + fallTime;
        }

        public static PlayerTraversalCapabilities FromJumpProfile(
            float moveSpeed,
            float jumpForce,
            float jumpDuration,
            float gravityMultiplier,
            float dashMultiplier,
            float dashDuration,
            float safetyFactor = DefaultSafetyFactor) {
            var safety = Mathf.Clamp(safetyFactor, 0.5f, 1f);
            var speed = HorizontalSpeed(moveSpeed);
            var airTime = MaxAirTime(jumpForce, jumpDuration, gravityMultiplier);
            var comfortableHeight = ComfortableJumpHeight(jumpForce, gravityMultiplier);
            var maxHeight = MaxJumpHeight(jumpForce, jumpDuration, gravityMultiplier);
            var dashBonus = Mathf.Max(0f, dashMultiplier - 1f) * speed * Mathf.Max(0f, dashDuration);
            var doubleJumpBonus = Mathf.Max(0f, maxHeight - comfortableHeight);

            return new PlayerTraversalCapabilities(
                speed * airTime * safety,
                comfortableHeight * safety,
                dashBonus * safety,
                doubleJumpBonus * safety);
        }
    }
}

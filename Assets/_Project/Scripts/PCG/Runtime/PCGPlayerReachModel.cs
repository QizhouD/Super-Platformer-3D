using UnityEngine;

namespace Platformer.PCG {
    public static class PCGPlayerReachModel {
        public const float DefaultSafetyFactor = 0.82f;
        const float DefaultFixedDeltaTime = 0.02f;

        public static float HorizontalSpeed(float moveSpeed) =>
            moveSpeed > 40f ? moveSpeed * DefaultFixedDeltaTime : Mathf.Max(0.01f, moveSpeed);

        public static float MaxJumpHeight(float jumpForce, float jumpDuration, float gravityMultiplier) {
            var gravity = Mathf.Abs(Physics.gravity.y) * Mathf.Max(0.01f, gravityMultiplier);
            return jumpForce * jumpForce / (2f * gravity);
        }

        public static float ComfortableJumpHeight(float jumpForce, float gravityMultiplier) {
            var cut = jumpForce * 0.45f;
            var gravity = Mathf.Abs(Physics.gravity.y) * Mathf.Max(0.01f, gravityMultiplier);
            return cut * cut / (2f * gravity);
        }

        public static float MaxAirTime(float jumpForce, float jumpDuration, float gravityMultiplier) {
            var gravity = Mathf.Abs(Physics.gravity.y) * Mathf.Max(0.01f, gravityMultiplier);
            var height = MaxJumpHeight(jumpForce, jumpDuration, gravityMultiplier);
            var up = jumpForce / gravity;
            var down = Mathf.Sqrt(Mathf.Max(0f, 2f * height / gravity));
            return up + down;
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

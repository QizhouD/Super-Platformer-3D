using UnityEngine;

namespace Platformer {
    /// <summary>
    /// Runtime-tunable Mario Odyssey / Astro Bot style feel knobs.
    /// Assign on PlayerController; Inspector values on the controller are used if this asset is missing.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerFeelConfig", menuName = "Platformer/Player Feel Config")]
    public sealed class PlayerFeelConfig : ScriptableObject {
        [Header("Jump timing")]
        [Tooltip("Grace window after walking off a ledge.")]
        [Min(0f)] public float coyoteTime = 0.1f;
        [Tooltip("Jump pressed slightly before landing still fires.")]
        [Min(0f)] public float jumpBuffer = 0.12f;
        [Min(0f)] public float minJumpHold = 0.08f;
        [Range(0.1f, 1f)] public float jumpCutMultiplier = 0.45f;

        [Header("Gravity")]
        [Tooltip("Gravity scale while moving upward and holding jump.")]
        [Range(0.4f, 3f)] public float riseGravity = 1f;
        [Tooltip("Gravity scale while falling. 1.8–2.2 reads as snappy.")]
        [Range(1f, 4f)] public float fallGravity = 2f;

        [Header("Air control")]
        [Range(0f, 1f)] public float airControl = 0.7f;
        [Min(0f)] public float airDrag = 0.4f;

        [Header("Ground assist")]
        [Min(0f)] public float stepHeight = 0.45f;

        [Header("Abilities")]
        [Tooltip("Unused by jump charges; PCG Lab enables double jump on the player bridge.")]
        public bool startWithDoubleJump;
    }
}

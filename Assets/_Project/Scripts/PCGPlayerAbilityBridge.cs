using Platformer;
using Platformer.PCG;
using UnityEngine;

public sealed class PCGPlayerAbilityBridge : MonoBehaviour {
    [SerializeField] PlayerController playerController;
    InputReader input;

    void Awake() {
        if (playerController == null) playerController = GetComponent<PlayerController>();
        input = playerController != null ? playerController.InputReader : null;
        SyncTraversalToGenerator();
    }

    void OnEnable() {
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        playerController.Jumped += HandleJumpStarted;
        playerController.Dashed += HandleDashStarted;
    }

    void OnDisable() {
        if (playerController == null) return;
        playerController.Jumped -= HandleJumpStarted;
        playerController.Dashed -= HandleDashStarted;
    }

    public void ApplyPCGTraversalAbilities(PlayerAbilityProfile abilities) {
        if (playerController == null) return;
        playerController.SetTraversalAbilities(abilities.HasDoubleJump, abilities.HasDash);
        SyncTraversalToGenerator();
    }

    void SyncTraversalToGenerator() {
        if (playerController == null) return;
        var generator = Object.FindObjectOfType<LevelGenerator>();
        if (generator == null) return;
        var safety = generator.Config != null ? generator.Config.ReachSafetyFactor : PCGPlayerReachModel.DefaultSafetyFactor;
        if (generator.Config != null && !generator.Config.SyncReachFromPlayer) return;
        generator.SetTraversalCapabilities(PCGPlayerReachModel.FromJumpProfile(
            playerController.MoveSpeed,
            playerController.JumpForce,
            playerController.JumpDuration,
            playerController.GravityMultiplier,
            playerController.DashForce,
            playerController.DashDuration,
            safety));
    }

    void HandleJumpStarted() => PCGLabSignals.RaiseJumpStarted();

    void HandleDashStarted() => PCGLabSignals.RaiseDashStarted();
}

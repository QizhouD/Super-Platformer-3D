using Platformer;
using Platformer.PCG;
using UnityEngine;

public sealed class PCGPlayerAbilityBridge : MonoBehaviour {
    [SerializeField] PlayerController playerController;

    void Awake() {
        if (playerController == null) playerController = GetComponent<PlayerController>();
    }

    public void ApplyPCGTraversalAbilities(PlayerAbilityProfile abilities) {
        if (playerController == null) return;
        playerController.SetTraversalAbilities(abilities.HasDoubleJump, abilities.HasDash);
    }
}

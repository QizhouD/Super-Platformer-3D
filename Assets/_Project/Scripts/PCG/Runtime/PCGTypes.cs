using System;
using UnityEngine;

namespace Platformer.PCG {
    public enum ChunkCategory {
        Basic,
        Moving,
        Timed,
        AbilityGate,
        Combat,
        Exploration,
        Recovery,
        Checkpoint,
        Finish
    }

    [Flags]
    public enum AbilityRequirement {
        None = 0,
        DoubleJump = 1 << 0,
        Dash = 1 << 1
    }

    [Serializable]
    public struct PlayerAbilityProfile {
        [SerializeField] bool hasDoubleJump;
        [SerializeField] bool hasDash;

        public bool HasDoubleJump => hasDoubleJump;
        public bool HasDash => hasDash;

        public PlayerAbilityProfile(bool hasDoubleJump, bool hasDash) {
            this.hasDoubleJump = hasDoubleJump;
            this.hasDash = hasDash;
        }

        public bool Supports(AbilityRequirement requirement) {
            if ((requirement & AbilityRequirement.DoubleJump) != 0 && !hasDoubleJump) return false;
            if ((requirement & AbilityRequirement.Dash) != 0 && !hasDash) return false;
            return true;
        }
    }
}

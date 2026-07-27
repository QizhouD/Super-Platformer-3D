using System;
using UnityEngine;

namespace Platformer.PCG {
    [Serializable]
    public struct PlayerTraversalCapabilities {
        [SerializeField, Min(0f)] float baseHorizontalReach;
        [SerializeField, Min(0f)] float baseVerticalReach;
        [SerializeField, Min(0f)] float dashHorizontalBonus;
        [SerializeField, Min(0f)] float doubleJumpVerticalBonus;

        public float BaseHorizontalReach => baseHorizontalReach;
        public float BaseVerticalReach => baseVerticalReach;
        public float DashHorizontalBonus => dashHorizontalBonus;
        public float DoubleJumpVerticalBonus => doubleJumpVerticalBonus;

        public PlayerTraversalCapabilities(
            float baseHorizontalReach,
            float baseVerticalReach,
            float dashHorizontalBonus,
            float doubleJumpVerticalBonus) {
            this.baseHorizontalReach = Mathf.Max(0f, baseHorizontalReach);
            this.baseVerticalReach = Mathf.Max(0f, baseVerticalReach);
            this.dashHorizontalBonus = Mathf.Max(0f, dashHorizontalBonus);
            this.doubleJumpVerticalBonus = Mathf.Max(0f, doubleJumpVerticalBonus);
        }

        public float HorizontalReach(PlayerAbilityProfile abilities) =>
            baseHorizontalReach + (abilities.HasDash ? dashHorizontalBonus : 0f);

        public float VerticalReach(PlayerAbilityProfile abilities) =>
            baseVerticalReach + (abilities.HasDoubleJump ? doubleJumpVerticalBonus : 0f);

        public static PlayerTraversalCapabilities LabDefaults =>
            new PlayerTraversalCapabilities(6.5f, 1.8f, 5f, 2.5f);
    }

    public static class ChunkReachabilityValidator {
        public static bool CanTraverse(
            PlatformChunkData chunk,
            PlayerAbilityProfile abilities,
            PlayerTraversalCapabilities capabilities) {
            if (chunk == null || !abilities.Supports(chunk.RequiredAbility)) return false;
            if (chunk.RequiredHorizontalReach > capabilities.HorizontalReach(abilities)) return false;
            if (chunk.RequiredVerticalReach > capabilities.VerticalReach(abilities)) return false;
            return true;
        }
    }
}

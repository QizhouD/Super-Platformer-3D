using System;
using UnityEngine;

namespace Platformer.PCG {
    [Serializable]
    public sealed class PCGGameAIObservation {
        public const int VectorSize = 20;

        public float timestamp;
        public Vector3 playerPosition;
        public Vector3 playerVelocity;
        public Vector3 cameraForward;
        public float normalizedProgress;
        public int currentChunkIndex;
        public int resetCount;
        public float nextChunkDifficulty;
        public float adaptiveSkill;
        public float difficultyBias;
        public float timedPlatformVisibleRatio;
        public int movingPlatformCount;
        public int visualFrameSequence;

        public float[] ToVector() {
            return new[] {
                playerPosition.x,
                playerPosition.y,
                playerPosition.z,
                playerVelocity.x,
                playerVelocity.y,
                playerVelocity.z,
                cameraForward.x,
                cameraForward.y,
                cameraForward.z,
                normalizedProgress,
                currentChunkIndex,
                resetCount,
                nextChunkDifficulty,
                adaptiveSkill,
                difficultyBias,
                timedPlatformVisibleRatio,
                movingPlatformCount,
                visualFrameSequence,
                playerVelocity.magnitude,
                timestamp
            };
        }
    }

}

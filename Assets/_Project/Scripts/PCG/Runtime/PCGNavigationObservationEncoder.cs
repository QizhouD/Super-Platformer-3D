using UnityEngine;

namespace Platformer.PCG {
    public interface IPCGTrainingController {
        bool TrainingMode { get; }
        int CompletedEpisodes { get; }
        int FailedEpisodes { get; }
        float LastEpisodeReward { get; }
        void SetTrainingMode(bool value);
    }

    public static class PCGNavigationObservationEncoder {
        public const int ObservationSize = 20;

        public static float[] Build(
            Vector3 playerPosition,
            Vector3 velocity,
            Vector3 playerForward,
            Vector3 cameraForward,
            Vector3 targetPosition,
            float progress,
            int resetCount,
            float nextDifficulty,
            float skill,
            float difficultyBias,
            float timedVisibleRatio,
            int movingPlatformCount,
            int chunkCount,
            int currentChunkIndex) {
            var toTarget = targetPosition - playerPosition;
            var targetDirection = toTarget.sqrMagnitude > 0.0001f
                ? toTarget.normalized
                : Vector3.zero;
            return new[] {
                targetDirection.x,
                targetDirection.y,
                targetDirection.z,
                Mathf.Clamp01(toTarget.magnitude / 30f),
                Mathf.Clamp(velocity.x / 10f, -1f, 1f),
                Mathf.Clamp(velocity.y / 10f, -1f, 1f),
                Mathf.Clamp(velocity.z / 10f, -1f, 1f),
                Mathf.Clamp(playerForward.x, -1f, 1f),
                Mathf.Clamp(playerForward.z, -1f, 1f),
                Mathf.Clamp(cameraForward.x, -1f, 1f),
                Mathf.Clamp(cameraForward.z, -1f, 1f),
                Mathf.Clamp01(progress),
                chunkCount > 0
                    ? Mathf.Clamp01((float)Mathf.Max(0, currentChunkIndex) / chunkCount)
                    : 0f,
                Mathf.Clamp01(resetCount / 10f),
                Mathf.Clamp01(nextDifficulty),
                Mathf.Clamp01(skill),
                Mathf.Clamp(difficultyBias / 0.5f, -1f, 1f),
                Mathf.Clamp01(timedVisibleRatio),
                Mathf.Clamp01(movingPlatformCount / 10f),
                velocity.y < -2f ? 1f : 0f
            };
        }
    }
}

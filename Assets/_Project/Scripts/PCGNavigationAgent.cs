using Platformer;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGNavigationAgent : Agent, IPCGTrainingController {
        public const string BehaviorName = "PCGNavigation";

        [SerializeField] InputReader input;
        [SerializeField] LevelGenerator generator;
        [SerializeField] PCGRunController runController;
        [SerializeField] PCGGameAIObservationSensor observationSensor;
        [SerializeField] Camera navigationCamera;
        [SerializeField] bool trainingMode;
        [SerializeField] float checkpointReward = 1f;
        [SerializeField] float completionReward = 5f;
        [SerializeField] float deathPenalty = -1f;
        [SerializeField] float decisionPenalty = -0.0005f;
        [SerializeField] float approachRewardScale = 0.01f;

        BehaviorParameters behaviorParameters;
        RenderTextureSensorComponent visualSensor;
        float previousTargetDistance;
        bool restartLevelOnNextEpisode;

        public bool TrainingMode => trainingMode;
        public int CompletedEpisodes { get; private set; }
        public int FailedEpisodes { get; private set; }
        public float LastEpisodeReward { get; private set; }

        public void Configure(
            InputReader inputReader,
            LevelGenerator levelGenerator,
            PCGRunController controller,
            PCGGameAIObservationSensor sensor,
            Camera sourceCamera) {
            input = inputReader;
            generator = levelGenerator;
            runController = controller;
            observationSensor = sensor;
            navigationCamera = sourceCamera;
        }

        public override void Initialize() {
            behaviorParameters = GetComponent<BehaviorParameters>();
            visualSensor = GetComponent<RenderTextureSensorComponent>();
            if (visualSensor != null && observationSensor != null)
                visualSensor.RenderTexture = observationSensor.GetOrCreateVisualFrame();
            Subscribe();
            SetTrainingMode(trainingMode);
            MaxStep = Mathf.Max(MaxStep, 5000);
        }

        void OnDisable() {
            Unsubscribe();
            ReleaseExternalInput();
        }

        public override void OnEpisodeBegin() {
            LastEpisodeReward = GetCumulativeReward();
            ReleaseExternalInput();
            if (restartLevelOnNextEpisode) {
                restartLevelOnNextEpisode = false;
                runController?.RestartRun(false);
            }
            previousTargetDistance = DistanceToTarget();
        }

        public override void CollectObservations(VectorSensor sensor) {
            var observation = observationSensor != null
                ? observationSensor.CaptureStructuredObservation()
                : new PCGGameAIObservation();
            var target = ResolveTargetPosition();
            var values = PCGNavigationObservationEncoder.Build(
                observation.playerPosition,
                observation.playerVelocity,
                transform.forward,
                navigationCamera != null
                    ? navigationCamera.transform.forward
                    : observation.cameraForward,
                target,
                observation.normalizedProgress,
                observation.resetCount,
                observation.nextChunkDifficulty,
                observation.adaptiveSkill,
                observation.difficultyBias,
                observation.timedPlatformVisibleRatio,
                observation.movingPlatformCount,
                generator != null ? generator.SpawnedChunks.Count : 0,
                observation.currentChunkIndex);
            foreach (var value in values) sensor.AddObservation(value);
        }

        public override void OnActionReceived(ActionBuffers actions) {
            if (input == null) return;
            var movement = new Vector2(
                Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f),
                Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f));
            input.SetExternalDirection(movement);
            input.SendExternalJump(actions.DiscreteActions[0] == 1);
            input.SendExternalDash(actions.DiscreteActions[1] == 1);

            AddReward(decisionPenalty);
            var targetDistance = DistanceToTarget();
            if (previousTargetDistance > 0f && targetDistance > 0f) {
                var approachDelta = Mathf.Clamp(
                    previousTargetDistance - targetDistance,
                    -1f,
                    1f);
                AddReward(approachDelta * approachRewardScale);
            }
            previousTargetDistance = targetDistance;
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            var continuous = actionsOut.ContinuousActions;
            var discrete = actionsOut.DiscreteActions;
            var humanDirection = input != null ? input.HumanDirection : Vector2.zero;
            continuous[0] = humanDirection.x;
            continuous[1] = humanDirection.y;
            discrete[0] = input != null && input.HumanJumpHeld ? 1 : 0;
            discrete[1] = input != null && input.HumanDashHeld ? 1 : 0;
        }

        public void SetTrainingMode(bool value) {
            trainingMode = value;
            if (behaviorParameters == null)
                behaviorParameters = GetComponent<BehaviorParameters>();
            if (behaviorParameters != null)
                behaviorParameters.BehaviorType = value
                    ? BehaviorType.Default
                    : BehaviorType.HeuristicOnly;
            input?.SetExternalControlEnabled(true);
        }

        void HandleCheckpointReached(int chunkIndex, Vector3 position) {
            AddReward(checkpointReward);
            previousTargetDistance = DistanceToTarget();
            if (generator == null ||
                chunkIndex < generator.SpawnedChunks.Count - 1) return;

            AddReward(completionReward);
            LastEpisodeReward = GetCumulativeReward();
            CompletedEpisodes++;
            restartLevelOnNextEpisode = true;
            EndEpisode();
        }

        void HandlePlayerRespawned(int resetCount, Vector3 position) {
            AddReward(deathPenalty);
            LastEpisodeReward = GetCumulativeReward();
            FailedEpisodes++;
            EndEpisode();
        }

        Vector3 ResolveTargetPosition() {
            if (generator == null || generator.SpawnedChunks.Count == 0)
                return transform.position;
            var index = runController != null
                ? Mathf.Clamp(runController.FurthestCheckpoint + 1, 0, generator.SpawnedChunks.Count - 1)
                : 0;
            var chunk = generator.SpawnedChunks[index];
            return chunk != null && chunk.Exits.Count > 0
                ? chunk.Exits[0].position
                : chunk.transform.position;
        }

        float DistanceToTarget() =>
            Vector3.Distance(transform.position, ResolveTargetPosition());

        void Subscribe() {
            Unsubscribe();
            if (runController == null) return;
            runController.CheckpointReached += HandleCheckpointReached;
            runController.PlayerRespawned += HandlePlayerRespawned;
        }

        void Unsubscribe() {
            if (runController == null) return;
            runController.CheckpointReached -= HandleCheckpointReached;
            runController.PlayerRespawned -= HandlePlayerRespawned;
        }

        void ReleaseExternalInput() {
            if (input == null) return;
            input.SetExternalDirection(Vector2.zero);
            input.SendExternalJump(false);
            input.SendExternalDash(false);
        }
    }
}

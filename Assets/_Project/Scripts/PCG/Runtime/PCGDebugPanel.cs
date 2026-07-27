using System;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGDebugPanel : MonoBehaviour {
        [SerializeField] LevelGenerator generator;
        [SerializeField] GameObject player;
        [SerializeField] PCGRunController runController;
        [SerializeField] PCGRunTelemetry telemetry;

        string seedText = "82431";
        bool doubleJump;
        bool dash;
        Vector2 manifestScroll;

        void Awake() {
            if (generator == null) generator = FindObjectOfType<LevelGenerator>();
            if (generator != null) seedText = generator.Seed.ToString();
        }

        public void Configure(
            LevelGenerator levelGenerator,
            GameObject labPlayer = null,
            PCGRunController labRunController = null,
            PCGRunTelemetry runTelemetry = null) {
            generator = levelGenerator;
            player = labPlayer;
            runController = labRunController;
            telemetry = runTelemetry;
            if (generator != null) seedText = generator.Seed.ToString();
        }

        void OnGUI() {
            if (generator == null) return;

            GUILayout.BeginArea(new Rect(16f, 16f, 390f, Screen.height - 32f), GUI.skin.box);
            GUILayout.Label("PCG Lab");
            GUILayout.Label("Move: WASD / Jump: Space / Dash: Shift");
            if (runController != null)
                GUILayout.Label($"Checkpoint: {runController.FurthestCheckpoint + 1}   Resets: {runController.ResetCount}");
            if (telemetry != null)
                GUILayout.Label($"Telemetry events: {telemetry.Events.Count}");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed", GUILayout.Width(55f));
            seedText = GUILayout.TextField(seedText, GUILayout.Width(120f));
            if (GUILayout.Button("Generate")) Generate();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            doubleJump = GUILayout.Toggle(doubleJump, "Double Jump");
            dash = GUILayout.Toggle(dash, "Dash");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Random Seed")) {
                seedText = Environment.TickCount.ToString();
                Generate();
            }
            if (GUILayout.Button("Copy Seed")) GUIUtility.systemCopyBuffer = generator.Seed.ToString();
            if (GUILayout.Button("Copy Manifest") && generator.LastManifest != null)
                GUIUtility.systemCopyBuffer = generator.LastManifest.ToJson();
            if (GUILayout.Button("Copy Telemetry") && telemetry != null)
                GUIUtility.systemCopyBuffer = telemetry.ToJson();
            GUILayout.EndHorizontal();

            var manifest = generator.LastManifest;
            if (manifest != null) {
                GUILayout.Space(8f);
                GUILayout.Label(manifest.completed
                    ? $"Generated {manifest.chunks.Count} chunks"
                    : $"Failed: {manifest.failureReason}");

                manifestScroll = GUILayout.BeginScrollView(manifestScroll);
                GUILayout.TextArea(manifest.ToJson(), GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();
            }

            GUILayout.EndArea();
        }

        void Generate() {
            if (!int.TryParse(seedText, out var parsedSeed)) parsedSeed = 82431;
            generator.Seed = parsedSeed;
            generator.SetCapabilities(doubleJump, dash);
            if (player != null)
                player.SendMessage(
                    "ApplyPCGTraversalAbilities",
                    new PlayerAbilityProfile(doubleJump, dash),
                    SendMessageOptions.DontRequireReceiver);
            generator.Generate();
            seedText = generator.Seed.ToString();
        }
    }
}

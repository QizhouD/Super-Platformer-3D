using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Platformer;
using Platformer.PCG;
using UnityEditor;
using UnityEngine;

public static class PCGGenerationReport {
    const string ConfigPath = "Assets/_Project/PCG/LevelGenerationConfig.asset";
    const string PlayerPath = "Assets/_Project/Prefabs/Player 2.prefab";
    const string OutputFolder = "docs/pcg-eval";
    const int SeedCount = 100;
    const int ShowcaseSeed = 82431;

    static readonly (string Name, PlayerAbilityProfile Abilities)[] Profiles = {
        ("none", new PlayerAbilityProfile(false, false)),
        ("double jump", new PlayerAbilityProfile(true, false)),
        ("dash", new PlayerAbilityProfile(false, true)),
        ("double jump + dash", new PlayerAbilityProfile(true, true))
    };

    sealed class LevelResult {
        public string Profile;
        public int Seed;
        public bool Completed;
        public bool Deterministic;
        public int Fallbacks;
        public int DistinctChunks;
        public int HeadingChanges;
        public float DifficultyError;
        public float ElevationSpan;
    }

    [MenuItem("Platformer/PCG/Write Generation Report")]
    public static void Write() {
        var config = AssetDatabase.LoadAssetAtPath<LevelGenerationConfig>(ConfigPath);
        var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath)?.GetComponent<PlayerController>();
        if (config == null || player == null)
            throw new FileNotFoundException($"Missing {ConfigPath} or PlayerController on {PlayerPath}.");

        var reach = PCGPlayerReachModel.FromJumpProfile(
            player.MoveSpeed, player.JumpForce, player.JumpDuration, player.GravityMultiplier,
            player.DashForce, player.DashDuration, config.ReachSafetyFactor);

        var results = new List<LevelResult>();
        GeneratedLevelManifest showcase = null;
        var root = new GameObject("PCG Generation Report") { hideFlags = HideFlags.HideAndDontSave };
        try {
            var anchor = new GameObject("Start Anchor").transform;
            anchor.SetParent(root.transform);
            var output = new GameObject("Output").transform;
            output.SetParent(root.transform);
            var generator = root.AddComponent<LevelGenerator>();
            generator.SetTraversalCapabilities(reach);

            foreach (var (name, abilities) in Profiles) {
                generator.SetCapabilities(abilities.HasDoubleJump, abilities.HasDash);
                for (var seed = 1; seed <= SeedCount; seed++) {
                    generator.Configure(config, anchor, output, seed);
                    generator.Generate();
                    var manifest = generator.LastManifest;
                    var json = manifest.ToJson();
                    generator.Generate();
                    results.Add(Measure(name, seed, manifest, json == generator.LastManifest.ToJson()));
                }

                if (name != "none") continue;
                generator.Configure(config, anchor, output, ShowcaseSeed);
                generator.Generate();
                showcase = generator.LastManifest;
            }

            generator.Clear();
        } finally {
            Object.DestroyImmediate(root);
        }

        var folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, OutputFolder);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "levels.csv"), LevelsCsv(results));
        File.WriteAllText(Path.Combine(folder, "summary.md"), Summary(results, reach, showcase));
        Debug.Log($"PCG generation report written to {folder}");
    }

    public static void WriteBatchMode() {
        try {
            Write();
            EditorApplication.Exit(0);
        } catch (System.Exception exception) {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    static LevelResult Measure(string profile, int seed, GeneratedLevelManifest manifest, bool deterministic) {
        var chunks = manifest.chunks;
        var result = new LevelResult {
            Profile = profile,
            Seed = seed,
            Completed = manifest.completed,
            Deterministic = deterministic,
            Fallbacks = chunks.Count(c => c.safeFallback),
            DistinctChunks = chunks.Select(c => c.chunkId).Distinct().Count()
        };
        if (chunks.Count == 0) return result;

        result.DifficultyError = chunks.Average(c => Mathf.Abs(c.targetDifficulty - c.actualDifficulty));
        result.ElevationSpan = chunks.Max(c => c.position.y) - chunks.Min(c => c.position.y);
        for (var i = 1; i < chunks.Count; i++) {
            if (Quaternion.Angle(chunks[i].rotation, chunks[i - 1].rotation) > 1f) result.HeadingChanges++;
        }
        return result;
    }

    static string LevelsCsv(IEnumerable<LevelResult> results) {
        var csv = new StringBuilder(
            "profile,seed,completed,deterministic,safe_fallbacks,distinct_chunks,heading_changes,difficulty_error,elevation_span_m\n");
        foreach (var r in results) {
            csv.AppendLine(string.Join(",", r.Profile, r.Seed, r.Completed, r.Deterministic, r.Fallbacks,
                r.DistinctChunks, r.HeadingChanges, F(r.DifficultyError), F(r.ElevationSpan)));
        }
        return csv.ToString();
    }

    static string Summary(List<LevelResult> results, PlayerTraversalCapabilities reach, GeneratedLevelManifest showcase) {
        var none = new PlayerAbilityProfile(false, false);
        var md = new StringBuilder();
        md.AppendLine("# PCG generation report");
        md.AppendLine();
        md.AppendLine("Written by `Platformer > PCG > Write Generation Report` (batch mode: " +
                      "`-executeMethod PCGGenerationReport.WriteBatchMode`). Do not edit by hand.");
        md.AppendLine();
        md.AppendLine($"Seeds 1-{SeedCount} for each ability profile, 16 chunks per level. Reach comes from " +
                      $"the `Player 2` prefab: {F(reach.HorizontalReach(none))}m horizontal and " +
                      $"{F(reach.VerticalReach(none))}m vertical without abilities. Quality columns average " +
                      "completed levels only; raw rows are in `levels.csv`.");
        md.AppendLine();
        md.AppendLine("| Abilities | Completed | Deterministic | Safe-fallback chunks | Mean difficulty error | " +
                      "Distinct chunks / level | Elevation span (m) | Heading changes / level |");
        md.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (var (name, _) in Profiles) {
            var all = results.Where(r => r.Profile == name).ToList();
            var done = all.Where(r => r.Completed).ToList();
            md.AppendLine($"| {name} | {done.Count}/{all.Count} | {all.Count(r => r.Deterministic)}/{all.Count} | " +
                          $"{done.Sum(r => r.Fallbacks)}/{done.Count * 16} | {F(done.Average(r => r.DifficultyError))} | " +
                          $"{F(done.Average(r => r.DistinctChunks))} | {F(done.Average(r => r.ElevationSpan))} | " +
                          $"{F(done.Average(r => r.HeadingChanges))} |");
        }

        if (showcase == null || showcase.chunks.Count == 0) return md.ToString();
        md.AppendLine();
        md.AppendLine($"## Seed {ShowcaseSeed}, no abilities");
        md.AppendLine();
        md.AppendLine("First line: target difficulty from the progress curve and rhythm plan. " +
                      "Second line: difficulty of the chunk actually placed.");
        md.AppendLine();
        md.AppendLine("```mermaid");
        md.AppendLine("xychart-beta");
        md.AppendLine("    x-axis \"Chunk\" [" + string.Join(", ", showcase.chunks.Select(c => c.index)) + "]");
        md.AppendLine("    y-axis \"Difficulty\" 0 --> 1");
        md.AppendLine("    line [" + string.Join(", ", showcase.chunks.Select(c => F(c.targetDifficulty))) + "]");
        md.AppendLine("    line [" + string.Join(", ", showcase.chunks.Select(c => F(c.actualDifficulty))) + "]");
        md.AppendLine("```");
        md.AppendLine();
        md.AppendLine("Chunks: " + string.Join(" → ", showcase.chunks.Select(c => $"`{c.chunkId}`")));
        return md.ToString();
    }

    static string F(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}

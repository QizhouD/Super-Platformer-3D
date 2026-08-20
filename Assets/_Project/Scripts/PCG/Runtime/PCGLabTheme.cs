using UnityEngine;

namespace Platformer.PCG {
    public static class PCGLabTheme {
        public static readonly Color SkyZenith = new Color(0.99f, 0.72f, 0.46f);
        public static readonly Color SkyHorizon = new Color(0.55f, 0.36f, 0.52f);
        public static readonly Color GroundTint = new Color(0.36f, 0.22f, 0.18f);
        public static readonly Color Fog = new Color(0.73f, 0.52f, 0.42f);
        public static readonly Color Sun = new Color(1f, 0.84f, 0.62f);
        public static readonly Color FillLight = new Color(0.42f, 0.55f, 0.72f);
        public static readonly Color Accent = new Color(1f, 0.62f, 0.28f);
        public static readonly Color AccentCool = new Color(0.35f, 0.86f, 0.82f);
        public static readonly Color Panel = new Color(0.07f, 0.06f, 0.08f, 0.82f);
        public static readonly Color PanelInner = new Color(0.13f, 0.1f, 0.11f, 0.92f);
        public static readonly Color Text = new Color(0.97f, 0.93f, 0.88f);
        public static readonly Color Muted = new Color(0.78f, 0.7f, 0.64f);
        public static readonly Color Danger = new Color(0.95f, 0.32f, 0.28f);
        public static readonly Color Success = new Color(0.45f, 0.86f, 0.48f);

        public static Color CategoryColor(ChunkCategory category) {
            switch (category) {
                case ChunkCategory.Moving: return new Color(0.28f, 0.78f, 0.92f);
                case ChunkCategory.Timed: return new Color(0.98f, 0.62f, 0.18f);
                case ChunkCategory.AbilityGate: return new Color(0.78f, 0.42f, 0.95f);
                case ChunkCategory.Combat: return new Color(0.86f, 0.28f, 0.24f);
                case ChunkCategory.Exploration: return new Color(0.32f, 0.72f, 0.62f);
                case ChunkCategory.Recovery: return new Color(0.42f, 0.82f, 0.46f);
                case ChunkCategory.Checkpoint: return Accent;
                case ChunkCategory.Finish: return new Color(1f, 0.84f, 0.32f);
                default: return new Color(0.78f, 0.56f, 0.38f);
            }
        }

        public static Color CategoryEmission(ChunkCategory category) {
            var color = CategoryColor(category);
            var intensity = category == ChunkCategory.Basic ? 0.12f : 0.45f;
            return color * intensity;
        }

        static Material template;

        public static void SetTemplate(Material source) {
            if (source != null && source.shader != null && source.shader.name != "Hidden/InternalErrorShader")
                template = source;
        }

        public static Material CreateLitMaterial(Color albedo, Color emission, float smoothness = 0.28f) {
            var material = CreateBaseMaterial();
            material.name = "PCGLabRuntime";
            ApplyColor(material, albedo);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.08f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            if (material.HasProperty("_EmissionColor")) {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            return material;
        }

        public static Material CreateTintedClone(Material source, Color albedo) {
            var material = source != null
                ? new Material(source)
                : CreateBaseMaterial();
            material.name = "PCGLabTint";
            ApplyColor(material, albedo);
            return material;
        }

        public static void ApplyColor(Material material, Color albedo) {
            if (material == null) return;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", albedo);
            if (material.HasProperty("_Color")) material.SetColor("_Color", albedo);
        }

        static Material CreateBaseMaterial() {
            if (template != null) return new Material(template);

            var shader = Shader.Find("ShaderTest/NoiseGround") ??
                         Shader.Find("Universal Render Pipeline/Lit") ??
                         Shader.Find("Universal Render Pipeline/Simple Lit") ??
                         Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Sprites/Default") ??
                         Shader.Find("Unlit/Color") ??
                         Shader.Find("UI/Default");
            if (shader == null)
                throw new System.InvalidOperationException(
                    "PCG Lab could not find a compatible shader. Keep NoiseGround.mat on the start platform.");
            return new Material(shader);
        }

        public static Material CreateSkybox() {
            var shader = Shader.Find("Skybox/Procedural");
            if (shader == null) return null;
            var sky = new Material(shader) { name = "PCGLabSky" };
            if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", SkyHorizon);
            if (sky.HasProperty("_GroundColor")) sky.SetColor("_GroundColor", GroundTint);
            if (sky.HasProperty("_SunSize")) sky.SetFloat("_SunSize", 0.045f);
            if (sky.HasProperty("_SunSizeConvergence")) sky.SetFloat("_SunSizeConvergence", 4f);
            if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", 1.15f);
            if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 1.25f);
            return sky;
        }

        public static Texture2D CreateRoundedRect(int width, int height, int radius, Color fill, Color border) {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {
                name = "PCGLabUI",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pixels = new Color[width * height];
            var inner = radius - 1;
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var distance = CornerDistance(x, y, width, height, radius);
                    Color color;
                    if (distance > radius) color = Color.clear;
                    else if (distance > inner) color = Color.Lerp(border, fill, 0.25f);
                    else color = fill;
                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        static float CornerDistance(int x, int y, int width, int height, int radius) {
            var cx = x < radius ? radius - x : x >= width - radius ? x - (width - radius - 1) : 0;
            var cy = y < radius ? radius - y : y >= height - radius ? y - (height - radius - 1) : 0;
            if (cx == 0 || cy == 0) return 0f;
            return Mathf.Sqrt(cx * cx + cy * cy);
        }
    }
}

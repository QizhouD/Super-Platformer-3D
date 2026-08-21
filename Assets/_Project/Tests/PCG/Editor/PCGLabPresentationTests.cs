using NUnit.Framework;
using UnityEngine;

namespace Platformer.PCG.Tests {
    public sealed class PCGLabPresentationTests {
        [Test]
        public void Theme_AssignsDistinctColorsToGameplayCategories() {
            var basic = PCGLabTheme.CategoryColor(ChunkCategory.Basic);
            var moving = PCGLabTheme.CategoryColor(ChunkCategory.Moving);
            var timed = PCGLabTheme.CategoryColor(ChunkCategory.Timed);
            var gate = PCGLabTheme.CategoryColor(ChunkCategory.AbilityGate);
            var combat = PCGLabTheme.CategoryColor(ChunkCategory.Combat);
            var recovery = PCGLabTheme.CategoryColor(ChunkCategory.Recovery);

            Assert.That(basic, Is.Not.EqualTo(moving));
            Assert.That(moving, Is.Not.EqualTo(timed));
            Assert.That(timed, Is.Not.EqualTo(gate));
            Assert.That(gate, Is.Not.EqualTo(combat));
            Assert.That(combat, Is.Not.EqualTo(recovery));
        }

        [Test]
        public void Theme_ConfiguresSunWithSoftShadowsAndDuskColor() {
            var sunObject = new GameObject("PCG Sun Test");
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.None;
            sun.intensity = 1f;
            sun.color = Color.white;

            PCGLabTheme.ConfigureSun(sun);

            Assert.That(sun.shadows, Is.EqualTo(LightShadows.Soft));
            Assert.That(sun.intensity, Is.EqualTo(PCGLabTheme.SunIntensity));
            Assert.That(sun.shadowStrength, Is.EqualTo(PCGLabTheme.SunShadowStrength));
            Assert.That(sun.shadowBias, Is.EqualTo(PCGLabTheme.SunShadowBias));
            Assert.That(sun.shadowNormalBias, Is.EqualTo(PCGLabTheme.SunShadowNormalBias));
            Assert.That(sun.color, Is.EqualTo(PCGLabTheme.Sun));
            Assert.That(sun.transform.rotation.eulerAngles.x, Is.EqualTo(PCGLabTheme.SunEuler.x).Within(0.1f));

            Object.DestroyImmediate(sunObject);
        }

        [Test]
        public void Theme_CreatesLitMaterialWithAlbedoAndEmission() {
            var albedo = new Color(0.4f, 0.2f, 0.1f, 1f);
            var emission = new Color(0.2f, 0.1f, 0.05f, 1f);
            var source = new Material(Shader.Find("ShaderTest/NoiseGround") ?? Shader.Find("Sprites/Default"));
            PCGLabTheme.SetTemplate(source);
            var material = PCGLabTheme.CreateLitMaterial(albedo, emission);

            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader, Is.Not.Null);
            Assert.That(material.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"));
            Assert.That(material.shader.name, Is.Not.EqualTo("Standard"));
            var expected = PCGLabTheme.ToHeroPalette(albedo);
            var actual = material.HasProperty("_BaseColor")
                ? material.GetColor("_BaseColor")
                : material.GetColor("_Color");
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.001f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.001f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.001f));

            Object.DestroyImmediate(material);
            Object.DestroyImmediate(source);
        }

        [Test]
        public void NoiseGround_ExposesPixelHeroSurfaceProperties() {
            var shader = Shader.Find("ShaderTest/NoiseGround");
            Assert.That(shader, Is.Not.Null);
            var material = new Material(shader);

            Assert.That(material.HasProperty("_PixelsPerMeter"), Is.True);
            Assert.That(material.HasProperty("_Grout"), Is.True);
            Assert.That(material.HasProperty("_RampSteps"), Is.True);
            Assert.That(material.HasProperty("_TriplanarSharpness"), Is.True);
            Assert.That(material.HasProperty("_Color"), Is.True);

            Object.DestroyImmediate(material);
        }

        [Test]
        public void Theme_TintedCloneUsesZeroGlossHeroPalette() {
            var shader = Shader.Find("ShaderTest/NoiseGround");
            Assert.That(shader, Is.Not.Null);
            var source = new Material(shader);
            var material = PCGLabTheme.CreateTintedClone(source, new Color(0.4f, 0.3f, 0.2f));

            Assert.That(material.GetFloat("_Glossiness"), Is.EqualTo(0f).Within(0.001f));
            Assert.That(material.GetFloat("_Metallic"), Is.EqualTo(0f).Within(0.001f));
            var color = material.GetColor("_Color");
            Color.RGBToHSV(new Color(0.4f, 0.3f, 0.2f), out _, out var sourceSat, out _);
            Color.RGBToHSV(color, out _, out var heroSat, out _);
            Assert.That(heroSat, Is.GreaterThan(sourceSat));

            Object.DestroyImmediate(material);
            Object.DestroyImmediate(source);
        }

        [Test]
        public void VisualStyler_InfersCategoryFromChunkId() {
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("basic_01"), Is.EqualTo(ChunkCategory.Basic));
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("moving_01"), Is.EqualTo(ChunkCategory.Moving));
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("timed_01"), Is.EqualTo(ChunkCategory.Timed));
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("dash_gap_01"), Is.EqualTo(ChunkCategory.AbilityGate));
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("double_jump_01"), Is.EqualTo(ChunkCategory.AbilityGate));
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("combat_01"), Is.EqualTo(ChunkCategory.Combat));
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("recovery_01"), Is.EqualTo(ChunkCategory.Recovery));
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("turn_left_01"), Is.EqualTo(ChunkCategory.Exploration));
            Assert.That(PCGLabVisualStyler.InferCategoryFromId("climb_01"), Is.EqualTo(ChunkCategory.Exploration));
        }

        [Test]
        public void PlatformFeel_ExpandsFootprintWithoutChangingHeight() {
            var expanded = PCGPlatformFeel.ExpandBoxSize(Vector3.one, new Vector3(5f, 1f, 5f), 0.22f);

            Assert.That(expanded.y, Is.EqualTo(1f));
            Assert.That(expanded.x, Is.GreaterThan(1f));
            Assert.That(expanded.z, Is.GreaterThan(1f));
            Assert.That(expanded.x * 5f, Is.EqualTo(5f + 0.22f).Within(0.001f));
        }

        [Test]
        public void DebugPanel_PublicApiDoesNotChangeSeedUntilGenerate() {
            var panelObject = new GameObject("PCG Debug Panel Test");
            var panel = panelObject.AddComponent<PCGDebugPanel>();

            panel.SetDoubleJump(true);
            panel.SetDash(true);
            panel.HideLegacyGui = true;

            Assert.That(panel.DoubleJumpEnabled, Is.True);
            Assert.That(panel.DashEnabled, Is.True);
            Assert.That(panel.HideLegacyGui, Is.True);
            Assert.That(panel.SeedText, Is.EqualTo("82431"));

            Object.DestroyImmediate(panelObject);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGLabVisualStyler {
        readonly Dictionary<ChunkCategory, Material> categoryMaterials =
            new Dictionary<ChunkCategory, Material>();
        readonly List<GameObject> worldObjects = new List<GameObject>();
        readonly List<GameObject> transientObjects = new List<GameObject>();
        Material timedWarning;
        Material timedHidden;
        Material startMaterial;
        Transform backdropRoot;

        public void ApplyWorld(Transform startPlatform, Light sun) {
            CaptureTemplate(startPlatform);
            ApplyAtmosphere(sun);
            StyleStartPlatform(startPlatform);
            BuildBackdrop(startPlatform);
        }

        static void CaptureTemplate(Transform startPlatform) {
            if (startPlatform != null) {
                var renderer = startPlatform.GetComponentInChildren<Renderer>();
                if (renderer != null) PCGLabTheme.SetTemplate(renderer.sharedMaterial);
            }
        }

        public void StyleGeneratedLevel(LevelGenerator generator) {
            ClearTransient();
            if (generator == null) return;

            foreach (var chunk in generator.SpawnedChunks) {
                if (chunk == null) continue;
                var category = ResolveCategory(generator, chunk);
                StyleChunk(chunk, category);
                DecorateCheckpoints(generator, chunk);
            }
        }

        public void PulseTimedPlatform(PCGTimedPlatform platform, TimedPlatformState state) {
            if (platform == null) return;
            if (timedWarning == null) {
                var source = platform.GetComponentInChildren<Renderer>()?.sharedMaterial;
                timedWarning = PCGLabTheme.CreateTintedClone(
                    source,
                    PCGLabTheme.CategoryColor(ChunkCategory.Timed) * 1.35f);
                timedHidden = PCGLabTheme.CreateTintedClone(
                    source,
                    new Color(0.18f, 0.1f, 0.06f));
            }

            var renderers = platform.GetComponentsInChildren<Renderer>(true);
            var material = state == TimedPlatformState.Warning ? timedWarning :
                state == TimedPlatformState.Hidden ? timedHidden :
                GetMaterial(ChunkCategory.Timed);
            foreach (var renderer in renderers) {
                if (renderer != null) renderer.sharedMaterial = material;
            }
        }

        public void Dispose() {
            ClearTransient();
            DestroyList(worldObjects);
            if (backdropRoot != null) Object.Destroy(backdropRoot.gameObject);
            DestroyMaterial(startMaterial);
            DestroyMaterial(timedWarning);
            DestroyMaterial(timedHidden);
            foreach (var pair in categoryMaterials) DestroyMaterial(pair.Value);
            categoryMaterials.Clear();
        }

        public static ChunkCategory ResolveCategory(LevelGenerator generator, PlatformChunk chunk) {
            if (generator == null || chunk == null)
                return ChunkCategory.Basic;

            var underscore = chunk.name.IndexOf('_');
            if (underscore < 0 || underscore + 1 >= chunk.name.Length) return ChunkCategory.Basic;
            var chunkId = chunk.name.Substring(underscore + 1);
            var config = generator.Config;
            if (config != null && config.Chunks != null) {
                foreach (var data in config.Chunks) {
                    if (data != null && data.ChunkId == chunkId) return data.Category;
                }
            }

            return InferCategoryFromId(chunkId);
        }

        public static ChunkCategory InferCategoryFromId(string chunkId) {
            if (string.IsNullOrEmpty(chunkId)) return ChunkCategory.Basic;
            if (chunkId.StartsWith("moving")) return ChunkCategory.Moving;
            if (chunkId.StartsWith("timed")) return ChunkCategory.Timed;
            if (chunkId.StartsWith("dash") || chunkId.StartsWith("double")) return ChunkCategory.AbilityGate;
            if (chunkId.StartsWith("combat")) return ChunkCategory.Combat;
            if (chunkId.StartsWith("recovery")) return ChunkCategory.Recovery;
            if (chunkId.StartsWith("turn") || chunkId.StartsWith("offset") || chunkId.StartsWith("climb") ||
                chunkId.StartsWith("descend"))
                return ChunkCategory.Exploration;
            return ChunkCategory.Basic;
        }

        void ApplyAtmosphere(Light sun) {
            var sky = PCGLabTheme.CreateSkybox();
            if (sky != null) RenderSettings.skybox = sky;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = PCGLabTheme.SkyZenith * 0.45f;
            RenderSettings.ambientEquatorColor = PCGLabTheme.SkyHorizon * 0.35f;
            RenderSettings.ambientGroundColor = PCGLabTheme.GroundTint * 0.2f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = PCGLabTheme.Fog;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.subtractiveShadowColor = PCGLabTheme.GroundTint;

            if (sun != null) {
                sun.color = PCGLabTheme.Sun;
                sun.intensity = 1.25f;
                sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(28f, -38f, 0f);
            }

            DynamicGI.UpdateEnvironment();
        }

        void StyleStartPlatform(Transform startPlatform) {
            if (startPlatform == null) return;
            var firstRenderer = startPlatform.GetComponentInChildren<Renderer>();
            startMaterial = PCGLabTheme.CreateTintedClone(
                firstRenderer != null ? firstRenderer.sharedMaterial : null,
                new Color(0.78f, 0.52f, 0.34f));
            foreach (var renderer in startPlatform.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = startMaterial;

            CreateTrim(startPlatform, new Vector3(0f, 0.56f, 0f), new Vector3(8.2f, 0.08f, 7.2f));
        }

        void StyleChunk(PlatformChunk chunk, ChunkCategory category) {
            var material = GetMaterial(category, chunk);
            foreach (var renderer in chunk.GetComponentsInChildren<Renderer>()) {
                if (renderer == null) continue;
                if (renderer.GetComponent<PCGTimedPlatform>() != null ||
                    renderer.GetComponentInParent<PCGTimedPlatform>() != null) {
                    renderer.sharedMaterial = GetMaterial(ChunkCategory.Timed, chunk);
                    continue;
                }
                renderer.sharedMaterial = material;
            }

            if (category == ChunkCategory.Moving) AddMovingHalo(chunk);
            if (category == ChunkCategory.Recovery) AddRecoveryMarker(chunk);
            if (category == ChunkCategory.AbilityGate) AddGatePylons(chunk);
        }

        void DecorateCheckpoints(LevelGenerator generator, PlatformChunk chunk) {
            foreach (var checkpoint in chunk.GetComponentsInChildren<ChunkCheckpoint>()) {
                var lastIndex = -1;
                if (generator != null && generator.LastManifest != null)
                    lastIndex = generator.LastManifest.chunks.Count - 1;
                var isFinish = lastIndex >= 0 && checkpoint.name.EndsWith($"_{lastIndex:00}");
                var color = isFinish
                    ? PCGLabTheme.CategoryColor(ChunkCategory.Finish)
                    : PCGLabTheme.Accent;
                var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beacon.name = "CheckpointBeacon";
                beacon.transform.SetParent(checkpoint.transform, false);
                beacon.transform.localPosition = new Vector3(0f, 0.1f, 0f);
                beacon.transform.localScale = new Vector3(0.35f, 1.1f, 0.35f);
                Object.Destroy(beacon.GetComponent<Collider>());
                beacon.GetComponent<Renderer>().sharedMaterial =
                    PCGLabTheme.CreateLitMaterial(color, color * 1.6f, 0.7f);
                transientObjects.Add(beacon);

                var lightObject = new GameObject("CheckpointLight");
                lightObject.transform.SetParent(checkpoint.transform, false);
                lightObject.transform.localPosition = Vector3.up * 1.2f;
                var point = lightObject.AddComponent<Light>();
                point.type = LightType.Point;
                point.range = 7f;
                point.intensity = isFinish ? 2.2f : 1.4f;
                point.color = color;
                point.shadows = LightShadows.None;
                transientObjects.Add(lightObject);

                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "CheckpointRing";
                ring.transform.SetParent(checkpoint.transform, false);
                ring.transform.localPosition = new Vector3(0f, -1.15f, 0f);
                ring.transform.localScale = new Vector3(3.2f, 0.05f, 3.2f);
                Object.Destroy(ring.GetComponent<Collider>());
                ring.GetComponent<Renderer>().sharedMaterial =
                    PCGLabTheme.CreateLitMaterial(color * 0.4f, color * 0.8f, 0.6f);
                transientObjects.Add(ring);
            }
        }

        void BuildBackdrop(Transform startPlatform) {
            var parent = startPlatform != null ? startPlatform.root : null;
            backdropRoot = new GameObject("PCG Lab Atmosphere").transform;
            if (parent != null) backdropRoot.SetParent(parent, true);

            var fillObject = new GameObject("Fill Light");
            fillObject.transform.SetParent(backdropRoot, false);
            fillObject.transform.rotation = Quaternion.Euler(18f, 140f, 0f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = PCGLabTheme.FillLight;
            fill.intensity = 0.38f;
            fill.shadows = LightShadows.None;
            worldObjects.Add(fillObject);

            var voidFloor = CreatePrimitive(
                PrimitiveType.Cube,
                "Void Floor",
                new Vector3(0f, -18f, 40f),
                new Vector3(220f, 1f, 220f),
                PCGLabTheme.CreateLitMaterial(new Color(0.08f, 0.06f, 0.07f), Color.black, 0.05f));
            worldObjects.Add(voidFloor);

            var mesaMaterial = PCGLabTheme.CreateLitMaterial(
                new Color(0.42f, 0.24f, 0.18f),
                Color.black,
                0.08f);
            CreateMesa(new Vector3(-38f, -6f, 28f), new Vector3(18f, 18f, 14f), mesaMaterial);
            CreateMesa(new Vector3(46f, -8f, 52f), new Vector3(22f, 24f, 16f), mesaMaterial);
            CreateMesa(new Vector3(-22f, -10f, 78f), new Vector3(26f, 16f, 20f), mesaMaterial);
            CreateMesa(new Vector3(18f, -12f, 110f), new Vector3(30f, 20f, 18f), mesaMaterial);
            CreateMesa(new Vector3(-55f, -4f, 64f), new Vector3(14f, 22f, 12f), mesaMaterial);

            var haze = CreatePrimitive(
                PrimitiveType.Quad,
                "Horizon Haze",
                new Vector3(0f, 6f, 140f),
                new Vector3(260f, 40f, 1f),
                PCGLabTheme.CreateLitMaterial(
                    new Color(PCGLabTheme.Fog.r, PCGLabTheme.Fog.g, PCGLabTheme.Fog.b, 0.35f),
                    PCGLabTheme.SkyZenith * 0.15f,
                    0.05f));
            worldObjects.Add(haze);
            worldObjects.Add(CreateDust(new Vector3(0f, 4f, 20f)));
        }

        void CreateMesa(Vector3 position, Vector3 scale, Material material) {
            var mesa = CreatePrimitive(PrimitiveType.Cube, "Mesa", position, scale, material);
            worldObjects.Add(mesa);
        }

        GameObject CreateDust(Vector3 position) {
            var dust = new GameObject("Dust Motes");
            dust.transform.position = position;
            var particles = dust.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 8f;
            main.startSpeed = 0.15f;
            main.startSize = 0.08f;
            main.startColor = new Color(1f, 0.86f, 0.7f, 0.35f);
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.rateOverTime = 8f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 10f, 40f);
            var renderer = dust.GetComponent<ParticleSystemRenderer>();
            renderer.material = PCGLabTheme.CreateLitMaterial(
                new Color(1f, 0.9f, 0.75f, 0.4f),
                new Color(1f, 0.8f, 0.5f) * 0.2f,
                0.1f);
            return dust;
        }

        void AddMovingHalo(PlatformChunk chunk) {
            foreach (var mover in chunk.GetComponentsInChildren<PCGOscillatingPlatform>()) {
                var halo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                halo.name = "MovingHalo";
                halo.transform.SetParent(mover.transform, false);
                halo.transform.localPosition = Vector3.up * 0.55f;
                halo.transform.localScale = new Vector3(1.05f, 0.08f, 1.05f);
                Object.Destroy(halo.GetComponent<Collider>());
                halo.GetComponent<Renderer>().sharedMaterial = GetMaterial(ChunkCategory.Moving);
                transientObjects.Add(halo);
            }
        }

        void AddRecoveryMarker(PlatformChunk chunk) {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "RecoveryMarker";
            marker.transform.SetParent(chunk.transform, false);
            marker.transform.localPosition = new Vector3(0f, 2.2f, 3.5f);
            marker.transform.localScale = Vector3.one * 0.45f;
            Object.Destroy(marker.GetComponent<Collider>());
            marker.GetComponent<Renderer>().sharedMaterial = GetMaterial(ChunkCategory.Recovery);
            transientObjects.Add(marker);
        }

        void AddGatePylons(PlatformChunk chunk) {
            var color = GetMaterial(ChunkCategory.AbilityGate);
            CreatePylon(chunk.transform, new Vector3(-1.6f, 1.2f, 1.5f), color);
            CreatePylon(chunk.transform, new Vector3(1.6f, 1.2f, 1.5f), color);
        }

        void CreatePylon(Transform parent, Vector3 localPosition, Material material) {
            var pylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pylon.name = "AbilityPylon";
            pylon.transform.SetParent(parent, false);
            pylon.transform.localPosition = localPosition;
            pylon.transform.localScale = new Vector3(0.25f, 1.1f, 0.25f);
            Object.Destroy(pylon.GetComponent<Collider>());
            pylon.GetComponent<Renderer>().sharedMaterial = material;
            transientObjects.Add(pylon);
        }

        void CreateTrim(Transform parent, Vector3 localPosition, Vector3 localScale) {
            var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "StartTrim";
            trim.transform.SetParent(parent, false);
            trim.transform.localPosition = localPosition;
            trim.transform.localScale = localScale;
            Object.Destroy(trim.GetComponent<Collider>());
            trim.GetComponent<Renderer>().sharedMaterial = PCGLabTheme.CreateLitMaterial(
                PCGLabTheme.Accent,
                PCGLabTheme.Accent * 0.7f,
                0.6f);
            worldObjects.Add(trim);
        }

        GameObject CreatePrimitive(
            PrimitiveType type,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material) {
            var createdObject = GameObject.CreatePrimitive(type);
            createdObject.name = name;
            createdObject.transform.SetParent(backdropRoot, true);
            createdObject.transform.position = position;
            createdObject.transform.localScale = scale;
            Object.Destroy(createdObject.GetComponent<Collider>());
            createdObject.GetComponent<Renderer>().sharedMaterial = material;
            return createdObject;
        }

        Material GetMaterial(ChunkCategory category, PlatformChunk chunk = null) {
            if (categoryMaterials.TryGetValue(category, out var existing) && existing != null)
                return existing;
            Material source = null;
            if (chunk != null) {
                var renderer = chunk.GetComponentInChildren<Renderer>();
                if (renderer != null) source = renderer.sharedMaterial;
            }
            var createdMaterial = PCGLabTheme.CreateTintedClone(
                source,
                PCGLabTheme.CategoryColor(category));
            categoryMaterials[category] = createdMaterial;
            return createdMaterial;
        }

        void ClearTransient() => DestroyList(transientObjects);

        static void DestroyList(List<GameObject> objects) {
            for (var i = objects.Count - 1; i >= 0; i--) {
                if (objects[i] != null) Object.Destroy(objects[i]);
            }
            objects.Clear();
        }

        static void DestroyMaterial(Material material) {
            if (material != null) Object.Destroy(material);
        }
    }
}

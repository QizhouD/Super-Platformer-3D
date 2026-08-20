using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Platformer.PCG {
    public sealed class PCGLabHud : MonoBehaviour {
        PCGDebugPanel panel;
        PCGRunController runController;
        PCGRunTelemetry telemetry;
        PCGAdaptiveDifficultyDirector difficulty;
        PCGMultimodalDatasetRecorder dataset;
        IPCGTrainingController training;
        LevelGenerator generator;

        Text titleMeta;
        Text progressText;
        Image progressFill;
        Text statsText;
        Text toastText;
        Text manifestText;
        InputField seedField;
        Toggle doubleJumpToggle;
        Toggle dashToggle;
        Toggle adaptiveToggle;
        Toggle trainingToggle;
        GameObject toolsBody;
        GameObject manifestBody;
        float toastUntil;
        string pendingToast;
        bool toolsOpen = true;
        bool manifestOpen;
        Font font;
        Sprite panelSprite;
        Sprite buttonSprite;
        Sprite fillSprite;

        public bool IsReady { get; private set; }

        public bool Configure(
            PCGDebugPanel debugPanel,
            LevelGenerator levelGenerator,
            PCGRunController controller,
            PCGRunTelemetry runTelemetry,
            PCGAdaptiveDifficultyDirector adaptiveDirector,
            PCGGameAIObservationSensor observationSensor,
            PCGMultimodalDatasetRecorder datasetRecorder,
            IPCGTrainingController trainingController) {
            panel = debugPanel;
            generator = levelGenerator;
            runController = controller;
            telemetry = runTelemetry;
            difficulty = adaptiveDirector;
            dataset = datasetRecorder;
            _ = observationSensor;
            training = trainingController;
            try {
                Build();
                IsReady = titleMeta != null;
            } catch (Exception exception) {
                IsReady = false;
                Debug.LogError($"PCG Lab HUD failed to build, using the legacy panel. {exception}", this);
            }

            if (panel != null) panel.HideLegacyGui = IsReady;
            return IsReady;
        }

        public void ShowToast(string message, float duration = 1.8f) {
            pendingToast = message;
            toastUntil = Time.unscaledTime + duration;
        }

        public void SyncSeed(string seed) {
            if (seedField != null && !seedField.isFocused) seedField.text = seed;
        }

        void Update() {
            if (titleMeta == null) return;
            Refresh();
        }

        void OnDestroy() {
            if (panelSprite != null) Destroy(panelSprite.texture);
            if (buttonSprite != null) Destroy(buttonSprite.texture);
            if (fillSprite != null) Destroy(fillSprite.texture);
        }

        void Build() {
            font = ResolveFont();
            if (font == null)
                throw new InvalidOperationException("No UI font is available.");

            panelSprite = CreateSprite(PCGLabTheme.CreateRoundedRect(64, 64, 12, PCGLabTheme.Panel, PCGLabTheme.Accent * 0.5f), 12f);
            buttonSprite = CreateSprite(PCGLabTheme.CreateRoundedRect(64, 64, 10, PCGLabTheme.PanelInner, PCGLabTheme.Accent), 10f);
            fillSprite = CreateSprite(PCGLabTheme.CreateRoundedRect(64, 64, 10, PCGLabTheme.Accent, PCGLabTheme.Accent), 10f);

            EnsureEventSystem();

            var canvasObject = new GameObject("PCG Lab HUD");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var left = CreatePanel(canvasObject.transform, "Left Panel", new Vector2(24f, -24f), new Vector2(430f, 980f), TextAnchor.UpperLeft);
            titleMeta = CreateText(left, "Title", "PCG LAB", 28, FontStyle.Bold, PCGLabTheme.Accent, new Vector2(18f, -16f), new Vector2(394f, 36f));
            progressText = CreateText(left, "Progress", "Checkpoint 0 / 16", 16, FontStyle.Bold, PCGLabTheme.Text, new Vector2(18f, -58f), new Vector2(394f, 24f));
            var barBack = CreateImage(left, "Progress Back", new Vector2(18f, -88f), new Vector2(394f, 14f), new Color(1f, 1f, 1f, 0.08f), panelSprite);
            progressFill = CreateImage(barBack.transform, "Progress Fill", Vector2.zero, new Vector2(394f, 14f), PCGLabTheme.Accent, fillSprite);
            var fillRect = progressFill.rectTransform;
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.sizeDelta = new Vector2(0f, 0f);
            statsText = CreateText(left, "Stats", string.Empty, 15, FontStyle.Normal, PCGLabTheme.Muted, new Vector2(18f, -114f), new Vector2(394f, 150f));

            var toolsToggle = CreateButton(left, "Tools Toggle", "LAB TOOLS", new Vector2(18f, -274f), new Vector2(394f, 36f), () => {
                toolsOpen = !toolsOpen;
                toolsBody.SetActive(toolsOpen);
            });
            _ = toolsToggle;
            toolsBody = new GameObject("Tools Body", typeof(RectTransform));
            toolsBody.transform.SetParent(left, false);
            Stretch(toolsBody.GetComponent<RectTransform>(), new Vector2(10f, -318f), new Vector2(410f, 430f));

            seedField = CreateInput(toolsBody.transform, "Seed", panel != null ? panel.SeedText : "82431", new Vector2(8f, -8f), new Vector2(250f, 34f));
            CreateButton(toolsBody.transform, "Generate", "GENERATE", new Vector2(266f, -8f), new Vector2(132f, 34f), () => {
                panel?.GenerateFromSeed(seedField.text);
            });
            CreateButton(toolsBody.transform, "Random", "RANDOM", new Vector2(8f, -50f), new Vector2(126f, 32f), () => panel?.GenerateRandomSeed());
            CreateButton(toolsBody.transform, "Copy Seed", "COPY SEED", new Vector2(142f, -50f), new Vector2(126f, 32f), () => panel?.CopySeed());
            CreateButton(toolsBody.transform, "Copy Manifest", "MANIFEST", new Vector2(276f, -50f), new Vector2(122f, 32f), () => panel?.CopyManifest());

            doubleJumpToggle = CreateToggle(toolsBody.transform, "Double Jump", panel != null && panel.DoubleJumpEnabled, new Vector2(8f, -94f), value => panel?.SetDoubleJump(value));
            dashToggle = CreateToggle(toolsBody.transform, "Dash", panel != null && panel.DashEnabled, new Vector2(210f, -94f), value => panel?.SetDash(value));
            adaptiveToggle = CreateToggle(toolsBody.transform, "Adaptive Difficulty", panel == null || panel.AdaptiveDifficultyEnabled, new Vector2(8f, -132f), value => panel?.SetAdaptiveDifficulty(value));
            if (training != null)
                trainingToggle = CreateToggle(toolsBody.transform, "ML-Agents Training Mode", panel != null && panel.TrainingModeEnabled, new Vector2(8f, -170f), value => panel?.SetTrainingMode(value));

            CreateButton(toolsBody.transform, "Copy Telemetry", "COPY TELEMETRY", new Vector2(8f, -214f), new Vector2(190f, 32f), () => panel?.CopyTelemetry());
            CreateButton(toolsBody.transform, "Copy Observation", "COPY OBSERVATION", new Vector2(206f, -214f), new Vector2(192f, 32f), () => panel?.CopyObservation());
            CreateButton(toolsBody.transform, "Start Dataset", "START DATASET", new Vector2(8f, -254f), new Vector2(190f, 32f), () => panel?.StartDatasetRecording());
            CreateButton(toolsBody.transform, "Stop Dataset", "STOP DATASET", new Vector2(206f, -254f), new Vector2(192f, 32f), () => panel?.StopDatasetRecording());
            CreateButton(toolsBody.transform, "Copy Dataset", "DATASET PATH", new Vector2(8f, -294f), new Vector2(390f, 32f), () => panel?.CopyDatasetPath());

            CreateButton(left, "Manifest Toggle", "SHOW MANIFEST", new Vector2(18f, -760f), new Vector2(394f, 32f), () => {
                manifestOpen = !manifestOpen;
                manifestBody.SetActive(manifestOpen);
            });
            manifestBody = CreatePanel(left, "Manifest Body", new Vector2(18f, -800f), new Vector2(394f, 160f), TextAnchor.UpperLeft).gameObject;
            manifestText = CreateText(manifestBody.transform, "Manifest", string.Empty, 12, FontStyle.Normal, PCGLabTheme.Muted, new Vector2(8f, -8f), new Vector2(378f, 144f));
            manifestBody.SetActive(false);

            CreateText(canvasObject.transform, "Hints", "WASD MOVE   SPACE JUMP   SHIFT DASH   RMB CAMERA", 14, FontStyle.Bold, PCGLabTheme.Text, new Vector2(0f, 28f), new Vector2(900f, 28f), TextAnchor.LowerCenter);
            toastText = CreateText(canvasObject.transform, "Toast", string.Empty, 26, FontStyle.Bold, PCGLabTheme.Accent, new Vector2(0f, -80f), new Vector2(900f, 48f), TextAnchor.UpperCenter);
        }

        void Refresh() {
            var total = generator != null && generator.LastManifest != null
                ? Mathf.Max(1, generator.LastManifest.chunks.Count)
                : 16;
            var checkpoint = runController != null ? Mathf.Max(0, runController.FurthestCheckpoint + 1) : 0;
            var seed = generator != null ? generator.Seed.ToString() : "—";
            titleMeta.text = $"PCG LAB    SEED {seed}";
            progressText.text = $"CHECKPOINT  {checkpoint} / {total}";
            progressFill.rectTransform.sizeDelta = new Vector2(394f * Mathf.Clamp01(checkpoint / (float)total), 0f);

            var time = Time.timeSinceLevelLoad;
            var resets = runController != null ? runController.ResetCount : 0;
            var skill = difficulty != null ? difficulty.SkillEstimate : 0.5f;
            var bias = difficulty != null ? difficulty.DifficultyBias : 0f;
            var events = telemetry != null ? telemetry.Events.Count : 0;
            var datasetLabel = dataset == null ? "off" :
                dataset.IsRecording ? "RECORDING" :
                dataset.LastSummary != null ? $"{dataset.LastSummary.sampleCount} saved" : "ready";
            var trainingLabel = training == null
                ? "idle"
                : $"{training.CompletedEpisodes} win / {training.FailedEpisodes} fail   R {training.LastEpisodeReward:F2}";
            statsText.text =
                $"TIME  {Mathf.FloorToInt(time / 60f):00}:{Mathf.FloorToInt(time % 60f):00}\n" +
                $"RESETS  {resets}     TELEMETRY  {events}\n" +
                $"SKILL  {skill:0.00}     PCG BIAS  {bias:+0.00;-0.00;0.00}\n" +
                $"OBS  {PCGGameAIObservation.VectorSize}D + {PCGGameAIObservationSensor.VisualWidth}x{PCGGameAIObservationSensor.VisualHeight}\n" +
                $"DATASET  {datasetLabel}\n" +
                $"ML  {trainingLabel}";

            if (seedField != null && !seedField.isFocused && generator != null)
                seedField.text = generator.Seed.ToString();
            if (doubleJumpToggle != null && panel != null) doubleJumpToggle.SetIsOnWithoutNotify(panel.DoubleJumpEnabled);
            if (dashToggle != null && panel != null) dashToggle.SetIsOnWithoutNotify(panel.DashEnabled);
            if (adaptiveToggle != null && panel != null) adaptiveToggle.SetIsOnWithoutNotify(panel.AdaptiveDifficultyEnabled);
            if (trainingToggle != null && panel != null) trainingToggle.SetIsOnWithoutNotify(panel.TrainingModeEnabled);

            if (manifestOpen && generator != null && generator.LastManifest != null)
                manifestText.text = generator.LastManifest.ToJson();

            if (Time.unscaledTime < toastUntil) {
                toastText.text = pendingToast;
                toastText.color = PCGLabTheme.Accent;
            } else {
                toastText.text = string.Empty;
            }
        }

        static Font ResolveFont() {
            return Resources.GetBuiltinResource<Font>("Arial.ttf") ??
                   Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ??
                   Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Segoe UI", "Helvetica", "Liberation Sans" }, 16);
        }

        static void EnsureEventSystem() {
            if (FindObjectOfType<EventSystem>() != null) return;
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        RectTransform CreatePanel(Transform parent, string name, Vector2 anchored, Vector2 size, TextAnchor _) {
            var image = CreateImage(parent, name, anchored, size, Color.white, panelSprite);
            return image.rectTransform;
        }

        Text CreateText(
            Transform parent,
            string name,
            string content,
            int size,
            FontStyle style,
            Color color,
            Vector2 anchored,
            Vector2 rectSize,
            TextAnchor screenAnchor = TextAnchor.UpperLeft) {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            ApplyAnchor(rect, screenAnchor, anchored, rectSize);
            var text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.text = content;
            text.alignment = TextAnchor.UpperLeft;
            if (screenAnchor == TextAnchor.UpperCenter || screenAnchor == TextAnchor.LowerCenter)
                text.alignment = screenAnchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        Button CreateButton(Transform parent, string name, string label, Vector2 anchored, Vector2 size, UnityEngine.Events.UnityAction onClick) {
            var image = CreateImage(parent, name, anchored, size, Color.white, buttonSprite);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.82f, 0.58f);
            colors.pressedColor = PCGLabTheme.Accent;
            button.colors = colors;
            button.onClick.AddListener(onClick);
            var text = CreateText(image.transform, "Label", label, 13, FontStyle.Bold, PCGLabTheme.Text, Vector2.zero, size);
            text.alignment = TextAnchor.MiddleCenter;
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        Toggle CreateToggle(Transform parent, string label, bool isOn, Vector2 anchored, UnityEngine.Events.UnityAction<bool> onChanged) {
            var root = new GameObject(label, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            ApplyAnchor(rect, TextAnchor.UpperLeft, anchored, new Vector2(190f, 28f));
            var toggle = root.AddComponent<Toggle>();
            var box = CreateImage(root.transform, "Box", new Vector2(0f, -2f), new Vector2(22f, 22f), Color.white, buttonSprite);
            var check = CreateImage(box.transform, "Check", new Vector2(3f, -3f), new Vector2(16f, 16f), PCGLabTheme.Accent, fillSprite);
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.isOn = isOn;
            toggle.onValueChanged.AddListener(onChanged);
            var text = CreateText(root.transform, "Label", label.ToUpperInvariant(), 13, FontStyle.Bold, PCGLabTheme.Text, new Vector2(30f, 0f), new Vector2(160f, 28f));
            text.alignment = TextAnchor.MiddleLeft;
            return toggle;
        }

        InputField CreateInput(Transform parent, string name, string value, Vector2 anchored, Vector2 size) {
            var image = CreateImage(parent, name, anchored, size, Color.white, buttonSprite);
            var text = CreateText(image.transform, "Text", value, 16, FontStyle.Bold, PCGLabTheme.Text, new Vector2(8f, 0f), new Vector2(size.x - 16f, size.y));
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            var input = image.gameObject.AddComponent<InputField>();
            input.textComponent = text;
            input.text = value;
            input.characterLimit = 12;
            input.contentType = InputField.ContentType.IntegerNumber;
            return input;
        }

        Image CreateImage(Transform parent, string name, Vector2 anchored, Vector2 size, Color color, Sprite sprite) {
            var imageObject = new GameObject(name, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            var rect = imageObject.GetComponent<RectTransform>();
            ApplyAnchor(rect, TextAnchor.UpperLeft, anchored, size);
            var image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            return image;
        }

        static void ApplyAnchor(RectTransform rect, TextAnchor anchor, Vector2 anchored, Vector2 size) {
            switch (anchor) {
                case TextAnchor.UpperCenter:
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    break;
                case TextAnchor.LowerCenter:
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    break;
                default:
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    break;
            }
            rect.anchoredPosition = anchored;
            rect.sizeDelta = size;
        }

        static void Stretch(RectTransform rect, Vector2 anchored, Vector2 size) {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = size;
        }

        static Sprite CreateSprite(Texture2D texture, float border) {
            var maxBorder = Mathf.Min(texture.width, texture.height) * 0.45f;
            var safeBorder = Mathf.Clamp(border, 0f, maxBorder);
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(safeBorder, safeBorder, safeBorder, safeBorder));
        }
    }
}

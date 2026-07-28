using System;
using UnityEngine;

namespace Platformer.PCG {
    public enum TimedPlatformState {
        Visible,
        Warning,
        Hidden
    }

    public sealed class PCGTimedPlatform : MonoBehaviour {
        public static event Action<PCGTimedPlatform, TimedPlatformState> StateChanged;

        [SerializeField, Min(0.1f)] float visibleDuration = 2.5f;
        [SerializeField, Min(0.1f)] float warningDuration = 0.8f;
        [SerializeField, Min(0.1f)] float hiddenDuration = 1.4f;
        [SerializeField, Min(0f)] float phaseOffset;
        [SerializeField, Min(1f)] float warningFlashRate = 8f;

        Collider[] platformColliders;
        Renderer[] platformRenderers;
        float elapsed;

        public TimedPlatformState State { get; private set; } = TimedPlatformState.Visible;

        void Awake() {
            platformColliders = GetComponentsInChildren<Collider>(true);
            platformRenderers = GetComponentsInChildren<Renderer>(true);
            elapsed = phaseOffset;
            ApplyState(EvaluateState(elapsed, visibleDuration, warningDuration, hiddenDuration), true);
        }

        void Update() {
            elapsed += Time.deltaTime;
            var nextState = EvaluateState(elapsed, visibleDuration, warningDuration, hiddenDuration);
            if (nextState != State) ApplyState(nextState, false);

            if (State == TimedPlatformState.Warning) {
                var visible = Mathf.FloorToInt(elapsed * warningFlashRate) % 2 == 0;
                SetRenderers(visible);
            }
        }

        void OnDisable() {
            SetColliders(true);
            SetRenderers(true);
        }

        public void Configure(float visible, float warning, float hidden, float offset = 0f) {
            visibleDuration = Mathf.Max(0.1f, visible);
            warningDuration = Mathf.Max(0.1f, warning);
            hiddenDuration = Mathf.Max(0.1f, hidden);
            phaseOffset = Mathf.Max(0f, offset);
        }

        public void ApplyDifficulty(float normalizedDifficulty) {
            var difficulty = Mathf.Clamp01(normalizedDifficulty);
            visibleDuration = Mathf.Lerp(4f, 2f, difficulty);
            warningDuration = Mathf.Lerp(1.2f, 0.5f, difficulty);
            hiddenDuration = Mathf.Lerp(0.8f, 1.8f, difficulty);
        }

        public static TimedPlatformState EvaluateState(
            float time,
            float visible,
            float warning,
            float hidden) {
            visible = Mathf.Max(0.1f, visible);
            warning = Mathf.Max(0.1f, warning);
            hidden = Mathf.Max(0.1f, hidden);
            var phase = Mathf.Repeat(Mathf.Max(0f, time), visible + warning + hidden);
            if (phase < visible) return TimedPlatformState.Visible;
            if (phase < visible + warning) return TimedPlatformState.Warning;
            return TimedPlatformState.Hidden;
        }

        void ApplyState(TimedPlatformState state, bool force) {
            if (!force && State == state) return;
            State = state;
            var available = state != TimedPlatformState.Hidden;
            SetColliders(available);
            SetRenderers(available);
            StateChanged?.Invoke(this, state);
        }

        void SetColliders(bool value) {
            if (platformColliders == null) return;
            foreach (var platformCollider in platformColliders)
                if (platformCollider != null) platformCollider.enabled = value;
        }

        void SetRenderers(bool value) {
            if (platformRenderers == null) return;
            foreach (var platformRenderer in platformRenderers)
                if (platformRenderer != null) platformRenderer.enabled = value;
        }
    }
}

using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGOscillatingPlatform : MonoBehaviour {
        [SerializeField] Vector3 localOffset = new Vector3(-3f, 0f, 0f);
        [SerializeField, Min(0.1f)] float travelDuration = 1.6f;
        [SerializeField, Min(0f)] float endpointPause = 0.35f;

        Vector3 startLocalPosition;
        float elapsed;

        public float NormalizedPosition { get; private set; }

        void Awake() {
            startLocalPosition = transform.localPosition;
        }

        void FixedUpdate() {
            elapsed += Time.fixedDeltaTime;
            NormalizedPosition = EvaluateNormalizedPosition(elapsed, travelDuration, endpointPause);
            transform.localPosition = startLocalPosition +
                                      localOffset * SmoothStep01(NormalizedPosition);
        }

        public void Configure(Vector3 offset, float duration, float pause) {
            localOffset = offset;
            travelDuration = Mathf.Max(0.1f, duration);
            endpointPause = Mathf.Max(0f, pause);
        }

        public static float EvaluateNormalizedPosition(float time, float duration, float pause) {
            duration = Mathf.Max(0.1f, duration);
            pause = Mathf.Max(0f, pause);
            var halfCycle = duration + pause;
            var cycle = halfCycle * 2f;
            var phase = Mathf.Repeat(Mathf.Max(0f, time), cycle);

            if (phase < pause) return 0f;
            if (phase < pause + duration) return (phase - pause) / duration;
            if (phase < pause * 2f + duration) return 1f;
            return 1f - (phase - pause * 2f - duration) / duration;
        }

        static float SmoothStep01(float value) =>
            value * value * (3f - 2f * value);
    }
}

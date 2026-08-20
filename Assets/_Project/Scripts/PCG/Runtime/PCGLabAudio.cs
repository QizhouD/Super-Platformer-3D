using UnityEngine;

namespace Platformer.PCG {
    public sealed class PCGLabAudio : MonoBehaviour {
        const string SoundRoot = "Assets/Casual Game Sounds U6/CasualGameSounds/";

        AudioSource music;
        AudioSource sfx;
        AudioClip jumpClip;
        AudioClip dashClip;
        AudioClip checkpointClip;
        AudioClip finishClip;
        AudioClip respawnClip;
        AudioClip generateClip;
        AudioClip warningClip;
        AudioClip uiClip;
        float warningCooldown;
        float nextJumpSoundTime;

        public void Configure() {
            music = CreateSource("Music", 0.12f, true);
            sfx = CreateSource("SFX", 0.7f, false);
            LoadClips();
            music.clip = CreateAmbientLoop();
            music.Play();
        }

        public void PlayJump() {
            if (Time.unscaledTime < nextJumpSoundTime) return;
            nextJumpSoundTime = Time.unscaledTime + 0.18f;
            Play(jumpClip, 0.55f, 0.96f, 1.06f);
        }
        public void PlayDash() => Play(dashClip, 0.62f, 0.92f, 1.08f);
        public void PlayCheckpoint() => Play(checkpointClip, 0.72f, 0.98f, 1.04f);
        public void PlayFinish() => Play(finishClip, 0.85f, 1f, 1f);
        public void PlayRespawn() => Play(respawnClip, 0.7f, 0.9f, 1f);
        public void PlayGenerate() => Play(generateClip, 0.55f, 1f, 1f);
        public void PlayUi() => Play(uiClip, 0.4f, 1f, 1f);

        public void PlayTimedWarning() {
            if (Time.unscaledTime < warningCooldown) return;
            warningCooldown = Time.unscaledTime + 0.35f;
            Play(warningClip, 0.35f, 1.05f, 1.15f);
        }

        void Play(AudioClip clip, float volume, float minPitch, float maxPitch) {
            if (sfx == null || clip == null) return;
            sfx.pitch = Random.Range(minPitch, maxPitch);
            sfx.PlayOneShot(clip, volume);
        }

        AudioSource CreateSource(string name, float volume, bool loop) {
            var source = new GameObject(name).AddComponent<AudioSource>();
            source.transform.SetParent(transform, false);
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }

        void LoadClips() {
#if UNITY_EDITOR
            jumpClip = LoadEditorClip("DM-CGS-21.wav");
            dashClip = LoadEditorClip("DM-CGS-20.wav");
            checkpointClip = LoadEditorClip("DM-CGS-26.wav");
            finishClip = LoadEditorClip("DM-CGS-45.wav");
            respawnClip = LoadEditorClip("DM-CGS-11.wav");
            generateClip = LoadEditorClip("DM-CGS-16.wav");
            warningClip = LoadEditorClip("DM-CGS-03.wav");
            uiClip = LoadEditorClip("DM-CGS-01.wav");
#endif
        }

#if UNITY_EDITOR
        static AudioClip LoadEditorClip(string fileName) =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(SoundRoot + fileName);
#endif

        static AudioClip CreateAmbientLoop() {
            const int sampleRate = 22050;
            const int length = sampleRate * 6;
            var clip = AudioClip.Create("PCGLabWind", length, 1, sampleRate, false);
            var data = new float[length];
            var noise = 0f;
            for (var i = 0; i < length; i++) {
                noise = Mathf.Lerp(noise, Random.Range(-1f, 1f), 0.02f);
                var t = i / (float)sampleRate;
                var pad = Mathf.Sin(t * 0.35f * Mathf.PI * 2f) * 0.12f +
                          Mathf.Sin(t * 0.51f * Mathf.PI * 2f) * 0.07f;
                var envelope = 0.55f + 0.45f * Mathf.Sin(t * Mathf.PI * 2f / 6f);
                data[i] = Mathf.Clamp((noise * 0.22f + pad) * envelope, -1f, 1f);
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}

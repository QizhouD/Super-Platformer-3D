using UnityEngine;

namespace Platformer {
    /// <summary>
    /// Coin or unique landmark (moon/star). Uses CollectibleDefinition plus the existing IntEventChannel.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CollectiblePickup : MonoBehaviour {
        [SerializeField] CollectibleDefinition definition;
        [SerializeField] IntEventChannel scoreChannel;
        [SerializeField] int fallbackScore = 10;
        [SerializeField] string playerTag = "Player";

        static readonly System.Collections.Generic.HashSet<string> TakenUnique =
            new System.Collections.Generic.HashSet<string>();

        void Reset() {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void OnEnable() {
            if (definition != null && definition.unique && TakenUnique.Contains(definition.id))
                gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider other) {
            if (!other.transform.root.CompareTag(playerTag)) return;
            Collect();
        }

        void Collect() {
            if (definition != null && definition.unique) {
                if (!TakenUnique.Add(definition.id)) return;
            }

            var value = definition != null ? definition.score : fallbackScore;
            scoreChannel?.Invoke(value);
            if (GameManager.Instance != null) GameManager.Instance.AddScore(value);

            var vfx = definition != null ? definition.collectVfx : null;
            if (vfx != null) Instantiate(vfx, transform.position, Quaternion.identity);

            var clip = definition != null ? definition.collectSfx : null;
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position);

            FeelFeedback.CollectAt(transform.position);
            Destroy(gameObject);
        }
    }
}

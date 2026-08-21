using UnityEngine;

namespace Platformer {
    public enum CollectibleKind {
        Coin,
        Landmark
    }

    [CreateAssetMenu(fileName = "CollectibleDefinition", menuName = "Platformer/Collectible Definition")]
    public sealed class CollectibleDefinition : ScriptableObject {
        public CollectibleKind kind = CollectibleKind.Coin;
        public string id = "coin";
        public int score = 10;
        public bool unique;
        public GameObject collectVfx;
        public AudioClip collectSfx;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.PCG {
    [Serializable]
    public sealed class GeneratedChunkRecord {
        public int index;
        public string chunkId;
        public Vector3 position;
        public Quaternion rotation;
        public float targetDifficulty;
        public float actualDifficulty;
    }

    [Serializable]
    public sealed class GeneratedLevelManifest {
        public int seed;
        public string configVersion = "1.0";
        public bool hasDoubleJump;
        public bool hasDash;
        public bool completed;
        public string failureReason;
        public List<GeneratedChunkRecord> chunks = new List<GeneratedChunkRecord>();

        public string ToJson(bool prettyPrint = true) => JsonUtility.ToJson(this, prettyPrint);

        public static GeneratedLevelManifest FromJson(string json) =>
            JsonUtility.FromJson<GeneratedLevelManifest>(json);
    }
}

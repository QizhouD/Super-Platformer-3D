using System.Collections;
using UnityEngine;

namespace Platformer {
    public sealed class CrumblingPlatform : MonoBehaviour {
        [SerializeField] string playerTag = "Player";
        [SerializeField, Min(0f)] float warnDelay = 0.45f;
        [SerializeField, Min(0.05f)] float fallDelay = 0.7f;
        [SerializeField] bool restore = true;
        [SerializeField, Min(0f)] float restoreDelay = 2.4f;

        Collider[] colliders;
        Renderer[] renderers;
        Vector3 restPosition;
        Quaternion restRotation;
        bool busy;

        void Awake() {
            colliders = GetComponentsInChildren<Collider>();
            renderers = GetComponentsInChildren<Renderer>();
            restPosition = transform.position;
            restRotation = transform.rotation;
        }

        void OnCollisionEnter(Collision collision) {
            if (busy || !collision.transform.root.CompareTag(playerTag)) return;
            StartCoroutine(Crumble());
        }

        IEnumerator Crumble() {
            busy = true;
            yield return new WaitForSeconds(warnDelay);
            SetVisible(false);
            SetSolid(false);
            yield return new WaitForSeconds(fallDelay);
            if (!restore) yield break;
            yield return new WaitForSeconds(restoreDelay);
            transform.SetPositionAndRotation(restPosition, restRotation);
            SetVisible(true);
            SetSolid(true);
            busy = false;
        }

        void SetSolid(bool value) {
            foreach (var col in colliders)
                if (col != null) col.enabled = value;
        }

        void SetVisible(bool value) {
            foreach (var renderer in renderers)
                if (renderer != null) renderer.enabled = value;
        }
    }
}

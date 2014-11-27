using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

namespace PrefabInPrefabAsset {

    [ExecuteInEditMode]
    public class VirtualPrefab : MonoBehaviour {

        void Awake() {

            if (name.StartsWith(">PrefabInPrefab") == false) {
                Debug.LogError("This is dummy script.");
                DestroyImmediate(this);
                return;
            }

            if (Application.isPlaying) {
                Destroy(gameObject);
            }
        }

        public PrefabInPrefab original;

        Vector3 lastPos;


        void Update() {
            UpdateTransform();
        }

        void UpdateTransform() {

            if (Application.isPlaying) {
                return;
            }

            if (original.gameObject == null) {
                DestroyImmediate(gameObject);
                return;
            }

            /* Skip by position */
            Transform orgTransform = original.gameObject.transform;

            if (lastPos == orgTransform.position) {
                return;
            }

            lastPos = orgTransform.position;

            /* Set transform from parent */
            this.transform.position = orgTransform.position;
            this.transform.rotation = orgTransform.rotation;
            this.transform.localScale = orgTransform.localScale;

            var virtualPrefabs = GetChildVirtualPrefabs();

            foreach (var virtualPrefab in virtualPrefabs) {
                virtualPrefab.UpdateTransform();
            }
        }

        List<VirtualPrefab> GetChildVirtualPrefabs() {
            var virtualPrefabList = new List<VirtualPrefab>();
            var prefabInPrefabs = GetComponentsInChildren<PrefabInPrefab>(true);

            foreach (var prefabInPrefab in prefabInPrefabs) {
                if (prefabInPrefab.GeneratedPrefab == null) {
                    continue;
                }

                VirtualPrefab virtualPrefab = prefabInPrefab.GeneratedPrefab.GetComponent<VirtualPrefab>();

                virtualPrefabList.Add(virtualPrefab);
            }

            return virtualPrefabList;
        }
    }

}

#endif


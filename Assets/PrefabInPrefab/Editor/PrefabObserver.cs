using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

namespace PrefabInPrefabAsset {

    public class PrefabObserver : UnityEditor.AssetModificationProcessor {

        static string[] OnWillSaveAssets(string[] paths) {

            Debug.Log("OnWillSaveAssets");

            GameObject[] virtualPrefabObjArray = GameObject.FindGameObjectsWithTag("EditorOnly");
            //string prefabPath;

            foreach (GameObject obj in virtualPrefabObjArray) {

                if (obj.name.StartsWith(">PrefabInPrefab") == false) {
                    continue;
                }

                //VirtualPrefab virtualPrefab = obj.GetComponent<VirtualPrefab>();
                //PrefabInPrefab component = virtualPrefab.original;

                //if (prefabPath == null) {
                //    prefabPath = component.GetPrefabFilePath();
                //}

                //if (paths.Any(path => path == prefabPath)) {
                //    component.ForceDrawDontEditablePrefab();
                //}
            }

            return paths;
        }
    }
}

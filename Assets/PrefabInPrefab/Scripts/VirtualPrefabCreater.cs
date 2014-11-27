using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;


public class VirtualPrefabCreater : MonoBehaviour {


    const string editorOnlyTag = "EditorOnly";

    public static void ForceDrawVirtualPrefab(PrefabInPrefab prefabInPrefab) {
        prefabInPrefab.LastPrefabHashCode = "";
        DrawVirtualPrefab(prefabInPrefab);
    }
    
    public static void DrawVirtualPrefab(PrefabInPrefab prefabInPrefab) {
        
        if (Application.isPlaying == true) {
            return;
        }
        
        if (IsNeedToRemoveVirtualPrefab(prefabInPrefab) == true ) {
            prefabInPrefab.DeleteGeneratedPrefab();
            UpdateGameView();
            return;
        }
        
        if (IsPrefabUpdated(prefabInPrefab) == false) {
            return;
        }
        
        if (IsValidationError(prefabInPrefab) == true) {
            return;
        }
        
        CreateVirtualPrefab(prefabInPrefab);
        
        UpdateGameView();
    }
    
    static bool IsNeedToRemoveVirtualPrefab(PrefabInPrefab prefabInPrefab) {
        
        if (prefabInPrefab.GeneratedPrefab == null) {
            return false;
        }
        
        if (prefabInPrefab.IsVisible_VirtualPrefab == false) {
            return true;
        }
        
        return false;
    }
    
    static bool IsPrefabUpdated(PrefabInPrefab prefabInPrefab) {
        string prefabHashCode = GetHashCodeFromPrefab(prefabInPrefab);
        
        if ( (prefabInPrefab.LastPrefabHashCode == prefabHashCode) &&
             (prefabInPrefab.GeneratedPrefab != null) ) {
            
            return false;
        }
        
        prefabInPrefab.LastPrefabHashCode = prefabHashCode;
        
        return true;
    }


    static bool IsValidationError(PrefabInPrefab prefabInPrefab) {
        
        if (prefabInPrefab.transform.parent == null) {
            /* copy & paseした時、一瞬だけparentがnullになるので、
             * 遅延実行. */
            EditorApplication.delayCall += () => {
                
                if (prefabInPrefab.transform.parent == null) {
                    Debug.LogError("Can't attach PrefabInPrefab to root gameobject.");
                    prefabInPrefab.ResetPrefab();
                } else {
                    ForceDrawVirtualPrefab(prefabInPrefab);
                }
            };
            
            return true;
        }
        
        return false;
    }

    static void CreateVirtualPrefab(PrefabInPrefab prefabInPrefab) {
        
        DeleteVirtualPrefab(prefabInPrefab);
        
        /* Find parent */
        GameObject virtualPrefabRoot = GameObject.Find("PrefabInPrefab_VirtualPrefab");
        
        if (virtualPrefabRoot == null) {
            Debug.LogError("VirtualPrefabRoot is null.");
            return;
        }
        
        /* Instantiate */
        prefabInPrefab.GeneratePrefab();
        
        prefabInPrefab.GeneratedPrefab.transform.parent = virtualPrefabRoot.transform;
        
        /* Set name */
        prefabInPrefab.GeneratedPrefab.name = GetInstanceName(prefabInPrefab);
        prefabInPrefab.GeneratedPrefab.tag = editorOnlyTag;
        
        /* Set hide flag */
        //foreach (var childTransform in generatedObject.GetComponentsInChildren<Transform>()) {
        //    childTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;
        //}
        
        /* Add VirtualPrefab */
        var child = prefabInPrefab.GeneratedPrefab.AddComponent<PrefabInPrefabAsset.VirtualPrefab>();
        
        child.original = prefabInPrefab;
        //child.UpdateTransform();
    }
    
	static void DeleteVirtualPrefab(PrefabInPrefab prefabInPrefab) {

		string instanceName = GetInstanceName(prefabInPrefab);

		foreach(GameObject obj in GameObject.FindGameObjectsWithTag(editorOnlyTag)) {
			if (obj.name != instanceName) {
				continue;
			}

			DestroyImmediate(obj);
		}
	}

    static void UpdateGameView() {
        
        if (Application.isPlaying) {
            return;
        }
        
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        SceneView.RepaintAll();
        
        /* force redraw anything (ex. NGUI's UICamera) */
        //var dummy = new GameObject();
        //dummy.transform.parent = null;
        //DestroyImmediate(dummy);
    }
    
    /* 循環参照をチェック */
    public static bool IsCircularReferenceError(PrefabInPrefab target) {
        
        /* Check circular reference */
        if (IsCircularReferenceError(target, null) == true) {
            Debug.LogError("Can't circular reference.");
            target.ResetPrefab();
            return true;
        }
        
        return false;
    }
    
    static bool IsCircularReferenceError(PrefabInPrefab target, List<int> usedPrefabsList) {
        
        if (target.Prefab == null) {
            return false;
        }
        
        if (usedPrefabsList == null) {
            usedPrefabsList = new List<int>();
        }
        
        int id = target.Prefab.GetInstanceID();
        
        if (usedPrefabsList.Contains(id)) {
            return true;
        }
        
        usedPrefabsList.Add(id);
        
        /* Check circular reference */
        PrefabInPrefab[] prefabInPrefab = target.Prefab.GetComponentsInChildren<PrefabInPrefab>(true);
        
        foreach (var nextTarget in prefabInPrefab) {
            
            if (nextTarget == target) {
                continue;
            }
            
            if (IsCircularReferenceError(nextTarget, usedPrefabsList) == true) {
                return true;
            }
        }
        
        return false;
    }

    static string GetHashCodeFromPrefab_OLD(PrefabInPrefab prefabInPrefab) {
        
        string result;
        string path = GetPrefabFilePath(prefabInPrefab);

        using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
            using (System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create()) {
                byte[] bs = sha1.ComputeHash(fs);
                result = BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }
        
        return result;
    }

    static string GetHashCodeFromPrefab(PrefabInPrefab prefabInPrefab) {
            
        string result;
        string path = GetPrefabFilePath(prefabInPrefab);
        DateTime dateUpdate = System.IO.File.GetLastWriteTime(path);
        
        result = dateUpdate.ToString();
        //Logger.Log(result);
        
        return result;
    }

    static public string GetPrefabFilePath(PrefabInPrefab prefabInPrefab) {
        return AssetDatabase.GetAssetPath(prefabInPrefab.Prefab);
    }

    static string GetInstanceName(PrefabInPrefab prefabInPrefab) {
        return string.Format(">PrefabInPrefab{0}", prefabInPrefab.GetInstanceID());
    }
    
}

#endif



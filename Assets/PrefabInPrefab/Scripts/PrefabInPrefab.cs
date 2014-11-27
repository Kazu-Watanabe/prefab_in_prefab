using UnityEngine;
using System;
using System.Reflection;


[ExecuteInEditMode]
public class PrefabInPrefab : MonoBehaviour {

    [SerializeField]
    GameObject prefab;

    [SerializeField]
    bool isMoveComponents = true;

    [SerializeField]
    bool isPreviewInEditor = true;


    GameObject generatedPrefab;

    string lastPrefabHashCode;
    

    void Awake() {

#if UNITY_EDITOR
        if (Application.isPlaying == false) {
            VirtualPrefabCreater.DrawVirtualPrefab(this);
            return;
        }
#endif

        generatedPrefab = InstantiatePrefab();
    }

#if UNITY_EDITOR

    void OnEnable() {
        VirtualPrefabCreater.DrawVirtualPrefab(this);
    }

    void OnDisable() {
        VirtualPrefabCreater.DrawVirtualPrefab(this);
    }

    void ParamChanged() {
        Debug.LogWarning("ParamChanged : " + this.GetInstanceID());
        VirtualPrefabCreater.ForceDrawVirtualPrefab(this);
    }

#endif

    public void GeneratePrefab() {
        generatedPrefab = InstantiatePrefab();
    }

    GameObject InstantiatePrefab() {

        if (prefab == null) {
            return null;
        }

        GameObject obj = Instantiate(prefab) as GameObject;

        /* Set transform */
        obj.transform.position = this.transform.position;
        obj.transform.rotation = this.transform.rotation;
        obj.transform.localScale = this.transform.localScale;

        /* Set parent */
        obj.transform.parent = this.transform;

        /* Move components */
        if (isMoveComponents) {
            MoveComponents(this.gameObject, obj);
        }

        return obj;
    }

    void MoveComponents(GameObject from, GameObject to) {

        var components = from.GetComponents(typeof(Component));

        foreach (var component in components) {

            /* Skip by type */
            if ( (component as Transform != null) ||
                 (component as PrefabInPrefab != null) ) {
                continue;
            }

            /* Move component */
            Type type = component.GetType();
            var copy = to.AddComponent(type);

            /* Set fields value */
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var field in fields) {
                field.SetValue(copy, field.GetValue(component));
            }

            if (Application.isPlaying) {
                Destroy(component);
            }
        }
    }

    public GameObject Prefab {
        get{ return this.prefab; }
    }

    public GameObject GeneratedPrefab {
        get {
            if ( (Application.isPlaying == true) &&
                 (prefab != null) &&
                 (generatedPrefab == null) ) {
                
                /*
                 * You can use Script Execution Order Settings.
                 * http://docs.unity3d.com/Documentation/Components/class-ScriptExecution.html
                 */
                Debug.LogError("Prefab In Prefab is Uninitialized. You can use this after Awake().");
            }
            
            return generatedPrefab;
        }
    }

    public void DeleteGeneratedPrefab() {
        
        if (generatedPrefab == null) {
            return;
        }
        
        DestroyImmediate(generatedPrefab);
    }

    public void ResetPrefab() {
        prefab = null;
        DeleteGeneratedPrefab();
    }

    public T GetComponentFromGeneratedObject<T>() where T : Component {
        return GeneratedPrefab.GetComponent<T>();
    }

    public bool IsVisible_VirtualPrefab {
        get {
            return (prefab != null) && isPreviewInEditor && this.gameObject.activeInHierarchy && this.enabled;
        }
    }

    public string LastPrefabHashCode {
        set{ this.lastPrefabHashCode = value; }
        get{ return this.lastPrefabHashCode; }
    }

}

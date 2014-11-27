using UnityEngine;
using UnityEditor;
using System.Collections;

namespace PrefabInPrefabAsset {

    [CustomEditor(typeof(PrefabInPrefab))]
    public class PrefabInPrefabEditor : Editor {

        private SerializedProperty prefab;
        private SerializedProperty moveComponents;
        private SerializedProperty previewInEditor;

        void OnEnable() {
            prefab = serializedObject.FindProperty("prefab");
            moveComponents = serializedObject.FindProperty("isMoveComponents");
            previewInEditor = serializedObject.FindProperty("isPreviewInEditor");
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            prefab.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab.objectReferenceValue, typeof(GameObject), false);
            moveComponents.boolValue = EditorGUILayout.Toggle("Move Components", moveComponents.boolValue);
            previewInEditor.boolValue = EditorGUILayout.Toggle("Preview In Editor", previewInEditor.boolValue);

            if (GUI.changed) {
                serializedObject.ApplyModifiedProperties();

                PrefabInPrefab prefabInPrefab = target as PrefabInPrefab;

                Debug.LogWarning("PrefabInPrefabEditor - GUI.changed : " + prefabInPrefab.GetInstanceID());

                if (VirtualPrefabCreater.IsCircularReferenceError(prefabInPrefab) == true ) {
                    return;
                }

                VirtualPrefabCreater.ForceDrawVirtualPrefab(prefabInPrefab);
            }
        }
    }

}

#define DEBUG

using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class NestedPrefab : MonoBehaviour
{
	public GameObject prefab;

	void Start()
	{
#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			StartInEditMode();
			return;
		}
#endif

		InstantiatePrefab();
		Destroy(this.gameObject);
	}

	GameObject InstantiatePrefab()
	{
		var generatedObject = Instantiate(prefab) as GameObject;

		generatedObject.transform.parent = this.transform.parent.transform;
		generatedObject.transform.position = this.transform.position;
		generatedObject.transform.rotation = this.transform.rotation;
		generatedObject.transform.localScale = this.transform.localScale;
		generatedObject.name = this.name;

		return generatedObject;
	}

	bool Validation()
	{
		// 1.
		// This game object couldn't have any other component.
		// Because this game object will delete in Start() in play mode.
		
		// 2.
		// This game object couldn't have children.
		// For the same reason.
	}

#if UNITY_EDITOR
	// ==============
	//  in edit mode
	// ==============

	private DateTime lastPrefabUpdateTime;

	void StartInEditMode()
	{
		DrawDontEditablePrefab();
	}

	void OnRenderObject()
	{
		if(Application.isPlaying) return;

		DrawDontEditablePrefab();
	}

	void DrawDontEditablePrefab()
	{
		if(prefab == null || !Validation() || !PrefabUpdated()) return;

		DeleteChildren();

		var generatedObject = InstantiatePrefab();
		generatedObject.transform.parent = null;
#if DEBUG
		generatedObject.hideFlags = HideFlags.NotEditable;
#else
		generatedObject.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
#endif
		generatedObject.tag = "EditorOnly";
		generatedObject.name = string.Format(">NestedPrefab{0}", GetInstanceID());

		var child = generatedObject.AddComponent<NestedPrefabChild>();
		child.stepparent = this.gameObject;

		UpdateGameView();
	}

	bool PrefabUpdated()
	{
		var prefabUpdateTime = GetPrefabUpdateTime();
		if(lastPrefabUpdateTime == prefabUpdateTime) return false;
		lastPrefabUpdateTime = GetPrefabUpdateTime();
		return true;
	}

	void DeleteChildren()
	{
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("EditorOnly"))
		{
			if(obj.name != string.Format(">NestedPrefab{0}", GetInstanceID())) continue;
			DestroyImmediate(obj);
		}
	}

	string GetFilePath()
	{
		return AssetDatabase.GetAssetPath(prefab);
	}

	DateTime GetPrefabUpdateTime()
	{
		return System.IO.File.GetLastWriteTime(GetFilePath());
	}

	void UpdateGameView()
	{
		UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
	}

#endif
}

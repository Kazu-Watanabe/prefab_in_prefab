using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PrefabInPrefab
{

[ExecuteInEditMode]
public class PrefabInPrefab : MonoBehaviour
{
	public GameObject Child { get { return generatedObject; } }

	[SerializeField] GameObject prefab;
	[SerializeField] bool moveComponents;
	private GameObject generatedObject;

	void Awake()
	{
#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			StartInEditMode();
			return;
		}
#endif
		if(prefab == null) return;
		InstantiatePrefab();
	}

	GameObject InstantiatePrefab()
	{
		generatedObject = Instantiate(prefab) as GameObject;

		// calc transform to base parent
		generatedObject.transform.parent = this.transform.parent.transform;
		generatedObject.transform.position = this.transform.position;
		generatedObject.transform.rotation = this.transform.rotation;
		generatedObject.transform.localScale = this.transform.localScale;

		// change parent
		generatedObject.transform.parent = this.transform;

		if(moveComponents) MoveComponents(this.gameObject, generatedObject);
		return generatedObject;
	}

	void MoveComponents(GameObject from, GameObject to)
	{
		var components = from.GetComponents(typeof(Component));
		foreach(var component in components)
		{
			if(component as Transform != null || component as PrefabInPrefab != null) continue;

			Type type = component.GetType();
			var copy = to.AddComponent(type);
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			foreach (var field in fields) field.SetValue(copy, field.GetValue(component));
			if(Application.isPlaying) Destroy(component);
		}
	}

#if UNITY_EDITOR
	// ==============
	//  in edit mode
	// ==============

	private static int Redraw = 0;
	private static bool updateGameView = false;
	private DateTime lastPrefabUpdateTime;
	private int redrawCount = 0;

	public static void RequestRedraw()
	{
		if(Application.isPlaying) return;
		EditorApplication.delayCall += () =>
		{
			Redraw++;
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			SceneView.RepaintAll();
		};
	}

	void StartInEditMode()
	{
		DrawDontEditablePrefab();
	}

	void OnRenderObject()
	{
		if(Application.isPlaying) return;
		DrawDontEditablePrefab();
	}

	void OnDisable()
	{
		SetChildActive();
	}

	void OnEnable()
	{
		SetChildActive();
	}

	void SetChildActive()
	{
		if(Application.isPlaying || generatedObject == null) return;
		generatedObject.GetComponent<VirtualPrefab>().SetActiveInEditor(this.gameObject.activeInHierarchy);
	}

	void DrawDontEditablePrefab()
	{
		if(prefab == null) return;
		if(Redraw == redrawCount && !PrefabUpdated()) return;
		if(ValidationError()) return;
		redrawCount = Redraw;

		DeleteChildren();

		var generatedObject = InstantiatePrefab();
		generatedObject.transform.parent = null;
		//generatedObject.hideFlags = HideFlags.NotEditable; // for debug
		generatedObject.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		generatedObject.tag = "EditorOnly";
		generatedObject.name = string.Format(">PrefabInPrefab{0}", GetInstanceID());

		var child = generatedObject.AddComponent<VirtualPrefab>();
		child.stepparent = this.gameObject;

		SetChildActive();
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
			if(obj.name != string.Format(">PrefabInPrefab{0}", GetInstanceID())) continue;
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
		if(updateGameView) return;
		updateGameView = true;
		EditorApplication.delayCall += () =>
		{
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			SceneView.RepaintAll();
			updateGameView = false;
		};
	}

	bool ValidationError()
	{
		// This game object can't be root.
		// Because this is not in prefab.
		if(this.transform.parent == null)
		{
			// copy&paseした時に、なぜか一瞬だけparentがnullになるので、
			// 少し遅らせる.
			EditorApplication.delayCall += () =>
			{
				if(this.transform.parent == null)
				{
					Debug.LogError("Can't attach PrefabInPrefab to root gameobject.");
					prefab = null;
					DeleteChildren();
				}
				else
				{
					redrawCount = -1; //force redraw
					DrawDontEditablePrefab();
				}
			};

			// stop
			return true;
		}

		return false;
	}
#endif
}

}

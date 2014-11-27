using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class TextSender : MonoBehaviour {
	[SerializeField] string text = null;

	void Start()
	{
		var receiver = GetComponent<TextReceiver>();

		if(receiver != null) {
			receiver.text = text;
		}
		else {
			Debug.LogError("TextReceiver is null.");
		}
	}
}

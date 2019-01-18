using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CustomCursor {
	public Sprite cursorImage;
	public string cursorName;
}

public class uGUICursorController : MonoBehaviour {

	public List<CustomCursor> cursorsList;
	private static List<CustomCursor> cursors;
	private static Image image;

	public void Start() {
		cursors = cursorsList;
		image = GetComponent<Image>();
		Cursor.visible = false;

	}

	public void Update() {

		cursors = cursorsList;

		image = GetComponent<Image>();

		if(image.enabled) {
			image.rectTransform.position = new Vector3(Input.mousePosition.x + image.rectTransform.sizeDelta.x * image.rectTransform.lossyScale.x * 0.5f, Input.mousePosition.y - image.rectTransform.sizeDelta.x * image.rectTransform.lossyScale.y * 0.5f, 0);
		}
	}

	public static void HideCursor() {
		image.enabled = false;
	}

	public static void ShowCursor() {
		image.enabled = true;
	}

	public static void ChangeCursor(string cursorName) {
		for(int i = 0; i < cursors.Count; i++) {
			if(cursors[i].cursorName == cursorName) {
				image.sprite = cursors[i].cursorImage;
			}
		}
	}
}

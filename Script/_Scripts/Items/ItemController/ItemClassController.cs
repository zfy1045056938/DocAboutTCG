using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemClassController : MonoBehaviour {

	public ItemClass item;

	public LayerMask mask;

	public LabelLookAt lookAt;

	public uGUIInventory inventory;

	// Use this for initialization
	void Awake () {
		gameObject.AddComponent<Rigidbody>();
		MeshCollider col = gameObject.AddComponent<MeshCollider>();
		col.convex = true;
	}

	void Start() {
		lookAt = GetComponentInChildren<LabelLookAt>();
		inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();
		OnMouseExit();
	}

	//The player right clicked the item - add it to the inventory
	void OnMouseOver() {
		if(Input.GetMouseButtonDown(1)) {
			if(item.stackable) {
				inventory.AddStackableItem(item);
			}
			else {
				inventory.AddItem(item);
			}
			DestroyObject(gameObject);
		}
	}

	//The player is hovering over the item
	void OnMouseEnter() {
		lookAt.show = true;
		foreach(Transform tr in transform) {
			tr.gameObject.SetActive(true);
		}
		Text text = GetComponentInChildren<Text>();
		text.text = item.itemName;
		text.color = inventory.FindColor(item);

		Image image = GetComponentInChildren<Image>();
		image.rectTransform.sizeDelta = new Vector2(text.preferredWidth + 12, text.preferredHeight + 8);
	}

	//The player stopped hovering over the item
	void OnMouseExit() {
		lookAt.show = false;
		foreach(Transform tr in transform) {
			tr.gameObject.SetActive(false);
		}
	}
}

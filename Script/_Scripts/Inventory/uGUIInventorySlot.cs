using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class uGUIInventorySlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {

	[HideInInspector]
	public ItemClass item;
	public bool containsItem;
	public int itemStartNumber;
	public uGUIInventory inventory;
	public Text stacksizeText;
	public Image itemImage;
	public Image unidentified;
	public Image itemFrame;

	// Use this fork initialization
	void Start () {

		inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();
	}

	//Check which slot was clicked and which button was click. Then send the data to the inventory.
	public void OnPointerDown(PointerEventData data) {
		if(data.button == PointerEventData.InputButton.Left) {
			inventory.OnSlotClick(gameObject, 0);
		}
		if(data.button == PointerEventData.InputButton.Right) {
			if(!inventory.identifying) {
				inventory.OnSlotClick(gameObject, 1);
			}
		}
	}

	public void OnPointerEnter(PointerEventData data) {
		if(item.itemName != "") {
			inventory.OnMouseEnter(gameObject);
		}
		inventory.hoveredSlot = this;
		if(inventory.dragging) {
			inventory.dragItemBackground.gameObject.SetActive(true);
		}
	}
	
	public void OnPointerExit(PointerEventData data) {
		if(item.itemName != "") {
			inventory.OnMouseExit(gameObject);
		}
		inventory.hoveredSlot = null;
		if(inventory.dragging) {
			inventory.dragItemBackground.gameObject.SetActive(false);
		}
	}
}

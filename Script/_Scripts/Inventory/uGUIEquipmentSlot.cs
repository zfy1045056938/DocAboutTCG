using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class uGUIEquipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler  {

	[HideInInspector]
	public ItemClass item;
	public EquipmentSlotType equipmentSlotType;
	[HideInInspector]
	public uGUIInventory inventory;
	public float iconScaleFactor;
	[HideInInspector]
	public Image itemIcon;
	
	// Use this for initialization
	void Start () {
		inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();
		itemIcon = transform.Find("ItemIcon").GetComponent<Image>();
	}
	
	//Check which slot was clicked and which button was click. Then send the data to the inventory.
	public void OnPointerClick(PointerEventData data) {
		if(data.button == PointerEventData.InputButton.Left) {
			inventory.OnEquipmentSlotClick(gameObject, 0);
		}
		if(data.button == PointerEventData.InputButton.Right) {
			inventory.OnEquipmentSlotClick(gameObject, 1);
		}
	}

	public void OnPointerEnter(PointerEventData data) {
		inventory.OnMouseEnter(gameObject);
	}
	
	public void OnPointerExit(PointerEventData data) {
		inventory.OnMouseExit(gameObject);
	}
}

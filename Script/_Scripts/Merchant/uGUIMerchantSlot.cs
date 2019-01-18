using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class uGUIMerchantSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

	[HideInInspector]
	public ItemClass item;
	[HideInInspector]
	public uGUIInventory inventory;
	[HideInInspector]
	public uGUIMerchant merchant;
	public int itemStartNumber;


	// Use this for initialization
	void Start () {
		inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();
		merchant = GameObject.FindGameObjectWithTag("Merchant").GetComponent<uGUIMerchant>();
	}

	public void OnPointerClick(PointerEventData data) {
		if(data.button == PointerEventData.InputButton.Left) {
			merchant.OnMerchantSlotClick(gameObject, 0);
		}
		else if(data.button == PointerEventData.InputButton.Right) {
			merchant.OnMerchantSlotClick(gameObject, 1);
		}
	}

	public void OnPointerEnter(PointerEventData data) {
		merchant.OnMerchantSlotEnter(gameObject);
	}

	public void OnPointerExit(PointerEventData data) {
		merchant.OnMerchantSlotExit();
	}
}

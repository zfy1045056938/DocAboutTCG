
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class uGUICraftingTab : MonoBehaviour, IPointerClickHandler {

	public CraftingTabType tabType;

	private uGUICrafting crafting;
	[HideInInspector]
	public List<CraftedItem> items;

	public void Start() {
		crafting = GameObject.FindGameObjectWithTag("Crafting").GetComponent<uGUICrafting>();
	}

	public void OnPointerClick(PointerEventData data) {
		crafting.ChangeCraftingTab(this);

	}
}

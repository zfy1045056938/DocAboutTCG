using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class uGUICraftingButton : MonoBehaviour, IPointerClickHandler {

	[HideInInspector]
	public CraftedItem item;
	public uGUICrafting crafting;
	public Text text;

	public List<int> craftingMaterialsCounters;

	public List<int> craftingAmountCounters;

	// Use this for initialization
	IEnumerator Start () {
		//set the button of the item to this button
		item.button = this;
		yield return new WaitForEndOfFrame();
		crafting = GameObject.FindGameObjectWithTag("Crafting").GetComponent<uGUICrafting>();
		craftingMaterialsCounters = new List<int>();
		UpdateText();
	}

	//Update the text of the button equal to the amount it's possible to create and the item quality and the name of the item
	public void UpdateText() {
		if(!crafting) {
			return;
		}
		//Count how many items it's possible to create given the materials in the inventory
		craftingMaterialsCounters = new List<int>();
		for(int i = 0; i < item.materials.Count; i++) {
			int temp = 0;
			for(int j = 0; j < crafting.inventory.items.Count; j++) {
				if(crafting.inventory.items[j].item.itemName == item.materials[i].itemName && j == crafting.inventory.items[j].itemStartNumber) {
					temp += crafting.inventory.items[j].item.stackSize;
				}
			}
			craftingMaterialsCounters.Add(temp);
		}
		//Find the highest one
		int highestItemAvaliable = 0;
		for(int i = 0; i < craftingMaterialsCounters.Count; i++) {
			if(craftingMaterialsCounters[i] != 0 && craftingMaterialsCounters[i]/item.materialRequiredAmount[i] < highestItemAvaliable || (highestItemAvaliable == 0 && i == 0)) {
				highestItemAvaliable = Mathf.FloorToInt(craftingMaterialsCounters[i]/item.materialRequiredAmount[i]);
			}
		}

		text = GetComponent<Text>();
		text.text = "";
		//If the highest amount is equal to zero just place "-" in front of the item name
		if(highestItemAvaliable == 0) {
			text.text += "- " + item.item.itemName;
			crafting.acceptButton.interactable = false;
		}
		//Else if it's higher than zero place the highest number in front of the item name
		else {
			text.text += "["+highestItemAvaliable.ToString()+"] "+ item.item.itemName;
			crafting.acceptButton.interactable = true;
			if(crafting.amountToCraft > highestItemAvaliable) {
				crafting.amountToCraftLabel.text = highestItemAvaliable.ToString();
				crafting.amountToCraft = highestItemAvaliable;
				crafting.craftCostLabel.text = (item.craftCost * highestItemAvaliable).ToString();
			}
		}
		text.color = crafting.inventory.FindColor(item.item);
	}

	//called when then player clicks on the item button
	public void OnPointerClick(PointerEventData date) {
		crafting.materials = new List<uGUICraftingMaterial>();
		while(crafting.materialsListGameObject.transform.childCount != 0) {
			DestroyImmediate(crafting.materialsListGameObject.transform.GetChild(0).gameObject);
		}
		crafting.ChangeCraftingItem(this);

	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CraftedItem {

	public ItemClass item;
	public List<ItemClass> materials;
	public List<int> materialIDs;
	public List<int> materialRequiredAmount;
	public float craftTime;
	public float craftTimer;
	public int craftCost;
	public uGUICraftingButton button;
	public string ID;
	public CraftingTabType baseType;
}

public class uGUICrafting : MonoBehaviour {
	
	[HideInInspector]
	public List<uGUICraftingButton> items;
	[HideInInspector]
	public List<CraftedItem> craftAbleItems;
	[HideInInspector]
	public uGUIInventory inventory;
	[HideInInspector]
	public CraftedItem selectedItem;
	[HideInInspector]
	public int amountToCraft = 1;
	[HideInInspector]
	public List<uGUICraftingMaterial> materials;
	[HideInInspector]
	public uGUICraftingTab selectedTab;

	public List<int> craftAbleItemIDs;

	public float materialIconSlotSize;
	public float iconSlotSize;
	public float tabWidth;
	public float tabHeight;

	public Color tabInactiveColor;
	public Color tabActiveColor;

	public ItemDatabase database;
	public GameObject scrollContent;
	public GameObject tabPrefab;
	public GameObject tabsObj;
	public ScrollRect scrollRect;
	public Image buttonHighlight;
	public GameObject materialsListGameObject;
	public uGUICraftingMaterial materialPrefab;
	public uGUICraftingButton craftingItemPrefab;
	public Image craftingItemImage;
	public Text selectedItemNameText;
	public Text craftCostLabel;
	public Text amountToCraftLabel;
	public Sprite armorTab;
	public Sprite weaponTab;
	
	public Button acceptButton;
	
	public uGUITooltip tooltip;

	private Player player;
	private List<int> materialAmounts;
	private List<uGUICraftingTab> craftingTabs;
	private bool crafting;

	// Use this for initialization
	void Start () {
		//Find the player
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		//Find the inventory
		inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();

		craftingTabs = new List<uGUICraftingTab>();
		selectedItem = new CraftedItem();

		//Run through all the craft item IDs
		for(int i = 0; i < craftAbleItemIDs.Count; i++) {
			//Find the item in the item database
			CraftedItem item = database.FindCraftItem(craftAbleItemIDs[i]);
			//If there is already any crafting tabs
			if(craftingTabs.Count > 0) {
				int counter = 0;
				//Run through all the crafting tabs
				for(int j = 0; j < craftingTabs.Count; j++) {
					//If the item base type is = to the tabtype of the crafting tab then add the item to the list
					if(craftingTabs[j].tabType == item.baseType) {
						if(craftingTabs[j].items == null) {
							craftingTabs[j].items = new List<CraftedItem>();
						}
						craftingTabs[j].items.Add(item);
						counter++;
					}
					//If we're at the end of the list and there's no tab with the current item type already
					//Instantiate the tab and add the item to the list of items in the tab
					if(j == craftingTabs.Count - 1 && counter == 0) {
						GameObject tempTab = Instantiate(tabPrefab) as GameObject;
						uGUICraftingTab tab = tempTab.AddComponent<uGUICraftingTab>();
						tab.transform.SetParent(tabsObj.transform);
						tab.GetComponent<RectTransform>().localScale = Vector3.one;
						tab.tabType = item.baseType;
						if(tab.tabType == CraftingTabType.armor) {
							tab.GetComponent<Image>().sprite = armorTab;
						}
						else if(tab.tabType == CraftingTabType.weapon) {
							tab.GetComponent<Image>().sprite = weaponTab;
						}
						craftingTabs.Add(tab);
						tabsObj.GetComponent<RectTransform>().sizeDelta = new Vector3(tabWidth, tabHeight * craftingTabs.Count);
					}
				}
			}
			//There isn't any tabs
			//Instantiate the tab and add the item
			else {
				GameObject tempTab = Instantiate(tabPrefab) as GameObject;
				uGUICraftingTab tab = tempTab.AddComponent<uGUICraftingTab>();
				tab.transform.SetParent(tabsObj.transform);
				tab.GetComponent<RectTransform>().localScale = Vector3.one;
				tab.tabType = item.baseType;
				if(tab.tabType == CraftingTabType.armor) {
					tab.GetComponent<Image>().sprite = armorTab;
				}
				else if(tab.tabType == CraftingTabType.weapon) {
					tab.GetComponent<Image>().sprite = weaponTab;
				}
				craftingTabs.Add(tab);
				craftingTabs[craftingTabs.IndexOf(tab)].items = new List<CraftedItem>();
				craftingTabs[craftingTabs.IndexOf(tab)].items.Add(item);
				tabsObj.GetComponent<RectTransform>().sizeDelta =
				 new Vector3(tabWidth, tabHeight * craftingTabs.Count);
			}
		}
		//Set the current selected tab to the first instance of the tabs
		selectedTab = craftingTabs[0];

		//Set the color of all the tabs to inactive
		for(int i = 0; i < craftingTabs.Count; i++) {
			craftingTabs[i].GetComponent<Image>().color = tabInactiveColor;
		}
		//Set the selected tabs color to active
		selectedTab.GetComponent<Image>().color = tabActiveColor;

		//Run through all the items in the selected tab and instantiate them
		for(int i = 0; i < selectedTab.items.Count; i++) {
			uGUICraftingButton item = Instantiate(craftingItemPrefab) as uGUICraftingButton;
			item.transform.SetParent(scrollContent.transform);
			item.transform.localScale = Vector3.one;
			selectedTab.items[i].button = item;
			if(!string.IsNullOrEmpty(selectedTab.items[i].ID)) {
				item.item = database.FindCraftItem(int.Parse(selectedTab.items[i].ID));

			}

			item.transform.name = i.ToString();
		}

		//Close the crafting window
		OpenCloseWindow(false);
	}

	public void Update() {
		//Open the crafting tab when the player presses O
		if(Input.GetKeyDown(KeyCode.O)) {
			OpenCloseWindow(true);
		}
		//If the selected item button is clicked change the selected item
		if(selectedItem.button) {
			ChangeCraftingItem(selectedItem.button);
			selectedItem.button.UpdateText();
		}
		//If the player have enough money set the accept button to interactable
		if(selectedItem.craftCost * amountToCraft <= player.money) {
			acceptButton.interactable = true;
			craftCostLabel.color = Color.white;
		}
		//If the player doesn't have enough money set the accept button to uninteractable
		else {
			acceptButton.interactable = false;
			craftCostLabel.color = Color.red;
		}
		//If the player presses escape then close the crafting window
		if(Input.GetKeyDown(KeyCode.Escape)) {
			OpenCloseWindow(false);
		}
	}

	//Add an item to the tabs
	public void AddCraftingItem(int ID) {
		CraftedItem item = database.FindCraftItem(ID);
		if(craftingTabs.Count > 0) {
			int counter = 0;
			for(int j = 0; j < craftingTabs.Count; j++) {
				if(craftingTabs[j].tabType == item.baseType) {
					if(craftingTabs[j].items == null) {
						craftingTabs[j].items = new List<CraftedItem>();
					}
					craftingTabs[j].items.Add(item);
					counter++;
				}
				if(j == craftingTabs.Count - 1 && counter == 0) {
					GameObject tempTab = Instantiate(tabPrefab) as GameObject;
					uGUICraftingTab tab = tempTab.AddComponent<uGUICraftingTab>();
					tab.transform.SetParent(tabsObj.transform);
					tab.GetComponent<RectTransform>().localScale = Vector3.one;
					tab.tabType = item.baseType;
					if(tab.tabType == CraftingTabType.armor) {
						tab.GetComponent<Image>().sprite = armorTab;
					}
					else if(tab.tabType == CraftingTabType.weapon) {
						tab.GetComponent<Image>().sprite = weaponTab;
					}
					craftingTabs.Add(tab);
					tabsObj.GetComponent<RectTransform>().sizeDelta = new Vector3(tabWidth, tabHeight * craftingTabs.Count);
				}
			}
		}
	}

	//Change the current selected tab
	public void ChangeCraftingTab(uGUICraftingTab tab) {
		//If the tab is already selected just return
		if(selectedTab == tab) {
			return;
		}
		//Set the color of all the tabs to inactive
		for(int i = 0; i < craftingTabs.Count; i++) {
			craftingTabs[i].GetComponent<Image>().color = tabInactiveColor;
		}
		selectedTab = tab;
		selectedTab.GetComponent<Image>().color = tabActiveColor;
		//Remove all the children of the tabs (the buttons)
		while(scrollContent.transform.childCount != 0) {
			DestroyImmediate(scrollContent.transform.GetChild(0).gameObject);
		}
		//Recreate the buttons
		for(int i = 0; i < selectedTab.items.Count; i++) {
			uGUICraftingButton item = Instantiate(craftingItemPrefab) as uGUICraftingButton;
			item.transform.SetParent(scrollContent.transform);
			item.transform.localScale = Vector3.one;
			item.item = database.FindCraftItem(int.Parse(selectedTab.items[i].ID));
		}
		//Set the selected crafting item to the first instance
		if(scrollContent.transform.GetChild(0).GetComponent<uGUICraftingButton>()) {
			ChangeCraftingItem(scrollContent.transform.GetChild(0).GetComponent<uGUICraftingButton>());
		}
		//Reset the scrolling position
		scrollRect.verticalNormalizedPosition = 1f;
	}

	//Change the selected crafting item
	public void ChangeCraftingItem(uGUICraftingButton item) {
		//Position the selected highlight on top of the selected item
		buttonHighlight.transform.position = item.transform.position;

		selectedItem = item.item;

		//Instantiate all the materials
		materialAmounts = new List<int>();
		for(int i = 0; i < item.item.materials.Count; i++) {
			if(materials.Count == 0) {
				for(int j = 0; j < item.item.materials.Count; j++) {
					uGUICraftingMaterial material = Instantiate(materialPrefab) as uGUICraftingMaterial;
					materials.Add(material);
				}
			}
		}
		//Position all the materials
		for(int j = 0; j < materials.Count; j++) {
			bool avaliable = false;
			materials[j].transform.SetParent(materialsListGameObject.transform);
			materials[j].transform.localScale = Vector3.one;
			materials[j].icon.sprite = item.item.materials[j].icon;
			materials[j].item = item.item.materials[j];
			int count = 0;
			for(int k = 0; k < inventory.items.Count; k++) {
				if(inventory.items[k].item.itemName == item.item.materials[j].itemName) {
					count += inventory.items[inventory.items[k].itemStartNumber].item.stackSize;
				}
			}
			if(count < item.item.materialRequiredAmount[j]) {
				materials[j].text.color = Color.red;
				Stop();
				avaliable = false;
			}
			else {
				materials[j].text.color = Color.white;
				acceptButton.interactable = true;
			}
			if(!avaliable) {
				acceptButton.interactable = false;
			}
			materials[j].text.text = count.ToString()+"/"+item.item.materialRequiredAmount[j];
			materialAmounts.Add(count/item.item.materialRequiredAmount[j]);
		}
		craftingItemImage.sprite = item.item.item.icon;
		craftingItemImage.rectTransform.sizeDelta =
		 new Vector2(item.item.item.width *
		 iconSlotSize, item.item.item.height * iconSlotSize);
		selectedItemNameText.text = item.item.item.itemName;
		craftCostLabel.text = (selectedItem.craftCost * amountToCraft).ToString();
		craftCostLabel.rectTransform.sizeDelta = new Vector2(craftCostLabel.preferredWidth, craftCostLabel.rectTransform.sizeDelta.y);
	}

	//Stop crafting
	public void Stop() {
		StopCoroutine("CraftItems");
		StopCoroutine("CraftItem");
		buttonHighlight.fillAmount = 0;
		selectedItem.craftTimer = 0;
		acceptButton.interactable = true;
		crafting = false;
	}

	//Called when the accept button is pressed
	public void Accept() {
		if(!crafting) {
			crafting = true;
			StartCoroutine("CraftItems");
		}
	}

	//Start crafting the item
	public IEnumerator CraftItems() {
		int tempAmount = amountToCraft;
		acceptButton.interactable = false;
		for(int i = 0 ; i < tempAmount; i++) {
			yield return StartCoroutine("CraftItem");
		}
	}

	//Craft the item over time
	public IEnumerator CraftItem() {
		yield return new WaitForEndOfFrame();

		while(selectedItem.craftTimer < selectedItem.craftTime) {
			yield return new WaitForEndOfFrame();
			selectedItem.craftTimer += Time.deltaTime;
			buttonHighlight.fillAmount = (selectedItem.craftTimer/selectedItem.craftTime);
		}

		selectedItem.craftTimer = 0;
		buttonHighlight.fillAmount = 0;
		if(amountToCraft > 1) {
			amountToCraft--;
		}
		amountToCraftLabel.text = amountToCraft.ToString();
		craftCostLabel.text = (selectedItem.craftCost * amountToCraft).ToString();
		craftCostLabel.rectTransform.sizeDelta = new Vector2(craftCostLabel.preferredWidth, craftCostLabel.rectTransform.sizeDelta.y);
		AddItem();
		ChangeCraftingItem(selectedItem.button);
		for(int i = 0; i < selectedTab.items.Count; i++) {
			selectedTab.items[i].button.UpdateText();
		}
		acceptButton.interactable = true;
	}

	//The item was created now add it to the inventory
	public void AddItem() {
		for(int i = 0; i < inventory.items.Count; i++) {

			if(inventory.CheckItemFit(selectedItem.item, inventory.items[i],false)) {
				RemoveItem();
				inventory.AddItem(selectedItem.item);
				crafting = false;
				return;
			}
			if(i == inventory.items.Count - 1) {
				//Display message
			}
		}
		crafting = false;
	}

	//The item was crafted now remove the crafting materials from the inventory
	public void RemoveItem() {
		for(int j = 0; j < selectedItem.materials.Count; j++) {
			int stackToRemove = selectedItem.materialRequiredAmount[j];
			for(int i = 0; i < inventory.items.Count; i++) {
				if(inventory.items[i].item.itemName ==
				 selectedItem.materials[j].itemName) {
					inventory.items[i].item.stackSize -= stackToRemove;
					if(inventory.items[i].item.stackSize == 0) {
						inventory.RemoveItemFromSlot(inventory.items[i]);
					}
					else if(inventory.items[i].item.stackSize < 0) {
						stackToRemove = Mathf.Abs(inventory.items[i].
							item.stackSize);
						inventory.RemoveItemFromSlot(inventory.items[i]);
					}
					else {
						inventory.items[i].stacksizeText.text = 
						inventory.items[i].item.stackSize.ToString();
						break;
					}
				}
			}
		}
	}

	//Increase the amount int
	public void IncreaseAmountToCraft() {

		int count = 0;
		for(int i = 0; i < materialAmounts.Count; i++) {
			if(materialAmounts[i] < count || (count == 0 && i == 0)) {
				count = materialAmounts[i];
			}
		}

		if(amountToCraft < count) {
			amountToCraft++;
		}

		amountToCraftLabel.text = amountToCraft.ToString();

		craftCostLabel.text = (selectedItem.craftCost * amountToCraft).ToString();

		craftCostLabel.rectTransform.sizeDelta = new Vector2(craftCostLabel.preferredWidth, craftCostLabel.rectTransform.sizeDelta.y);
	}

	//Descrease the amount int
	public void DescreaseAmountToCraft() {
		if(amountToCraft > 1) {
			amountToCraft--;
		}
		amountToCraftLabel.text = amountToCraft.ToString();
		craftCostLabel.text = (selectedItem.craftCost * amountToCraft).ToString();

	}

	//Called when the material icon is hovered over
	public void OnMaterialIconEnter(uGUICraftingMaterial item) {
		StartCoroutine(tooltip.ShowTooltip(false, item.item, SlotType.crafting, 0, item.GetComponent<RectTransform>(),false));
	}

	//Called when the material icon isn't hovered over anymore
	public void OnMaterialIconExit(uGUICraftingMaterial item) {
		tooltip.HideTooltip();
	}

	//Called when the craft item icon is hovered over
	public void OnIconEnter(GameObject obj) {
		StartCoroutine(tooltip.ShowTooltip(false, selectedItem.item, SlotType.crafting, 0, obj.GetComponent<RectTransform>(),false));
	}

	//Called when the craft item icon isn't hovered over anymore
	public void OnIconExit() {
		tooltip.HideTooltip();
	}

	//Open or close the crafting window
	public void OpenCloseWindow(bool state) {
		foreach(Transform trans in transform) {
			trans.gameObject.SetActive(state);
		}
		if(state == true) {
			GameObject.FindGameObjectWithTag("Merchant").GetComponent<uGUIMerchant>().OpenCloseMerchant(!state);
			ChangeCraftingItem(selectedTab.items[0].button);
		}
	}
}

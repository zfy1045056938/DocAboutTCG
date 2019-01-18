using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class uGUIMerchant : MonoBehaviour {

	//Variables that doesn't need to be exposed in the inspector

	[HideInInspector]
	public Player player;
	[HideInInspector]
	public List<uGUIMerchantSlot> items;
	[HideInInspector]
	public uGUITooltip tooltip;
	[HideInInspector]
	public uGUIInventory inventory;
	[HideInInspector]
	public bool showMerchant;
	[HideInInspector]
	public uGUIMessageManager messageManager;
	[HideInInspector]
	public List<uGUIMerchantTab> tabs;
	[HideInInspector]
	public uGUIMerchantTab selectedTab;
	[HideInInspector]
	public uGUIMerchantController selectedMerchant;
	[HideInInspector]
	public ItemClass itemToRepair;
	[HideInInspector]
	public bool dragging;
	[HideInInspector]
	public uGUIMerchantSlot draggedItem;

	public bool removeStackableItemsWhenBought;
	public Color tabInactiveColor;
	public Color tabActiveColor;
	public int merchantWidth;
	public int merchantHeight;
	public float iconSlotSize = 39f;
	public int tabWidth = 70;
	public int tabHeight = 140;
	public Vector3 tabOffset;


	public Image dragImage;
	public Transform merchantSlots;
	public GameObject slotPrefab;
	public Text avaliableGoldText;
	public GameObject tabsObj;
	public GameObject tabPrefab;
	public GameObject repair;
	public Image itemRepairIcon;
	public Text repairSingleText;
	public Text repairEquippedText;
	public Text repairAllText;
	public Text singleRepairLabel;
	public Image singleRepairCoin;
	public AudioClip repairSound;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();
		messageManager = GameObject.FindGameObjectWithTag("MessageManager").GetComponent<uGUIMessageManager>();
		ResetItems();
		avaliableGoldText.text = player.money.ToString();
		OpenCloseMerchant(false);
	}
	
	// Update is called once per frame
	void Update () {
		//If the player presses escape then close the window
		if(Input.GetKeyDown(KeyCode.Escape)) {
			OpenCloseMerchant(false);
		}

		//If the repair tab is active
		if(repair.activeSelf == true) {
			//Calculate the repair costs
			int repairCostEquipped = 0;
			int repairCostAll = 0;
			for(int i = 0; i < inventory.equipmentSlots.Count; i++) {
				if(inventory.equipmentSlots[i].item.itemName != "" && inventory.equipmentSlots[i].item.curDurability < inventory.equipmentSlots[i].item.maxDurability) {
					repairCostEquipped += (int)(inventory.equipmentSlots[i].item.totalRepairCost * 0.1f * (1 - ((float)inventory.equipmentSlots[i].item.curDurability/inventory.equipmentSlots[i].item.maxDurability)));
				}
			}
			//Update repair cost label
			repairEquippedText.text = repairCostEquipped.ToString();
			repairEquippedText.rectTransform.sizeDelta = new Vector2(repairEquippedText.preferredWidth, repairEquippedText.preferredHeight);

			//Calculate the repair cost for all items
			for(int i = 0; i < inventory.items.Count; i++) {
				if(i == inventory.items[i].itemStartNumber && inventory.items[i].item.itemName != "" && inventory.items[i].item.curDurability < inventory.items[i].item.maxDurability) {
					repairCostAll += (int)(inventory.items[i].item.totalRepairCost * 0.1f * (1 - ((float)inventory.items[i].item.curDurability/inventory.items[i].item.maxDurability)));
				}
			}
			//Update total repair cost labels
			repairAllText.text = (repairCostEquipped + repairCostAll).ToString();
			repairAllText.rectTransform.sizeDelta = new Vector2(repairAllText.preferredWidth, repairAllText.preferredHeight);
		}

		if(dragging && Input.GetMouseButtonDown(1)) {
			dragging = false;
			inventory.dragItem.gameObject.SetActive(false);
		}

		if(dragging) {
			inventory.dragItem.rectTransform.position = new Vector3(Input.mousePosition.x + inventory.dragItem.rectTransform.sizeDelta.x * inventory.dragItem.rectTransform.lossyScale.x * 0.5f, Input.mousePosition.y - inventory.dragItem.rectTransform.sizeDelta.x * inventory.dragItem.rectTransform.lossyScale.y * 0.5f, - 20);
		}
	}

	//Reset the items in the slots
	public void ResetItems() {
		for(int i = 0 ; i < items.Count; i++) {
			Destroy(items[i].gameObject);
		}
		items = new List<uGUIMerchantSlot>();
		for(int i = 0; i < merchantWidth * merchantHeight; i++) {
			GameObject slot = Instantiate(slotPrefab) as GameObject;
			slot.transform.SetParent(merchantSlots);
			slot.name = i.ToString();
			slot.transform.localScale = Vector3.one;
			uGUIMerchantSlot merchantSlot = slot.GetComponent<uGUIMerchantSlot>();
			merchantSlot.item = new ItemClass();
			merchantSlot.item.itemName = "";
			items.Add(merchantSlot);
		}
	}

	//Add an item to the slots
	public void AddItem(ItemClass item) {
		item = uGUIInventory.DeepCopy(item);
		for(int i = 0; i < items.Count; i++) {
			//There's nothing in the slot
			if(items[i].item.itemName == "") {
				//Check the to see if the item can fit in the slot
				if(!CheckFit(item,i)) {
					//If it doesn't fit then continue to the next slot
					continue;
				}
				//The item fits
				else {
					//Run through all the slots and assign the item and hide the slot
					for(int j = 0; j < item.height; j++) {
						for(int k = 0; k < item.width; k++) {
							items[i + merchantWidth * j + k].item = item;
							items[i + merchantWidth * j + k].itemStartNumber = i;
							items[i + merchantWidth * j + k].GetComponent<Image>().color = Color.clear;
							items[i + merchantWidth * j + k].transform.Find("ItemFrame").GetComponent<CanvasGroup>().interactable = false;
							items[i + merchantWidth * j + k].transform.Find("ItemFrame").GetComponent<CanvasGroup>().blocksRaycasts = false;
							items[i + merchantWidth * j + k].GetComponent<CanvasGroup>().blocksRaycasts = false;
							//If it's the first slot then make the icon and frame equal to the size of the item and show them
							if(i == i + merchantWidth * j + k) {
								items[i + merchantWidth * j + k].transform.Find("ItemIcon").gameObject.SetActive(true);
								items[i + merchantWidth * j + k].transform.Find("ItemIcon").GetComponent<Image>().sprite = item.icon;
								items[i + merchantWidth * j + k].transform.Find("ItemIcon").GetComponent<RectTransform>().sizeDelta = new Vector2(item.width * iconSlotSize, item.height * iconSlotSize);
								items[i + merchantWidth * j + k].transform.Find("ItemFrame").GetComponent<RectTransform>().sizeDelta = new Vector2(item.width * iconSlotSize, item.height * iconSlotSize);
								items[i + merchantWidth * j + k].transform.Find("ItemFrame").gameObject.SetActive(true);
								items[i + merchantWidth * j + k].transform.Find("ItemFrame").GetComponent<CanvasGroup>().interactable = true;
								items[i + merchantWidth * j + k].transform.Find("ItemFrame").GetComponent<CanvasGroup>().blocksRaycasts = true;
								items[i + merchantWidth * j + k].GetComponent<CanvasGroup>().blocksRaycasts = true;

							}
						}
					}
					//The item was successfully added
					return;
				}
			}
		}
	}

	//Check to see if the item fits
	public bool CheckFit(ItemClass item, int i) {
		for(int j = 0; j < item.height; j++) {
			for(int k = 0; k < item.width; k++) {
				//There's already an item in the slot
				if(items[i + merchantWidth * j + k].item.itemName != "") {
					return false;
				}
				//Check to see if the slot is at the edge of the merchants slots
				for(int l = 0; l < item.height; l++) {
					if(i + merchantWidth * l + k != i + merchantWidth * l) {
						if(((i + merchantWidth * j + k ) % merchantWidth == 0) && item.width != 1) {
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	//Buy the item of the slot
	public void BuyItem(uGUIMerchantSlot slot, out bool succes, out string failMessage, out AudioClip sound) {
		ItemClass item = uGUIInventory.DeepCopy(slot.item);
		sound = item.itemSound;
		succes = false;
		failMessage = "";
		//If the player has enough gold
		if(player.money >= slot.item.buyPrice) {
			//If the item is stackable then add the stackable item
			if(item.stackable) {
				item.stackSize = 1;
				if(inventory.AddStackableItem(item)) {
					player.money -= slot.item.buyPrice;
					if(removeStackableItemsWhenBought) {
						for(int i = 0; i < selectedMerchant.tabs.Count; i++) {
							for(int j = 0; j < selectedMerchant.tabs[i].items.Count; j++) {
								if(selectedMerchant.tabs[i].items[j] == slot.item) {
									selectedMerchant.tabs[i].items.Remove(slot.item);
								}
							}
						}
						for(int i = 0; i < item.height; i++) {
							for(int j = 0; j < item.width; j++) {
								items[slot.itemStartNumber + merchantWidth * i + j].item = new ItemClass();
								items[slot.itemStartNumber + merchantWidth * i + j].item.itemName = "";
								items[slot.itemStartNumber + merchantWidth * i + j].GetComponent<Image>().color = Color.white;
								items[slot.itemStartNumber + merchantWidth * i + j].transform.Find("ItemIcon").gameObject.SetActive(false);
								items[slot.itemStartNumber + merchantWidth * i + j].transform.Find("ItemFrame").gameObject.SetActive(false);
							}
						}
					}
					avaliableGoldText.text = player.money.ToString();
					succes = true;
					failMessage = "";
					return;
				}
				//There's no open slots in the inventory so display a fail message
				else {
					succes = false;
					failMessage = "I need more space!";
				}
			}
			//Add the item to the inventory
			else if(inventory.AddItem(uGUIInventory.DeepCopy(slot.item))) {
				player.money -= slot.item.buyPrice;
				for(int i = 0; i < selectedMerchant.tabs.Count; i++) {
					for(int j = 0; j < selectedMerchant.tabs[i].items.Count; j++) {
						if(selectedTab.tabType == selectedMerchant.tabs[i].tabType) {
							if(selectedMerchant.tabs[i].items[j].itemName == slot.item.itemName) {
								selectedMerchant.tabs[i].items.Remove(selectedMerchant.tabs[i].items[j]);
								for(int k = 0; k < item.height; k++) {
									for(int l = 0; l < item.width; l++) {
										items[slot.itemStartNumber + merchantWidth * k + l].item = new ItemClass();
										items[slot.itemStartNumber + merchantWidth * k + l].item.itemName = "";
										items[slot.itemStartNumber + merchantWidth * k + l].GetComponent<Image>().color = Color.white;
										items[slot.itemStartNumber + merchantWidth * k + l].transform.Find("ItemIcon").gameObject.SetActive(false);
										items[slot.itemStartNumber + merchantWidth * k + l].transform.Find("ItemFrame").gameObject.SetActive(false);
									}
								}
								avaliableGoldText.text = player.money.ToString();
								succes = true;
								failMessage = "";
								return;
							}
						}
					}
				}
			}
			//There's no open slots in the inventory so display a fail message
			else {
				succes = false;
				failMessage = "I need more space!";
			}
			ResetTabs();
		}
		//The player doesn't have enough gold to purchase the item
		else {
			//Display a fail message
			succes = false;
			failMessage = "Not enough gold!";
		}
	}

	public void RemoveItem(ItemClass item, uGUIMerchantSlot slot) {
		for(int i = 0; i < selectedMerchant.tabs.Count; i++) {
			for(int j = 0; j < selectedMerchant.tabs[i].items.Count; j++) {
				if(selectedTab.tabType == selectedMerchant.tabs[i].tabType) {
					if(selectedMerchant.tabs[i].items[j].itemName == slot.item.itemName) {
						selectedMerchant.tabs[i].items.Remove(selectedMerchant.tabs[i].items[j]);
						for(int k = 0; k < item.height; k++) {
							for(int l = 0; l < item.width; l++) {
								items[slot.itemStartNumber + merchantWidth * k + l].item = new ItemClass();
								items[slot.itemStartNumber + merchantWidth * k + l].item.itemName = "";
								items[slot.itemStartNumber + merchantWidth * k + l].GetComponent<Image>().color = Color.white;
								items[slot.itemStartNumber + merchantWidth * k + l].transform.Find("ItemIcon").gameObject.SetActive(false);
								items[slot.itemStartNumber + merchantWidth * k + l].transform.Find("ItemFrame").gameObject.SetActive(false);
							}
						}
						return;
					}
				}
			}
		}
	}

	//Change the current merchant tab
	public void ChangeTab(uGUIMerchantTab tab) {
		//If the selected tab is of type repair then hide the repair window
		if(selectedTab.tabType == TabType.repair) {
			repair.SetActive(false);
		}

		//Set the selected tabs color to inactive
		selectedTab.GetComponent<Image>().color = tabInactiveColor;

		//Assign the clicked tab to the selected tab
		selectedTab = tab;
		//Set the color of the newly selected tab to active
		selectedTab.GetComponent<Image>().color = tabActiveColor;

		//If the selected tab isn't of type repair
		if(tab.tabType != TabType.repair) {
			//Run through all the tabs and the reset the items in them
			ResetTabs();
		}
		//Else if it is of type repair then show the repair tab
		else {
			repair.SetActive(true);
		}
	}

	//Reset the tabs of the merchant
	public void ResetTabs() {
		for(int i = 0; i < selectedMerchant.tabs.Count; i++) {
			if(selectedMerchant.tabs[i].tabType == selectedTab.tabType) {
				ResetItems();
				for(int j = 0; j < selectedMerchant.tabs[i].items.Count; j++) {
					AddItem(selectedMerchant.tabs[i].items[j]);
				}
			}
		}
	}

	//Called when the repair slot is clicked
	public void OnRepairSlotClick() {
		if(inventory.dragging) {
			if(inventory.draggedItem.curDurability != inventory.draggedItem.maxDurability) {
				itemToRepair = inventory.draggedItem;
				int repairCost = (int)(itemToRepair.totalRepairCost * 0.1f * (1 - ((float)itemToRepair.curDurability/itemToRepair.maxDurability)));
				repairSingleText.text = repairCost.ToString();
				repairSingleText.rectTransform.sizeDelta = new Vector2(repairSingleText.preferredWidth, repairSingleText.preferredHeight);
				itemRepairIcon.sprite = itemToRepair.icon;
				itemRepairIcon.rectTransform.sizeDelta = new Vector2(itemToRepair.width * iconSlotSize, itemToRepair.height * iconSlotSize);
				itemRepairIcon.gameObject.SetActive(true);
				inventory.ReturnDraggedItem();
				singleRepairCoin.gameObject.SetActive(true);
				singleRepairLabel.gameObject.SetActive(true);
			}
			else {
				messageManager.AddMessage(Color.red, "Item is already at max durability.");
			}
		}
	}

	//Called when the repair a single item button is clicked
	public void OnRepairSingleClick() {
		int repairCost = (int)(itemToRepair.totalRepairCost * 0.1f * (1 - ((float)itemToRepair.curDurability/itemToRepair.maxDurability)));
		if(itemToRepair.itemName != "") {
			if(player.money >= repairCost) {
				player.money -= repairCost;
				avaliableGoldText.text = player.money.ToString();
				itemToRepair.curDurability = itemToRepair.maxDurability;
				itemToRepair = new ItemClass();
				itemToRepair.itemName = "";
				itemRepairIcon.gameObject.SetActive(false);
				repairSingleText.text = "Place item to see repair cost.";
				repairSingleText.rectTransform.sizeDelta = new Vector2(repairSingleText.preferredWidth, repairSingleText.preferredHeight);

				singleRepairCoin.gameObject.SetActive(false);
				singleRepairLabel.gameObject.SetActive(false);

				transform.root.GetComponent<AudioSource>().PlayOneShot(repairSound);
			}
			else {
				messageManager.AddMessage(Color.red, "I need more gold!");
			}
		}
	}

	//Called when the equipped item repair button is clicked
	public void OnRepairEquippedClick() {
		int repairCost = 0;
		for(int i = 0; i < inventory.equipmentSlots.Count; i++) {
			if(inventory.equipmentSlots[i].item.itemName != "" && inventory.equipmentSlots[i].item.curDurability < inventory.equipmentSlots[i].item.maxDurability) {
				repairCost += (int)(inventory.equipmentSlots[i].item.totalRepairCost * 0.1f * (1 - ((float)inventory.equipmentSlots[i].item.curDurability/inventory.equipmentSlots[i].item.maxDurability)));
			}
		}
		if(player.money >= repairCost) {
			for(int i = 0; i < inventory.equipmentSlots.Count; i++) {
				if(inventory.equipmentSlots[i].item.itemName != "" && inventory.equipmentSlots[i].item.curDurability < inventory.equipmentSlots[i].item.maxDurability) {
					inventory.equipmentSlots[i].item.curDurability = inventory.equipmentSlots[i].item.maxDurability;
				}
			}
			player.money -= repairCost;
			avaliableGoldText.text = player.money.ToString();
			transform.root.GetComponent<AudioSource>().PlayOneShot(repairSound);
		}
		else {
			messageManager.AddMessage(Color.red, "I need more gold!");
		}
	}

	//Called when the repair all button is clicked
	public void OnRepairAllClick() {
		int repairCostAll = 0;
		for(int i = 0; i < inventory.equipmentSlots.Count; i++) {
			if(inventory.equipmentSlots[i].item.itemName != "" && inventory.equipmentSlots[i].item.curDurability < inventory.equipmentSlots[i].item.maxDurability) {
				repairCostAll += (int)(inventory.equipmentSlots[i].item.totalRepairCost * 0.1f * (1 - ((float)inventory.equipmentSlots[i].item.curDurability/inventory.equipmentSlots[i].item.maxDurability)));
			}
		}
		for(int i = 0; i < inventory.items.Count; i++) {
			if(i == inventory.items[i].itemStartNumber && inventory.items[i].item.itemName != "" && inventory.items[i].item.curDurability < inventory.items[i].item.maxDurability) {
				repairCostAll += (int)(inventory.items[i].item.totalRepairCost * 0.1f * (1 - ((float)inventory.items[i].item.curDurability/inventory.items[i].item.maxDurability)));
			}
		}

		if(player.money >= repairCostAll) {
			for(int i = 0; i < inventory.equipmentSlots.Count; i++) {
				if(inventory.equipmentSlots[i].item.itemName != "" && inventory.equipmentSlots[i].item.curDurability < inventory.equipmentSlots[i].item.maxDurability) {
					inventory.equipmentSlots[i].item.curDurability = inventory.equipmentSlots[i].item.maxDurability;
				}
			}
			for(int i = 0; i < inventory.items.Count; i++) {
				if(inventory.items[i].item.itemName != "" && inventory.items[i].item.curDurability < inventory.items[i].item.maxDurability) {
					inventory.items[i].item.curDurability = inventory.items[i].item.maxDurability;
				}
			}
			player.money -= repairCostAll;
			avaliableGoldText.text = player.money.ToString();
			transform.root.GetComponent<AudioSource>().PlayOneShot(repairSound);
		}
		else {
			messageManager.AddMessage(Color.red, "I need more gold!");
		}
	}

	//Called when a merchant slot is clicked
	public void OnMerchantSlotClick(GameObject obj, int mouseIndex) {
		uGUIMerchantSlot slot = obj.GetComponent<uGUIMerchantSlot>();

		if(inventory.dragging) {
			int i = inventory.dragStartIndex;
			inventory.ReturnDraggedItem();
			inventory.SellItem(inventory.items[i]);
			return;
		}

		//Currently nothing happens when left clicking on a slot
		if(mouseIndex == 0) {
			if(slot.item.itemName != "") {
				draggedItem = obj.GetComponent<uGUIMerchantSlot>();
				draggedItem.item.stackSize = 1;
				dragging = true;
				inventory.dragItem.sprite = draggedItem.item.icon;
				inventory.dragItem.gameObject.SetActive(true);
				inventory.dragItem.GetComponent<RectTransform>().sizeDelta = new Vector2(draggedItem.item.width * iconSlotSize, draggedItem.item.height * iconSlotSize);
			}
			else {

			}
		}
		//If the slot is right clicked then buy the item
		else if(mouseIndex == 1) {
			if(slot.item.itemName != "") {
				bool succes;
				string failMessage;
				AudioClip sound;
				BuyItem(items[slot.itemStartNumber], out succes, out failMessage, out sound);
				if(!succes) {
					messageManager.AddMessage(Color.red, failMessage);
				}
				else {
					transform.root.GetComponent<AudioSource>().PlayOneShot(sound);
				}
			}
		}
	}

	//Called when the mouse is hovering above the merchant slot
	public void OnMerchantSlotEnter(GameObject obj) {
		ItemClass item = obj.GetComponent<uGUIMerchantSlot>().item;

		if(item.itemName == "" || inventory.dragging)
			return;

		uGUICursorController.ChangeCursor("SellCursor");
		
		StartCoroutine(tooltip.ShowTooltip(false, item, SlotType.merchant, obj.GetComponent<uGUIMerchantSlot>().itemStartNumber, obj.GetComponent<RectTransform>(), true));
	}

	//Finds the color based on the rarity of the item
	Color FindColor(ItemClass item) {
		if(item.itemQuality == ItemQuality.Junk) {
			return inventory.junkColor;
		}
		else if(item.itemQuality == ItemQuality.Legendary) {
			return inventory.legendaryColor;
		}
		else if(item.itemQuality == ItemQuality.Magical) {
			return inventory.magicColor;
		}
		else if(item.itemQuality == ItemQuality.Normal) {
			return inventory.normalColor;
		}
		else if(item.itemQuality == ItemQuality.Rare) {
			return inventory.rareColor;
		}
		else if(item.itemQuality == ItemQuality.Set) {
			return inventory.setColor;
		}
		return Color.clear;
	}

	//Called when the mouse isn't hovering over the slot anymore
	public void OnMerchantSlotExit() {
		tooltip.HideTooltip();

		uGUICursorController.ChangeCursor("Default");

	}

	//Opens or closes the merchant
	public void OpenCloseMerchant(bool state) {
		foreach(Transform trans in transform) {
			trans.gameObject.SetActive(state);
		}
		showMerchant = state;
		repair.SetActive(false);
		if(state == true) {
			GameObject.FindGameObjectWithTag("Crafting").GetComponent<uGUICrafting>().OpenCloseWindow(!state);
		}
		if(!inventory.showInventory && state) {
			inventory.showInventory = true;
			inventory.OpenCloseInventory(inventory.showInventory);
		}
	}
}

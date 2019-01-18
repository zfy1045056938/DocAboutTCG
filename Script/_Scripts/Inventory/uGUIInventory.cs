using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Reflection;

public class uGUIInventory : MonoBehaviour {
	
	public float slotIconSize = 39f;					//Size of the icons
	public int inventoryHeight;							//How many rows should the inventory contain?
	public int inventoryWidth;							//How many coloumns should the inventory contain?
	
	public List<uGUIEquipmentSlot> equipmentSlots;		//List of the equipment slots

	public bool dragSwap;								//Can you swap items when dragging?
	public bool autoFindEquipmentSlot;					//Should the equipment slot be found automaticly?
	public bool rightClickUnequipItems;					//Can you unequip equipped items be right clicking them?
	public bool closeIfMerchantOpen;					//Also close when the merchant closes
	public bool snapItemWhenDragging;

	//Color for when drag snapping
	public Color snapCanFitColor;
	public Color snapCannotFitColor;

	//Colors for the item qualities

	public Color legendaryColor;
	public Color normalColor;
	public Color magicColor;
	public Color rareColor;
	public Color junkColor;
	public Color setColor;

	public Image dragItem;								//The icon of the dragged item
	public Image dragItemBackground;					//The background of the dragged item
	public Text dragStackText;							//The stacksize label of the dragged item
	public GameObject splitWindow;						//The GameObject of the split item window
	public Sprite buyBackSprite;						//The sprite to add to the merchant when an item is sold
	public ItemDatabase database;						//Reference to the item database
	public uGUITooltip tooltip;							//The tooltip
	public AudioClip sellSound;							//Sound to play when an item is sold
	public Transform inventorySlots;					//The transform that holds the slots
	public GameObject slotPrefab;						//The inventory slot prefab
	public GameObject itemCanvas;						//Canvas to add to items that's dropped on the ground

	//Variables that doesn't need to be displayed in the inspector but must be public

	[HideInInspector]
	public int dragStartIndex;
	[HideInInspector]
	public uGUIMerchant merchant;
	[HideInInspector]
	public Player player;
	[HideInInspector]
	public uGUIInventorySlot itemToSplit;
	[HideInInspector]
	public ItemClass draggedItem;
	[HideInInspector]
	public List<uGUIInventorySlot> items;
	[HideInInspector]
	public bool dragging;
	[HideInInspector]
	public uGUIMessageManager messageManager;
	[HideInInspector]
	public bool identifying;
	[HideInInspector]
	public bool startIdentifying;
	[HideInInspector]
	public uGUIInventorySlot identifyingScrollOrignalSlot;
	[HideInInspector]
	public bool showInventory;
	[HideInInspector]
	public uGUIInventorySlot hoveredSlot;
	
	// Use this for initialization
	void Awake () {
		//Find the instance of the message manager
		messageManager = GameObject.FindGameObjectWithTag("MessageManager").GetComponent<uGUIMessageManager>();
		//Find the instance of the player
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		//Find the instance of the merchant
		merchant = GameObject.FindGameObjectWithTag("Merchant").GetComponent<uGUIMerchant>();
		//Add the amount of slots we want
		for(int i = 0; i < inventoryWidth * inventoryHeight; i++) {
			GameObject slot = Instantiate(slotPrefab) as GameObject;
			slot.transform.SetParent(inventorySlots);
			slot.name = i.ToString();
			slot.transform.localScale = Vector3.one;

			//
			uGUIInventorySlot inventorySlot = slot.GetComponent<uGUIInventorySlot>();

			//
			inventorySlot.item = new ItemClass();
			inventorySlot.item.itemName = "";
			inventorySlot.itemStartNumber = i;
			items.Add(inventorySlot);

		}
		//Get the grid component of the inventory slot transform
		GridLayoutGroup grid = GetComponentInChildren<GridLayoutGroup>();
		//Set the amount of coloumns equal to the width of the inventory
		grid.constraintCount = inventoryWidth;
		//Set the size of the slots equal to the slot size
		grid.cellSize = new Vector2(slotIconSize,slotIconSize);
		//Reset all the names of the equipment slots
		for(int i = 0; i < equipmentSlots.Count; i++) {
			equipmentSlots[i].item = new ItemClass();
			equipmentSlots[i].item.itemName = "";
		}
		//Hide the inventory
		OpenCloseInventory(false);
	}

	void Start() {
		//Set all the slots equal to un-interactable
		for(int i = 0; i < items.Count; i++) {
			items[i].GetComponent<CanvasGroup>().interactable = false;
		}
	}

	// Update is called once per frame
	void Update () {
		//Open the inventory when the player presses I
		if(Input.GetKeyDown(KeyCode.I)) {
			showInventory = !showInventory;
			OpenCloseInventory(showInventory);
		}
		//Close the inventory if the player presses escape
		if(Input.GetKeyDown(KeyCode.Escape)) {
			if(merchant.showMerchant && closeIfMerchantOpen) {
				OpenCloseInventory(false);
				showInventory = !showInventory;
			}
			else if(!merchant.showMerchant) {
				OpenCloseInventory(false);
				showInventory = !showInventory;
			}
		}

		//If the player is dragging an item
		if(dragging) {
			if(snapItemWhenDragging) {
				if(hoveredSlot) {
					dragItem.rectTransform.position = hoveredSlot.transform.position;
					dragItemBackground.rectTransform.position = hoveredSlot.transform.position;
					dragItemBackground.rectTransform.sizeDelta = new Vector2(draggedItem.width * slotIconSize, draggedItem.height * slotIconSize);
					if(CheckItemFit(draggedItem, hoveredSlot, false)) {
						dragItemBackground.color = snapCanFitColor;
					}
					else {
						dragItemBackground.color = snapCannotFitColor;
					}
				}
				else {
					dragItem.rectTransform.position = Input.mousePosition;
					dragItemBackground.rectTransform.position = Input.mousePosition;
					dragItemBackground.color = Color.white;
				}
			}
			else {
				//Set the position of the dragged item icon equal to the position of the mouse
				dragItem.rectTransform.position = new Vector3(Input.mousePosition.x +
				 dragItem.rectTransform.sizeDelta.x *
				  dragItem.rectTransform.lossyScale.x * 0.5f,
				   Input.mousePosition.y - dragItem.rectTransform.sizeDelta.x 
				   * dragItem.rectTransform.lossyScale.y * 0.5f, - 20);
			}

			//Show the stacksize label if the item is stackable
			if(draggedItem.stackable) {
				dragStackText.gameObject.SetActive(true);
				dragStackText.text = draggedItem.stackSize.ToString();
			}
			//Else hide it
			else {
				dragStackText.gameObject.SetActive(false);
			}
			//If the player left click while dragging and isn't hovering over a UI element then drop the dragged item
			if(Input.GetMouseButtonDown(0)) {
				if(!EventSystem.current.IsPointerOverGameObject()) {
					DropDraggedItem();
				}
			}
			else if(Input.GetMouseButtonDown(1)) {
				ReturnDraggedItem();
			}
		}
		//If the player is currently identifying an item
		if(identifying) {
			//Abort if the player right clicks or presses escape
			if(Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
				identifying = false;
				uGUICursorController.ChangeCursor("Default");
			}
		}
		//We need to delay this because of the delay of the buttons
		if(startIdentifying) {
			startIdentifying = false;
			identifying = true;
		}
	}

	//Instantiate the dropped item in world space using the world object of the dragged item
	public void DropDraggedItem() {
		GameObject obj = Instantiate(draggedItem.worldObject, 
			player.transform.position + player.transform.forward,
			 Quaternion.identity) as GameObject;
		ItemClassController ic = obj.AddComponent<ItemClassController>();
		ic.item = DeepCopy(draggedItem);
		GameObject itemCanvasObj = Instantiate(itemCanvas) as GameObject;
		itemCanvasObj.transform.SetParent(obj.transform);
		itemCanvasObj.transform.localPosition = new Vector3(0,1,0);
		StopDragging();
	}

	//Add a stackable item to the inventory
	public bool AddStackableItem(ItemClass item) {
		//Run through all the slots
		for(int i = 0; i < items.Count; i++) {
			//If the item in the slot is the same and the one the player is trying to add and the item isn't already at max stacksize
			if(items[i].item.itemName != "" && items[i].item.itemName == item.itemName && items[i].item.stackSize != items[i].item.maxStackSize) {
				//Calculate the complete stacksize
				int count = items[i].item.stackSize + item.stackSize;
				//If the complete stacksize is below max then add the stack to the item
				if(count <= items[i].item.maxStackSize) {
					items[i].item.stackSize = count;
					items[i].stacksizeText.text = items[i].item.stackSize.ToString();
					return true;
				}
				//If the complete stacksize is above max then create a temp stacksize and add a new item with that stacksize
				else if(count > items[i].item.maxStackSize) {
					ItemClass temp = DeepCopy(item);
					temp.stackSize = count - items[i].item.maxStackSize;
					if(AddItem(temp)) {
						items[i].item.stackSize = items[i].item.maxStackSize;
						return true;
					}
					else {
						return false;
					}
				}
			}
			//We've searched all of the slots and there's no item matching the stackable item
			//So add it to the inventory
			if(i == items.Count - 1) {
				AddItem(item);
				return true;
			}
		}
		return false;
	}

	//Adds an item to the inventory
	public bool AddItem(ItemClass item) {
		for (int i = 0; i < items.Count; i++) {
			//Found an empty slot
			if(items[i].item.itemName == "") {
				//If the item doesn't fit in the slow just go to the next
				if(!CheckItemFit(item,items[i], false)) {
					continue;
				}
				//Here we calculate wether there's any items occupying the slots that the item would fill
				int counter = 0;
				for(int j = 0; j < item.height; j++) {
					for(int k = 0; k < item.width; k++) {
						if(items[i + inventoryWidth * j + k].item.itemName != "") {
							counter++;
						}
					}
				}
				//There's no items in the slots that the item occupies
				//So we can add the item now
				if(counter == 0) {
					for(int l = 0; l < item.height; l++) {
						for(int m = 0; m < item.width; m++) {
							//First we add the items to the slots the it fills and set their slots to clear
							items[i + inventoryWidth * l + m].item = DeepCopy(item);
							items[i + inventoryWidth * l + m].itemStartNumber = i;
							items[i + inventoryWidth * l + m].GetComponent<Image>().color = Color.clear;
							items[i + inventoryWidth * l + m].stacksizeText.gameObject.SetActive(false);
							//If it's the first index of the added item
							if(items.IndexOf(items[i + inventoryWidth * l + m]) == i) {
								SetSlotImageSprite(items[i + inventoryWidth * l + m], item.icon);
								items[i + inventoryWidth * l + m].itemFrame.gameObject.SetActive(true);
								items[i + inventoryWidth * l + m].itemFrame.GetComponent<CanvasGroup>().interactable = true;
								items[i + inventoryWidth * l + m].itemFrame.GetComponent<CanvasGroup>().blocksRaycasts = true;
								items[i + inventoryWidth * l + m].GetComponent<CanvasGroup>().blocksRaycasts = true;
								items[i + inventoryWidth * l + m].itemFrame.rectTransform.sizeDelta = 
								new Vector2(item.width * slotIconSize, item.height * slotIconSize);
								//If the item is stackable
								if(item.stackable) {
									items[i + inventoryWidth * l + m].stacksizeText.gameObject.SetActive(true);
									items[i + inventoryWidth * l + m].stacksizeText.text = item.stackSize.ToString();
								}
								//The item is unidentified
								if(item.unidentified) {
									items[i + inventoryWidth * l + m].itemImage.color = Color.red;
									items[i + inventoryWidth * l + m].unidentified.gameObject.SetActive(true);
								}
							}
						}
					}
					//Item succesfully added
					return true;
				}
			}
		}
		//Item unsuccesfully added
		return false;
	}

	//Add an item at a specific slot
	public bool AddItemAtSlot(ItemClass item, uGUIInventorySlot slot) {
		int i = items.IndexOf(slot);
		for(int j = 0; j < item.height; j++) {
			for(int k = 0; k < item.width; k++) {
				//The item we want to add doesn't fit so just return
				if(!CheckItemFit(item,slot,true)) {
					return false;
				}
				//There's something in the slot we want to add the item
				if(items[i + inventoryWidth * j + k].item.itemName != "") {
					//If the player can drag and swap items
					if(dragSwap) {
						//Replace the dragged item and item in the slot
						int counter = 0;
						uGUIInventorySlot foundSlot = null;
						int itemStartNumber = Mathf.RoundToInt(Mathf.Infinity);
						for(int l = 0; l < item.height; l++) {
							for(int m = 0; m < item.width; m++) {
								if(items[slot.itemStartNumber + inventoryWidth * l + m].item.itemName 
									!= "" && itemStartNumber != items[slot.itemStartNumber + inventoryWidth * l + m].itemStartNumber) 
								{
									itemStartNumber = items[slot.itemStartNumber + inventoryWidth * l + m].itemStartNumber;
									counter++;
									foundSlot = items[slot.itemStartNumber + inventoryWidth * l + m];
								}
							}
						}
						if(counter == 1) {
							ItemClass tempItem = DeepCopy(draggedItem);
							DragItemFromSlot(foundSlot);
							AddItemAtSlot(tempItem, slot);
							transform.root.GetComponent<AudioSource>().PlayOneShot(items[i + inventoryWidth * j + k].item.itemSound);
							return false;
						}
						else {
							return false;
						}
					}
					else {
						return false;
					}
				}
				//There's no items in the slots that the item occupies
				//So we can add the item now
				if(j == item.height - 1 && k == item.width - 1) {
					for(int l = 0; l < item.height; l++) {
						for(int m = 0; m < item.width; m++) {
							//Add the item to the slots that the item fills and set their icons to clear
							items[i + inventoryWidth * l + m].item = DeepCopy(item);
							items[i + inventoryWidth * l + m].itemStartNumber = i;
							items[i + inventoryWidth * l + m].GetComponent<Image>().color = Color.clear;
							items[i + inventoryWidth * l + m].stacksizeText.gameObject.SetActive(false);
							//If it's the first index of the added item
							//Set the icon and frame to the size of the item and the color to white
							if(items.IndexOf(items[i + inventoryWidth * l + m]) == i) {
								SetSlotImageSprite(items[i + inventoryWidth * l + m],item.icon);
								items[i + inventoryWidth * l + m].itemFrame.gameObject.SetActive(true);
								items[i + inventoryWidth * l + m].itemFrame.GetComponent<CanvasGroup>().interactable = true;
								items[i + inventoryWidth * l + m].itemFrame.GetComponent<CanvasGroup>().blocksRaycasts = true;
								items[i + inventoryWidth * l + m].GetComponent<CanvasGroup>().blocksRaycasts = true;
								items[i + inventoryWidth * l + m].itemFrame.rectTransform.sizeDelta = new Vector2
								(item.width * slotIconSize, item.height * slotIconSize);
								//If the item is unidentified
								if(item.unidentified) {
									items[i + inventoryWidth * l + m].itemImage.color = Color.red;
									items[i + inventoryWidth * l + m].unidentified.gameObject.SetActive(true);
								}
								//If the item is stackable
								if(item.stackable) {
									items[i + inventoryWidth * l + m].stacksizeText.gameObject.SetActive(true);
									items[i + inventoryWidth * l + m].stacksizeText.text = item.stackSize.ToString();
								}
							}
						}
					}
					//Item was successfully added
					return true;
				}
			}
		}
		//Item was unsuccessfully added
		return false;
	}

	//Swap the dragged item and the item of the slot clicked
	public bool SwapItems(uGUIInventorySlot slot) {
		//Make a copy of the item in the slot
		ItemClass item = DeepCopy(items[slot.itemStartNumber].item);
		//Remove the item from the slot
		RemoveItemFromSlot(slot);
		AddItemAtSlot(draggedItem, slot);
		//Start dragging
		dragging = true;
		draggedItem = item;
		dragItem.sprite = item.icon;
		dragItem.rectTransform.sizeDelta = new Vector2(item.width * slotIconSize, item.height * slotIconSize);
		dragItem.gameObject.SetActive(true);
		return true;
	}

	//Remove item from it's slot and start dragging
	public bool DragItemFromSlot(uGUIInventorySlot slot) {
		//If the merchant's repair window is open and the item is added to the repair slot then remove it from the repair slot
		if(merchant.repair.activeSelf) {
			if(items[slot.itemStartNumber].item == merchant.itemToRepair) {
				merchant.itemToRepair = new ItemClass();
				merchant.itemToRepair.itemName = "";
				merchant.itemRepairIcon.gameObject.SetActive(false);
				merchant.singleRepairCoin.gameObject.SetActive(false);
				merchant.singleRepairLabel.gameObject.SetActive(false);
				merchant.repairSingleText.text = "Place item to see repair cost.";
			}
		}
		//Make a copy of the item in the slot and remove it from the slot and set the copied item equal to the dragged item
		ItemClass item = DeepCopy(slot.item);
		dragStartIndex = slot.itemStartNumber;
		RemoveItemFromSlot(slot);
		dragging = true;
		draggedItem = item;
		dragItem.sprite = item.icon;
		dragItem.rectTransform.sizeDelta = new Vector2(item.width * slotIconSize, item.height * slotIconSize);
		dragItem.gameObject.SetActive(true);
		uGUICursorController.ChangeCursor("Default");
		return true;
	}

	//Return the dragged item to the slot it came from
	public void ReturnDraggedItem() {

		AddItemAtSlot(draggedItem, items[dragStartIndex]);

		StopDragging();
	}

	//Equip an item
	public bool EquipItem(uGUIInventorySlot slot) {
		//First we check if the item is of type offhand
		if(slot.item.itemType == EquipmentSlotType.offHand) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player already has a two handed weapon equipped and can't equip both a shield and a two handed weapon
				//Return the two handed weapon to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.weapon && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.twoHanded) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
		//Check if the item is of type weapon and is two-handed
		if(slot.item.itemType == EquipmentSlotType.weapon && slot.item.twoHanded) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player already has a offhand equipped and can't equip both a shield and a two handed weapon
				//Return the offhand to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.offHand && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.itemType == EquipmentSlotType.offHand) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
		//Run through all the equipment slots
		for(int i = 0; i < equipmentSlots.Count; i++) {
			//We've found the right slot
			if(equipmentSlots[i].equipmentSlotType == slot.item.itemType) {
				//There's no item in the slot
				if(equipmentSlots[i].item.itemName == "") {
					//Equip the item at the slot
					equipmentSlots[i].item = DeepCopy(slot.item);
					SetSlotImageSprite(equipmentSlots[i], equipmentSlots[i].item.icon);
					equipmentSlots[i].transform.Find("ItemBackground").gameObject.SetActive(false);
					return true;
				}
				//There's something in the slot
				else {
					//Make a copy of the item in the equipment slot
					ItemClass item = DeepCopy(equipmentSlots[i].item);
					ItemClass tempSlotItem = DeepCopy(slot.item);
					//Check if the item can fit in the slot where the original item came from
					if(CheckItemFit(item,items[slot.itemStartNumber], true)) {
						int startNumber = slot.itemStartNumber;
						RemoveItemFromSlot(items[startNumber]);
						AddItemAtSlot(item,items[startNumber]);
						equipmentSlots[i].item = tempSlotItem;
						SetSlotImageSprite(equipmentSlots[i], equipmentSlots[i].item.icon);
						transform.root.GetComponent<AudioSource>().PlayOneShot(item.itemSound);
						equipmentSlots[i].transform.Find("ItemBackground").gameObject.SetActive(false);
						StartCoroutine(tooltip.ShowTooltip(true, slot.item, SlotType.inventory, slot.itemStartNumber, slot.GetComponent<RectTransform>(), false));
						return false;
					}
					//if the item doesn't fit in the slot it came from then just add the item to the inventory
					else {
						RemoveItemFromSlot(slot);
						equipmentSlots[i].item = tempSlotItem;
						AddItem(item);
						SetSlotImageSprite(equipmentSlots[i], equipmentSlots[i].item.icon);
						transform.root.GetComponent<AudioSource>().PlayOneShot(item.itemSound);
						OnMouseEnter(slot.gameObject);
						equipmentSlots[i].transform.Find("ItemBackground").gameObject.SetActive(false);
						StartCoroutine(tooltip.ShowTooltip(true, slot.item, SlotType.inventory, slot.itemStartNumber, slot.GetComponent<RectTransform>(), false));
						return false;
					}
				}
			}
		}
		return false;
	}

	//Equip an item at a specific equipment slot
	public void EquipItemAtSlot(uGUIEquipmentSlot slot, ItemClass item) {
		//Create a copy of the item to equip
		slot.item = DeepCopy(item);

		SetSlotImageSprite(slot, item.icon);

		slot.transform.Find("ItemBackground").gameObject.SetActive(false);

		//Check to see if the item equipped is of type offhand
		if(slot.item.itemType == EquipmentSlotType.offHand) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player has a two-handed weapon equipped and the player can't equip both a shield and a two handed weapon
				//Then add the weapon to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.weapon && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.twoHanded) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
		//Check to see if the item equipped is of type two-handed weapon
		if(slot.item.itemType == EquipmentSlotType.weapon && slot.item.twoHanded) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player has a shield equipped and can't equip both a two-handed weapon and a shield
				//Then add the shield to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.offHand && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.itemType == EquipmentSlotType.offHand) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
	}

	//Swap an inventory item and a equipmentslot item
	public void SwapInvItemEquipped(uGUIEquipmentSlot equipSlot, uGUIInventorySlot invSlot) {
		//Create a copy of the item in the equipment slot
		ItemClass item = DeepCopy(equipSlot.item);
		//Create a copy of the item in the inventory slot and add it to the equipment slot
		equipSlot.item = DeepCopy(invSlot.item);
		SetSlotImageSprite(equipSlot, equipSlot.item.icon);
		int startNumber = invSlot.itemStartNumber;
		//Remove the item from the inventory
		RemoveItemFromSlot(items[startNumber]);
		//Add the item from the equipment slot to the inventory
		AddItemAtSlot(item, items[startNumber]);

		//Show the tooltip for the swapped item in the inventory
		StartCoroutine(tooltip.ShowTooltip(true, invSlot.item, SlotType.inventory, invSlot.itemStartNumber, equipSlot.GetComponent<RectTransform>(), false));
		//Play the sound of the item in the inventory
		transform.root.GetComponent<AudioSource>().PlayOneShot(invSlot.item.itemSound);

		//Check to see if the item equipped is of type offhand
		if(equipSlot.item.itemType == EquipmentSlotType.offHand) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player has a two-handed weapon equipped and the player can't equip both a shield and a two handed weapon
				//Then add the weapon to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.weapon && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.twoHanded) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
		//Check to see if the item equipped is of type two-handed weapon
		if(equipSlot.item.itemType == EquipmentSlotType.weapon && equipSlot.item.twoHanded) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player has a shield equipped and can't equip both a two-handed weapon and a shield
				//Then add the shield to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.offHand && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.itemType == EquipmentSlotType.offHand) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}

	}

	//Function used to find the ring slot
	public bool EquipRing(uGUIInventorySlot slot) {
		//Run through all the equipment slots
		for(int i = 0; i < equipmentSlots.Count; i++) {
			//We've found the first ring slot
			if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.ring) {
				//If there's nothing in the slot
				if(equipmentSlots[i].item.itemName == "") {
					//Equip the item
					EquipItemAtSlot(equipmentSlots[i], slot.item);
					slot.transform.Find("ItemBackground").gameObject.SetActive(false);
					return true;
				}
				//There's already a ring in the slot
				else {
					//Run through the equipment slots again and find the next ring slot
					for(int j = 0; j < equipmentSlots.Count; j++) {
						//If the slot isn't the same as the first we found and it's of type ring
						if(equipmentSlots[i] != equipmentSlots[j] && equipmentSlots[j].equipmentSlotType == EquipmentSlotType.ring) {
							//There's nothing in this ring slot so equip the ring here
							if(equipmentSlots[j].item.itemName == "") {
								EquipItemAtSlot(equipmentSlots[j], slot.item);
								slot.transform.Find("ItemBackground").gameObject.SetActive(false);
								return true;
							}
							//There's a ring in the slot so we swap it with the one in the inventory
							else {
								SwapInvItemEquipped(equipmentSlots[i], slot);
								slot.transform.Find("ItemBackground").gameObject.SetActive(false);
								//We need to return false here because else the ring in the inventory would get deleted
								return false;
							}
						}
					}
				}
			}
		}
		//Only returns here if there's no ring slot
		return false;
	}

	//Removes an item from a slot
	public void RemoveItemFromSlot(uGUIInventorySlot slot) {
		//Make a copy of the item in the slot
		ItemClass item = DeepCopy(slot.item);
		//Run through all the slot that item occupies and remove it from the slot
		for(int i = 0; i < item.height; i++) {
			for(int j = 0; j < item.width; j++) {
				items[slot.itemStartNumber + inventoryWidth * i + j].item = new ItemClass();
				items[slot.itemStartNumber + inventoryWidth * i + j].item.itemName = "";
				items[slot.itemStartNumber + inventoryWidth * i + j].itemImage.color = Color.white;
				items[slot.itemStartNumber + inventoryWidth * i + j].unidentified.gameObject.SetActive(false);
				items[slot.itemStartNumber + inventoryWidth * i + j].GetComponent<Image>().color = Color.white;
				items[slot.itemStartNumber + inventoryWidth * i + j].itemImage.gameObject.SetActive(false);
				items[slot.itemStartNumber + inventoryWidth * i + j].itemFrame.gameObject.SetActive(false);
				items[slot.itemStartNumber + inventoryWidth * i + j].stacksizeText.gameObject.SetActive(false);
				items[slot.itemStartNumber + inventoryWidth * i + j].GetComponent<CanvasGroup>().blocksRaycasts = true;
			}
		}
		//Reset the item start numbers
		ResetItemStartNumbers();
	}

	//Stoppes drag
	public void StopDragging() {
		dragging = false;
		draggedItem = new ItemClass();
		draggedItem.itemName = "";
		dragItem.gameObject.SetActive(false);
		dragItemBackground.gameObject.SetActive(false);
	}

	public void SetSlotImageSprite(uGUIInventorySlot slot, Sprite sprite) {
		slot.itemImage.sprite = slot.item.icon;
		slot.itemImage.rectTransform.sizeDelta = new Vector2(slot.item.width * slotIconSize, slot.item.height * slotIconSize);
		slot.itemImage.gameObject.SetActive(true);
	}

	public void SetSlotImageSprite(uGUIEquipmentSlot slot, Sprite sprite) {
		slot.itemIcon.sprite = slot.item.icon;
		slot.itemIcon.rectTransform.sizeDelta = new Vector2(slot.item.width * slotIconSize * slot.iconScaleFactor, slot.item.height * slotIconSize * slot.iconScaleFactor);
		slot.itemIcon.gameObject.SetActive(true);
	}

	//Check to see if an item can fit in the slot
	public bool CheckItemFit(ItemClass item, uGUIInventorySlot slot, bool skipLastCheck) {
		//Run through all the slots that the item occupies
		for(int i = 0; i < item.height; i++) {
			for(int j = 0; j < item.width; j++) {
				//Check if the slot exists
				if(slot.itemStartNumber + inventoryWidth * i + j >= items.Count) {
					return false;
				}
				//Check to see if the first slot is located at the edge of the inventory
				for(int k = 0; k < item.height; k++) {
					if(slot.itemStartNumber + inventoryWidth * k + j != slot.itemStartNumber + inventoryWidth * k) {
					    if(((slot.itemStartNumber + inventoryWidth * i + j ) % inventoryWidth == 0)
					     && item.width != 1) {
							return false;
						}
					}
				}
				//Last check is only used sometimes
				//Checks to see if there's already something in the slots
				if(!skipLastCheck) {
					if(items[slot.itemStartNumber + inventoryWidth * i + j].itemStartNumber != slot.itemStartNumber + inventoryWidth * i + j) {
						return false;
					}
				}
				//Run through the slots and check if there's already something in the slots
				else {
					List<int> counter = new List<int>();
					for(int l = 0; l < item.height; l++) {
						for(int m = 0; m < item.width; m++) {
							if((slot.itemStartNumber + inventoryWidth * (item.height - 1) + (item.width - 1)) < items.Count - 1 
								&& items[slot.itemStartNumber + inventoryWidth * l + m].itemStartNumber != slot.itemStartNumber
								 && items[slot.itemStartNumber + inventoryWidth * l + m].item.itemName != "" 
								 && !counter.Contains(items[slot.itemStartNumber + inventoryWidth * l + m].itemStartNumber)) {
								counter.Add(items[slot.itemStartNumber + inventoryWidth * l + m].itemStartNumber);
							}
						}
					}
					if(counter.Count > 1) {
						//return false if there's more than one item
						return false;
					}
					else if(counter.Count == 1) {
						//Return true if there's only one item in the slots
						return true;
					}
				}
			}
		}
		return true;
	}

	//Reset the item start number of the empty slots
	public void ResetItemStartNumbers() {
		for(int i = 0; i < items.Count; i++) {
			if(items[i].item.itemName == "") {
				items[i].itemStartNumber = i;
			}
		}
	}

	//Called when the equipment slot is clicked
	public void OnEquipmentSlotClick(GameObject obj, int mouseIndex) {
		uGUIEquipmentSlot slot = obj.GetComponent<uGUIEquipmentSlot>();
		//Left button was clicked
		if(mouseIndex == 0) {
			//If the player is dragging an item
			if(dragging) {
				//return if the dragged item is unidentified because we shouldn't be able to equip unidentified items
				if(draggedItem.unidentified) {
					return;
				}
				//If the item is of the same type as the equipment slot
				if(draggedItem.itemType == slot.equipmentSlotType) {
					//There's no item in the slot
					if(slot.item.itemName == "") {
						transform.root.GetComponent<AudioSource>().PlayOneShot(draggedItem.itemSound);
						EquipItemAtSlot(slot);
					}
					//There's an item in the slot
					else {
						transform.root.GetComponent<AudioSource>().PlayOneShot(draggedItem.itemSound);
						DragSwapEquippedItem(slot);
					}
				}
				//If the item isn't of the same type and we have set it to auto find the slot of the same type of the item
				else if(autoFindEquipmentSlot) {
					for(int i = 0; i < equipmentSlots.Count; i++) {
						//If the player can dual wield and the dragged item is of type weapon
						if(player.canDualWield && draggedItem.itemType == EquipmentSlotType.weapon) {
							//The right slot is found
							if(equipmentSlots[i].equipmentSlotType == draggedItem.itemType) {
								//If there's nothing in the slot equip the item at the slot
								if(equipmentSlots[i].item.itemName == "") {
									transform.root.GetComponent<AudioSource>().PlayOneShot(draggedItem.itemSound);
									EquipItemAtSlot(equipmentSlots[i], draggedItem);
									StopDragging();
								}
								//If there's something in the slot
								else {
									for(int j = 0; j < equipmentSlots.Count; j++) {
										//Check to see if there's something in the offhand slot
										if(equipmentSlots[j].equipmentSlotType == EquipmentSlotType.offHand) {
											//The dragged item isn't two handed and there's nothing in the offhand slot or the dragged item is two handed and the player can dual wield two handed weapon and there's nothing in the offhand slot
											//Then equip the item
											if((!draggedItem.twoHanded && equipmentSlots[j].item.itemName == "") || (draggedItem.twoHanded && player.canDualWieldTwoHanded && equipmentSlots[j].item.itemName == "")) {
												transform.root.GetComponent<AudioSource>().PlayOneShot(draggedItem.itemSound);
												EquipItemAtSlot(equipmentSlots[j], draggedItem);
												StopDragging();
											}
											//Else if the dragged item is two handed and the player can dual wield two handed weapon and there's something in the slot
											//Swap the dragged item with the one in the offhand slot
											else if((draggedItem.twoHanded && player.canDualWieldTwoHanded && equipmentSlots[j].item.itemName != "") || (draggedItem.itemType == EquipmentSlotType.weapon && player.canDualWield && equipmentSlots[j].item.itemName != "")) {
												transform.root.GetComponent<AudioSource>().PlayOneShot(draggedItem.itemSound);
												DragSwapEquippedItem(equipmentSlots[j]);
												return;
											}
											//Else just swap the items
											else {
												transform.root.GetComponent<AudioSource>().PlayOneShot(draggedItem.itemSound);
												DragSwapEquippedItem(equipmentSlots[i]);
											}
										}
									}
								}
							}
						}
						else {
							//If the dragged item and the equipment slot is of same type
							if(equipmentSlots[i].equipmentSlotType == draggedItem.itemType) {
								transform.root.GetComponent<AudioSource>().PlayOneShot(draggedItem.itemSound);
								//If there's nothing in the equipment slot just add the item
								if(equipmentSlots[i].item.itemName == "") {
									EquipItemAtSlot(equipmentSlots[i], draggedItem);
									StopDragging();
								}
								//If there's something in the equipment slot swap the dragged item and the item in the slot
								else {
									transform.root.GetComponent<AudioSource>().PlayOneShot(draggedItem.itemSound);
									DragSwapEquippedItem(equipmentSlots[i]);
								}
							}
						}
					}
				}
			}
			//The player is not dragged anything
			else if(!identifying) {
				//Drag the item from the equipment slot
				if(slot.item.itemName != "") {
					DragEquippedItem(slot);
					RemoveEquippedItem(slot);
				}
			}
		}
		//Right button was clicked
		else {
			//If the player isn't already dragging
			if(!dragging && !identifying) {
				//If the player can unequip item by right clicking the item
				//Then remove the item from the slot and add it to the inventory
				if(rightClickUnequipItems) {
					if(AddItem(slot.item)) {
						transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
						RemoveEquippedItem(slot);
					}
				}
			}
		}
	}

	//Equip an item at a specific slot
	public void EquipItemAtSlot(uGUIEquipmentSlot slot) {
		//Make a copy of the dragged item
		slot.item = DeepCopy(draggedItem);
		SetSlotImageSprite(slot, draggedItem.icon);
		slot.transform.Find("ItemBackground").gameObject.SetActive(false);
		StopDragging();
		StartCoroutine(tooltip.ShowTooltip(true, slot.item, 
		SlotType.equipment,0, slot.GetComponent<RectTransform>(), false));

		//Check to see if the item equipped is of type offhand
		if(slot.item.itemType == EquipmentSlotType.offHand) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player has a two-handed weapon equipped and the player can't equip both a shield and a two handed weapon
				//Then add the weapon to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.weapon && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.twoHanded) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
		//Check to see if the item equipped is of type two-handed weapon
		if(slot.item.itemType == EquipmentSlotType.weapon && slot.item.twoHanded) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player has a shield equipped and can't equip both a two-handed weapon and a shield
				//Then add the shield to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.offHand && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.itemType == EquipmentSlotType.offHand) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
	}

	//Swap the dragged item with the item in the equipment slot
	public void DragSwapEquippedItem(uGUIEquipmentSlot slot) {
		ItemClass item = DeepCopy(slot.item);
		slot.item = DeepCopy(draggedItem);
		SetSlotImageSprite(slot, slot.item.icon);
		draggedItem = item;
		dragItem.sprite = item.icon;
		dragItem.rectTransform.sizeDelta = new Vector2(item.width * slotIconSize, item.height * slotIconSize);

		//Check to see if the item equipped is of type offhand
		if(slot.item.itemType == EquipmentSlotType.offHand) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player has a two-handed weapon equipped and the player can't equip both a shield and a two handed weapon
				//Then add the weapon to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.weapon && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.twoHanded) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
		//Check to see if the item equipped is of type two-handed weapon
		if(slot.item.itemType == EquipmentSlotType.weapon && slot.item.twoHanded) {
			for(int i = 0; i < equipmentSlots.Count; i++) {
				//If the player has a shield equipped and can't equip both a two-handed weapon and a shield
				//Then add the shield to the inventory
				if(equipmentSlots[i].equipmentSlotType == EquipmentSlotType.offHand && !player.canWieldTwoHandedAndShield) {
					if(equipmentSlots[i].item.itemType == EquipmentSlotType.offHand) {
						AddItem(equipmentSlots[i].item);
						RemoveEquippedItem(equipmentSlots[i]);
					}
				}
			}
		}
	}

	//Drag an equipped item from the equipment slot
	public void DragEquippedItem(uGUIEquipmentSlot slot) {
		//Make a copy of the item in the equipment slot and assign it to the item that's being dragged
		draggedItem = DeepCopy(slot.item);
		dragItem.sprite = slot.item.icon;
		dragItem.rectTransform.sizeDelta = new Vector2(slot.item.width * slotIconSize, slot.item.height * slotIconSize);
		dragItem.gameObject.SetActive(true);
		dragging = true;
		slot.transform.Find("ItemBackground").gameObject.SetActive(true);
	}

	//Removes an item from the equipment slot
	public void RemoveEquippedItem(uGUIEquipmentSlot slot) {
		slot.item = new ItemClass();
		slot.item.itemName = "";
		slot.itemIcon.gameObject.SetActive(false);
		tooltip.HideTooltip();
		slot.transform.Find("ItemBackground").gameObject.SetActive(true);
	}

	//Called when an inventory slot is clicked
	public void OnSlotClick(GameObject obj, int mouseIndex) {
		//Get the slot component from the slot
		uGUIInventorySlot slot = obj.GetComponent<uGUIInventorySlot>();
		//If left click
		if(mouseIndex == 0) {
			//There's an item in the slot
			if(slot.item.itemName != "") {
				if(merchant.draggedItem) {
					if(slot.item.itemName == merchant.draggedItem.item.itemName) {
						if(slot.item.stackSize < slot.item.maxStackSize) {
							slot.item.stackSize += merchant.draggedItem.item.stackSize;
							slot.stacksizeText.text = slot.item.stackSize.ToString();
							merchant.dragging = false;
							StopDragging();
							return;
						}
					}
					else {
						return;
					}
				}
				//If the player is currently identifying an item
				if(identifying) {
					//If the item is unidentified then identify it
					if(slot.item.unidentified) {
						IdentifyItem(slot);
					}
				}
				//The player isn't identifying but is dragging
				else if(dragging) {

					if(draggedItem.itemName == slot.item.itemName && slot.item.stackable) {
						slot.item.stackSize += draggedItem.stackSize;
						transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
						if(slot.item.stackSize > slot.item.maxStackSize) {
							int tempStack = slot.item.maxStackSize - slot.item.stackSize;
							slot.item.stackSize = slot.item.maxStackSize;
							draggedItem.stackSize = Mathf.Abs(tempStack);
							slot.stacksizeText.text = slot.item.stackSize.ToString();
							return;
						}
						else {
							slot.stacksizeText.text = slot.item.stackSize.ToString();
							StopDragging();
							return;
						}
					}

					//Find if the item isn't trying to replace two items
					int counter = 0;
					uGUIInventorySlot foundSlot = null;
					int itemStartNumber = Mathf.RoundToInt(Mathf.Infinity);
					for(int i = 0; i < draggedItem.height; i++) {
						for(int j = 0; j < draggedItem.width; j++) {
							if(items[slot.itemStartNumber + inventoryWidth * i + j].item.itemName 
							!= "" && 
							itemStartNumber != 
							items[slot.itemStartNumber + inventoryWidth * i + j].itemStartNumber) {
								itemStartNumber = items[slot.itemStartNumber + inventoryWidth * i + j].
								itemStartNumber;
								counter++;
								foundSlot = items[slot.itemStartNumber + inventoryWidth * i + j];
							}
						}
					}
					//If it only tries to remove one item
					if(counter == 1) {
						//Swap the dragged item and the item in the slot
						if(SwapItems(foundSlot)) {
							transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
							OnMouseEnter(slot.gameObject);
						}
					}
				}
				//The player isn't dragging
				else {
					//Drag the item from the slot and hide the tooltip
					if(DragItemFromSlot(slot)) {
						RemoveItemFromSlot(slot);
						tooltip.HideTooltip();
					}
				}
			}
			//There's nothing in the slot
			else {
				//If the player is dragging an item from the merchant
				if(merchant.dragging) {
					if(CheckItemFit(merchant.draggedItem.item, slot, false)) {
						player.money -= merchant.draggedItem.item.buyPrice;
						merchant.avaliableGoldText.text = player.money.ToString();
						transform.root.GetComponent<AudioSource>().PlayOneShot(merchant.draggedItem.item.itemSound);
						if(merchant.draggedItem.item.stackable) {
							AddItemAtSlot(merchant.draggedItem.item, slot);
						}
						else {
							AddItemAtSlot(merchant.draggedItem.item, slot);
							merchant.RemoveItem(merchant.draggedItem.item, merchant.draggedItem);
						}
						merchant.dragging = false;
						merchant.draggedItem = null;
						StopDragging();
						merchant.ResetTabs();
					}
				}
				//Else if the player is dragging
				else if(dragging) {
					//Try to add the item at the slot
					if(AddItemAtSlot(draggedItem, slot)) {
						transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
						StopDragging();
						OnMouseEnter(slot.gameObject);
					}
				}
				else {
					//Maybe play empty sound?
				}
			}
		}
		//If right click
		else if(mouseIndex == 1) {
			//We don't wont anything to happen when player is identifying or the item is unidentified so just return.
			if(identifying || slot.item.unidentified) {
				return;
			}
			//There's an item in the slot
			if(slot.item.itemName != "") {
				//If the player presses the shift 
				if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					//If the stacksize of the item is less or equal to 1 just return - it can't be splitted
					if(slot.item.stackSize <= 1) {
						return;
					}
					//Open up the split window
					itemToSplit = slot;
					splitWindow.SetActive(true);
					splitWindow.GetComponent<uGUISplitWindow>().Start();
				}
				//If the merchant isn't open
				else if(!merchant.showMerchant) {
					//If the right clicked item is of type consumable then use the item
					if(slot.item.itemType == EquipmentSlotType.consumable) {
						UseItem(slot);
					}
					//We need seperate logic for the rings because there's two ring slots
					else if(slot.item.itemType == EquipmentSlotType.ring) {
						//Equip the ring
						if(EquipRing(slot)) {
							transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
							RemoveItemFromSlot(slot);
						}
					}
					//If the player can dual wield and the item is of type weapons
					else if(player.canDualWield && slot.item.itemType == EquipmentSlotType.weapon) {
						//Run through all the equipment slots
						for(int i = 0; i < equipmentSlots.Count; i++) {
							//If the found equipment slot is the same as the item
							if(equipmentSlots[i].equipmentSlotType == slot.item.itemType) {
								//There's nothing in the slot
								if(equipmentSlots[i].item.itemName == "") {
									//Equip the item and remove the item from the inventory
									transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
									EquipItemAtSlot(equipmentSlots[i], slot.item);
									RemoveItemFromSlot(slot);
								}
								//There's something in the equipment slot
								else {
									//Run through all the equipment slots
									for(int j = 0; j < equipmentSlots.Count; j++) {
										//If the equipment slot found is of type offhand
										if(equipmentSlots[j].equipmentSlotType == EquipmentSlotType.offHand) {
											//If the item in the inventory slot isn't a two handed weapon and there's nothing in the equipment slot
											//or the item in the inventory slot is a two handed weapon and the player can dual wield two handed weapons and there is an item in the equipment slot
											if((!slot.item.twoHanded && equipmentSlots[j].item.itemName == "") || (slot.item.twoHanded && player.canDualWieldTwoHanded && equipmentSlots[j].item.itemName == "")) {
												//Equip the item
												transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
												EquipItemAtSlot(equipmentSlots[j], slot.item);
												RemoveItemFromSlot(slot);
											}
											//If the item in the inventory slot is a two handed weapon and there's something in the weapon slot and the there's something in the offhand slot too
											else if(slot.item.twoHanded && player.canDualWieldTwoHanded && equipmentSlots[i].item.itemName != "" && equipmentSlots[j].item.itemName != "") {
												//Then swap the item in the offhand slot and the item in the inventory
												transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
												SwapInvItemEquipped(equipmentSlots[j], slot);
												return;
											}
											//Else just swap the items
											else {
												transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
												SwapInvItemEquipped(equipmentSlots[i], slot);
												return;
											}
										}
									}
								}
							}
						}
					}
					//Else try to equip the item
					else if(EquipItem(slot)) {
						transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.itemSound);
						RemoveItemFromSlot(slot);
					}
				}
				//The merchant is open
				else {
					//If the repair tab is active
					if(merchant.repair.activeSelf) {
						//If the isn't already fully repaired add the item to the repair slot of the merchant
						if(slot.item.curDurability != slot.item.maxDurability) {
							merchant.itemToRepair = items[slot.itemStartNumber].item;
							merchant.itemRepairIcon.sprite = slot.item.icon;
							merchant.itemRepairIcon.gameObject.SetActive(true);
							merchant.itemRepairIcon.rectTransform.sizeDelta = new Vector2(slot.item.width * merchant.iconSlotSize, slot.item.height * merchant.iconSlotSize);
							merchant.repairSingleText.text = ((int)(merchant.itemToRepair.totalRepairCost * 0.1f * (1 - ((float)merchant.itemToRepair.curDurability/merchant.itemToRepair.maxDurability)))).ToString();
							merchant.singleRepairCoin.gameObject.SetActive(true);
							merchant.singleRepairLabel.gameObject.SetActive(true);
						}
						//The item is already at full durability so display a warning
						else {
							messageManager.AddMessage(Color.red, "Item is already fully repaired.");
						}
					}
					//The merchant's repair tab isn't active so sell the item
					else {
						SellItem(slot);
					}
				}
			}
			//There's nothing in the slot
			else {
				//Maybe play empty sound?
				//transform.root.audio.PlayOneShot(emptySound);
			}
		}
	}

	//Sell an item from the inventory
	public void SellItem(uGUIInventorySlot slot) {
		//Change the cursor
		uGUICursorController.ChangeCursor("Default");
		//Hide the tooltip
		tooltip.HideTooltip();
		//Add the sell price to the player
		player.money += slot.item.sellPrice;
		//Update the avaliable gold text
		merchant.avaliableGoldText.text = player.money.ToString();
		//Find the buyback tab and add the item to it
		//If the buyback tab isn't already there then instantiate it
		for(int i = 0; i < merchant.selectedMerchant.tabs.Count; i++) {
			if(merchant.selectedMerchant.tabs[i].tabType == TabType.buyBack) {
				if(merchant.selectedMerchant.tabs[i].items.Count == 0) {
					for(int j = 0; j < merchant.tabs.Count; j++) {
						if(merchant.tabs[j].tabType == TabType.buyBack) {
							break;
						}
						if(j == merchant.tabs.Count - 1) {
							GameObject tempTab = Instantiate(merchant.tabPrefab) as GameObject;
							tempTab.transform.SetParent(merchant.tabsObj.transform);
							if(!merchant.selectedMerchant.canRepair) {
								tempTab.transform.SetSiblingIndex(merchant.tabs.Count);
							}
							else {
								tempTab.transform.SetSiblingIndex(merchant.tabs.Count - 1);
							}
							tempTab.transform.localScale = Vector3.one;
							tempTab.GetComponent<Image>().color = merchant.tabInactiveColor;
							uGUIMerchantTab tab = tempTab.AddComponent<uGUIMerchantTab>();
							tab.tabType = TabType.buyBack;
							tab.items = new List<ItemClass>();
							tempTab.GetComponent<Image>().sprite = buyBackSprite;
							merchant.tabs.Add(tab);
							merchant.tabsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(merchant.tabWidth, merchant.tabHeight * merchant.tabs.Count);
						}
					}
				}
			}
		}
		for(int i = 0; i < merchant.selectedMerchant.tabs.Count; i++) {
			if(merchant.selectedMerchant.tabs[i].tabType == TabType.buyBack) {
				ItemClass item = DeepCopy(slot.item);
				item.buyPrice = item.sellPrice;
				merchant.selectedMerchant.tabs[i].items.Add(item);
			}
		}
		//Remove the sold item
		RemoveItemFromSlot(slot);
		//Reset the tabs of the merchant
		merchant.ResetTabs();
	}

	//Use an item when right clicked
	public void UseItem(uGUIInventorySlot slot) {
		//Create a new game object, add the use effect script to it and then execute the script
		//When done remove the created item
		GameObject obj = new GameObject("Use Effect");
		UseEffect effect = obj.AddComponent(System.Type.GetType(slot.item.useEffectScriptName)) as UseEffect;
		effect.item = slot.item;
		string message = effect.Use();
		//If the message returned is of type potion then remove one from the stacksize of the potion
		if(message == "Potion") {
			items[slot.itemStartNumber].item.stackSize--;
			items[slot.itemStartNumber].stacksizeText.text = items[slot.itemStartNumber].item.stackSize.ToString();
			transform.root.GetComponent<AudioSource>().PlayOneShot(slot.item.useSound);
			//remove the item if the stacksize is zero
			if(items[slot.itemStartNumber].item.stackSize == 0) {
				RemoveItemFromSlot(items[slot.itemStartNumber]);
			}
		}
		//If the returned message is of type Identify then start identifying
		else if(message == "Identify") {
			identifyingScrollOrignalSlot = slot;
			//Change the cursor to the identify cursor
			uGUICursorController.ChangeCursor("IdentifyingGlass");
		}
		//If the returned message isn't one of the above consider it a warning and display the warning
		else {
			messageManager.AddMessage(Color.red,message);
		}
		//Destroy the Use Effect game object after 1 second
		DestroyObject(obj, 1);
	}

	//Identify the item
	public void IdentifyItem(uGUIInventorySlot slot) {
		//Run through all the slots that the item fills and remove the unidentified flag from them
		for(int i = 0; i < slot.item.height; i++) {
			for(int j = 0; j < slot.item.width; j++) {
				items[slot.itemStartNumber + inventoryWidth * i + j].item.unidentified = false;
			}
		}
		items[slot.itemStartNumber].itemImage.color = Color.white;
		items[slot.itemStartNumber].unidentified.gameObject.SetActive(false);
		identifying = false;
		identifyingScrollOrignalSlot.item.stackSize--;
		identifyingScrollOrignalSlot.stacksizeText.text = identifyingScrollOrignalSlot.item.stackSize.ToString();
		uGUICursorController.ChangeCursor("Default");
		if(identifyingScrollOrignalSlot.item.stackSize == 0) {
			RemoveItemFromSlot(identifyingScrollOrignalSlot);
		}
		//updates the tooltip
		OnMouseEnter(slot.gameObject);
	}

	//Called when the mouse enters a slot
	public void OnMouseEnter(GameObject obj) {

		ItemClass item = new ItemClass();

		//If the entered slot is of type inventory slot
		if(obj.GetComponent<uGUIInventorySlot>()) {
			item = obj.GetComponent<uGUIInventorySlot>().item;
			for(int i = 0; i < items.Count; i++) {
				//If the tooltip isn't already showing the show it
				if(!tooltip.showTooltip) {
					StartCoroutine(tooltip.ShowTooltip(true, item, SlotType.inventory, obj.GetComponent<uGUIInventorySlot>().itemStartNumber, obj.GetComponent<RectTransform>(), false));
				}
			}
		}
		//Else if the entered slot is of type equipment slot
		else if(obj.GetComponent<uGUIEquipmentSlot>()) {
			item = obj.GetComponent<uGUIEquipmentSlot>().item;
		}

		//If there's nothing in the slot or the player is dragging then return
		if(item.itemName == "" || dragging) {
			return;
		}

		//If the player isn't not identifying and the merchant is active then show the sell cursor
		if(!identifying && merchant.showMerchant) {
			uGUICursorController.ChangeCursor("SellCursor");
		}

		//if the entered slot is of type equipment slot
		if(obj.GetComponent<uGUIEquipmentSlot>()) {
			StartCoroutine(tooltip.ShowTooltip(true, item, SlotType.equipment, 0, obj.GetComponent<RectTransform>(), false));
		}
	}

	//Called when the mouse leaves a slot
	public void OnMouseExit(GameObject obj) {
		//Hide the tooltip
		tooltip.HideTooltip();

		//If the player is not identifying then show the normal cursor
		if(!identifying) {
			uGUICursorController.ChangeCursor("Default");
		}
	}

	//Find the color based on the rarity of the item
	public Color FindColor(ItemClass item) {
		if(item.itemQuality == ItemQuality.Junk) {
			return junkColor;
		}
		else if(item.itemQuality == ItemQuality.Legendary) {
			return legendaryColor;
		}
		else if(item.itemQuality == ItemQuality.Magical) {
			return magicColor;
		}
		else if(item.itemQuality == ItemQuality.Normal) {
			return normalColor;
		}
		else if(item.itemQuality == ItemQuality.Rare) {
			return rareColor;
		}
		else if(item.itemQuality == ItemQuality.Set) {
			return setColor;
		}
		return Color.clear;
	}

	//Open or close the inventory
	public void OpenCloseInventory(bool state) {
		for(int i = 0; i < transform.childCount; i++) {
			transform.GetChild(i).gameObject.SetActive(state);
		}
	}

	//Creates a complete copy of an item
	public static ItemClass DeepCopy(ItemClass obj) {
		GameObject oj = obj.worldObject;
		Sprite tempTex = obj.icon;
		obj.icon = null;
		AudioClip clip = obj.itemSound;
		AudioClip useClip = obj.useSound;
		obj.itemSound = null;
		if(obj==null)
			throw new ArgumentNullException("Object cannot be null");
		ItemClass i = (ItemClass)Process(obj);
		i.worldObject = oj;
		i.icon = tempTex;
		obj.worldObject = oj;
		obj.icon = tempTex;
		obj.itemSound = clip;
		obj.useSound = useClip;
		i.itemSound = clip;
		i.useSound = useClip;
		DestroyImmediate(GameObject.Find("New Game Object"));
		return i;
	}
	
	static object Process(object obj) {
		if(obj==null)
			return null;
		Type type=obj.GetType();
		if(type.IsValueType || type==typeof(string)) {
			return obj;
		}
		else if(type.IsArray) {
			Type elementType=Type.GetType(
				type.FullName.Replace("[]",string.Empty));
			var array=obj as Array;
			Array copied=Array.CreateInstance(elementType,array.Length);
			for(int i=0; i<array.Length; i++) {
				copied.SetValue(Process(array.GetValue(i)),i);
			}
			return Convert.ChangeType(copied,obj.GetType());
		}
		else if(type.IsClass) {
			object toret=Activator.CreateInstance(obj.GetType());
			FieldInfo[] fields=type.GetFields(BindingFlags.Public| 
			                                  BindingFlags.NonPublic|BindingFlags.Instance);
			foreach(FieldInfo field in fields) {
				object fieldValue=field.GetValue(obj);
				if(fieldValue==null)
					continue;
				field.SetValue(toret, Process(fieldValue));
			}
			return toret;
		}
		else
			throw new ArgumentException("Unknown type");
	}
}

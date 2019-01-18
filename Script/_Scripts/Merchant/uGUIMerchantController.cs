using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MerchantItemList {
	[HideInInspector]
	public List<ItemClass> items;
	public TabType tabType;
	public Sprite tabSprite;
}

public class uGUIMerchantController : MonoBehaviour {

	public List<MerchantItemList> tabs;

	[HideInInspector]
	public uGUIMerchant merchant;

	public bool canRepair;

	public Sprite repairSprite;

	public ItemDatabase itemDatabase;

	[HideInInspector]
	public List<int> itemIDs;

	public float interactionRange;

	private Player player;

	// Use this for initialization
	void Awake () {
		itemDatabase = (ItemDatabase)Resources.Load("ItemDatabase", typeof(ItemDatabase)) as ItemDatabase;
		merchant = GameObject.FindGameObjectWithTag("Merchant").GetComponent<uGUIMerchant>();
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
	}

	void Start() {
		//Reset the list of items in the tabs
		for(int i = 0; i < tabs.Count; i++) {
			tabs[i].items = new List<ItemClass>();
		}

		//Find and add the items based on their IDs
		for(int i = 0; i < itemIDs.Count; i++) {
			ItemClass item = itemDatabase.FindItem(itemIDs[i]);
			TabType tabType = FindTabType(item);
			for(int j = 0; j < tabs.Count; j++) {
				if(tabs[j].tabType == tabType) {
					tabs[j].items.Add(item);
				}
			}
		}
	}

	//Finds the tab type of the item.
	TabType FindTabType(ItemClass item) {
		//Item is of type weapon
		if(item.itemType == EquipmentSlotType.weapon) {
			return TabType.weapon;
		}
		//Item is neither of type weapon, reagent, consumable or socket
		//This means the item is of type armor or jewelry
		else if(item.itemType != EquipmentSlotType.consumable && item.itemType != EquipmentSlotType.reagent && item.itemType != EquipmentSlotType.socket) {
			return TabType.armor;
		}
		//Item is of type weapon, reagent, consumable or socket
		else {
			return TabType.misc;
		}
	}

	// Update is called once per frame
	void Update () {
		//If the player right clicks
		if(Input.GetMouseButtonDown(1)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			//Send a ray to the position of the mouse
			if(Physics.Raycast(ray,out hit,Mathf.Infinity)) {
				//If the ray hits the merchant
				if(hit.transform.CompareTag("MerchantController") && hit.transform == transform) {

					//Set the selected merchant to be this one
					merchant.selectedMerchant = this;

					//Open the merchant
					merchant.OpenCloseMerchant(true);

					//remove all the tabs of the merchant
					foreach(uGUIMerchantTab t in merchant.tabsObj.GetComponentsInChildren<uGUIMerchantTab>()) {
						merchant.tabs.Remove(t);
						Destroy(t.gameObject);
					}

					//instantiate the tabs that this merchant has
					for(int i = 0; i < tabs.Count; i++) {
						if(tabs[i].tabType != TabType.buyBack || (tabs[i].tabType == TabType.buyBack && tabs[i].items.Count != 0)) {
							GameObject tempTab = Instantiate(merchant.tabPrefab) as GameObject;
							tempTab.transform.SetParent(merchant.tabsObj.transform);
							tempTab.transform.localScale = Vector3.one;
							tempTab.GetComponent<Image>().color = merchant.tabInactiveColor;
							uGUIMerchantTab tab = tempTab.AddComponent<uGUIMerchantTab>();
							tab.tabType = tabs[i].tabType;
							tab.items = tabs[i].items;
							tempTab.GetComponent<Image>().sprite = tabs[i].tabSprite;
							merchant.tabs.Add(tab);
						}
					}

					//If the merchant can repair then add the repair tab
					if(canRepair) {
						GameObject tempTab = Instantiate(merchant.tabPrefab) as GameObject;
						tempTab.transform.SetParent(merchant.tabsObj.transform);

						tempTab.transform.localScale = Vector3.one;
						tempTab.GetComponent<Image>().color = merchant.tabInactiveColor;
						uGUIMerchantTab tab = tempTab.AddComponent<uGUIMerchantTab>();
						tab.tabType = TabType.repair;
						tab.items = new List<ItemClass>();
						tempTab.GetComponent<Image>().sprite = repairSprite;
						merchant.tabs.Add(tab);
					}

					//Set the seleted tab of the merchant to be equal to the first of this merchants tabs
					merchant.selectedTab = merchant.tabs[0];
					merchant.selectedTab.GetComponent<Image>().color = merchant.tabActiveColor;
					merchant.ChangeTab(merchant.selectedTab);

					merchant.tabsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(merchant.tabWidth, merchant.tabHeight * merchant.tabs.Count);

					//set the items of the merchant's tabs
					for(int i = 0; i < merchant.tabs.Count; i++) {
						for(int j = 0; j < tabs.Count; j++) {
							if(merchant.tabs[i].tabType == tabs[j].tabType) {
								merchant.tabs[i].items = tabs[j].items;
							}
						}
					}
				}
			}
		}
		//If the player is further away from the merchant than the interaction range of the merchant then close the merchant window
		if(Vector3.Distance(player.transform.position, transform.position) > interactionRange && merchant.selectedMerchant == this) {
			merchant.OpenCloseMerchant(false);
		}
	}	
}

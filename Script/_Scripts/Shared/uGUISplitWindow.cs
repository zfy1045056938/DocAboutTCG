using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class uGUISplitWindow : MonoBehaviour {

	private int splitSize;
	private uGUIInventory inventory;

	public InputField inputfield;
	public Slider slider;

	// Use this for initialization
	public void Start () {
		//Initialize the values
		inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();
		splitSize = 1;
		slider.minValue = 1;
		slider.maxValue = inventory.itemToSplit.item.stackSize - 1;
		slider.value = splitSize;
		inputfield.text = splitSize.ToString();
		slider.value = slider.minValue;
	}

	public void Update() {
		//If the player presses the return key then accept the amount to split
		if(Input.GetKeyDown(KeyCode.Return)) {
			Accept();
		}
		//Cancel if the player presses the escape key
		if(Input.GetKeyDown(KeyCode.Escape)) {
			Cancel();
		}
	}

	//Update the split size
	public void UpdateInputField() {
		if(!string.IsNullOrEmpty(inputfield.text) && int.Parse(inputfield.text) <= inventory.itemToSplit.item.stackSize - 1) {
			splitSize = int.Parse(inputfield.text);
		}
		else if(string.IsNullOrEmpty(inputfield.text)) {
			splitSize = 0;
		}
		else {
			splitSize = inventory.itemToSplit.item.stackSize - 1;
			inputfield.text = splitSize.ToString();
		}
	}

	//update the split size when the slider changes
	public void UpdateSlider() {
		splitSize = (int)slider.value;
		inputfield.text = splitSize.ToString();
	}

	//Called when the player presses accept or the return key
	public void Accept() {
		if(inventory.itemToSplit.item.itemName != "") {
			ItemClass item = uGUIInventory.DeepCopy(inventory.itemToSplit.item);
			item.stackSize = splitSize;
			inventory.itemToSplit.item.stackSize -= splitSize;
			inventory.itemToSplit.stacksizeText.text = inventory.itemToSplit.item.stackSize.ToString();
			inventory.draggedItem = item;
			inventory.dragging = true;
			inventory.dragItem.sprite = item.icon;
			inventory.dragItem.gameObject.SetActive(true);
			inventory.dragItem.rectTransform.sizeDelta = new Vector2(item.width * inventory.slotIconSize, item.height * inventory.slotIconSize);
			this.gameObject.SetActive(false);
		}
	}

	//Hide the split window
	public void Cancel() {
		this.gameObject.SetActive(false);
	}
}

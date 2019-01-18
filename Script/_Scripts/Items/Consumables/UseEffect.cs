/*
 * This is the base for any use effect
 * You can see example uses in the potion and identifying scroll scripts
 * You may need to add custom checks to the UseItem() function in the inventory
 */ 


using UnityEngine;
using System.Collections;

public class UseEffect : MonoBehaviour {

	public ItemClass item;
	public Player player;
	public uGUIMessageManager messageManager;
	public uGUIInventory inventory;

	// Use this for initialization
	public void Start () {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		messageManager = GameObject.FindGameObjectWithTag("MessageManager").GetComponent<uGUIMessageManager>();
		inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();
	}

	public virtual string Use() {
		return "";
	}
}

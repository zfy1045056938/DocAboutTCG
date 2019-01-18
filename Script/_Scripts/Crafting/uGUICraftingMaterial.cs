using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class uGUICraftingMaterial : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public Text text;
	public Image icon;
	public ItemClass item;
	private uGUICrafting crafting;

	public void Start() {
		crafting = GameObject.FindGameObjectWithTag("Crafting").GetComponent<uGUICrafting>();
	}

	public void OnPointerEnter (PointerEventData data) {
		crafting.OnMaterialIconEnter(this);
	}
	
	public void OnPointerExit (PointerEventData data) {
		crafting.OnMaterialIconExit(this);
	}
}

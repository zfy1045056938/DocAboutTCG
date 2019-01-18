using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class uGUIMerchantTab : MonoBehaviour, IPointerClickHandler {

	public List<ItemClass> items;

	public TabType tabType;
	
	private uGUIMerchant merchant;

	// Use this for initialization
	void Start () {
		merchant = GameObject.FindGameObjectWithTag("Merchant").GetComponent<uGUIMerchant>();
	}

	public void OnPointerClick(PointerEventData data) {
		merchant.ChangeTab(this);
	}
}

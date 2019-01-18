using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class uGUITooltip : MonoBehaviour {

	public GameObject tooltip;
	public float tooltipMainHeight;
	public Text tooltipText;
	public Text tooltipHeaderText;
	public Text tooltipSellValueText;
	public Text tooltipSellValueTextLabel;
	public Text tooltipDescriptionText;
	public Text tooltipRequiredLevelText;
	public Image tooltipHeader;
	public Color attributeColor;
	public float tooltipDelay;

	[HideInInspector]
	public bool showTooltip;

	//Calculate and show the tooltip
	public IEnumerator ShowTooltip(bool right, ItemClass item, SlotType type, int startNumber, RectTransform trans, bool merchant) {

		showTooltip = true;

		if(right) {
			tooltip.GetComponent<RectTransform>().pivot = new Vector2(1,1);
		}
		else {			tooltip.GetComponent<RectTransform>().pivot = new Vector2(0,1);
		}

		yield return new WaitForSeconds(tooltipDelay);

		if(showTooltip) {
			foreach(Transform t in transform) {
				t.gameObject.SetActive(true);
			}

			tooltipText.text = GenerateTooltip(item);
			
			tooltip.gameObject.SetActive(true);
			tooltipHeader.color = FindColor(item);
			tooltipHeaderText.text = item.tooltipHeader.ToUpper();
			tooltipDescriptionText.text = item.descriptionText;
			
			tooltipRequiredLevelText.text = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t" + "Required level: " + item.levelReq.ToString();
			tooltipRequiredLevelText.rectTransform.sizeDelta = new Vector2(tooltipRequiredLevelText.preferredWidth, tooltipRequiredLevelText.preferredHeight);
			
			tooltipText.rectTransform.sizeDelta = new Vector2(tooltipRequiredLevelText.preferredWidth, tooltipText.preferredHeight + 4);
			tooltipDescriptionText.rectTransform.sizeDelta = new Vector2(tooltipRequiredLevelText.preferredWidth, tooltipDescriptionText.preferredHeight);
			
			tooltip.GetComponent<RectTransform>().sizeDelta = new Vector2(tooltipRequiredLevelText.rectTransform.sizeDelta.x + 50, 
				tooltipMainHeight + tooltipText.rectTransform.sizeDelta.y + tooltipDescriptionText.rectTransform.sizeDelta.y + tooltipRequiredLevelText.rectTransform.sizeDelta.y);

			if(merchant) {
				tooltipSellValueText.text = item.buyPrice.ToString();
				tooltipSellValueTextLabel.text = "Buy Price:";
			}
			else {
				tooltipSellValueText.text = item.sellPrice.ToString();
				tooltipSellValueTextLabel.text = "Sell Value:";

			}

			tooltipSellValueText.rectTransform.sizeDelta = new Vector2(tooltipSellValueText.preferredWidth, tooltipSellValueText.rectTransform.sizeDelta.y);

			RectTransform rect = tooltip.GetComponent<RectTransform>();
			
			tooltip.transform.position = trans.position;


			//Make sure the tooltip is at the correct side of the screen
			if(right) {
				tooltip.GetComponent<RectTransform>().pivot = new Vector2(1,1);
				if(type == SlotType.equipment) {
					tooltip.transform.localPosition -= new Vector3(trans.sizeDelta.x * 0.5f, -trans.sizeDelta.y * 0.5f);
				}
			}
			else {
				tooltip.GetComponent<RectTransform>().pivot = new Vector2(0,1);
				if(type == SlotType.crafting) {
				}
				else {
					tooltip.transform.localPosition += new Vector3(trans.sizeDelta.x * item.width,0);

				}
			}

			//Make sure that the tooltip doesn't leave the screen
			if(rect.localPosition.y < 0) {
				if(Mathf.Abs(rect.localPosition.y) + rect.sizeDelta.y > transform.root.GetComponent<CanvasScaler>().referenceResolution.y * 0.5f) {
					rect.localPosition -= new Vector3(0, transform.root.GetComponent<CanvasScaler>().referenceResolution.y * 0.5f - (Mathf.Abs(rect.localPosition.y) + rect.sizeDelta.y),0);
				}
			}
			else {
				if(Mathf.Abs(rect.sizeDelta.y - Mathf.Abs(rect.localPosition.y)) > transform.root.GetComponent<CanvasScaler>().referenceResolution.y * 0.5f) {
					rect.localPosition += new Vector3(0, Mathf.Abs(rect.sizeDelta.y - Mathf.Abs(rect.localPosition.y)) - transform.root.GetComponent<CanvasScaler>().referenceResolution.y * 0.5f,0);
				}
			}
		}
		else {
			HideTooltip();
		}
	}

	//Hide the tooltip
	public void HideTooltip() {
		showTooltip = false;
		foreach(Transform trans in transform) {
			trans.gameObject.SetActive(false);
		}
	}

	//Generate the tooltip for a given item
	public string GenerateTooltip(ItemClass item) {
		string generatedTooltipText = "";
		if(item.unidentified) {
			Color color = Color.red;
			generatedTooltipText = "<color=#" + ColorToHex(color) + ">Unidentified </color>";
			
			if(item.itemType == EquipmentSlotType.weapon) {
				
				generatedTooltipText += "<color=#"+ColorToHex(color)+ ">" + item.itemQuality.ToString() + " ";
				generatedTooltipText += item.weaponType.ToString()+"</color>\n\n";

				generatedTooltipText += item.weaponType.ToString()+"</color>\n\n";
				generatedTooltipText += "<size=30>" + Mathf.FloorToInt(((item.minDamage+item.maxDamage)/2)/item.attackSpeed).ToString() + "</size>\n";
				generatedTooltipText += "<color=grey>Damage per second</color>\n\n";
				generatedTooltipText += item.minDamage.ToString() + "-" + item.maxDamage.ToString() + " <color=grey>damage</color>\n";
				generatedTooltipText += item.attackSpeed.ToString() + " <color=grey> attacks per second</color>";
			}
			
			generatedTooltipText += "<size=10><color=#"+ColorToHex(color)+ ">This item cannot be equipped until it's identified.</color></size>\n\n";
			
			generatedTooltipText += "????\n";
			generatedTooltipText += "????\n";
			generatedTooltipText += "????\n";
			generatedTooltipText += "????";
		}
		else {
			Color color = FindColor(item);
			item.tooltipHeader = "<color=#" + ColorToHex(color) + ">" + item.itemName + "</color>";
			
			if(item.itemType == EquipmentSlotType.weapon) {
				if(item.twoHanded) {
					generatedTooltipText += "<color=grey>2-handed</color> <color=#"+ColorToHex(FindColor(item))+ ">" + item.itemQuality.ToString() + " ";
				}
				else {
					generatedTooltipText += "<color=grey>1-handed</color> <color=#"+ColorToHex(FindColor(item))+ ">" + item.itemQuality.ToString() + " ";
				}
				generatedTooltipText += item.weaponType.ToString()+"</color>\n\n";
				generatedTooltipText += "<size=30>" + Mathf.FloorToInt(((item.minDamage+item.maxDamage)/2)/item.attackSpeed).ToString() + "</size>\n";
				generatedTooltipText += "<color=grey>Damage per second</color>\n\n";
				generatedTooltipText += item.minDamage.ToString() + "-" + item.maxDamage.ToString() + " <color=grey>damage</color>\n";
				generatedTooltipText += item.attackSpeed.ToString() + " <color=grey> attacks per second</color>";

			}
			else if(item.itemType == EquipmentSlotType.consumable) {
				generatedTooltipText += item.consumableType.ToString()+"\n\n";
				if(item.consumableType == ConsumableType.potion) {
					generatedTooltipText += "Heals you instantly for: <color=green>" + item.healAmount.ToString() + "</color> life.\n\n";
					
					generatedTooltipText += "Cooldown: " + item.cooldown.ToString();
				}
				else if(item.consumableType == ConsumableType.Scroll) {
					generatedTooltipText += 	"<color=green>Right to use.</color>\n\n" +
						"Use on unidentified items to reveal their hidden powers.";
				}
			}
			else {
				generatedTooltipText += "<color=#"+ColorToHex(FindColor(item))+ ">" + item.itemQuality.ToString() + " ";
				generatedTooltipText += item.itemType.ToString()+"</color>";

				if(item.itemType != EquipmentSlotType.ring && item.itemType != EquipmentSlotType.amulet && item.itemType != EquipmentSlotType.reagent) {
					generatedTooltipText += "\n\n<size=30>" + item.armor.ToString() + "</size>\n";
					generatedTooltipText += "<color=grey>Armor</color>\n\n";
				}

				if(item.itemType == EquipmentSlotType.reagent) {
					generatedTooltipText += "<color=white>\n\nUsed by blacksmiths and others to craft powerfull items.</color>\n";
				}

				if(item.itemType == EquipmentSlotType.offHand) {
					generatedTooltipText += item.blockChance.ToString() + "% <color=grey>chance to block</color>\n";
					generatedTooltipText += item.minBlockAmount.ToString() + "-" + item.maxBlockAmount.ToString() + " <color=grey>block amount</color>";
				}

			}
		}

		if(item.itemQuality != ItemQuality.Normal && item.itemQuality != ItemQuality.Junk) {

			generatedTooltipText += "<color=#"+ColorToHex(attributeColor)+">\n\n";
			if(item.arcaneDamage > 0) {
				generatedTooltipText += "+" + item.arcaneDamage.ToString() + " arcane damage\n";
			}
			if(item.lightningDamage > 0) {
				generatedTooltipText += "+" + item.lightningDamage.ToString() + " lightning damage\n";
			}
			if(item.frostDamage > 0) {
				generatedTooltipText += "+" + item.frostDamage.ToString() + " frost damage\n";
			}
			if(item.holyDamage > 0) {
				generatedTooltipText += "+" + item.holyDamage.ToString() + " holy damage\n";
			}
			if(item.poisonDamage > 0) {
				generatedTooltipText += "+" + item.poisonDamage.ToString() + " poison damage\n";
			}
			if(item.dexterity > 0) {
				generatedTooltipText += "+" + item.dexterity.ToString() + " Dexterity\n";
			}
			if(item.strength > 0) {
				generatedTooltipText += "+" + item.strength.ToString() + " Strength\n";
			}
			if(item.vitality > 0) {
				generatedTooltipText += "+" + item.vitality.ToString() + " Vitality\n";
			}
			if(item.magic > 0) {
				generatedTooltipText += "+" + item.magic.ToString() + " Magic\n";
			}
			if(item.allResistance > 0) {
				generatedTooltipText += "+" + item.allResistance.ToString() + " Resistance to all elements\n";
			}
			if(item.arcaneResistance > 0) {
				generatedTooltipText += "+" + item.arcaneResistance.ToString() + " Resistance to arcane elements\n";
			}
			if(item.lightningResistance > 0) {
				generatedTooltipText += "+" + item.lightningResistance.ToString() + " Resistance to lightning elements\n";
			}
			if(item.frostResistance > 0) {
				generatedTooltipText += "+" + item.frostResistance.ToString() + " Resistance to frost elements\n";
			}
			if(item.holyResistance > 0) {
				generatedTooltipText += "+" + item.holyResistance.ToString() + " Resistance to holy elements\n";
			}
			if(item.poisonResistance > 0) {
				generatedTooltipText += "+" + item.poisonResistance.ToString() + " Resistance to poison elements\n";
			}
			if(item.attackSpeedModifier > 0) {
				generatedTooltipText += "Increase attack speed by " + item.attackSpeedModifier.ToString() + "%\n";
			}
			if(item.blockChance > 0) {
				generatedTooltipText += "Increase block change by " + item.blockChance.ToString() + "%\n"; 
			}
			if(item.lifeSteal > 0) {
				generatedTooltipText += "Converts " + item.lifeSteal.ToString() + "% damage into life on hit\n";
			}
			if(item.lifePercentage > 0) {
				generatedTooltipText += "+ " + item.lifePercentage.ToString() + "% life\n";
			}

			//Remove the last "\n"
			generatedTooltipText = generatedTooltipText.Remove(generatedTooltipText.Length - 1,1);

			generatedTooltipText += "</color>";
		}

		return generatedTooltipText;
	}

	//Find the color based on the quality of the item
	Color FindColor(ItemClass item) {
		uGUIInventory inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<uGUIInventory>();
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

	//Convert the color into hex decimal
	public static string ColorToHex(Color32 color)
	{
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour {

	public List<ItemClass> items;
	public List<CraftedItem> craftItems;

	//Add an item to the list of items
	public void AddItem(ItemClass item) {
		items.Add(item);
	}

	//Find an item based on ID
	public ItemClass FindItem(int id) {
		for(int i = 0; i < items.Count; i++) {
			if(int.Parse(items[i].ID) == id) {
				return GenerateItem(items[i]);
			}
		}
		return new ItemClass();
	}

	//Add a crafting item to the list
	public void AddCraftItem(CraftedItem item) {
		craftItems.Add(item);
	}

	//Find a crafting item based on ID
	public CraftedItem FindCraftItem(int id) {
		for(int i = 0; i < craftItems.Count; i++) {
			if(int.Parse(craftItems[i].ID) == id) {
				return craftItems[i];
			}
		}
		return new CraftedItem();
	}

	public ItemClass GenerateItem(ItemClass item) {
		//Create a copy of the item

		item = uGUIInventory.DeepCopy(item);

		//Stats
		item.strength = Random.Range(item.minStrength, item.maxStrength);
		item.dexterity = Random.Range(item.minDexterity, item.maxDexterity);
		item.vitality = Random.Range(item.minVitality, item.maxVitality);
		item.magic = Random.Range(item.minMagic, item.maxMagic);
		//Resistances
		item.allResistance = Random.Range(item.minAllResistance, item.maxAllResistance);
		item.arcaneResistance = Random.Range(item.minArcaneResistance, item.maxArcaneResistance);
		item.lightningResistance = Random.Range(item.minLightningResistance, item.maxLightningResistance);
		item.frostResistance = Random.Range(item.minFrostResistance, item.maxFrostResistance);
		item.holyResistance = Random.Range(item.minHolyResistance, item.maxHolyResistance);
		item.poisonResistance = Random.Range(item.minPoisonResistance, item.maxPoisonResistance);
		//damages
		item.arcaneDamage = Random.Range(item.minArcaneDamage, item.maxArcaneDamage);
		item.lightningDamage = Random.Range(item.minLightningDamage, item.maxLightningDamage);
		item.frostDamage = Random.Range(item.minFrostDamage, item.maxFrostDamage);
		item.holyDamage = Random.Range(item.minHolyDamage, item.maxHolyDamage);
		item.poisonDamage = Random.Range(item.minPoisonDamage, item.maxPoisonDamage);
		//Block
		item.blockChance = Random.Range(item.minBlockChance, item.maxBlockChance);
		item.minBlockAmount = Random.Range(item.minMinBlockAmount, item.minMaxBlockAmount);
		item.maxBlockAmount = Random.Range(item.maxMinBlockAmount, item.maxMaxBlockAmount);
		//Cricitcal
		item.criticalHitChancePercentage = Random.Range(item.minCritical, item.maxCritical);
		item.criticalHitDamagePercentage = Random.Range(item.minCriticalDamage, item.maxCriticalDamage);
		//Life steal
		item.lifeSteal = (float)System.Math.Round(Random.Range(item.minBaseLifeSteal, item.maxBaseLifeSteal),1);
		//Life modifier
		item.lifePercentage = Random.Range(item.minLifePercentage, item.maxLifePercentage);
		//Attack speed
		item.attackSpeedModifier = Random.Range(item.minAttackSpeed, item.maxAttackSpeed);
		//Armor
		item.armor = Random.Range(item.minBaseArmor, item.maxBaseArmor);
		//damage
		item.minDamage = Random.Range(item.minMinDamage, item.maxMinDamage);
		item.maxDamage = Random.Range(item.minMaxDamage, item.maxMaxDamage);
		return item;
	}
}

using UnityEngine;
using System.Collections;

[System.Serializable]
public class ItemClass {
	
	public int level;
	public int levelReq;
	
	public string ID;
	public int baseID;
	public int height;
	public int width;

	public int sellPrice;
	public int buyPrice;
	
	public int curDurability;
	public int maxDurability;

	public int totalRepairCost;

	public ItemQuality itemQuality;

	public AudioClip itemSound;

	#region useables

	public bool stackable;
	public int stackSize;
	public int maxStackSize;
	public int healAmount;
	public int manaAmount;
	public bool consumable;
	public AudioClip useSound;
	public string useEffectScriptName;

	#endregion

	public string tooltip;
	public string tooltipHeader;

	public string descriptionText;
	
	public GameObject worldObject;

	public int startSlot;

	public bool twoHanded;

	public string itemName;
	
	public EquipmentSlotType itemType;

	public WeaponType weaponType;

	public ConsumableType consumableType;

	public int cooldown;

	public string iconName;

	public Sprite icon;

	public int armor;

	public float attackSpeed;

	public float dps;

	public int minBaseDamage;
	public int maxBaseDamage;

	public float lifeSteal;

	public bool unidentified;

	public int minMinBlockAmount;
	public int minMaxBlockAmount;
	public int maxMinBlockAmount;
	public int maxMaxBlockAmount;

	#region base elemental damages

	public int lightningDamage;
	public int frostDamage;
	public int arcaneDamage;
	public int poisonDamage;
	public int holyDamage;

	#endregion

	#region base resistances

	public int lightningResistance;
	public int frostResistance;
	public int arcaneResistance;
	public int poisonResistance;
	public int holyResistance;
	public int allResistance;

	#endregion

	#region base stats

	public int strength;
	public int dexterity;
	public int vitality;
	public int magic;

	#endregion

	#region base damage modifiers

	public int attackSpeedModifier;
	public int criticalHitChancePercentage;
	public int criticalHitDamagePercentage;

	#endregion

	public int lifePercentage;

	public int damagePercentage;

	public int blockChance;

	//Those values are used for generating items and should not be used for calculations

	public int minDamage;
	public int minMinDamage;
	public int maxMinDamage;
	public int maxDamage;
	public int minMaxDamage;
	public int maxMaxDamage;

	public int minDamagePercentage;
	public int maxDamagePercentage;

	public int minLifePercentage;
	public int maxLifePercentage;

	#region stats
	
	public int minStrength;
	public int maxStrength;
	public int minDexterity;
	public int maxDexterity;
	public int minVitality;
	public int maxVitality;
	public int minMagic;
	public int maxMagic;
	
	#endregion

	#region damage modifiers
	
	public int minAttackSpeed;
	public int maxAttackSpeed;
	
	public int minCritical;
	public int maxCritical;
	
	public int minCriticalDamage;
	public int maxCriticalDamage;
	
	#endregion

	#region resistances
	
	public int minLightningResistance;
	public int maxLightningResistance;
	public int minFrostResistance;
	public int maxFrostResistance;
	public int minArcaneResistance;
	public int maxArcaneResistance;
	public int minPoisonResistance;
	public int maxPoisonResistance;
	public int minHolyResistance;
	public int maxHolyResistance;
	public int minAllResistance;
	public int maxAllResistance;
	
	#endregion

	#region elemental damages
	
	public int minLightningDamage;
	public int maxLightningDamage;
	public int minFrostDamage;
	public int maxFrostDamage;
	public int minArcaneDamage;
	public int maxArcaneDamage;
	public int minPoisonDamage;
	public int maxPoisonDamage;
	public int minHolyDamage;
	public int maxHolyDamage;
	
	#endregion

	public int minBaseArmor;
	public int maxBaseArmor;

	public float minBaseLifeSteal;
	public float maxBaseLifeSteal; 

	public int minBlockChance;
	public int maxBlockChance;

	public int minBlockAmount;
	public int maxBlockAmount;

	public int minAmountOfRandomMagicProperties;
	public int maxAmountOfRandomMagicProperties;

}

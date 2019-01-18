using UnityEngine;
using System.Collections;

//Derives from the UseEffect script
public class HealthPotion : UseEffect {

	//Overried the Use function of the UseEffect script
	public override string Use() {
		//Run the start function of the UseEffect script the get the needed references
		base.Start();
		//If the player is already at max health return a warning and don't consume the potion
		if(player.curHealth >= player.maxHealth) {
			return "Already at maximum health";
		}
		//The player isn't at full health
		else {
			//Add the health to the player
			player.curHealth += item.healAmount;
			//If the health of the player is above maximum
			if(player.curHealth > player.maxHealth) {
				//Set the health of the player to maximum
				player.curHealth = player.maxHealth;
			}
			//Return string to tell the inventory to remove one from the stack of the item
			return "Potion";
		}
	}
}

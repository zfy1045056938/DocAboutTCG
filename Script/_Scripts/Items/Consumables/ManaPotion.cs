using UnityEngine;
using System.Collections;

public class ManaPotion : UseEffect {

	public override string Use() {
		base.Start();

		if(player.curMana >= player.maxMana) {
			return "Already at maximum mana";
		}
		else {
			player.curMana += item.manaAmount;

			if(player.curMana > player.maxMana) {
				player.curMana = player.maxMana;
			}

			return "Potion";
		}
	}
}

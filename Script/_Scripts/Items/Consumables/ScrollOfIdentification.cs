using UnityEngine;
using System.Collections;

//Derives from the UseEffect script
public class ScrollOfIdentification : UseEffect {

	public override string Use() {
		//Run the Start function of the UseEffect script to get the needed references
		base.Start();

		//Tell the inventory to start identifying
		inventory.startIdentifying = true;

		//Return string to tell the inventory to store the original slot identifying began so the stacksize can be decreased later
		return "Identify";
	}
}

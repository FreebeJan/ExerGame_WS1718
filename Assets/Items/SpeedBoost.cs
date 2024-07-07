using UnityEngine;

public class SpeedBoost : BaseItem
{
	public float Multiplier = 1.1f;
	
	public override void applyEffect(GameObject player)
	{
		base.applyEffect(player);
		
		var comp = player.GetComponent<BikeBase>();
		if(comp == null)
		{
			return;
		}
		
		comp.MaxSpeed = comp.MaxSpeed * Multiplier;
	}
	
	public override void revertEffect(GameObject player)
	{
		base.revertEffect(player);
		
		var comp = player.GetComponent<BikeBase>();
		if(comp == null)
		{
			return;
		}
		
		comp.MaxSpeed = comp.MaxSpeed / Multiplier;
	}
}

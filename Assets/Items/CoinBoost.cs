using UnityEngine;

public class CoinBoost : BaseItem
{
	public float Multiplier = 1.1f;
	
	public override void applyEffect(GameObject player)
	{
		base.applyEffect(player);
		
		var comp = player.GetComponent<ScoreManager>();
		if(comp == null)
		{
			return;
		}
		
		comp.Multiplier = comp.Multiplier + Multiplier;
	}
	
	public override void revertEffect(GameObject player)
	{
		base.revertEffect(player);
		
		var comp = player.GetComponent<ScoreManager>();
		if(comp == null)
		{
			return;
		}
		
		comp.Multiplier = comp.Multiplier - Multiplier;
	}
}

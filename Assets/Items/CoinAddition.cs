using UnityEngine;

public class CoinAddition : BaseItem
{
	public float Amount = 1.1f;
	
	public override void applyEffect(GameObject player)
	{
		base.applyEffect(player);
		
		var comp = player.GetComponent<ScoreManager>();
		if(comp == null)
		{
			return;
		}
		
		comp.Score = (int) (comp.Score + Amount);
	}
	
	// NEVER REVERT! HAHAHA!
	public override void revertEffect(GameObject player)
	{
		
	}
}

using UnityEngine;
using System.Collections.Generic;

/**
 * @brief Handles all effects caused by collecting items.
 *
 * This behavior should be attached to the player. It will then
 * detect collisions with item boxes and apply their effects according to their type.
 */
public class ItemManager : MonoBehaviour
{
	private List<GameObject> CurrentlyActive = new List<GameObject>();
	void OnTriggerEnter(Collider trigger)
	{
		var baseItem = trigger.gameObject.GetComponent<BaseItem>();
		if(trigger.tag == "Item" && baseItem.visibleMesh.activeSelf)
		{
			baseItem.visibleMesh.SetActive(false);
			baseItem.applyEffect(gameObject);
			CurrentlyActive.Add(trigger.gameObject);
		}
	}
	
	void Update()
	{
		for(int i = 0; i < CurrentlyActive.Count; i++)
		{
			var item = CurrentlyActive[i].GetComponent<BaseItem>();
			if(item.isDone())
			{
				item.revertEffect(gameObject);
				CurrentlyActive.Remove(item.gameObject);
				
				item.visibleMesh.SetActive(true);
				//Destroy(item.gameObject);
			}
		}
	}
}


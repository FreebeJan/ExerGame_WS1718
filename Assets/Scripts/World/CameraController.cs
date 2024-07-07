using UnityEngine;

public class CameraController : MonoBehaviour
{

	public GameObject player;
	private Vector3 offset = new Vector3(0, 5, -7);
	// Use this for initialization
	void Start()
	{
		transform.position = player.transform.position + offset;
		transform.rotation = Quaternion.Euler(25, 0, 0);
	}

	// Update is called once per frame
	void Update()
	{
		transform.position = player.transform.position + offset;
	}
}

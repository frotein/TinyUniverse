using UnityEngine;
using System.Collections;

public class SnapPoint : MonoBehaviour {

	Transform connectedSnap;
	SpriteRenderer sprite; 
	ShipBuilder builder;
	ShipPart part;
	bool connected;
	// Use this for initialization
	void Start () 
	{
		sprite = transform.GetComponent<SpriteRenderer>();
		builder = Camera.main.GetComponent<ShipBuilder>();
		part = transform.parent.GetComponent<ShipPart>();
	}
	
	// Toggle whether sprite is displaying or not
	public void ToggleSprite(bool on)
	{
		if(sprite == null)
		sprite = transform.GetComponent<SpriteRenderer>();

		if(connected)
		sprite.enabled = false;
		else
		sprite.enabled = on;
	}

	public void OnTriggerEnter2D(Collider2D col)
	{
		
		if(!part.OnShip() && !part.snapped && col.transform.GetComponent<SnapPoint>() != null)
		{
			connectedSnap = col.transform;
			builder.ConnectPoints(connectedSnap, transform);	
			part.snapped = true;
		}
	}

	public void OnTriggerExit2D(Collider2D col)
	{
		//Debug.Log("exiting");
	}

	public Transform ConnectedSnap() { return connectedSnap; }

}

using UnityEngine;
using System.Collections;

public class SnapPoint : MonoBehaviour {

	Transform connectedSnap;
	SpriteRenderer sprite; 
	ShipBuilder builder;
	ShipPart part;
	public bool connected;
	public Transform corner1, corner2;
	// Use this for initialization
	void Start () 
	{
		builder = Camera.main.GetComponent<ShipBuilder>();
		part = transform.parent.GetComponent<ShipPart>();
		connected = false;
	}
	
	public void OnTriggerEnter2D(Collider2D col)
	{
		if(!part.OnShip() && !part.snapped && col.transform.GetComponent<SnapPoint>() != null && !connected)
		{
			if(!col.transform.GetComponent<SnapPoint>().connected)
			{
				connectedSnap = col.transform;
				builder.ConnectPoints(connectedSnap, transform);	
				part.snapped = true;
			}
		}
	}

	public void OnTriggerExit2D(Collider2D col)
	{
		//Debug.Log("exiting");
	}

	public Transform ConnectedSnap() { return connectedSnap; }

}

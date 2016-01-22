using UnityEngine;
using System.Collections;

public class SnapPoint : MonoBehaviour {

	Transform connectedSnap;
	SpriteRenderer sprite; 
	ShipBuilder builder;
	ShipPart part;
	public bool connected;
	public bool connectedToParent;
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
				builder.SnapPoints(connectedSnap, transform);	
				part.snapped = true;
			}
		}
	}
	public void Disconnect()
	{
		Debug.Log("Disconnecting " +  gameObject.name + " in " + gameObject.transform.parent.gameObject.name);
		connected = false;
		connectedToParent = false;

	}
	public void OnTriggerExit2D(Collider2D col)
	{
		//Debug.Log("exiting");
	}

	public Transform ConnectedSnap() { return connectedSnap; }

}

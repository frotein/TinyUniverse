using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipPart : MonoBehaviour {

	public bool startingPart;
	public int length;
	public int scale = 1;
	public bool snapped;
	bool onShip;
	Collider2D collider;
	bool overlapping;
	// Use this for initialization
	void Start () 
	{
		snapped = false;
		overlapping = false;
		onShip = startingPart;
		collider = transform.GetComponent<Collider2D>();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public Collider2D[] OverlappingColliders()
	{
		return null;
	}

	float VectorToAngle(Vector2 vector)
	{
   		return (float)Mathf.Atan2(vector.x, vector.y);
	}

	public bool OnShip() { return onShip; }


	public bool Overlapping() { return overlapping; }
	
	public void OnCollisionStay2D(Collision2D col)
	{
		overlapping = true;
	}
	public void OnCollisionExit2D(Collision2D col)
	{
		overlapping = false;
	}

}

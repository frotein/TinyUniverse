using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipPart : MonoBehaviour {

	public bool startingPart;
	public GameObject snapPointPrefab;
	public int length;
	public int scale = 1;
	public bool snapped;
	public Transform snappedTo;
	bool onShip;
	Collider2D collider;
	bool overlapping;
	List<Transform> corners;
	// Use this for initialization
	void Start () 
	{
		snapped = false;
		overlapping = false;
		onShip = startingPart;
		collider = transform.GetComponent<Collider2D>();
		BoxCollider2D box = transform.GetComponent<BoxCollider2D>();
		corners = new List<Transform>();
		if(box != null)
		{
			Vector2 wp = (Vector2)transform.position;
			wp += box.offset;
			float x = box.size.x / 2f;
			float y = box.size.y / 2f;
			
			for(int i = 0; i < 4; i++)
			{
				GameObject corner = new GameObject("Corner" + i);
				
				if(i == 0)
				{
					corner.transform.position = new Vector3(wp.x - x, wp.y - y,transform.position.z); 
				}

				if(i == 1)
				{
					corner.transform.position = new Vector3(wp.x + x, wp.y - y,transform.position.z); 
				}

				if(i == 2)
				{
					corner.transform.position = new Vector3(wp.x + x, wp.y + y,transform.position.z); 
				}

				if(i == 3)
				{
					corner.transform.position = new Vector3(wp.x - x, wp.y + y,transform.position.z); 
				}
				corner.transform.parent = transform;
				corners.Add(corner.transform);
			}				
		}
		else
		{
			PolygonCollider2D poly = transform.GetComponent<PolygonCollider2D>();
			if(poly != null)
			{
				int i = 0;
				foreach(Vector2 point in poly.points)
				{
					GameObject corner = new GameObject("Corner " + i);
					corner.transform.position = transform.position + new Vector3(point.x, point.y,0); 
					corner.transform.parent = transform;
					corners.Add(corner.transform);
					i++;
				}
			}
		}

		int j = 0;
		foreach(Transform corner in corners)
		{
			Transform nextCorner = corners[(j + 1) % corners.Count];
			float lnth = Vector2.Distance((Vector2) corner.position, (Vector2) nextCorner.position);
			//Debug.Log(lnth);
			if(lnth < 1.01f && lnth > 0.99f)
			{
				Vector2 midPoint = (((Vector2) corner.position) - ((Vector2) nextCorner.position)) *.5f + ((Vector2) nextCorner.position);
				GameObject snapPoint = GameObject.Instantiate(snapPointPrefab);
				snapPoint.transform.position = new Vector3(midPoint.x, midPoint.y, transform.position.z);
				snapPoint.GetComponent<SnapPoint>().corner1 = corner;
				snapPoint.GetComponent<SnapPoint>().corner2 = nextCorner;
				snapPoint.transform.parent = transform;
			}

			  
			j++;
		}
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

	public void SetOnShip(bool os) { onShip = os; }


}

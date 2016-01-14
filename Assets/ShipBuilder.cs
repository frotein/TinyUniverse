using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipBuilder : MonoBehaviour {

	public GameObject ship;
	GameObject grabbedObject;
	Vector3 mWorldPos; // Mouse world position;
	Vector3 prevMWorldPos; // the mouse world position from the previouse frame;
	// Use this for initialization
	bool visibleSnap;
	Vector2 snappedPosition;
	Vector2 snapSpotOffset;
	Vector2 grabOffset;
	public float snapRadius;

	void Start () 
	{
		visibleSnap = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Mouse world position assigning per frame
		mWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if(grabbedObject != null) // if we have a grabbed object
		{
			grabbedObject.transform.position = new Vector3(mWorldPos.x, mWorldPos.y, grabbedObject.transform.position.z) + new Vector3(grabOffset.x, grabOffset.y,0);
			if(grabbedObject.GetComponent<ShipPart>().snapped) // if the object has been snapped into place keep it there;
			{
				grabbedObject.transform.localPosition = new Vector3(snappedPosition.x,snappedPosition.y, grabbedObject.transform.localPosition.z);		
				
				Vector3 worldPos = grabbedObject.transform.position;
				float dist = Vector2.Distance(((Vector2) worldPos)  , (Vector2) mWorldPos);
			
				if(dist > snapRadius * 6)// if mouse gets far enough away, unsnap
				{
					grabbedObject.GetComponent<ShipPart>().snapped = false;	
					grabbedObject.transform.parent = null;	
				} 
			}


		}





		

	}

	bool GrabbedOverlappingSnapPoint()
	{
		return false;	
	}

	// Toggles the visiblity for the Snap points on the ship based on the bool input
	public void ToggleSnapPointsVisiblility(bool on)
	{
		visibleSnap = !visibleSnap;
		List<Transform> snapPoints = SnapPoints();
		foreach(Transform point in snapPoints)
		{
			point.GetComponent<SnapPoint>().ToggleSprite(on);
		}
	}
	// Get all the snap points connected to the ship
	List<Transform> SnapPoints()
	{
		List<Transform> final = new List<Transform>();
		SnapPoint[] points = ship.GetComponentsInChildren<SnapPoint>();
		foreach(SnapPoint point in points)
		{
			if(point.transform.tag == "Snap Point")
				final.Add(point.transform);
		}	
		return final;
	}

	// Assign the currently grabbed object
	public void AssignGrabbed(GameObject go, bool fromButton)
	{
		grabbedObject = go;
				
		if(fromButton)
			grabbedObject.transform.position = new Vector3(mWorldPos.x, mWorldPos.y, grabbedObject.transform.position.z);

		grabOffset = (Vector2)(mWorldPos - grabbedObject.transform.position);

		foreach(Transform child in grabbedObject.transform)
		{
			SnapPoint sp = child.GetComponent<SnapPoint>();
			if(sp !=  null)
			{
				sp.ToggleSprite(true);
			}
		}

		ToggleSnapPointsVisiblility(true);
	}

	public void ConnectPoints(Transform body, Transform piece)
	{
		float bodyAngle = body.eulerAngles.z;
		float pieceAngle = piece.eulerAngles.z;
		float maxAng = Mathf.Max(body.eulerAngles.z, piece.eulerAngles.z);
		float minAng = Mathf.Min(body.eulerAngles.z, piece.eulerAngles.z);
		float combinedAngles = pieceAngle - bodyAngle;
	
		float dist = ((Vector2)piece.localPosition).magnitude;
		//Vector3 diff = body.position - piece.position;
		Vector2 vec = (Angle2Vector(combinedAngles) * dist);
		Debug.Log(bodyAngle - pieceAngle);
		piece.parent.position =  body.position + new Vector3(vec.x, vec.y,0); 
		piece.parent.GetComponent<ShipPart>().snapped = true;
		piece.parent.parent = ship.transform;
		snappedPosition = (Vector2) piece.parent.localPosition;
	}

	public Vector2 Angle2Vector(float ang)
	{
		return new Vector2(
    (float)Mathf.Cos(ang * Mathf.Deg2Rad),
    -(float)Mathf.Sin(ang * Mathf.Deg2Rad)).normalized;
	}
}

﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShipBuilder : MonoBehaviour {

	public GameObject ship;
	public List<Button> assignButtons;
	public bool placedThisFrame, grabbedThisFrame;
	public GameObject grabbedObject;
	Vector3 mWorldPos; // Mouse world position;
	Vector3 prevMWorldPos; // the mouse world position from the previouse frame;
	// Use this for initialization
	bool visibleSnap;
	float storedAngle;
	Vector2 snappedPosition;
	Vector2 snapSpotOffset;
	Vector2 grabOffset;
	List<GameObject> floatingObjects;
	SnapPoint connectedSP1, connectedSP2;
	public float snapRadius;
	public bool assigningActions;
	CustomAction assigningAction;
	int assigningActionNum;
	
	void Start () 
	{
		visibleSnap = false;
		floatingObjects = new List<GameObject>();
	}
	public void ToggleAssigingAction()
	{
		assigningActions = !assigningActions;
	}
	// Update is called once per frame
	void Update () 
	{
		if(!assigningActions)
		{
			placedThisFrame = false;
			// Mouse world position assigning per frame
			mWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			if(grabbedObject != null) // if we have a grabbed object
			{
				if(!grabbedObject.GetComponent<ShipPart>().snapped)
				{
					grabbedObject.transform.position = new Vector3(mWorldPos.x, mWorldPos.y, grabbedObject.transform.position.z) + new Vector3(grabOffset.x, grabOffset.y,0);
					
					// if you click when an object is not snapped to a surface, leave it floating their
					if(Input.GetMouseButtonDown(0) && !grabbedThisFrame) 
					{
						if(!grabbedObject.GetComponent<ShipPart>().startingPart)
						{
							grabbedObject.transform.parent = Camera.main.transform;
							floatingObjects.Add(grabbedObject);
						}

						grabbedObject = null;
						placedThisFrame = true;
					} 
				}
				else // if the object has been snapped into place keep it there;
				{
					
					Vector3 worldPos = grabbedObject.transform.position;
					float dist = Vector2.Distance(((Vector2) worldPos)  , (Vector2) mWorldPos);
					
					if(dist > snapRadius * 6)// if mouse gets far enough away, unsnap
					{
						grabbedObject.GetComponent<ShipPart>().snapped = false;	
						grabbedObject.transform.eulerAngles = new Vector3(grabbedObject.transform.eulerAngles.x,
																		  grabbedObject.transform.eulerAngles.y, storedAngle); 
						grabbedObject.transform.parent = null;	
						//grabbedObject.GetComponent<ShipPart>().Disconnect();
					}

					if(Input.GetMouseButtonDown(0))
					{
						if(grabbedObject.GetComponent<ShipPart>().snapped) // if the piece is snapped and you click again, connect the points
						{
							grabbedObject.GetComponent<ShipPart>().SetOnShip(true);
							connectedSP1.connected = true;
							connectedSP2.connected = true;
							connectedSP2.connectedToParent = true;
							placedThisFrame = true;
							grabbedObject = null;
						}
						
					}			
				}

				if(Input.GetMouseButtonDown(1) && !grabbedObject.GetComponent<ShipPart>().snapped)
				{
					grabbedObject.transform.eulerAngles -= new Vector3(0,0,90);
				}
			}
			
			grabbedThisFrame = false;
		}
		else
		{
			if(assigningAction != null && assigningActionNum != 0)
			{
				if(detectPressedKeyOrButton() != KeyCode.Delete)
				{
					assigningAction.AssignAction(assigningActionNum, detectPressedKeyOrButton());
					assigningAction = null;
					assigningActionNum = 0;
				}
			}
		}
	}

	public KeyCode detectPressedKeyOrButton()
 	{
	     foreach(KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
	     {
	         if (Input.GetKeyDown(kcode))
	             return kcode;
	     }
	     return KeyCode.Delete;
    }
	// Toggles the visiblity for the Snap points on the ship based on the bool input
	public void ToggleSnapPointsVisiblility(bool on)
	{
		
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
		grabbedThisFrame = true;
		if(fromButton)
			grabbedObject.transform.position = new Vector3(mWorldPos.x, mWorldPos.y, grabbedObject.transform.position.z);

		grabOffset = (Vector2)(mWorldPos - grabbedObject.transform.position);

		ToggleSnapPointsVisiblility(true);
	}

	public void SnapPoints(Transform body, Transform piece)
	{
		SnapPoint bodySP = body.GetComponent<SnapPoint>();
		SnapPoint pieceSP = piece.GetComponent<SnapPoint>();
		storedAngle = piece.parent.eulerAngles.z;
		connectedSP1 = bodySP;
		connectedSP2 = pieceSP;
		Vector2 offset = ((Vector2) pieceSP.corner1.position) - ((Vector2) bodySP.corner2.position);
		piece.parent.position-= new Vector3(offset.x, offset.y,0);
		
		float angle = AngleBetween3Points((Vector2)pieceSP.corner2.position,(Vector2) bodySP.corner2.position, (Vector2) bodySP.corner1.position);
		//Debug.Log(angle);
		float storedAng = piece.eulerAngles.z;
		Transform pieceP = piece.parent;
		piece.parent = null;
		pieceP.parent = piece;


		
		
		piece.eulerAngles += new Vector3(0,0,-angle);
		
		pieceP.parent = null;
		piece.parent = pieceP;
		piece.eulerAngles = new Vector3(0,0,storedAng);

		offset = ((Vector2) pieceSP.corner1.position) - ((Vector2) bodySP.corner2.position);
		piece.parent.position-= new Vector3(offset.x, offset.y,0);
		piece.parent.parent = body.parent;
	}


	public void StartAssigningAction(CustomAction action)
	{
		assigningAction = action;	
	}
	public void StartAssigningActionNum( int actionNum)
	{
		assigningActionNum = actionNum;
	}
	public Vector2 Angle2Vector(float ang)
	{
		return new Vector2(
    (float)Mathf.Cos(ang * Mathf.Deg2Rad),
    -(float)Mathf.Sin(ang * Mathf.Deg2Rad)).normalized;
	}
	// gets the angle between the 3 points where point 2 is the Middle
	public float AngleBetween3Points(Vector2 p1, Vector2 p2, Vector2 p3) 
	{
		float Xc = p2.x;
		float Yc = p2.y;
		float Xa = p1.x;
		float Ya = p1.y;
		float Xb = p3.x;
		float Yb = p3.y;

		float v1x = Xb - Xc;
		float v1y = Yb - Yc;
		float v2x = Xa - Xc;
		float v2y = Ya - Yc;

		return (Mathf.Atan2(v1x, v1y) - Mathf.Atan2(v2x, v2y)) * Mathf.Rad2Deg;
	}

	public bool hasGrabbed()
	{
		return grabbedObject != null;
	}
}

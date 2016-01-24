using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomAction : MonoBehaviour {

	public Dictionary<KeyCode, int> assignedActions; 
	public ShipBuilder builder;
	Collider2D myCollider;
	// Use this for initialization
	public virtual void Start () 
	{
		builder = Camera.main.GetComponent<ShipBuilder>();
		myCollider = transform.GetComponent<Collider2D>();
	}
	
	// Update is called once per frame
	public virtual void Update () 
	{
		if(Input.GetMouseButtonDown(0) && builder.assigningActions)
		{
			Vector3 worldP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			if(myCollider.OverlapPoint((Vector2) worldP))
			{
				
				AssignButtons();
			}
		}
	}
	public void AssignAction(int actionNum, KeyCode key)
	{
		if(assignedActions.ContainsKey(key))
		assignedActions[key] = actionNum;
		else
		assignedActions.Add(key, actionNum);
	}	

	public virtual void  AssignButtons(){}
	
	public virtual void PreformAction(int actionNum) {}
	public virtual void Initialize() {}
}

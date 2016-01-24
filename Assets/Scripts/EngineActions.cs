using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EngineActions : CustomAction {

	Spaceship ship;
	int assigningAction;
	
	// Use this for initialization
	void Start () 
	{
		base.Start();
		assignedActions = new Dictionary<KeyCode, int>();
		assigningAction = 0;
	}
	
	public override void Initialize()
	{
		Transform parent = transform.parent;
		while(parent.parent != null)
		{
			parent = parent.parent;
		}
		ship = parent.GetComponentInChildren<Spaceship>();
	}
	// Update is called once per frame
	void Update () 
	{
		base.Update();
	}
	/* Engine Actions by number
		1 throttle up
		2 throttle down
		3 max throttle / turn on max
		4 min throttle / turn off to min
		5 toggle on / off	*/
	public override void PreformAction( int actionNumber)
	{
		switch(actionNumber)
		{
			case 1:
				Debug.Log("Throttle Up");
				break;
		}
	}

	public override	void AssignButtons()
	{
		int i = 0;
		foreach(Button b in builder.assignButtons)
		{
			switch(i)
			{
				case 0 : 
					b.transform.GetChild(0).GetComponent<Text>().text = "Throttle Up";
					b.onClick.AddListener(() => builder.StartAssigningAction(this));
					break;
			}
			i++;
		}
	}

	public void StartAssigningAction(int actionNum)
	{
		assigningAction = actionNum;
	}
}

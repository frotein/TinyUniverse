﻿using UnityEngine;
using System.Collections;

public class Part_Button : MonoBehaviour {

	public string myName; 
	public GameObject part;
	public bool onShip;
	ShipBuilder builder;
	// Use this for initialization
	void Start () 
	{
		builder = Camera.main.GetComponent<ShipBuilder>();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void PressedButton()
	{
		
		GameObject spawnedObject = GameObject.Instantiate(part);
		spawnedObject.GetComponent<ShipPart>().builder = builder;
		builder.AssignGrabbed(spawnedObject, true);
	}
}

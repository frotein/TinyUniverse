using System.Collections;
using System.Collections.Generic;
using SpaceGravity2D;
using UnityEngine;

public class DisplayMovePath : MonoBehaviour {

	public CelestialBody tracking;
	public float time, perFrame;
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		SimulationControl.instance.DisplayPathOverTime(time, perFrame, tracking, transform.GetComponent<LineRenderer>());
		//SimulationControl.instance.DisplayPath(time, tracking, transform.GetComponent<LineRenderer>());
	}
}

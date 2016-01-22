using UnityEngine;
using System.Collections;
using  SpaceGravity2D;

public class Spaceship : MonoBehaviour {

	public CelestialBody startingPlanet;
	CelestialBody cBody;
	Rigidbody2D body;
	// Starts when turned on
	void Start () 
	{
		Initialize(5);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Initialize(float mass)
	{
		body = gameObject.AddComponent<Rigidbody2D>();
		body.mass = mass;
		cBody = gameObject.AddComponent<CelestialBody>();
		cBody.IsDrawOrbit = false;
		cBody.Attractor = startingPlanet;
		cBody.RelativeVelocity= startingPlanet.RelativeVelocity;

	}
}

using UnityEngine;
using System.Collections;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
public class OrbitAround : MonoBehaviour {

	public GameObject[] planets;
	public float forceScale;
	Body[] planetBodies;
	FSWorldComponent world;
	float pullScale = 1;
	float myMass;
	// Use this for initialization
	void Start () 
	{
		world = GetComponent<FSWorldComponent>();
		myMass = transform.GetComponent<FSBodyComponent>().PhysicsBody.Mass;
		planetBodies = new Body[planets.Length];
		int i = 0;
		foreach(GameObject planet in planets)
		{
			Vector2 initForce = planet.GetComponent<PlanetController>().calcInitForce(transform);
			Body planetBody = planet.GetComponent<FSBodyComponent>().PhysicsBody;
			
			initForce = initForce * forceScale;
			planetBody.ApplyForce(new FVector2(initForce.x, initForce.y));
			planetBodies[i] = planetBody;  
			i++;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		

	}

	public void ApplyForce()
	{
		foreach(GameObject planet in planets)
		{
			Body body = planet.GetComponent<FSBodyComponent>().PhysicsBody;			
			float planetMass = body.Mass;
			float dist = Vector2.Distance(transform.position, planet.transform.position);
			
			float pull = pullScale *  ((myMass * planetMass) / (dist * dist));
			pull *= Time.fixedDeltaTime; 
			Vector2 dir = (((Vector2) transform.position) - ((Vector2)planet.transform.position)).normalized; 
			
			body.ApplyForce(new FVector2(dir.x * pull,dir.y * pull) );
			
		}
	}
}

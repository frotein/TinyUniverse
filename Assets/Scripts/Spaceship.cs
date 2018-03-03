using UnityEngine;
using System.Collections;
using  SpaceGravity2D;

public class Spaceship : MonoBehaviour {

	Camera shipCam;
	public CelestialBody startingPlanet;
	CelestialBody cBody;
	Rigidbody2D body;
	public CustomAction[] actions;
	public float turningForce;
	bool onPlanet;
	// Starts when turned on
	void Start () 
	{
		Initialize(5);
		actions = transform.GetComponentsInChildren<CustomAction>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		if(shipCam != null)
		{
			shipCam.transform.position = new Vector3(transform.position.x, transform.position.y, shipCam.transform.position.z);
		}

		Controls();
		//UpdateGravity();
	}

	/*public void UpdateGravity()
	{
		if (!onPlanet)
		{
			float dist = Vector2.Distance(transform.position, startingPlanet.transform.position);
			float force = (startingPlanet.GetComponent<Rigidbody2D>().mass) / (dist * dist);
			Vector2 dir = startingPlanet.transform.position - transform.position;
			body.AddForce(dir * force);
		}
	}*/
	public void Controls()
	{
		// Temporary hard coded controls
		if(Input.GetAxis("Horizontal") != 0)
		{
			body.AddTorque(Input.GetAxis("Horizontal") * -Time.deltaTime * turningForce);
		}

		foreach(CustomAction action in actions)
		{

			foreach(KeyCode key in action.assignedActions.Keys)
			{
				if(Input.GetKeyDown(key))
				{
					Debug.Log("went");
					action.PreformAction(action.assignedActions[key]);
				}
			}
		}

	}

	public void Initialize(float mass)
	{
		transform.parent = null;
		body = gameObject.AddComponent<Rigidbody2D>();
		body.mass = mass;
		body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		cBody = gameObject.AddComponent<CelestialBody>();
		cBody.IsDrawOrbit = false;
		cBody.Attractor = startingPlanet;
		cBody.RelativeVelocity= startingPlanet.RelativeVelocity;
		cBody.personalTimeScale = 10f;
		shipCam = Camera.main;
		//shipCam.transform.parent = transform;
		//shipCam.transform.localPosition = new Vector3(0,0,shipCam.transform.localPosition.z); 

	}

	public void OnCollisionStay2D(Collision2D col)
	{
		//body.velocity = (body.velocity - startingPlanet.RelativeVelocity) * 0.7f + startingPlanet.RelativeVelocity;
		body.AddForce((body.velocity - startingPlanet.RelativeVelocity) * 0.7f + startingPlanet.RelativeVelocity);
		onPlanet = true;
	}


	public void OnCollisionExit2D(Collision2D col)
	{
		//body.velocity = (body.velocity - startingPlanet.RelativeVelocity) * 0.7f + startingPlanet.RelativeVelocity;
		//body.AddForce((body.velocity - startingPlanet.RelativeVelocity) * 0.7f + startingPlanet.RelativeVelocity);
		onPlanet = false;
	}

}

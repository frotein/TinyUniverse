using UnityEngine;
using System.Collections;
using FarseerPhysics.Dynamics;

public class PlanetController : MonoBehaviour {

	public float scale = 1;
	Body body;

	// Use this for initialization
	void Start () 
	{
		body = transform.GetComponent<FSBodyComponent>().PhysicsBody;	
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	// Calculates the initial force needed to orbit circularly around a given transform
	public Vector2 calcInitForce(Transform tr2)
	{
		// gets the angle between the given transform and this transform;
		float ang = Mathf.Atan2(transform.position.y - tr2.position.y,
								transform.position.x - tr2.position.x);
		body = transform.GetComponent<FSBodyComponent>().PhysicsBody;
		// gets the farseer body component from the given transform 
		Body body2 = tr2.GetComponent<FSBodyComponent>().PhysicsBody;	 
		Debug.Log(body);
		// hets the distance between this transform and the given transform
		float dist = Vector2.Distance((Vector2) transform.position, (Vector2) tr2.position);
		float iForce = Mathf.Sqrt((body2.Mass + body.Mass) / dist);
		Vector2 aForce = new Vector2(iForce * Mathf.Cos(ang + (Mathf.PI / 2)),iForce * Mathf.Sin(ang + (Mathf.PI / 2)));
		return aForce;
	}
}

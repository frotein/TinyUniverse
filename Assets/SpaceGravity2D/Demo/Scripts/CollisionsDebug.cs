using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {
	/// <summary>
	/// Component for displaying collision info in editor.
	/// Blue star - contact point; Green line - normal; Red line - relative velocity; Pink line - velocity ;Yellow line = relative velocity projection on normal.
	/// </summary>
	public class CollisionsDebug : MonoBehaviour {

		public float SizeMlt = 1f;

		void OnCollisionEnter2D( Collision2D coll ) {
			OnCollisionStay2D( coll );
		}

		void OnCollisionStay2D( Collision2D coll ) {
			DrawDebugStar( coll.contacts[0].point, SizeMlt / 30f, Color.blue );
			Debug.DrawLine( coll.contacts[0].point, coll.contacts[0].point + coll.contacts[0].normal * SizeMlt, Color.green );
			Debug.DrawLine( coll.contacts[0].point, coll.contacts[0].point + coll.relativeVelocity * SizeMlt, Color.red );
			var projection = coll.contacts[0].normal * ( coll.contacts[0].normal.x * coll.relativeVelocity.x + coll.contacts[0].normal.y * coll.relativeVelocity.y );
			Debug.DrawLine( coll.contacts[0].point, coll.contacts[0].point + projection * SizeMlt, Color.yellow );
			Debug.DrawLine( coll.contacts[0].point, coll.contacts[0].point + GetComponent<Rigidbody2D>().velocity * SizeMlt, new Color( 1, 130f / 255f, 148f / 255f ) );
		}


		public static void DrawDebugStar( Vector3 p, float radius, Color col, int raysCount = 8 ) {
			float angleStep = 360f / ( raysCount > 0 ? raysCount : 1f ) * Mathf.Deg2Rad;
			for ( float i = 0; i < Mathf.PI * 2f; i += angleStep ) {
				var n = new Vector3( Mathf.Sin( i ) * radius, Mathf.Cos( i ) * radius, 0f );
				Debug.DrawLine( p + n, p + new Vector3( Mathf.Sin( i + angleStep ) * radius, Mathf.Cos( i + angleStep ) * radius ), Color.blue );
				Debug.DrawLine( p + n, p + n + n * 0.5f, Color.blue );
			}
		}
	}
}
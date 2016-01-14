using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {
	public class SimpleFollow : MonoBehaviour {

		public Transform target;
		public Vector2 offset;
		public bool disableOnLostTarget= true;

		void Update() {
			if ( target != null ) {
				transform.position = new Vector3( target.position.x + offset.x, target.position.y + offset.y, transform.position.z );
			} else {
				if ( disableOnLostTarget ) {
					gameObject.SetActive( false );
				}
			}
		}
	}
}
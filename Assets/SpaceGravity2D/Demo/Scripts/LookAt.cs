using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {

    /// <summary>
    /// Make object look at target. Used for rotating planet's lights side towards sun
    /// </summary>
	[ExecuteInEditMode]
    public class LookAt : MonoBehaviour {

        Transform _transform;
        public Transform Target;
        /// <summary>
        /// fixed angle relative to target
        /// </summary>
        public float RotationAngle;

        void Start() {
            if ( Target == null ) {
                enabled = false;
            }
            _transform = transform;
        }

        void Update() {
			if ( !_transform ) {
				_transform = GetComponent<Transform>();
			}
			if ( !Target || !_transform ) {
				return;
			}
            var v = Target.position - _transform.position;
            _transform.rotation = Quaternion.LookRotation( Vector3.forward, RotationAngle != 0 ? (Vector3)CelestialBody.RotateVectorByAngle( v, Mathf.Deg2Rad * RotationAngle ) : v );
        }
    }
}
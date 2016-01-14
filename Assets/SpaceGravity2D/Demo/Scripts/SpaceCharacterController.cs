using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {

	/// <summary>
	/// Controls attached celestial body with keyboard.
	/// </summary>
	public class SpaceCharacterController : MonoBehaviour {

		Transform _transform;
		CelestialBody _cbody;
		public float AccelForce = 1f;
		public float RotateSpeed = 1f;
		public float JumpForce = 10f;
		Vector2 _jumpDir = Vector2.zero;
		Quaternion _targetRotation;

		void Awake() {
			_transform = transform;
			_cbody = GetComponent<CelestialBody>();
		}

		void Update() {
			if ( _cbody ) {
				if ( _cbody.Attractor ) {
					var v = _transform.position - _cbody.Attractor._transform.position;
					_targetRotation = Quaternion.LookRotation( Vector3.forward, v );
					_cbody.DrawOrbit();
				} else {
					_cbody.HideOrbit();
				}
			}
			KeyboardInput();
			_transform.rotation = Quaternion.Lerp( _transform.rotation, _targetRotation, 0.1f );
		}

		void KeyboardInput() {
			if ( _cbody && _transform ) {
				float x = Input.GetAxis( "Horizontal" );
				float y = Input.GetAxis( "Vertical" );
				if ( x != 0 || y != 0 ) {
					var dirUp = new Vector2( _transform.up.x * y, _transform.up.y * y );
					var dirLeft = new Vector2( _transform.up.y * x, -_transform.up.x * x );
					_cbody.AddExternalVelocity( ( dirUp + dirLeft ) * AccelForce * Time.deltaTime );
				}
				float rot = ( Input.GetKey( KeyCode.Q ) ? 1f : 0f ) + ( Input.GetKey( KeyCode.E ) ? -1f : 0f );
				if ( rot != 0 ) {
					_targetRotation = Quaternion.Euler( _targetRotation.eulerAngles + new Vector3( 0, 0, rot ) );
				}
			}
		}

		void OnCollisionStay2D( Collision2D coll ) {
			if ( Input.GetKey( KeyCode.Space ) ) {
				if ( _jumpDir == Vector2.zero ) {
					StartCoroutine( Jump() );
				}
				_jumpDir += coll.contacts[0].normal;
			}
		}

		IEnumerator Jump() {
			yield return new WaitForEndOfFrame();
			_cbody.AddExternalVelocity( _jumpDir.normalized * JumpForce );
			_jumpDir = Vector2.zero;
		}

		public void OnCharDestroy() {
			Application.LoadLevel( Application.loadedLevelName );
		}
	}
}
using UnityEngine;
using System.Collections;


namespace SpaceGravity2D {
	/// <summary>
	/// Basic static Sphere of Influence component script, alternative to Dynamic Attractor Changing. If attached to gameobject whith no colliders, new collider will be created.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu( "SpaceGravity2D/SphereOfInfluence" )]
	public class SphereOfInfluence : MonoBehaviour {

		CircleCollider2D _detector;
		CelestialBody _body;
		[Header( "Range of ingluence:" )]
		public float TriggerRadius;
		[Header( "Calculate radius value based on orbit data:" )]
		/// <summary>
		/// if true and attractor is not null, range of influence will be calculated automaticaly. Useful for making first approach
		/// </summary>
		public bool UseAutoROI = false;
		[Space( 15 )]
		/// <summary>
		/// Dynamic attractor changing of celestial body is full alternative to this component.
		/// </summary>
		public bool IgnoreBodiesWithDynamicAttrChanging = true;
		public bool IgnoreTransformsScale = true;
		public bool IgnoreOtherSpheresOfInfluences = true;
		public bool drawGizmo;

		void Awake() {
			GetTriggerCollider();
			_body = GetComponentInParent<CelestialBody>();
			if ( !_detector || !_body ) {
				enabled = false;
			}
			TriggerRadius = Mathf.Abs( TriggerRadius );
		}

		void GetTriggerCollider() {
			var colliders = GetComponentsInChildren<CircleCollider2D>();
			for ( int i = 0; i < colliders.Length; i++ ) {
				if ( colliders[i].isTrigger ) {
					_detector = colliders[i];
				}
			}
			if ( !_detector ) {
				_detector = gameObject.AddComponent<CircleCollider2D>();
				Debug.Log("SpaceGravity2D: Sphere Of Influence autocreate trigger for " + name);
			}
		}

#if UNITY_EDITOR
		void Update() {
			if ( UseAutoROI ) {
				if ( _body && _body.Attractor && !double.IsNaN( _body.SemiMajorAxys ) ) {
					_body.Attractor.FindReferences();
					_body.FindReferences();
					TriggerRadius = (float)_body.SemiMajorAxys * Mathf.Pow( _body.Mass / _body.Attractor.Mass, 2f / 5f );
				}
			}
			float parentScale = 1f;
			float scale = 1f;
			if ( IgnoreTransformsScale ) {
				parentScale = transform.parent == null ? 1 : ( transform.parent.localScale.x + transform.parent.localScale.y ) / 2f;
				scale = ( transform.localScale.x + transform.localScale.y ) / 2f;
			}
			_detector.radius = TriggerRadius / scale / parentScale;
		}
#endif


		void OnTriggerEnter2D( Collider2D col ) {
			if ( col.transform != transform.parent ) {
				if ( IgnoreOtherSpheresOfInfluences && col.GetComponentInChildren<SphereOfInfluence>() != null ) {
					return;
				}
				var cBody = col.GetComponentInParent<CelestialBody>();
				if ( cBody && cBody.Attractor != _body && cBody.Mass < _body.Mass && ( !IgnoreBodiesWithDynamicAttrChanging || !cBody.IsUsingDynamicAttractorChanging ) ) {
					if ( cBody.Attractor != null ) {
						//Check if body is already attracted by child of current _body.
						if ( cBody.Attractor.Attractor == _body ) {
							return; 
						}
					}
					cBody.SetAttractor( _body );
				}
			}
		}

		void OnTriggerExit2D( Collider2D col ) {
			if ( col.transform != transform.parent ) {
				var colBody = col.GetComponentInParent<CelestialBody>();
				if ( colBody && colBody.Attractor == _body && ( !IgnoreBodiesWithDynamicAttrChanging || !colBody.IsUsingDynamicAttractorChanging ) ) {
					colBody.SetAttractor( _body.Attractor, checkIsInRange: true );
				}
			}
		}

		void OnDrawGizmos() {
			if ( enabled && drawGizmo ) {
				Gizmos.color = new Color( 1f, 1f, 1f, 0.2f );
				Gizmos.DrawSphere( transform.position, TriggerRadius );
			}
		}

	}
}
using UnityEngine;
using System.Collections.Generic;

namespace SpaceGravity2D {

	[AddComponentMenu("SpaceGravity2D/SimulationControl")]
	public class SimulationControl : MonoBehaviour {

		public enum NBodyCalculationType {
			Euler = 0,
			Verlet,
			RungeKutta,
		}
		/// <summary>
		/// Main constant. The real value 6.67384 * 10E-11 may not be very useful for gaming purposes
		/// </summary>
		public float GravitationalConstant = 0.001f;
		/// <summary>
		/// For changing gravitational constant and all velocity vectors proportionally. 
		/// </summary>
		public float GravitationalConstantProportional {
			get { return GravitationalConstant; }
			set {
				if (GravitationalConstant != value) {
					var deltaRatio = Mathf.Approximately(GravitationalConstant, 0f) ? 1f : value / GravitationalConstant;
					GravitationalConstant = value;
					ChangeAllVelocitiesByFactor(Mathf.Sqrt(Mathf.Abs(deltaRatio)));
				}
			}
		}
		/// <summary>
		/// Global constraint for gravitational attraction range
		/// </summary>
		public float MaxAttractionRange = float.PositiveInfinity;
		/// <summary>
		/// Global constraint for gravitational attraction range
		/// </summary>
		public float MinAttractionRange = 0.1f;
		/// <summary>
		/// TimeScale of simulation process. May be dynamicaly changed, but very large values decreasing precision of calculations
		/// </summary>
		public float TimeScale = 1f;
		/// <summary>
		/// Mass threshold for body to became attractor
		/// </summary>
		public float MinAttractorMass = 100f;
		/// <summary>
		/// Draw orbits in playmode
		/// </summary>
		public bool drawOrbits = true;
		/// <summary>
		/// Draw orbits in editor. Works also in playmode
		/// </summary>
		public bool drawOrbitsInEditor = true;
		public bool drawDisabledOrbitsInEditor = true;
		/// <summary>
		/// Debug orbiting data
		/// </summary>
		public bool drawDebugOrbitPoints = false;
		/// <summary>
		/// Display handy draggable arrows in scene window
		/// </summary>
		public bool drawVelocityVectors = true;
		/// <summary>
		/// Material for customization of orbit displaying in playmode
		/// </summary>
		public Material orbitLineRendererMat;
		/// <summary>
		/// Width of orbitline in screen pixels
		/// </summary>
		public int orbitsLinesWidth = 5;
		/// <summary>
		/// Precision of orbits display
		/// </summary>
		public uint orbitPointsCount = 52;
		/// <summary>
		/// Velocity arrow's size multiplier
		/// </summary>
		public float arrowsSize = 1f;
		/// <summary>
		/// Select object by selecting velocity arrow
		/// </summary>
		public bool selectWhenDraggingArrow = true;
		/// <summary>
		/// Switch: show global/local space velocity
		/// </summary>
		public bool globalVelocities;
		/// <summary>
		/// Buffer for references. Used only in playmode
		/// </summary>
		public List<CelestialBody> _bodies = new List<CelestialBody>();
		/// <summary>
		/// Optimisation cache
		/// </summary>
		List<CelestialBody> _attractorsCache = new List<CelestialBody>();
		/// <summary>
		/// If scene has central attractor, it will be stored here
		/// </summary>
		public CelestialBody _centralAttractor=null;
		/// <summary>
		/// Used primarily to prevent multiple copies of singleton
		/// </summary>
		public static SimulationControl instance;
		/// <summary>
		/// Current n-body simulation type
		/// </summary>
		public NBodyCalculationType CalculationType = NBodyCalculationType.Verlet;
		/// <summary>
		/// Draw debug lines in editor
		/// </summary>
		public bool drawLinesToAttractors;

		void OnEnable() {
			//Singleton:
			if (instance && instance != this) {
				Debug.LogWarning("SpaceGravity2D: SimulationControl already exists");
				enabled = false;
				return;
			}
			instance = this;
			SubscribeForEvents();
		}
		void OnDisable() {
			UnsubscribeFromEvents();
		}

		void Start() {
			GetCentralAttractor();
		}

		void FixedUpdate() {
			SimulationStep(Time.deltaTime * TimeScale);
		}

		/// <summary>
		/// Simulate gravity on scene. 
		/// Newtoninan motion and keplerian motion type
		/// </summary>
		void SimulationStep(float deltaTime) {
			//cache attractors to temporary list which improves performance in situations, when scene contains a lot of non-attracting low mass celestial bodies.
			_attractorsCache.Clear();
			for (int i = 0; i < _bodies.Count; i++) {
				if (_bodies[i].isActiveAndEnabled && _bodies[i].Mass > MinAttractorMass) {
					_attractorsCache.Add(_bodies[i]);
				}
			}

			for (int i = 0; i < _bodies.Count; i++) {
				if (!_bodies[i].isActiveAndEnabled) {
					continue;
				}
				///=========== fixed position:
				if (_bodies[i].IsFixedPosition) {
					_bodies[i].HideOrbit();
					if (!_bodies[i]._rigidbody2D.isKinematic) {
						_bodies[i]._rigidbody2D.isKinematic = true;
					}
					if (_bodies[i].Attractor != null) {
						_bodies[i]._transform.position = _bodies[i].Attractor._transform.position + (Vector3)_bodies[i].RadiusVector;
					}
					_bodies[i].LastPosition = _bodies[i]._transform.position;
					continue;
				}
				///===========
				///======================= Keplerian motion type:
				if (_bodies[i].UseRailMotion && _bodies[i].IsOnRailsMotion) {
					if (drawOrbits && _bodies[i].IsDrawOrbit) {
						_bodies[i].DrawOrbit();
					}
					else {
						_bodies[i].HideOrbit();
					}
					if (_bodies[i].IgnoreAllCollisions) {
						if (!_bodies[i]._rigidbody2D.isKinematic) {
							_bodies[i]._rigidbody2D.isKinematic = true;
						}
					}
					else {
						if (_bodies[i]._rigidbody2D.isKinematic) {
							_bodies[i]._rigidbody2D.isKinematic = false;
						}
					}
					if (_bodies[i].Attractor != null) {
						_bodies[i].UpdateObjectOrbitDynamicParameters(deltaTime);
						_bodies[i].RelativePosition = _bodies[i].GetBodyFocalPosition();
						_bodies[i].Velocity = _bodies[i].Attractor.Velocity + _bodies[i].GetBodyOrbitVelocity();
						_bodies[i].LastPosition = _bodies[i]._transform.position;
						_bodies[i]._rigidbody2D.velocity = _bodies[i].Velocity;
					}
					else {
						if (Mathf.Approximately(_bodies[i].Velocity.x, 0f) || Mathf.Approximately(_bodies[i].Velocity.y, 0f)) {
							_bodies[i]._transform.position += (Vector3)( _bodies[i].Velocity * deltaTime );
						}
					}
				}

			}
			///=====================

			///===================== Newtonian motion type:
			for (int i = 0; i < _bodies.Count; i++) {
				if (!_bodies[i].isActiveAndEnabled || _bodies[i].IsFixedPosition || _bodies[i].UseRailMotion && _bodies[i].IsOnRailsMotion) {
					continue;
				}
				if (drawOrbits && _bodies[i].IsDrawOrbit) {
					_bodies[i].DrawOrbit();
				}
				else {
					_bodies[i].HideOrbit();
				}
				if (_bodies[i]._rigidbody2D.isKinematic) {
					_bodies[i]._rigidbody2D.isKinematic = false;
				}

				switch (CalculationType) {
				case NBodyCalculationType.Euler:
					_bodies[i]._transform.position = _bodies[i].LastPosition;
					_bodies[i].Velocity += CalcAccelerationEuler(_bodies[i].LastPosition) * deltaTime;
					if (_bodies[i].CollidingCount > 0 && Mathf.Abs(TimeScale) > 1e-6f) {
						_bodies[i]._additionalVelocity += ( _bodies[i]._rigidbody2D.velocity / TimeScale - _bodies[i].Velocity ) / TimeScale;
					}
					if (_bodies[i]._additionalVelocity != Vector2.zero) {
						_bodies[i].Velocity += _bodies[i]._additionalVelocity;
						_bodies[i]._additionalVelocity = Vector2.zero;
					}
					_bodies[i].LastPosition = _bodies[i].LastPosition + (Vector3)( _bodies[i].Velocity * deltaTime );
					_bodies[i]._transform.position = _bodies[i].LastPosition;
					break;
				case NBodyCalculationType.Verlet:
					_bodies[i].LastPosition += (Vector3)_bodies[i].Velocity * ( deltaTime / 2f );
					_bodies[i]._transform.position = _bodies[i].LastPosition;
					_bodies[i].Velocity += CalcAccelerationEuler(_bodies[i].LastPosition) * deltaTime;
					if (_bodies[i].CollidingCount > 0 && Mathf.Abs(TimeScale) > 1e-6f) {
						_bodies[i]._additionalVelocity += ( _bodies[i]._rigidbody2D.velocity / TimeScale - _bodies[i].Velocity ) / TimeScale;
					}
					if (_bodies[i]._additionalVelocity != Vector2.zero) {
						_bodies[i].Velocity += _bodies[i]._additionalVelocity;
						_bodies[i]._additionalVelocity = Vector2.zero;
					}
					_bodies[i].LastPosition += (Vector3)_bodies[i].Velocity * ( deltaTime / 2f );
					_bodies[i]._transform.position = _bodies[i].LastPosition;
					break;
				case NBodyCalculationType.RungeKutta:
					_bodies[i]._transform.position = _bodies[i].LastPosition;
					_bodies[i].Velocity += CalcAccelerationRungeKutta(_bodies[i].LastPosition, deltaTime);
					if (_bodies[i].CollidingCount > 0 && Mathf.Abs(TimeScale) > 1e-6f) {
						_bodies[i]._additionalVelocity += ( _bodies[i]._rigidbody2D.velocity / TimeScale - _bodies[i].Velocity ) / TimeScale;
					}
					if (_bodies[i]._additionalVelocity != Vector2.zero) {
						_bodies[i].Velocity += _bodies[i]._additionalVelocity;
						_bodies[i]._additionalVelocity = Vector2.zero;
					}
					_bodies[i].LastPosition = _bodies[i].LastPosition + (Vector3)( _bodies[i].Velocity * deltaTime );
					_bodies[i]._transform.position = _bodies[i].LastPosition;
					break;
				}
				_bodies[i]._rigidbody2D.velocity = _bodies[i].Velocity * TimeScale;
				_bodies[i].CalculateNewOrbitData();
				if (_bodies[i].UseRailMotion) {
					_bodies[i].IsOnRailsMotion = true; //transit to keplerian motion at next frame
				}
			}
			///=====================

		}

		public Vector2 CalcAcceleration(CelestialBody body) {
			Vector2 forceAcceleration = Vector2.zero;
			for (int i = 0; i < _attractorsCache.Count; i++) {
				if (_attractorsCache[i] == body) {
					continue;
				}
				forceAcceleration += AccelerationByAttractionForce(body._transform.position, _attractorsCache[i]._transform.position, _attractorsCache[i].MG, MinAttractionRange, Mathf.Min(MaxAttractionRange, _attractorsCache[i].MaxAttractionRange));
			}

			return forceAcceleration;
		}

		public Vector2 CalcAccelerationEuler(Vector2 pos) {
			Vector2 result = Vector2.zero;
			for (int i = 0; i < _attractorsCache.Count; i++) {
				if (_attractorsCache[i]._transform.position.x == pos.x && _attractorsCache[i]._transform.position.y == pos.y) {
					continue;
				}
				result += AccelerationByAttractionForce(pos, _attractorsCache[i]._transform.position, _attractorsCache[i].MG, MinAttractionRange, Mathf.Min(MaxAttractionRange, _attractorsCache[i].MaxAttractionRange));
			}
			return result;
		}

		public Vector2 CalcAccelerationRungeKutta(Vector2 pos, float dt) {
			Vector2 result = Vector2.zero;
			for (int i = 0; i < _attractorsCache.Count; i++) {
				if (_attractorsCache[i]._transform.position.x == pos.x && _attractorsCache[i]._transform.position.y == pos.y) {
					continue;
				}
				var t1 = AccelerationByAttractionForce(pos, _attractorsCache[i]._transform.position, _attractorsCache[i].MG, MinAttractionRange, Mathf.Min(MaxAttractionRange, _attractorsCache[i].MaxAttractionRange)) * dt;
				var t2 = AccelerationByAttractionForce(pos + t1 * 0.5f, _attractorsCache[i]._transform.position, _attractorsCache[i].MG, MinAttractionRange, Mathf.Min(MaxAttractionRange, _attractorsCache[i].MaxAttractionRange)) * dt;
				var t3 = AccelerationByAttractionForce(pos + t2 * 0.5f, _attractorsCache[i]._transform.position, _attractorsCache[i].MG, MinAttractionRange, Mathf.Min(MaxAttractionRange, _attractorsCache[i].MaxAttractionRange)) * dt;
				var t4 = AccelerationByAttractionForce(pos + t3, _attractorsCache[i]._transform.position, _attractorsCache[i].MG, MinAttractionRange, Mathf.Min(MaxAttractionRange, _attractorsCache[i].MaxAttractionRange)) * dt;
				result += new Vector2(( t1.x + t2.x * 2 + t3.x * 2 + t4.x ) / 6, ( t1.y + t2.y * 2 + t3.y * 2 + t4.y ) / 6);
			}
			return result;
		}

		public static Vector2 AccelerationByAttractionForce(Vector2 bodyPosition, Vector2 attractorPosition, float attractorMG, float minRange = 0.1f, float maxRange = 0) {
			Vector2 distanceVector =  attractorPosition - bodyPosition;
			if (maxRange != 0 && distanceVector.sqrMagnitude > maxRange * maxRange || distanceVector.sqrMagnitude < minRange * minRange) {
				return Vector2.zero;
			}
			var distanceMagnitude = distanceVector.magnitude;
			return distanceVector * attractorMG / distanceMagnitude / distanceMagnitude / distanceMagnitude; // combination of newtonian scalar force and normalized direction vector
		}

		/// <summary>
		/// Find biggest attractor in scene
		/// </summary>
		public void GetCentralAttractor() {
			var tempBodies = Application.isPlaying ? _bodies.ToArray() : GameObject.FindObjectsOfType<CelestialBody>();
			if (tempBodies.Length == 0) {
				_centralAttractor = null;
				return;
			}
			if (tempBodies.Length == 1) {
				_centralAttractor = tempBodies[0];
				return;
			}
			var biggestMassIndex = 0;
			for (int i = 1; i < tempBodies.Length; i++) {
				if (!Application.isPlaying) {
					tempBodies[i - 1].FindReferences();
					tempBodies[i].FindReferences();
				}
				if (tempBodies[i].Mass > tempBodies[biggestMassIndex].Mass) {
					biggestMassIndex = i;
				}
			}
			if (biggestMassIndex >= 0 && biggestMassIndex < tempBodies.Length) {
				_centralAttractor = tempBodies[biggestMassIndex];
			}
		}

		/// <summary>
		/// Used for changing gravitational parameter without breaking orbits.
		/// </summary>
		public void ChangeAllVelocitiesByFactor(float multiplier) {
			if (!Application.isPlaying) {
				_bodies.Clear();
				_bodies.AddRange(GameObject.FindObjectsOfType<CelestialBody>());
				for (int i = 0; i < _bodies.Count; i++) {
#if UNITY_EDITOR
					UnityEditor.Undo.RecordObject(_bodies[i], "Proportional velocity change");
#endif
					_bodies[i].Velocity *= multiplier;
				}
				if (!Application.isPlaying) {
					_bodies.Clear();
				}
			}
			else {
				for (int i = 0; i < _bodies.Count; i++) {
					_bodies[i].Velocity *= multiplier;
				}
			}
		}

		/// <summary>
		/// Fast and simple way to find attractor; 
		/// But note, that not always nearest attractor is most proper
		/// </summary>
		/// <param name="body"></param>
		public void FindNearestAttractorForBody(CelestialBody body) {
			CelestialBody resultAttractor = null;
			float sqrDistance = 0;
			if (!Application.isPlaying) {
				_bodies = new List<CelestialBody>(GameObject.FindObjectsOfType<CelestialBody>());
			}
			foreach (var otherBody in _bodies) {
				if (otherBody == body || otherBody.Mass < MinAttractorMass || ( otherBody.transform.position - body.transform.position ).magnitude > Mathf.Min(MaxAttractionRange, otherBody.MaxAttractionRange)) {
					continue;
				}
				float _sqrDistance = ( body._transform.position - otherBody._transform.position ).sqrMagnitude;
				if (resultAttractor == null || sqrDistance > _sqrDistance) {
					resultAttractor = otherBody;
					sqrDistance = _sqrDistance;
				}
			}
			if (!Application.isPlaying) {
				_bodies.Clear(); //_bodies must be empty in editor mode
			}
			if (resultAttractor != null) {
				if (resultAttractor != body.Attractor) {
					body.SetAttractor(resultAttractor, false, true);
				}
			}
		}

		/// <summary>
		/// Find attractor which has biggest gravitational influence on body comparing to others. If fail, null will be assigned.
		/// It can be used in realtime for implementing more precise transitions beetween spheres of influence, 
		/// but, without optimisations, performance cost definetly will be very high
		/// </summary>
		/// <param name="body"></param>
		public void FindMostProperAttractorForBody(CelestialBody body) {
			CelestialBody resultAttractor = null;
			if (!Application.isPlaying) {
				_bodies = new List<CelestialBody>(GameObject.FindObjectsOfType<CelestialBody>());
			}
			// Search logic:
			// calculate mutual perturbation for every pair of attractors in scene and select one, 
			// which attracts the body with biggest force and is least affected by others.
			foreach (var otherBody in _bodies) {
				if (otherBody == body || !otherBody.isActiveAndEnabled || otherBody.Mass < MinAttractorMass || ( otherBody.transform.position - body.transform.position ).magnitude > Mathf.Min(MaxAttractionRange, otherBody.MaxAttractionRange)) {
					continue;
				}
				if (!Application.isPlaying) {
					otherBody.FindReferences();
				}
				if (resultAttractor == null) {
					resultAttractor = otherBody;
					continue;
				}
				if (RelativePerturbationRatio(body, resultAttractor, otherBody) > RelativePerturbationRatio(body, otherBody, resultAttractor)) {
					resultAttractor = otherBody;
				}
			}
			if (!Application.isPlaying) {
				_bodies.Clear(); //_bodies must be empty in editor mode
			}
			if (resultAttractor != body.Attractor) {
				body.SetAttractor(resultAttractor, false, true);
				//body.Attractor = resultAttractor;
			}
		}

		/// <summary>
		/// One more convenient way to set attractor;
		/// </summary>
		/// <param name="body"></param>
		public void SetBiggestAttractorForBody(CelestialBody body) {
			GetCentralAttractor();
			if (_centralAttractor != null) {
				body.SetAttractor(_centralAttractor, false, true);
			}
		}

		/// <summary>
		/// Return ratio of perturbation force from third body relative to attraction force of mainAttractor
		/// </summary>
		/// <param name="targetBody"></param>
		/// <param name="mainAttractor"></param>
		/// <param name="perturbatingAttractor"></param>
		public static float RelativePerturbationRatio(CelestialBody targetBody, CelestialBody mainAttractor, CelestialBody perturbatingAttractor) {
			float mainAcceleration = AccelerationByAttractionForce(targetBody._transform.position, mainAttractor._transform.position, mainAttractor.MG).magnitude;
			float perturbAcceleration = AccelerationByAttractionForce(targetBody._transform.position, perturbatingAttractor._transform.position, perturbatingAttractor.MG).magnitude;
			return perturbAcceleration / mainAcceleration;
		}

		#region events


		void SubscribeForEvents() {
			CelestialBody.OnBodyCreatedEvent += RegisterBody;
			CelestialBody.OnBodyDestroyedEvent += UnregisterBody;
		}

		void UnsubscribeFromEvents() {
			CelestialBody.OnBodyCreatedEvent -= RegisterBody;
			CelestialBody.OnBodyDestroyedEvent -= UnregisterBody;
		}

		void RegisterBody(CelestialBody body) {
			_bodies.Add(body);
		}
		void UnregisterBody(CelestialBody body) {
			_bodies.Remove(body);
		}
		#endregion
	}
}
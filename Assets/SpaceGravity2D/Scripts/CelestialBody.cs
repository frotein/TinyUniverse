using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace SpaceGravity2D {
	public delegate void BodyEventHandler(CelestialBody body);

	[RequireComponent(typeof(Rigidbody2D))]
	[AddComponentMenu("SpaceGravity2D/CelestialBody")]
	[ExecuteInEditMode]
	public class CelestialBody : MonoBehaviour {

		//used for register in SimulationControl's references buffer
		public static event BodyEventHandler OnBodyCreatedEvent;
		public static event BodyEventHandler OnBodyDestroyedEvent;

		#region fields and properties

		public float personalTimeScale = 1f;
		/// <summary>
		/// Cached Transform
		/// </summary>
		public Transform _transform;
		/// <summary>
		/// Cached Rigidbody
		/// </summary>
		public Rigidbody2D _rigidbody2D;
		/// <summary>
		/// Cached SimulationControl
		/// </summary>
		public SimulationControl _simulationControl;
		/// <summary>
		/// actual postion in simulation
		/// </summary>
		public Vector3 LastPosition;
		/// <summary>
		/// Mass is stored in Rigidbody2D component.
		/// </summary>
		public float Mass {
			get {
				return _rigidbody2D.mass;
			}
			set {
				_rigidbody2D.mass = value;
			}
		}
		/// <summary>
		/// Easy access to frequently used setting
		/// </summary>
		public float MG {
			get {
				if (_simulationControl == null) {
					return Mass;
				}
				return Mass * _simulationControl.GravitationalConstant;
			}
		}
		/// <summary>
		/// Own maximum influence range parameter. Is used when value is less than same global parameter. 
		/// </summary>
		public float MaxAttractionRange = float.PositiveInfinity;
		/// <summary>
		/// Reference to attractor
		/// </summary>
		public CelestialBody Attractor;
		/// <summary>
		/// Global (not relative to attractor) velocity
		/// </summary>
		public Vector2 Velocity;
		/// <summary>
		/// Relative to attractor velocity
		/// </summary>
		public Vector2 RelativeVelocity {
			get {
				if (Attractor) {
					return Velocity - Attractor.Velocity;
				}
				else {
					return Velocity;
				};
			}
			set {
				if (Attractor) {
					Velocity = Attractor.Velocity + value;
				}
				else {
					Velocity = value;
				}
			}
		}
		/// <summary>
		/// Relative to attractor transform position
		/// </summary>
		public Vector2 RelativePosition {
			get {
				if (Attractor) {
					return _transform.position - Attractor._transform.position;
				}
				else {
					return _transform.position;
				}
			}
			set {
				if (Attractor) {
					_transform.position = (Vector3)value + Attractor._transform.position;
				}
				else {
					_transform.position = value;
				}
			}
		}
		/// <summary>
		/// Is fixed point in relative space
		/// </summary>
		public bool IsFixedPosition;
		/// <summary>
		/// Draw orbit, if it exists
		/// </summary>
		public bool IsDrawOrbit=true;
		/// <summary>
		/// Is used keplerian motion type at current moment
		/// </summary>
		public bool IsOnRailsMotion=true;
		/// <summary>
		/// Is prefer to use keplerian motion
		/// </summary>
		public bool UseRailMotion;
		/// <summary>
		/// Toggle collisions influence on orbit; Works only for keplerian motion.
		/// </summary>
		public bool IgnoreAllCollisions=true;
		/// <summary>
		/// If true, body will continiously look for most proper attractor.
		/// It precisely makes transitions between dynamic spheres of influence,
		/// but its too expensive for using on many objects.
		/// </summary>
		[SerializeField]
		bool _isUsingDynamicAttractorChanging = false;
		/// <summary>
		/// property for activating and deactivating search process.
		/// </summary>
		public bool IsUsingDynamicAttractorChanging {
			get {
				return _isUsingDynamicAttractorChanging;
			}
			set {
				if (_isUsingDynamicAttractorChanging != value) {
					_isUsingDynamicAttractorChanging = value;
					if (Application.isPlaying) {
						if (_isUsingDynamicAttractorChanging) {
							StartCoroutine(ContiniousAttractorSearch());
						}
						else {
							StopAllCoroutines();
						}
					}
				}
			}
		}
		/// <summary>
		/// Interval in seconds of how often body is looking for most proper attractor if _isUsingDynamicAttractorChanging == true.
		/// </summary>
		public float SearchAttractorInterval = 1.0f;
		/// <summary>
		/// Orbital parameter. also named 'b'
		/// </summary>
		public double SemiMinorAxys;
		/// <summary>
		/// Orbital parameter. also named 'a'
		/// </summary>
		public double SemiMajorAxys;
		/// <summary>
		/// Orbital parameter. 
		/// </summary>
		public double FocalParameter;
		/// <summary>
		/// Orbital parameter. eccentricity determines the type of orbit: elliptic, parabolic or hyperbolic
		/// </summary>
		public double Eccentricity;
		/// <summary>
		/// Orbital parameter. Sum of kinetic and potential energy of body. if energy<0 then orbit is elliptic
		/// </summary>
		public float EnergyTotal;
		/// <summary>
		/// Orbital parameter. Is not affected by global TimeScale
		/// </summary>
		public float Period;
		/// <summary>
		/// Orbital parameter. Angle between radius-vector and eccentric-vector or laplas-vector. Determines position on orbit;
		/// </summary>
		public double TrueAnomaly; //in radians
		/// <summary>
		/// Orbital parameter. Not a real angle, but a parameter. Determines position on orbit (all angles are in radians)
		/// </summary>
		public double MeanAnomaly;
		/// <summary>
		/// Orbital parameter. Determines position on orbit (all angles are in radians)
		/// </summary>
		public double EccentricAnomaly;
		/// <summary>
		/// Orbital parameter. Tilt angle of orbit (all angles are in radians)
		/// </summary>
		public double ArgumentOfPeriapsis;
		/// <summary>
		/// Orbital parameter. Vector from focus of orbit to body position
		/// </summary>
		public Vector2 RadiusVector;
		/// <summary>
		/// Orbital parameter. Distance to attracotr
		/// </summary>
		public float RadiusVectorMagnitude;
		/// <summary>
		/// Orbital parameter. Constant parameter for each orbit
		/// </summary>
		public double SquaresConstantVectorMagnitude;
		/// <summary>
		/// Orbital parameter. Global position of periapsis point
		/// </summary>
		public Vector2 Periapsis;
		public float PeriapsisDistance {
			get {
				if (Attractor != null) {
					return ( Periapsis - FocusPoint ).magnitude;
				}
				return 0f;

			}
		}
		/// <summary>
		/// Orbital parameter. Global position of apoapsis point
		/// </summary>
		public Vector2 Apoapsis;
		public float ApoapsisDistance {
			get {
				if (Attractor != null) {
					return ( Apoapsis - FocusPoint ).magnitude;
				}
				return 0f;
			}
		}
		/// <summary>
		/// Orbital parameter. Center of orbit in world coordinates
		/// </summary>
		public Vector2 CenterPoint;
		/// <summary>
		/// Orbital parameter. Attractor's position in world coordinates
		/// </summary>
		public Vector2 FocusPoint;
		/// <summary>
		/// Orbital parameter. Vector from focus to periapsis
		/// </summary>
		public Vector2 RelativeFocusPoint;
		/// <summary>
		/// Orbital parameter.
		/// </summary>
		public double OrbitCompressionRatio;
		/// <summary>
		/// Reference to orbit drawing LineRenderer
		/// </summary>
		LineRenderer _lineRenderer;
		/// <summary>
		/// buffer for new attractor assignation
		/// </summary>
		List<CelestialBody> _newAttractorsBuffer;
		/// <summary>
		/// buffer for external velocity changing
		/// </summary>
		public Vector2 _additionalVelocity;
		/// <summary>
		/// indicator of current collision state
		/// </summary>
		public int CollidingCount;

		#endregion


		void Awake() {
			FindReferences();
			if (_rigidbody2D.gravityScale != 0) {
				_rigidbody2D.gravityScale = 0;
			}
		}

		void Start() {
			LastPosition = _transform.position;
			if (Application.isPlaying) {
				if (OnBodyCreatedEvent != null) {
					OnBodyCreatedEvent(this);
				}
				if (!_simulationControl) {
					if (SimulationControl.instance == null) {
						Debug.LogWarning("SpaceGravity2D: Simulation Control not found");
						enabled = false;
						return;
					}
					else {
						_simulationControl = SimulationControl.instance;
					}
				}
				if (IsFixedPosition) {
					RelativeVelocity = Vector3.zero;
				}
				if (UseRailMotion) {
					CalculateNewOrbitData();
				}
			}
		}


		float _attractorUpdateTime = 0; //timer used in editor only

		void Update() {
			if (!Application.isPlaying) {
				if (_isUsingDynamicAttractorChanging) {
					if (Time.realtimeSinceStartup > _attractorUpdateTime) {
						_attractorUpdateTime = Time.realtimeSinceStartup + SearchAttractorInterval;
						FindAndSetMostProperAttractor();
					}
				}
				if (IsFixedPosition) {
					RelativeVelocity = Vector2.zero;
				}
			}
		}

		IEnumerator ContiniousAttractorSearch() {
			yield return null;
			while (isActiveAndEnabled) {
				FindAndSetMostProperAttractor();
				yield return new WaitForSeconds(SearchAttractorInterval);
			}
		}

		void OnEnable() {
			if (Application.isPlaying) {
				if (_isUsingDynamicAttractorChanging) {
					StartCoroutine(ContiniousAttractorSearch());
				}
			}
		}

		void OnDisable() {
			StopAllCoroutines();
			HideOrbit();
		}

		/// <summary>
		/// Get all main references
		/// </summary>
		public void FindReferences() {
			if (!_transform) {
				_transform = transform;
			}
			if (!_rigidbody2D) {
				_rigidbody2D = GetComponent<Rigidbody2D>();
			}
			if (!_simulationControl) {
				_simulationControl = GameObject.FindObjectOfType<SimulationControl>();
			}
		}

		void OnDestroy() {
			if (OnBodyDestroyedEvent != null) {
				OnBodyDestroyedEvent(this);
			}
		}

		void OnCollisionEnter2D(Collision2D coll) {
			CollidingCount++;
			OnCollisionStay2D(coll);
		}

		void OnCollisionStay2D(Collision2D coll) {
			if (!isActiveAndEnabled || IsFixedPosition) {
				return;
			}
			if (UseRailMotion && IsOnRailsMotion && IgnoreAllCollisions) {
				if (!_rigidbody2D.isKinematic) {
					_rigidbody2D.isKinematic = true;
				}
				return;
			}
			TerminateRailMotion();
		}

		void OnCollisionExit2D() {
			CollidingCount--;
		}

		/// <summary>
		/// Set the appropriate velocity for a circular orbit around current attractor. Call multiple times to change clockwise direction.
		/// </summary>
		public void MakeOrbitCircle() {
			if (Attractor) {
				if (!Application.isPlaying) {
					FindReferences();
					Attractor.FindReferences();
				}
				else {
					TerminateRailMotion();
				}
				var distanceVector = (Vector2)( _transform.position - Attractor._transform.position );
				if (distanceVector.sqrMagnitude < _simulationControl.MinAttractionRange * _simulationControl.MinAttractionRange) {
					Debug.Log("SpaceGravity2D: " + name + " is too close to attractor");
					return;
				}
				var dist = distanceVector.magnitude;
				var v = Mathf.Sqrt(Attractor.MG / dist);
				distanceVector = distanceVector / dist;
				var circVelo = new Vector2(-distanceVector.y * v, distanceVector.x * v);
				if (RelativeVelocity == circVelo) {
					RelativeVelocity = new Vector2(distanceVector.y * v, -distanceVector.x * v);
				}
				else {
					RelativeVelocity = circVelo;
				}
			}
			else {
				if (!Application.isPlaying) {
					Debug.Log("SpaceGravity2D: " + name + " has no attractor");
				}
			}
		}

		public void MakeOrbitCircle(bool clockwise) {
			if (Attractor) {
				MakeOrbitCircle();
				float crossProduct = (float)CrossProductZ(RelativeVelocity, (Vector2)( Attractor._transform.position - _transform.position ));
				if (clockwise && crossProduct > 0 || !clockwise && crossProduct < 0) {
					RelativeVelocity = -RelativeVelocity;
				}
			}
		}


		/// <summary>
		/// Assign new attractor for body at the end of current frame. If called multiple times in single frame, nearest attractor will be chosen
		/// </summary>
		/// <param name="attr">new attractor reference or null</param>
		/// <param name="checkIsInRange"></param>
		/// <param name="instant">if false, attractor will be assigned at End Of Frame</param>
		public void SetAttractor(CelestialBody attr, bool checkIsInRange = false, bool instant = false) {
			if (attr == null || attr != Attractor && attr != this) {
				if (!Application.isPlaying || instant) {
					TerminateRailMotion();
					Attractor = attr;
					CalculateNewOrbitData();
					return;
				}
				if (_newAttractorsBuffer == null || _newAttractorsBuffer.Count == 0) {
					_newAttractorsBuffer = new List<CelestialBody>();
					StartCoroutine(SetNearestAttractor(checkIsInRange));
					TerminateRailMotion();
				}
				_newAttractorsBuffer.Add(attr);
			}
		}

		/// <summary>
		/// Post frame assignation coroutine is used for preventing cases, when random attractor is assigned from set of attractors
		/// </summary>
		IEnumerator SetNearestAttractor(bool checkIsInRange) {
			yield return new WaitForEndOfFrame();
			if (_newAttractorsBuffer == null || _newAttractorsBuffer.Count == 0) {
				yield break;
			}
			if (_newAttractorsBuffer.Count == 1) {
				if (checkIsInRange) {
					if (_newAttractorsBuffer[0] == null || ( _newAttractorsBuffer[0]._transform.position - _transform.position ).magnitude < Mathf.Min(_newAttractorsBuffer[0].MaxAttractionRange, _simulationControl.MaxAttractionRange)) {
						Attractor = _newAttractorsBuffer[0];
						CalculateNewOrbitData();
					}
				}
				else {
					Attractor = _newAttractorsBuffer[0];
					CalculateNewOrbitData();
				}
				_newAttractorsBuffer.Clear();
				yield break;
			}
			CelestialBody nearest = _newAttractorsBuffer[0];
			float sqrDistance = nearest != null ? ( nearest._transform.position - _transform.position ).sqrMagnitude : float.MaxValue;
			for (int i = 1; i < _newAttractorsBuffer.Count; i++) {
				if (_newAttractorsBuffer[i] == nearest) {
					continue;
				}
				if (_newAttractorsBuffer[i] != null && ( _newAttractorsBuffer[i].transform.position - _transform.position ).sqrMagnitude < sqrDistance) {
					nearest = _newAttractorsBuffer[i];
					sqrDistance = ( _newAttractorsBuffer[i].transform.position - _transform.position ).sqrMagnitude;
				}
			}
			_newAttractorsBuffer.Clear();
			Attractor = nearest;
			CalculateNewOrbitData();
		}

		/// <summary>
		/// Used with colisions or other velocity changing events
		/// </summary>
		public void TerminateRailMotion() {
			if (!IgnoreAllCollisions) {
				IsOnRailsMotion = false;
			}
		}

		/// <summary>
		/// Used for quick setup
		/// </summary>
		public void FindAndSetNearestAttractor() {
			if (!Application.isPlaying) {
				FindReferences();
			}
			if (_simulationControl) {
				_simulationControl.FindNearestAttractorForBody(this);
			}
			else {
				Debug.Log("SpaceGravity2D: Simulation Control not found");
			}
		}

		public void FindAndSetMostProperAttractor() {
			if (!Application.isPlaying) {
				FindReferences();
			}
			if (_simulationControl) {
				_simulationControl.FindMostProperAttractorForBody(this);
			}
			else {
				Debug.Log("SpaceGravity2D: Simulation Control not found");
			}
		}

		public void FindAndSetBiggestAttractor() {
			if (!Application.isPlaying) {
				FindReferences();
			}
			if (_simulationControl) {
				_simulationControl.SetBiggestAttractorForBody(this);
			}
			else {
				Debug.Log("SpaceGravity2D: Simulation Control not found");
			}
		}

		/// <summary>
		/// Check for chain loops
		/// </summary>
		/// <param name="result"></param>
		public void GetFullAttractorChain(List<CelestialBody> result) {
			var current = this.Attractor;
			if (result == null) {
				result = new List<CelestialBody>();
			}
			else {
				result.Clear();
			}
			while (current != null) {
				result.Add(current);
				if (current.Attractor == this) {
					current.Attractor = null;
					break;
				}
				current = current.Attractor;
			}
		}

		/// <summary>
		/// Test body for chain loops
		/// </summary>
		public bool AttractorChainContains(CelestialBody body) {
			CelestialBody current = Attractor;
			while (current != null) {
				if (current == body) {
					return true;
				}
				current = current.Attractor;
			}
			return false;
		}


		/// <summary>
		/// Calculate all orbit parameters based on current body position and velocity
		/// if attractor is not null;
		/// </summary>
		public void CalculateNewOrbitData() {
			if (Attractor) {
				if (!Application.isPlaying) {
					FindReferences();
					Attractor.FindReferences();
				}
				FocusPoint = Attractor._transform.position;
				RadiusVector = _transform.position - Attractor._transform.position;
				RadiusVectorMagnitude = RadiusVector.magnitude;
				SquaresConstantVectorMagnitude = CrossProductZ(RadiusVector, RelativeVelocity);
				double eccentricVectorX = ( 1.0 / Attractor.MG ) * (float)( RelativeVelocity.y * SquaresConstantVectorMagnitude ) - (double)( RadiusVector.x / RadiusVectorMagnitude );
				double eccentricVectorY = ( 1.0 / Attractor.MG ) * (float)( -RelativeVelocity.x * SquaresConstantVectorMagnitude ) - (double)( RadiusVector.y / RadiusVectorMagnitude );
				FocalParameter = SquaresConstantVectorMagnitude * SquaresConstantVectorMagnitude / (double)Attractor.MG;
				Eccentricity = System.Math.Sqrt(eccentricVectorX * eccentricVectorX + eccentricVectorY * eccentricVectorY);
				EnergyTotal = RelativeVelocity.sqrMagnitude - 2f * Attractor.MG / RadiusVectorMagnitude;

				//two main cases of orbit are separated by eccentricity. (parabolic and hyperbolic considered together)
				//elliptic orbit:
				if (Eccentricity < 1) {
					OrbitCompressionRatio = 1.0 - Eccentricity * Eccentricity;
					SemiMajorAxys = FocalParameter / OrbitCompressionRatio;
					SemiMinorAxys = SemiMajorAxys * System.Math.Sqrt(OrbitCompressionRatio);
					RelativeFocusPoint = (float)SemiMajorAxys * new Vector2((float)eccentricVectorX, (float)eccentricVectorY);
					CenterPoint = FocusPoint - RelativeFocusPoint;
					Period = Mathf.PI * 2 * Mathf.Sqrt(Mathf.Pow((float)SemiMajorAxys, 3f) / Attractor.MG);
					Apoapsis = CenterPoint - new Vector2((float)eccentricVectorX, (float)eccentricVectorY) * (float)( SemiMajorAxys / Eccentricity );
					Periapsis = CenterPoint + new Vector2((float)eccentricVectorX, (float)eccentricVectorY) * (float)( SemiMajorAxys / Eccentricity );
					TrueAnomaly = Vector2.Angle(RadiusVector, Periapsis - FocusPoint) * Mathf.Deg2Rad;
					if (CrossProduct(RadiusVector, new Vector2((float)eccentricVectorX, (float)eccentricVectorY)).z > 0) {
						TrueAnomaly = 2 * Mathf.PI - TrueAnomaly;
					}
					var cosT = System.Math.Cos(TrueAnomaly);
					EccentricAnomaly = System.Math.Acos(( Eccentricity + cosT ) / ( 1f + Eccentricity * cosT ));
					if (CrossProductZ(_transform.position - (Vector3)CenterPoint, new Vector2((float)eccentricVectorX, (float)eccentricVectorY)) > 0) {
						EccentricAnomaly = 2 * Mathf.PI - EccentricAnomaly;
					}
					MeanAnomaly = EccentricAnomaly - Eccentricity * System.Math.Sin(EccentricAnomaly);
				}//hyperbolic & parabolic orbit: 
				else {
					OrbitCompressionRatio = Eccentricity * Eccentricity - 1f;
					SemiMajorAxys = FocalParameter / OrbitCompressionRatio;
					SemiMinorAxys = SemiMajorAxys * System.Math.Sqrt(OrbitCompressionRatio);
					RelativeFocusPoint = -(float)SemiMajorAxys * new Vector2((float)eccentricVectorX, (float)eccentricVectorY);
					CenterPoint = FocusPoint - RelativeFocusPoint;
					Period = 0;
					Apoapsis = Vector2.zero;
					Periapsis = CenterPoint - new Vector2((float)eccentricVectorX, (float)eccentricVectorY) * (float)( SemiMajorAxys / Eccentricity );
					TrueAnomaly = Vector2.Angle(RadiusVector, new Vector2((float)eccentricVectorX, (float)eccentricVectorY)) * Mathf.Deg2Rad;
					if (CrossProductZ(RadiusVector, new Vector2((float)eccentricVectorX, (float)eccentricVectorY)) > 0) {
						TrueAnomaly = -TrueAnomaly;
					}
					var cosT= System.Math.Cos(TrueAnomaly);
					EccentricAnomaly = Arcosh(( Eccentricity + cosT ) / ( 1 + Eccentricity * cosT )) * ( TrueAnomaly < 0f ? -1f : 1f );
					MeanAnomaly = (float)System.Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
				}
				ArgumentOfPeriapsis = Mathf.Deg2Rad * Vector2.Angle(Periapsis - FocusPoint, Vector2.up) * ( eccentricVectorX >= 0 ? -1 : 1 );
			}
		}



		/// <summary>
		/// Check orbit data calculations errors
		/// </summary>
		/// <returns></returns>
		public bool IsValidOrbit() {
			return Attractor != null && !double.IsNaN(Eccentricity) && !double.IsInfinity(Eccentricity) && System.Math.Abs(Eccentricity - 1) > 1E-5f && !double.IsInfinity(SemiMajorAxys) && !double.IsNaN(SemiMajorAxys) && !double.IsInfinity(SemiMinorAxys);
		}


		/// <summary>
		/// Draw orbit by using of LineRender. LineRenderer can be customized in global settings editor window
		/// </summary>
		public void DrawOrbit() {
			if (!_lineRenderer || Camera.main == null) {
				CreateLineRend();
			}
			_lineRenderer.enabled = true;
			float width = _simulationControl.orbitsLinesWidth * Camera.main.orthographicSize * 2 / Screen.height;
			_lineRenderer.SetWidth(width, width);
			var points = GetOrbitPoints((int)_simulationControl.orbitPointsCount, 100000f);
			_lineRenderer.SetVertexCount(points.Length);
			for (int i = 0; i < points.Length; i++) {
				_lineRenderer.SetPosition(i, points[i]);
			}
		}

		/// <summary>
		/// Get array of orbit points in world space.
		/// </summary>
		public Vector2[] GetOrbitPoints(int pointsCount, float maxDistanceForHyperbolicCase = 1000f) {
			if (!Attractor) {
				if (Mathf.Approximately(Velocity.x, 0) && Mathf.Approximately(Velocity.y, 0)) {
					return new Vector2[0];
				}
				var normal = Velocity.normalized;
				return new Vector2[]{
					_transform.position - (Vector3)(normal * maxDistanceForHyperbolicCase), 
					_transform.position, 
					_transform.position + (Vector3)(normal * maxDistanceForHyperbolicCase)
				};
			}
			if (!IsValidOrbit() || pointsCount < 1) {
				return new Vector2[0];
			}
			var result = new Vector2[pointsCount];
			var correction = -Attractor.Velocity * _simulationControl.TimeScale * Time.deltaTime; //correction compensate attractor speed
			if (Eccentricity < 1) {
				for (var i = 0; i < pointsCount; i++) {
					result[i] = CenterPoint + GetBodyCentralPositionAt(i * Mathf.PI * 2f / ( pointsCount - 1 )) - correction;
				}
				return result;
			}
			else {
				var maxAngle = Mathf.Acos((float)( ( FocalParameter / maxDistanceForHyperbolicCase - 1f ) / Eccentricity ));
				maxAngle = Mathf.Sqrt((float)( ( Eccentricity - 1 ) / ( Eccentricity + 1 ) )) * Mathf.Tan(maxAngle / 2f);
				maxAngle = Mathf.Log(Mathf.Abs(( 1 + maxAngle ) / ( 1 - maxAngle )));
				if (float.IsNaN(maxAngle)) {
					maxAngle = Mathf.PI / 4f;
				}
				for (int i = 0; i < pointsCount; i++) {
					result[i] = CenterPoint + GetBodyCentralPositionAt(-maxAngle + i * 2f * maxAngle / pointsCount) - correction;
				}
			}
			return result;
		}

		/// <summary>
		/// Get array of orbit points in world space with additional parameters. angle unit is radian.
		/// oppositeDirection flag indicates whatever returned path must to be along motion direction or opposite to it.
		/// note, in hyperbolic case fromAngle and toAngle can be out of orbit range (empty path will be returned).
		/// If fromAngle == toAngle received, then full ellipse will be returned in elliptic orbit case and one branch of hyperbolic orbit - in hyperbolic case (maxDistance parameter will be used for calculating outer end)
		/// </summary>
		public Vector2[] GetOrbitPointsRestricted(int pointsCount, bool oppositeDirection = false, float fromAngle = 0, float toAngle = 0, float maxDistanceForHyperbolicCase = 1000f) {
			if (!Attractor) {
				var normal = Velocity.normalized;
				return new Vector2[]{
					_transform.position - (Vector3)(normal * maxDistanceForHyperbolicCase), 
					_transform.position, 
					_transform.position + (Vector3)(normal * maxDistanceForHyperbolicCase)
				};
			}
			if (!IsValidOrbit() || pointsCount < 1) {
				return new Vector2[0];
			}
			var result = new Vector2[0];
			if (Eccentricity < 1.0f) {
				result = new Vector2[pointsCount];
				float deltaAngle = 0;
				if (fromAngle == toAngle) {
					deltaAngle = ( SquaresConstantVectorMagnitude > 0 ? 1 : -1 ) * Mathf.PI * 2f;
				}
				else {
					deltaAngle = oppositeDirection && SquaresConstantVectorMagnitude > 0 || SquaresConstantVectorMagnitude < 0 ? ( toAngle - fromAngle ) : ( fromAngle - toAngle );
				}
				for (int i = 0; i < pointsCount; i++) {
					result[i] = CenterPoint + GetBodyCentralPositionAt(fromAngle + deltaAngle * i / ( pointsCount - 1 ));
				}
				return result;
			}
			else {
				float maxAngle =  Mathf.Acos(-1 / (float)Eccentricity);
				if (Mathf.Abs(fromAngle) >= maxAngle || Mathf.Abs(toAngle) >= maxAngle) {
					return result;
				}
				if (fromAngle == toAngle) {
					toAngle = Mathf.Acos((float)( ( FocalParameter / maxDistanceForHyperbolicCase - 1f ) / Eccentricity ));
					toAngle = Mathf.Sqrt((float)( ( Eccentricity - 1 ) / ( Eccentricity + 1 ) )) * Mathf.Tan(toAngle / 2f);
					toAngle = Mathf.Log(( 1 + toAngle ) / ( 1 - toAngle )) * ( SquaresConstantVectorMagnitude > 0 && oppositeDirection || SquaresConstantVectorMagnitude < 0 && !oppositeDirection ? -1 : 1 );
				}
				else
					if (SquaresConstantVectorMagnitude > 0 && fromAngle > toAngle || SquaresConstantVectorMagnitude < 0 && fromAngle < toAngle) {
						float _temp = fromAngle;
						fromAngle = toAngle;
						toAngle = _temp;
					}
				float delta = toAngle - fromAngle;
				result = new Vector2[pointsCount];
				for (int i = 0; i < pointsCount; i++) {
					result[i] = CenterPoint + GetBodyCentralPositionAt(fromAngle + delta * i / ( pointsCount - 1 ));
				}
			}
			return result;
		}


		/// <summary>
		/// Disable LineRenderer
		/// </summary>
		public void HideOrbit() {
			if (_lineRenderer) {
				_lineRenderer.enabled = false;
			}
		}

		/// <summary>
		/// Use for control Celestial body. for example, accelerate spaceship.
		/// Result will be affected by Timescale.
		/// </summary>
		/// <param name="inputVelocity">Acceleration vector</param>
		public void AddExternalVelocity(Vector2 inputVelocity) {
			TerminateRailMotion();
			_additionalVelocity += inputVelocity * _simulationControl.TimeScale;
		}

		/// <summary>
		/// Apply force to the center of body.
		/// </summary>
		/// <param name="forceVector"> force magnitude and direction </param>
		/// <param name="asImpulse"> continiously or single time </param>
		public void AddExternalForce(Vector2 forceVector, bool asImpulse) {
			TerminateRailMotion();
			_additionalVelocity += forceVector / Mass * ( asImpulse ? 1f : Time.deltaTime );
		}


		/// <summary>
		/// Set new world space position with orbit data update.
		/// </summary>
		public void SetPosition(Vector2 newPos) {
			_transform.position = newPos;
			CalculateNewOrbitData();
			if (!IsOnRailsMotion) {
				LastPosition = _transform.position;
			}
		}

		/// <summary>
		/// Get body position relative to center point of current orbit at certain eccentric anomaly
		/// </summary>
		/// <param name="eccentricAnomaly">angle parameter in radians</param>
		/// <returns></returns>
		public Vector2 GetBodyCentralPositionAt(double eccentricAnomaly) {
			Vector2 result = Eccentricity < 1 ?
				new Vector2(-(float)( System.Math.Sin(eccentricAnomaly) * SemiMinorAxys ), (float)( System.Math.Cos(eccentricAnomaly) * SemiMajorAxys )) :
				new Vector2(-(float)( System.Math.Sinh(eccentricAnomaly) * SemiMinorAxys ), -(float)( System.Math.Cosh(eccentricAnomaly) * SemiMajorAxys ));
			result = RotateVectorByAngle(result, (float)ArgumentOfPeriapsis);
			return result;
		}

		/// <summary>
		/// Get body position relative to centerpoint at current eccentric anomaly
		/// </summary>
		public Vector2 GetBodyCentralPosition() {
			return GetBodyCentralPositionAt(EccentricAnomaly);
		}

		/// <summary>
		/// Get body position relative to focalpoint at current eccentric anomaly
		/// </summary>
		public Vector2 GetBodyFocalPosition() {
			return CenterPoint + GetBodyCentralPosition() - FocusPoint;
		}

		/// <summary>
		/// Get body orbital velocity at current eccentric anomaly. If orbit is not valid, absolute velocity will be returned
		/// </summary>
		public Vector2 GetBodyOrbitVelocity() {
			if (!IsValidOrbit()) {
				return Velocity;
			}
			var sqrtMGdivP = System.Math.Sqrt(Attractor.MG / FocalParameter) * ( SquaresConstantVectorMagnitude >= 0 ? -1 : 1 );
			Vector2 velocity = new Vector2((float)( sqrtMGdivP * ( Eccentricity + System.Math.Cos(TrueAnomaly) ) ), (float)( sqrtMGdivP * System.Math.Sin(TrueAnomaly) ));
			return RotateVectorByAngle(velocity, (float)ArgumentOfPeriapsis);
		}

		/// <summary>
		/// Calculate orbital position progression by certain time. Attractor must be not null.
		/// First calculating new Mean anomaly symply by adding mean motion (which is constant) and then calculates eccentric anomaly from it,
		///	and then - true anomaly.
		///	Used for moving along keplerian orbit.
		/// </summary>
		/// <param name="deltatime">timestep</param>
		public void UpdateObjectOrbitDynamicParameters(float deltatime) {
			if (Attractor == null) {
				return;
			}
			var relativePeriapsis = Periapsis - CenterPoint;
			var relativeApoapsis = Apoapsis - CenterPoint;
			FocusPoint = Attractor._transform.position;
			CenterPoint = FocusPoint - RelativeFocusPoint;
			Periapsis = CenterPoint + relativePeriapsis;
			Apoapsis = CenterPoint + relativeApoapsis;

			if (Eccentricity < 1) {
				MeanAnomaly += 2 * Mathf.PI * deltatime / Period * ( SquaresConstantVectorMagnitude > 0 ? 1 : -1 );
				MeanAnomaly %= 2 * Mathf.PI;
				if (MeanAnomaly < 0) {
					MeanAnomaly = 2f * Mathf.PI - MeanAnomaly;
				}
				EccentricAnomaly = KeplerSolver(MeanAnomaly);
				var cosE = System.Math.Cos(EccentricAnomaly);
				TrueAnomaly = System.Math.Acos(( cosE - Eccentricity ) / ( 1 - Eccentricity * cosE ));
				if (MeanAnomaly > Mathf.PI) {
					TrueAnomaly = 2 * Mathf.PI - TrueAnomaly;
				}
			}
			else {
				var n = System.Math.Sqrt(Attractor.MG / System.Math.Pow(SemiMajorAxys, 3f)) * ( CrossProductZ(RelativeVelocity, Periapsis - FocusPoint) > 0 ? -1 : 1 );
				MeanAnomaly = MeanAnomaly + n * deltatime;
				EccentricAnomaly = KeplerSolverHyperbolicCase(MeanAnomaly);
				TrueAnomaly = System.Math.Atan2(System.Math.Sqrt(Eccentricity * Eccentricity - 1.0) * System.Math.Sinh(EccentricAnomaly), Eccentricity - System.Math.Cosh(EccentricAnomaly));
			}
			RadiusVector = GetBodyCentralPositionAt(EccentricAnomaly) - ( FocusPoint - CenterPoint );
		}

		/// <summary>
		/// Change eccentricity and keep current orbit' argument of periapsis, mean anomaly and periapsis distance.
		/// Used in editor inspector.
		/// </summary>
		/// <param name="e">new eccentricity</param>
		public void SetEccentricity(double e) {
			if (!IsValidOrbit()) {
				if (Attractor == null) {
					return;
				}
				CalculateNewOrbitData();
			}
			if (!IsValidOrbit()) {
				return;
			}
			if (Abs(e) < 1e-6 || Abs(e - 1) < 1e-6) {
				e += 2e-6;
			}
			bool flipedSign = false;
			if (e < 1 && Eccentricity > 1 || e > 1 && Eccentricity < 1) {
				flipedSign = true;
			}
			TerminateRailMotion();
			var _periapsis = SemiMajorAxys * ( 1 - Eccentricity ); // Periapsis remains constant
			Eccentricity = Abs(e);
			var compresion = Eccentricity < 1 ? ( 1 - Eccentricity * Eccentricity ) : ( Eccentricity * Eccentricity - 1 );
			SemiMajorAxys = Abs(_periapsis / ( 1 - Eccentricity ));
			FocalParameter = SemiMajorAxys * compresion;
			SemiMinorAxys = SemiMajorAxys * Mathf.Sqrt((float)compresion);
			CenterPoint = FocusPoint - (float)( SemiMajorAxys * Abs(Eccentricity) ) * RelativeFocusPoint.normalized * ( flipedSign ? -1 : 1 );
			if (Eccentricity < 1) {
				EccentricAnomaly = KeplerSolver(MeanAnomaly);
				var cosE = System.Math.Cos(EccentricAnomaly);
				TrueAnomaly = System.Math.Acos(( cosE - Eccentricity ) / ( 1 - Eccentricity * cosE ));
				if (MeanAnomaly > Mathf.PI) {
					TrueAnomaly = 2f * Mathf.PI - TrueAnomaly;
				}
			}
			else {
				EccentricAnomaly = KeplerSolverHyperbolicCase(MeanAnomaly);
				TrueAnomaly = System.Math.Atan2(System.Math.Sqrt(Eccentricity * Eccentricity - 1) * System.Math.Sinh(EccentricAnomaly), Eccentricity - System.Math.Cosh(EccentricAnomaly));
			}
			RelativeVelocity = GetBodyOrbitVelocity();
			_transform.position = GetBodyCentralPositionAt(EccentricAnomaly) + CenterPoint;
			LastPosition = _transform.position;
			if (Application.isPlaying) {
				CalculateNewOrbitData();
			}
			else {
				//SetAttractor(null, false, true);
			}
		}

		/// <summary>
		/// Set exact position on current orbit
		/// </summary>
		/// <param name="m">mean anomaly in radians</param>
		public void SetMeanAnomaly(double m) {
			if (double.IsNaN(m) || double.IsInfinity(m) || !IsValidOrbit()) {
				return;
			}
			MeanAnomaly = m;
			if (Eccentricity < 1) {
				if (MeanAnomaly < 0) {
					MeanAnomaly = 2f * Mathf.PI + MeanAnomaly;
				}
				MeanAnomaly %= 2f * Mathf.PI;
				EccentricAnomaly = KeplerSolver(MeanAnomaly);
				var cosE = System.Math.Cos(EccentricAnomaly);
				TrueAnomaly = System.Math.Acos(( cosE - Eccentricity ) / ( 1 - Eccentricity * cosE ));
				if (MeanAnomaly > Mathf.PI) {
					TrueAnomaly = 2 * Mathf.PI - TrueAnomaly;
				}
			}
			else {
				EccentricAnomaly = KeplerSolverHyperbolicCase(MeanAnomaly);
				TrueAnomaly = System.Math.Atan2(System.Math.Sqrt(Eccentricity * Eccentricity - 1) * System.Math.Sinh(EccentricAnomaly), Eccentricity - System.Math.Cosh(EccentricAnomaly));
			}
			RelativeVelocity = GetBodyOrbitVelocity();
			_transform.position = CenterPoint + GetBodyCentralPosition();
			LastPosition = _transform.position;
		}

		/// <summary>
		/// Set exact position on current orbit
		/// </summary>
		/// <param name="t">true anomaly in radians</param>
		public void SetTrueAnomaly(double t) {
			if (double.IsNaN(t) || double.IsInfinity(t) || !IsValidOrbit()) {
				return;
			}
			t %= ( 2 * Mathf.PI );
			if (Eccentricity < 1) {
				var cosT = System.Math.Cos(TrueAnomaly);
				EccentricAnomaly = System.Math.Acos(( Eccentricity + cosT ) / ( 1 + Eccentricity * cosT ));
				if (CrossProductZ(_transform.position - (Vector3)CenterPoint, RelativeFocusPoint) > 0) {
					EccentricAnomaly = 2 * Mathf.PI - EccentricAnomaly;
				}
				MeanAnomaly = EccentricAnomaly - Eccentricity * System.Math.Sin(EccentricAnomaly);
			}
			else {
				var cosT= System.Math.Cos(TrueAnomaly);
				EccentricAnomaly = Arcosh(( Eccentricity + cosT ) / ( 1 + Eccentricity * cosT )) * ( TrueAnomaly < 0 ? -1 : 1 );
				MeanAnomaly = System.Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
			}
			RelativeVelocity = GetBodyOrbitVelocity();
			_transform.position = CenterPoint + GetBodyCentralPosition();
			LastPosition = _transform.position;
		}
		/// <summary>
		/// Set exact position on current orbit
		/// </summary>
		/// <param name="e">eccentric anomaly in radians</param>
		public void SetEccentricAnomaly(double e) {
			if (double.IsNaN(e) || double.IsInfinity(e) || !IsValidOrbit()) {
				return;
			}
			if (e < 0) {
				e = 2 * Mathf.PI + e;
			}
			e %= ( 2 * Mathf.PI );
			EccentricAnomaly = e;
			if (Eccentricity < 1) {
				var cosE = System.Math.Cos(EccentricAnomaly);
				TrueAnomaly = System.Math.Acos(( cosE - Eccentricity ) / ( 1 - Eccentricity * cosE ));
				MeanAnomaly = EccentricAnomaly - Eccentricity * System.Math.Sin(EccentricAnomaly);
				if (MeanAnomaly > Mathf.PI) {
					TrueAnomaly = 2 * Mathf.PI - TrueAnomaly;
				}
			}
			else {
				TrueAnomaly = System.Math.Atan2(System.Math.Sqrt(Eccentricity * Eccentricity - 1) * System.Math.Sinh(EccentricAnomaly), Eccentricity - System.Math.Cosh(EccentricAnomaly));
				MeanAnomaly = System.Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
			}
			RelativeVelocity = GetBodyOrbitVelocity();
			_transform.position = CenterPoint + GetBodyCentralPosition();
		}

		/// <summary>
		/// Set Angle of orbit orientation
		/// </summary>
		/// <param name="r">angle in radians</param>
		public void SetArgumentOfPeriapsis(double r) {
			if (double.IsNaN(r) || double.IsInfinity(r) || !IsValidOrbit()) {
				return;
			}
			ArgumentOfPeriapsis = r;
			if (Eccentricity < 1) {
				CenterPoint = FocusPoint + RotateVectorByAngle(new Vector2(0, -1) * RelativeFocusPoint.magnitude, (float)ArgumentOfPeriapsis);
			}
			else {
				CenterPoint = FocusPoint - RotateVectorByAngle(new Vector2(0, -1) * RelativeFocusPoint.magnitude, (float)ArgumentOfPeriapsis);
			}
			_transform.position = CenterPoint + GetBodyCentralPosition();
			RelativeVelocity = GetBodyOrbitVelocity();
			LastPosition = _transform.position;
		}

		void CreateLineRend() {
			GameObject lineRendObj = new GameObject("OrbitLines");
			lineRendObj.transform.SetParent(_transform);
			lineRendObj.transform.position = Vector3.zero;
			_lineRenderer = lineRendObj.AddComponent<LineRenderer>();
			_lineRenderer.material = _simulationControl.orbitLineRendererMat;
		}

		/// <summary>
		/// Get eccentric anomaly from mean anomaly by solving Kepler's equation M = E - e * Sin(E)
		/// </summary>
		/// <param name="M">Mean Anomaly</param>
		/// <returns>eccentric anomaly</returns>
		double KeplerSolver(double M) {
			//one stable method
			int iterations = Eccentricity < 0.4 ? 2 : 4;
			double E = M;
			for (int i = 0; i < iterations; i++) {
				double esinE = Eccentricity * System.Math.Sin(E);
				double ecosE = Eccentricity * System.Math.Cos(E);
				double deltaE = E - esinE - M;
				double n = 1.0 - ecosE;
				E += -5.0 * deltaE / ( n + System.Math.Sign(n) * System.Math.Sqrt(System.Math.Abs(16.0 * n * n - 20.0 * deltaE * esinE)) );
			}
			return E;
		}

		/// <summary>
		/// Get hyperbolic(eccentric) anomaly from mean anomaly
		/// </summary>
		/// <param name="M">Mean Anomaly</param>
		/// <returns></returns>
		double KeplerSolverHyperbolicCase(double M) {
			double epsilon = 0.001f;
			double delta = 1f;
			double F = System.Math.Log(2.0 * System.Math.Abs(M) / Eccentricity + 1.8);//danby guess
			while (System.Math.Abs(delta) > epsilon) {
				delta = ( Eccentricity * (float)System.Math.Sinh(F) - F - M ) / ( Eccentricity * (float)System.Math.Cosh(F) - 1f );
				F -= delta;
			}
			if (double.IsNaN(F) || double.IsInfinity(F)) {
				F = M;
			}
			return F;
		}

		#region helpers
		public static double Artanh(double x) {
			return 0.5 * System.Math.Log(( 1.0 + x ) / ( 1.0 - x ));
		}

		public static double Arcosh(double x) {
			if (x < 1.0) {
				return 0;
			}
			return System.Math.Log(x + System.Math.Sqrt(x * x - 1.0));
		}

		public static Vector2 RotateVectorByAngle(Vector2 vector, float angle) {
			var cos = Mathf.Cos(angle);
			var sin = Mathf.Sin(angle);
			return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
		}

		public static Vector3 CrossProduct(Vector3 a, Vector3 b) {
			return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
		}

		public static double CrossProductZ(Vector2 a, Vector2 b) {
			return a.x * b.y - a.y * b.x;
		}

		public static double Abs(double d) {
			return d < 0 ? -d : d;
		}
		#endregion
	}//celestial body class
}//namespace
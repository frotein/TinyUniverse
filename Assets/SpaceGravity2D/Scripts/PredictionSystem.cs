using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceGravity2D {

	/// <summary>
	/// basic prediction orbits calculator
	/// </summary>
	[AddComponentMenu("SpaceGravity2D/PredictionSystem")]
	public class PredictionSystem : MonoBehaviour {

		struct BodyPoints {
			public Vector2 pos;
			public Vector2 v;
			public float m;
			public bool isFixed;
			public bool isVisible;
			public Material material;
			public float width;
			public Vector2[] points;
		}

		public SimulationControl SimControl;

		/// <summary>
		/// larger - better
		/// </summary>
		public float CalcStep = 1f;

		public int PointsCount = 50;
		public Material LinesMaterial;
		public float LinesWidth = 0.05f;

		private BodyPoints[] bodies;
		List<LineRenderer> lineRends = new List<LineRenderer>();

		void Start() {
			if ( SimControl == null ) {
				SimControl = GameObject.FindObjectOfType<SimulationControl>();
			}
			if ( SimControl == null ) {
				enabled = false;
			}
		}

		void Update() {
			Calc();
			ShowPredictOrbit();
		}

		void OnDisable() {
			HideAllOrbits();
		}

		void Calc() {
			//==================== Filter disabled bodies
			List<CelestialBody> targets = new List<CelestialBody>();
			for ( int i = 0; i < SimControl._bodies.Count; i++ ) {
				if ( SimControl._bodies[i].isActiveAndEnabled ) {
					targets.Add( SimControl._bodies[i] );
				}
			}
			//=====================
			//===================== Create working data array from bodies.
			bodies = new BodyPoints[targets.Count];
			for ( int i = 0; i < bodies.Length; i++ ) {
				var targetComponent = targets[i].GetComponent<PredictionSystemTarget>();
				bool isVisibleOrb = targetComponent == null || targetComponent.AllowDisplayPredictionOrbit;
				Material mat = targetComponent == null ? LinesMaterial : targetComponent.OrbitMaterial;
				
				bodies[i] = new BodyPoints() {
					pos = targets[i]._transform.position,
					v = targets[i].Velocity,
					m = targets[i].Mass,
					isFixed = targets[i].IsFixedPosition,
					isVisible = isVisibleOrb,
					material = mat,
					width = targetComponent == null ? LinesWidth : targetComponent.OrbitWidth,
					points = new Vector2[PointsCount]
				};
			}
			//=====================
			//===================== Calculate scene motion progress and record points into arrays.
			for ( int i = 0; i < PointsCount; i++ ) {
				//======================== calculate next step velocities for each body
				for ( int j = 0; j < bodies.Length; j++ ) {
					if ( bodies[j].isFixed ) {
						continue;
					}
					Vector2 acceleration = Vector2.zero;
					for ( int n = 0; n < bodies.Length; n++ ) {
						if ( n != j ) {
							acceleration += Acceleration( bodies[j].pos, bodies[n].pos, bodies[n].m * SimControl.GravitationalConstant, 0.5f, SimControl.MaxAttractionRange );
						}
					}
					bodies[j].v += acceleration * CalcStep;
				}
				//======================== move bodies and store current step positions
				for ( int j = 0; j < bodies.Length; j++ ) {
					if ( bodies[j].isFixed ) {
						continue;
					}
					bodies[j].points[i] = bodies[j].pos;
					bodies[j].pos += bodies[j].v * CalcStep;
				}
			}
			//=====================
		}

		void ShowPredictOrbit() {
			int t = 0;
			while ( lineRends.Count < bodies.Length && t < 1000 ) {
				CreateLineRenderer();
				t++;
			}
			var i = 0;
			for ( i = 0; i < bodies.Length; i++ ) {
				lineRends[i].SetVertexCount( PointsCount + 1 );
				lineRends[i].SetWidth(bodies[i].width, bodies[i].width);
				lineRends[i].material = bodies[i].material == null ? LinesMaterial : bodies[i].material;
				for ( int j = 0; j < bodies[i].points.Length; j++ ) {
					lineRends[i].SetPosition( j, bodies[i].points[j] );
				}
				lineRends[i].SetPosition(PointsCount, bodies[i].pos); //last point is not in array;
				lineRends[i].enabled = true;
			}
			for ( ; i < lineRends.Count; i++ ) {
				lineRends[i].enabled = false;
			}
		}

		void CreateLineRenderer() {
			var o = new GameObject( "prediction orbit " + lineRends.Count );
			o.transform.SetParent( transform );
			var lineRend = o.AddComponent<LineRenderer>();
			lineRends.Add( lineRend );
		}

		public void HideAllOrbits() {
			for (int i = 0; i < lineRends.Count; i++) {
				lineRends[i].enabled = false;
			}
		}

		static Vector2 Acceleration( Vector2 pos, Vector2 attractorPos, float attractorMG, float minRange, float maxRange ) {
			Vector2 distanceVector =  attractorPos - pos;
			if ( maxRange != 0 && distanceVector.sqrMagnitude > maxRange * maxRange || distanceVector.sqrMagnitude < minRange * minRange ) {
				return Vector2.zero;
			}
			var distanceMagnitude = distanceVector.magnitude;
			return distanceVector * attractorMG / distanceMagnitude / distanceMagnitude / distanceMagnitude;
		}
	}
}
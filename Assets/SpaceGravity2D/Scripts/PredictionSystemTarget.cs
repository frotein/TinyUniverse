using UnityEngine;
using System.Collections;

namespace SpaceGravity2D {
	[AddComponentMenu("SpaceGravity2D/PredictionSystemTarget")]
	public class PredictionSystemTarget : MonoBehaviour {
		public bool AllowDisplayPredictionOrbit;
		public Material OrbitMaterial;
		public float OrbitWidth = 0.1f;
	}
}
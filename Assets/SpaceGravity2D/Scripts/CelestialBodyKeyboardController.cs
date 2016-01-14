using UnityEngine;
using System.Collections;

namespace SpaceGravity2D {

	/// <summary>
	/// Basic celestial body controller.
	/// </summary>
	public class CelestialBodyKeyboardController : MonoBehaviour {

		CelestialBody cbody;
		public float Strenght = 1f;

		void Start() {
			cbody = GetComponentInParent<CelestialBody>();
			if (cbody == null) {
				enabled = false;
			}
		}

		void Update() {
			var x = Input.GetAxis("Horizontal");
			var y = Input.GetAxis("Vertical");
			if (!Mathf.Approximately(x, 0) || !Mathf.Approximately(y, 0)) {
				if (cbody == null) {
					enabled = false;
					return;
				}
				cbody.AddExternalVelocity(new Vector2(x * Strenght * Time.deltaTime, y * Strenght * Time.deltaTime));
			}
		}
	}
}

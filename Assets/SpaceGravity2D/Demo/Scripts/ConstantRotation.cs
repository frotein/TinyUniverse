using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {
	public class ConstantRotation : MonoBehaviour {

		public float RotationSpeed = 0f;
		void Update() {
			if (RotationSpeed != 0f) {
				transform.Rotate(new Vector3(0, 0, RotationSpeed * Time.deltaTime));
			}
		}
	}
}
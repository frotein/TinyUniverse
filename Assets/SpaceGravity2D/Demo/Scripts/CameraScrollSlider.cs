using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SpaceGravity2D.Demo {
	/// <summary>
	/// Part of the Scroll Silder prefab
	/// </summary>
	public class CameraScrollSlider : MonoBehaviour {

		Slider _slider;
		public float MinOrtho=1f;
		public float OrthoDelta=10f; // max - min
		public bool mouseWheelAllowed = false;
		public float mouseWheelSpeed = 10f;

		void Start() {
			if (OrthoDelta < 0) {
				MinOrtho = MinOrtho - OrthoDelta;
				OrthoDelta = -OrthoDelta;
			}
			_slider = GetComponentInChildren<Slider>();
			if (_slider) {
				_slider.minValue = MinOrtho;
				_slider.maxValue = MinOrtho + OrthoDelta;
				if (!Camera.main.orthographic) {
					Debug.LogWarning("SpaceGravity2D.Demo: Camera is not orthographic!");
				}
				_slider.value = Camera.main.orthographicSize;
				_slider.onValueChanged.AddListener((float f) => { Camera.main.orthographicSize = f; });
			}
		}

		void Update() {
			if (mouseWheelAllowed && Camera.main != null) {
				float w = Input.GetAxis("Mouse ScrollWheel");
				if (w != 0f) {
					SetOrtho(Camera.main.orthographicSize - w * mouseWheelSpeed * Time.deltaTime);
				}
			}
		}

		/// <summary>
		/// Set cam ortho size and value of slider
		/// </summary>
		/// <param name="f">new ortho</param>
		public void SetOrtho(float f) {
			if (_slider) {
				_slider.value = Mathf.Clamp(f, MinOrtho, MinOrtho + OrthoDelta); //slider will set ortho to cam by itself
			}
			else {
				Camera.main.orthographicSize = f;
			}
		}
	}
}
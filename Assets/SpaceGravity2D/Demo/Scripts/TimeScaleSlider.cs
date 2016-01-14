using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace SpaceGravity2D.Demo {

	/// <summary>
	/// Part of timescale slider prefab;
	/// </summary>
	public class TimeScaleSlider : MonoBehaviour {

		public SimulationControl SimControl;

		public Slider slider;

		public float minValue = 1f;
		public float maxValue = 20f;

		void Start() {
			if ( !SimControl ) {
				SimControl = GameObject.FindObjectOfType<SimulationControl>();
			}
			if ( !slider ) {
				slider = GetComponentInChildren<Slider>();
			}
			if ( slider ) {
				slider.minValue = minValue;
				slider.maxValue = maxValue;
				if ( SimControl ) {
					slider.value = SimControl.TimeScale;
				}
				slider.onValueChanged.AddListener( ( float f ) => { if ( SimControl ) { SimControl.TimeScale = f; } } );
			}
		}

	}
}

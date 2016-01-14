using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SpaceGravity2D.Demo {

    /// <summary>
    /// Demo scene GUI script
    /// </summary>
    public class ShipHUD : MonoBehaviour {

        [Header("Fill all references")]
        public CelestialBody Target;
        public ShipController ShipControl;
        public Transform VelocityPanel;
        public Transform OrientationPanel;
        public Transform ToBottomPanel;
        public Button[] timeScaleButtons;
        float _defaultTimeScale;
        public Slider FuelSlider;
        public Transform MarkPrefab;
        Transform _markApoapsis;
        Transform _markPeriapsis;

        void Start() {
            if ( !Target ) {
                var ship = GameObject.Find( "Ship" );
                if ( ship ) {
                    Target = ship.GetComponentInParent<CelestialBody>();
                }
            }
            _defaultTimeScale = SimulationControl.instance.TimeScale;
            if ( timeScaleButtons.Length == 3 ) {
                timeScaleButtons[0].interactable = false;
            }
            if ( !ShipControl ) {
                ShipControl = Target.GetComponent<ShipController>();
            }
            if ( !FuelSlider ) {
				Debug.Log("SpaceGravity2D.ShipGUI: fuel slider ref is null");
            } else {
                FuelSlider.maxValue = ShipControl.MaxFuel;
                FuelSlider.value = ShipControl.Fuel;
            }
            if ( MarkPrefab ) {
                _markApoapsis = Instantiate( MarkPrefab ) as Transform;
                _markApoapsis.name = "apoapsisMark";
                _markApoapsis.GetComponentInChildren<Text>().text = "A";
                _markPeriapsis = Instantiate( MarkPrefab ) as Transform;
                _markPeriapsis.name = "periapsisMark";
                _markPeriapsis.GetComponentInChildren<Text>().text = "P";
            }

        }

        void Update() {
            RefreshHud();
        }

        void RefreshHud() {
            if ( !Target ) {
                enabled = false;
                return;
            }
            float camRot = Camera.main.transform.eulerAngles.z;
			if ( VelocityPanel ) {
				VelocityPanel.localRotation = Quaternion.Euler( new Vector3( 0, 0, Vector2.Angle( Target.RelativeVelocity, new Vector2( 0, 1 ) ) * ( Target.RelativeVelocity.x > 0 ? -1 : 1 ) - camRot ) );
			}
			if ( OrientationPanel ) {
				OrientationPanel.localRotation = Quaternion.Euler( new Vector3( 0, 0, Target._transform.eulerAngles.z - 90f - camRot ) );
			}
			if ( ToBottomPanel ) {
				ToBottomPanel.localRotation = Quaternion.Euler( new Vector3( 0, 0, Vector2.Angle( Target.RadiusVector, new Vector2( 0, 1 ) ) * ( Target.RadiusVector.x > 0 ? -1 : 1 ) + 180f - camRot ) );
			}
            if ( FuelSlider && ShipControl ) {
                FuelSlider.value = ShipControl.Fuel;
            }
            if ( _markPeriapsis && _markApoapsis ) {
                if ( SimulationControl.instance.drawOrbits && !float.IsNaN( Target.Periapsis.x ) ) {
                    if ( Target.Apoapsis != Vector2.zero ) {
                        _markApoapsis.gameObject.SetActive( true );
                        _markApoapsis.position = Target.Apoapsis;
                    } else {
                        _markApoapsis.gameObject.SetActive( false );
                    }
                    _markPeriapsis.gameObject.SetActive( true );
                    _markPeriapsis.position = Target.Periapsis;
                } else {
                    _markApoapsis.gameObject.SetActive( false );
                    _markPeriapsis.gameObject.SetActive( false );
                }
            }
        }

        /// <summary>
        /// Change TimeScale Buttons callbacks
        /// buttonId possible values = 0, 1, 2
        /// </summary>
        /// <param name="buttonId"></param>
        public void ChangeSpeed( int buttonId ) {
            if ( buttonId < 0 || buttonId > 2 ) {
                return;
            }
            if ( timeScaleButtons.Length == 3 ) {
                timeScaleButtons[buttonId].interactable = false;
                for ( int i = 0; i < 3; i++ ) {
                    if ( i == buttonId ) {
                        continue;
                    }
                    timeScaleButtons[i].interactable = true;
                }
            }
            SimulationControl.instance.TimeScale = _defaultTimeScale * Mathf.Pow( 5, buttonId );
        }

        public void ScaleCamUp() {
            var cammove = Camera.main.GetComponent<CameraMovement>();
            cammove.Scroll( 1 ); //dirty and not good
        }

        public void ScaleCamDown() {
            var cammove = Camera.main.GetComponent<CameraMovement>();
            cammove.Scroll( -1 );
        }
    }
}
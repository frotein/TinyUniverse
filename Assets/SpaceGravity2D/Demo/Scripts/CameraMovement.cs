using UnityEngine;

namespace SpaceGravity2D.Demo {

    /// <summary>
    /// Camera behaviour script
    /// </summary>
    public class CameraMovement : MonoBehaviour {

        public Transform Target;
        public float ScrollSpeed = 500f;
		public float OrbitShowOrtho = 15f;

        Transform _transform;
        Camera _cam;


        void Start() {
            _transform = transform;
            _cam = GetComponent<Camera>();
            if ( !Target ) {
                var ship = GameObject.Find( "Ship" );
                Target = ship ? ship.transform : null;
            }
            if ( !Target ) {
                enabled = false;
            }
        }

        void Update() {
            _transform.position = new Vector3( Target.position.x, Target.position.y, _transform.position.z ); //basic fallowing
			SimulationControl.instance.drawOrbits = _cam.orthographicSize >= OrbitShowOrtho;
        }

        /// <summary>
        /// Scroll() is called on scroll event from InputProvider
        /// </summary>
        /// <param name="delta">Orthographics size delta</param>
        public void Scroll( float delta ) {
            delta = delta * Mathf.Sqrt(_cam.orthographicSize);
			var scrollControl = GameObject.FindObjectOfType<CameraScrollSlider>();
			if ( scrollControl ) {
				scrollControl.SetOrtho( _cam.orthographicSize + delta * ScrollSpeed * Time.deltaTime * 60f );
			}
        }

        void OnEnable() {
            InputProvider.InputScrollEvent += Scroll;
        }

        void OnDisable() {
            InputProvider.InputScrollEvent -= Scroll;
        }
    }
}
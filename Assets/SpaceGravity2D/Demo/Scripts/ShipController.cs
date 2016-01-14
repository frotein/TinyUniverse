using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {

    /// <summary>
    /// basic Ship controller.
    /// </summary>
    public class ShipController : MonoBehaviour {

        Transform _transform;
        CelestialBody _cBody;
        Rigidbody2D _rigidBody2D;
        public float Acceleration = 1f;
        public float RotationSpeed = 1f;
        ParticleSystem _ps; //trail particlesSystem
        float _maxEmissionRate;
        bool _isAccelerating;
        float _lastAccelerationValue; //used to track if engine is throttled up 
        public float MaxFuel = 200f;
        public bool InfiniteFuel = false;
        public float FuelSpendRate = 1f;
        public float Fuel;
        public Transform ExplosionPrefab;
		public float MinRotatePointDistance = 0.1f;

        void Start() {
            Fuel = MaxFuel;
            _ps = GetComponentInChildren<ParticleSystem>();
            _maxEmissionRate = _ps.emissionRate;
            _transform = transform;
            _cBody = GetComponent<CelestialBody>();
            if ( !_cBody ) {
				Debug.Log("SpaceGravity2D.ShipController: Celestial Body component not found on " + name);
                enabled = false;
            }
            _rigidBody2D = GetComponent<Rigidbody2D>();
        }

        void OnCollisionEnter2D( Collision2D col ) {
            gameObject.SetActive( false );
            if ( ExplosionPrefab ) {
                var ex = Instantiate( ExplosionPrefab ) as Transform;
                ex.position = _transform.position;
                ex.GetComponent<ParticleSystem>().Play();
            }
			_cBody._simulationControl.TimeScale = 0;
            Invoke( "ReloadScene", 1f );
        }

        void ReloadScene() {
            Application.LoadLevel( Application.loadedLevelName );
        }

        public void AddFuel( float amount ) {
            Fuel = Mathf.Min( Fuel + amount, MaxFuel );
        }

        void Accelerate( float force ) {
            if ( Fuel > 0 || InfiniteFuel ) {
                if ( !InfiniteFuel ) {
                    Fuel -= FuelSpendRate * SimulationControl.instance.TimeScale;
                }
                _cBody.AddExternalVelocity( _transform.right * force * Acceleration * Time.deltaTime );
                _lastAccelerationValue = force;
                if ( !_isAccelerating ) {
                    StartCoroutine( TrackAcceleration() );
                }
            } else {
                Fuel = 0;
            }
        }

        void AccelerateToPoint( Vector2 point ) {
            Vector2 v = point - (Vector2)_transform.position;
            float distance = v.magnitude;
            float camSize = Camera.main.orthographicSize * 2f * ( Camera.main.aspect < 1 ? Camera.main.aspect : 1f );

            if ( distance > camSize / 10f ) {
                Accelerate( Mathf.Min( ( distance - camSize / 10f ) / camSize*3f, 1f ) );
            }
        }


		/// <param name="dir"> dir less than 0 = right, dir more than 0 = left </param>
        void Rotate( float dir ) {
            _rigidBody2D.AddTorque( -dir * RotationSpeed * Time.deltaTime );
        }

        void RotateToPoint( Vector2 point ) {
            Vector2 v = point - (Vector2)_transform.position;
			if ( v.magnitude > MinRotatePointDistance ) {
                float dir = v.x * _transform.right.y - v.y * _transform.right.x;
                Rotate( dir > 0 ? 1f : ( dir < 0 ? -1f : 0f ) );
            }
        }

        /// <summary>
        /// Coroutine is active when engine is working
        /// </summary>
        IEnumerator TrackAcceleration() {
            _isAccelerating = true;
            while ( _lastAccelerationValue > 0 ) {
                if ( !_ps.isPlaying ) {
                    _ps.Play();
                }
                _ps.emissionRate = _lastAccelerationValue * _maxEmissionRate;
                _lastAccelerationValue = 0; // Coroutine loop executes after Update(), so if next frame engine is still active - this variable will be reset to some non-zero value, and if not - coroutine will be finished until next engine activation
                yield return null;
            }
            _ps.Stop();
            _isAccelerating = false;
        }

        void OnEnable() {
            Subscribe();
        }

        void OnDisable() {
            Unsubscribe();
        }

        void Subscribe() {
            InputProvider.InputMoveEvent += Accelerate;
            InputProvider.InputRotateEvent += Rotate;
            InputProvider.InputRotateToPointEvent += RotateToPoint;
            InputProvider.InputMoveToPointEvent += AccelerateToPoint;
        }

        void Unsubscribe() {
            InputProvider.InputMoveEvent -= Accelerate;
            InputProvider.InputRotateEvent -= Rotate;
            InputProvider.InputRotateToPointEvent -= RotateToPoint;
            InputProvider.InputMoveToPointEvent -= AccelerateToPoint;
        }
    }
}
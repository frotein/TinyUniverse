using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {

    /// <summary>
    /// Used for asteroids
    /// </summary>
    public class RandomStartingRotation : MonoBehaviour {

        public float MaxTorqueForce = 100;

        void Start() {
            var rigid = GetComponent<Rigidbody2D>();
            if ( rigid ) {
                rigid.AddTorque( Random.Range( -MaxTorqueForce, MaxTorqueForce ) );
            }
            Destroy( this );
        }
    }
}
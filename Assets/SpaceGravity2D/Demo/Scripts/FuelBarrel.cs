using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {

    /// <summary>
    /// Class for making refuel objects. It is coupled to demo project and is not reusable
    /// </summary>
    public class FuelBarrel : MonoBehaviour {

        public float FuelAmount = 100f;

        void OnTriggerEnter2D( Collider2D col ) {
            var ship = col.GetComponentInParent<ShipController>();
            if ( ship ) {
                ship.AddFuel( FuelAmount );
                Destroy( this.gameObject );
            }
        }

    }
}

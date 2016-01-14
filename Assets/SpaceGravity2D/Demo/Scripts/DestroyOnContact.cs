using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {
	public class DestroyOnContact : MonoBehaviour {

		[Header("Only trigger colliders detectors")]
		public bool UseTrigger = true;

		[Header("Destroy others or this object")]
		public bool DestroyThis = false;

		public float VelocityTreshold = 0f;

		public Transform ExplosionPrefab;


		void OnTriggerEnter2D(Collider2D other) {
			if (UseTrigger) {
				var v1 = Vector2.zero;
				var v2 = Vector2.zero;
				var r1 = GetComponent<Rigidbody2D>();
				if (r1) {
					v1 = r1.velocity;
				}
				var r2 = GetComponent<Rigidbody2D>();
				if (r2) {
					v2 = r2.velocity;
				}
				DestroyObj(other.gameObject, v2 - v1);
			}
		}

		void OnCollisionEnter2D(Collision2D coll) {

			if (!UseTrigger) {
				DestroyObj(coll.gameObject, coll.relativeVelocity);
			}
		}

		void DestroyObj(GameObject obj, Vector2 relV) {
			if (DestroyThis) {
				if (VelocityTreshold > 0f) {
					if (relV.magnitude < VelocityTreshold) {
						return;
					}
				}
				if (ExplosionPrefab != null) {
					var ex = Instantiate(ExplosionPrefab) as Transform;
					ex.transform.position = this.transform.position;
					var ps = ex.GetComponent<ParticleSystem>();
					if (ps){
						ps.Play();
					}
					else {
						Destroy(ex.gameObject);//wrong prefab
					}
					Destroy(gameObject);
				}
			}
			else {
				var c = obj.GetComponent<SpaceCharacterController>();
				if (c != null) {
					c.OnCharDestroy();
				}
				else {
					if (!obj.GetComponent<ShipController>()) {
						Destroy(obj);
					}
				}
			}
		}
	}
}
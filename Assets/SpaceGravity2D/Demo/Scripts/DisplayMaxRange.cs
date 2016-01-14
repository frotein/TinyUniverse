using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {
	[ExecuteInEditMode]
	public class DisplayMaxRange : MonoBehaviour {
		public Sprite displaySprite;

		SpriteRenderer _srend;
		SimulationControl _sim;
		CelestialBody _cbody;

		void Awake() {
			CreateSpriteObj();
		}

		void CreateSpriteObj() {
			if ( !_srend && displaySprite ) {
				var t = transform.Find( "gravity_range_sprite" );
				if ( t ) {
					_srend = t.GetComponent<SpriteRenderer>();
					return;
				}
				var go = new GameObject( "gravity_range_sprite" );
				_srend = go.AddComponent<SpriteRenderer>();
				_srend.sprite = displaySprite;
				_srend.sortingOrder = 12;
				go.transform.SetParent( transform );
				go.transform.localPosition = new Vector3();
				go.transform.localScale = new Vector3( 1, 1, 1 );
			}
		}

		void Update() {
			if ( !_sim){
				_sim = GameObject.FindObjectOfType<SimulationControl>();
			}
			if (!_sim){
				return;
			}
			if ( !_srend ) {
				CreateSpriteObj();
			}
			if ( _srend ) {
				if (!_cbody){
					_cbody = GetComponent<CelestialBody>();
				}
				float maxrange = Mathf.Min(_sim.MaxAttractionRange, _cbody != null ? _cbody.MaxAttractionRange : _sim.MaxAttractionRange);
				if ( maxrange > 100000f){
					_srend.gameObject.SetActive(false);
					return;
				}else{
					_srend.gameObject.SetActive(true);
				}
				float radius =  _srend.sprite.pixelsPerUnit / _srend.sprite.texture.width * maxrange * 2f;
				_srend.transform.localScale = new Vector3( radius / transform.localScale.x, radius / transform.localScale.y, 1 / transform.localScale.z );
			}
		}
	}
}

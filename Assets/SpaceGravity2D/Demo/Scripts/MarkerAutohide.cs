using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {

	/// <summary>
	/// Component for making (primary) world space GUI labels autohiding and autoscaling relative to camera orthographic size
	/// </summary>
	public class MarkerAutohide : MonoBehaviour {
		public float visibleFromOrtho = 1f;
		public float visibleToOrtho = 1000f;
		public float sizeRelativeCamSize = 0.05f;
		public float maxAffectingCamOrtho = 100f;
		GameObject[] content;
		Vector2 visibleSize;

		void Start() {
			content = new GameObject[transform.childCount];
			var renderer = GetComponent<Renderer>();
			if ( renderer ) {
				//at testing this did not work at all, so it will be working differently for various sized objects.
				visibleSize = (Vector2)renderer.bounds.size;
			} else {
				visibleSize = new Vector2( 0.01f, 0.01f );
			}
			for ( int i = 0; i < content.Length; i++ ) {
				content[i] = transform.GetChild( i ).gameObject;
			}
			if ( !Camera.main.orthographic ) {
				Debug.LogWarning("SpaceGravity2D.Demo: Main camera is not orthographic!");
			}
		}

		void Update() {
			float ortho = Camera.main.orthographicSize;
			SetContentActive( ortho > visibleFromOrtho && ortho < visibleToOrtho );
			sizeRelativeCamSize = Mathf.Clamp( sizeRelativeCamSize, 0f, 1f );
			if ( sizeRelativeCamSize != 0 ) {
				ortho = Mathf.Min( ortho, maxAffectingCamOrtho );
				transform.localScale = new Vector3( visibleSize.x * ortho * sizeRelativeCamSize, visibleSize.y * ortho * sizeRelativeCamSize, 1f );
			}
		}

		void SetContentActive( bool active ) {
			for ( int i = 0; i < content.Length; i++ ) {
				content[i].SetActive( active );
			}
		}
	}
}
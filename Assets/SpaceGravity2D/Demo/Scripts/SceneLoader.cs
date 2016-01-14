using UnityEngine;
using System.Collections;

namespace SpaceGravity2D.Demo {
	public class SceneLoader : MonoBehaviour {

		public void LoadScene( string str ) {
			if ( str != "" ) {
				Application.LoadLevel( str );
			}
		}

		public void LoadScene(int ind) {
			Application.LoadLevel( ind );
		}
	}
}
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SpaceGravity2D.Demo {

    [System.Serializable]
    public class MessageText {
        public string Message;
        public float StartDelay;
        public float CharDelay = 0.1f;
    }


    /// <summary>
    /// Basic screen messages system
    /// </summary>
    public class MessageDisplaySystem : MonoBehaviour {

        public MessageText[] Messages;
        Text _text;

        void Start() {
            _text = GetComponent<Text>();
            if ( _text ) {
                if ( Messages.Length > 0 ) {
                    StartCoroutine( DisplayAll() );
                } else {
					Debug.Log("SpaceGravity2D.MessageDisplaySystem: No messages to display");
                }
            } else {
				Debug.Log("SpaceGravity2D.MessageDisplaySystem: No Text found");
            }
        }

        IEnumerator DisplayAll() {
            foreach ( var msg in Messages ) {
                yield return new WaitForSeconds( msg.StartDelay );
                if ( msg.CharDelay > 0 ) {
                    int chrs = 0;
                    while ( chrs < msg.Message.Length ) {
                        chrs++;
                        _text.text = msg.Message.Substring( 0, chrs );
                        yield return new WaitForSeconds( msg.CharDelay );
                    }
                } else {
                    _text.text = msg.Message;
                }
            }
            yield return new WaitForSeconds( 5f );
            _text.text = "";
        }

    }
}
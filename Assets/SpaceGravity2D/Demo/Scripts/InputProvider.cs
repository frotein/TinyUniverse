using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceGravity2D.Demo {

    /// Events types
    public delegate void InputRotateToPointEventHandler( Vector2 point );
    public delegate void InputRotateEventHandler( float direction );
    public delegate void InputScroolEventHandler( float direction );
    public delegate void InputMoveToPointEventHandler( Vector2 point );
    public delegate void InputMoveEventHandler( float power );


    /// <summary>
    /// Input events provider script. It is coupled to demo ship controlling and is not reusable
    /// </summary>
    public class InputProvider : MonoBehaviour {

        /// Static events
        public static event InputRotateToPointEventHandler InputRotateToPointEvent;
        public static event InputRotateEventHandler InputRotateEvent;
        public static event InputMoveToPointEventHandler InputMoveToPointEvent;
        public static event InputMoveEventHandler InputMoveEvent;
        public static event InputScroolEventHandler InputScrollEvent;

        delegate void CurrentScreenInputHandler();
        CurrentScreenInputHandler ProcessScreenInputHandler; //Used for selecting input source between mouse and touch

        // Use this for initialization
        void Start() {
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
		    ProcessScreenInputHandler = TouchInput;
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER || UNITY_STANDALONE
			ProcessScreenInputHandler = MouseInput;
#endif
        }

        void MouseInput() {
            if ( Input.GetMouseButton( 0 ) ) {
                if ( InputRotateToPointEvent != null ) {
                    InputRotateToPointEvent( Camera.main.ScreenToWorldPoint( Input.mousePosition ) );
                }
            }
            if ( Input.GetMouseButton( 1 ) ) {
                if ( InputMoveToPointEvent != null ) {
                    InputMoveToPointEvent( Camera.main.ScreenToWorldPoint( Input.mousePosition ) );
                }
            }
            float mouseWheel = Input.GetAxis( "Mouse ScrollWheel" );
            if ( mouseWheel != 0 && InputScrollEvent != null ) {
                InputScrollEvent( -mouseWheel );
            }
        }

        void TouchInput() {
            if ( Input.touchCount == 0 ) {
                return;
            }
            var touch = Input.GetTouch( 0 );
            var worldPoint = Camera.main.ScreenToWorldPoint( touch.position );
            if ( InputRotateToPointEvent != null ) {
                InputRotateToPointEvent( worldPoint );
            }
            if ( InputRotateToPointEvent != null ) {
                InputRotateToPointEvent( worldPoint );
            }
            // touch scroll code may be putted here
        }

        void KeyboardInput() {
            float x = Input.GetAxis( "Horizontal" );
            if ( x != 0f && InputRotateEvent != null ) {
                InputRotateEvent( x );
            }
            float y = Input.GetAxis( "Vertical" );
            if ( y > 0 && InputMoveEvent != null ) {
                InputMoveEvent( y );
            }
        }

        void Update() {
            if ( EventSystem.current==null || !EventSystem.current.IsPointerOverGameObject() ) { //One way for making GUI blocking screen input
                ProcessScreenInputHandler();
            }
            KeyboardInput();
        }
    }
}
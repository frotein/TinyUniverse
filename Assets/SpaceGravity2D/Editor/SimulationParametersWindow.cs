using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
namespace SpaceGravity2D.Inspector {
	[InitializeOnLoad]
	public class SimulationParametersWindow : EditorWindow {

		/// <summary>
		/// this reference is used to access global properties, as it is Singleton class
		/// </summary>
		SimulationControl _simControl;

		/// <summary>
		/// Serialized object requiered because this window is used as inspector for this SimulationControl
		/// </summary>
		SerializedObject _simControlSerialized;

		/// <summary>
		/// Class for managing scene widnow elements display
		/// </summary>
		static SceneViewDisplayManager _displayManager;

		//public static SimulationParametersWindow Instance;

		#region serialized properties

		SerializedProperty gravConstProp;
		SerializedProperty inflRangeProp;
		SerializedProperty inflRangeMinProp;
		SerializedProperty timeScaleProp;
		SerializedProperty minMassProp;
		SerializedProperty drawArrowsProp;
		SerializedProperty drawEditorOrbsProp;
		SerializedProperty drawDisabledOrbsProp;
		SerializedProperty drawPlayOrbsProp;
		SerializedProperty lineRendMatProp;
		SerializedProperty orbitWidthProp;
		SerializedProperty orbitPointsProp;
		SerializedProperty drawDebugPointsProp;
		SerializedProperty arrowsSizeProp;
		SerializedProperty arrowsGlobalProp;
		SerializedProperty calcTypeProp;

		#endregion

		#region initialization
		[MenuItem("Window/Space Gravity 2D Window")]
		public static void ShowWindow() {
			EditorWindow.GetWindow<SimulationParametersWindow>();
		}

		static SimulationParametersWindow() {
			if (_displayManager == null) {
				_displayManager = new SceneViewDisplayManager(null);
			}
		}

		void OnEnable() {
			if (_displayManager == null) {
				_displayManager = new SceneViewDisplayManager(null);
			}

		}

		/// <summary>
		/// Look for SimulationControl and, if fail, create it
		/// </summary>
		public static SimulationControl FindOrCreateSimulationControlGameObject() {
			if (SimulationControl.instance != null) {
				return SimulationControl.instance;
			}
			var simControl = GameObject.FindObjectOfType<SimulationControl>();
			if (!simControl) {
				simControl = CreateSimulationControl();
				SimulationControl.instance = simControl;
			}
			return simControl;
		}

		[MenuItem("GameObject/SpaceGravity2D/Simulation Control")]
		public static SimulationControl CreateSimulationControl() {
			var obj = new GameObject("SimulationControl");
			Undo.RegisterCreatedObjectUndo(obj, "SimControl creation");
			//Debug.Log("SpaceGravity2D: Simulation Control created");
			return Undo.AddComponent<SimulationControl>(obj);
		}
		/// <summary>
		/// creating serialized properties:
		/// </summary>
		void InitializeProperties() {
			_simControlSerialized = new SerializedObject(_simControl);
			gravConstProp = _simControlSerialized.FindProperty("GravitationalConstant");
			inflRangeProp = _simControlSerialized.FindProperty("MaxAttractionRange");
			inflRangeMinProp = _simControlSerialized.FindProperty("MinAttractionRange");
			timeScaleProp = _simControlSerialized.FindProperty("TimeScale");
			minMassProp = _simControlSerialized.FindProperty("MinAttractorMass");
			drawArrowsProp = _simControlSerialized.FindProperty("drawVelocityVectors");
			drawEditorOrbsProp = _simControlSerialized.FindProperty("drawOrbitsInEditor");
			drawDisabledOrbsProp = _simControlSerialized.FindProperty("drawDisabledOrbitsInEditor");
			drawPlayOrbsProp = _simControlSerialized.FindProperty("drawOrbits");
			lineRendMatProp = _simControlSerialized.FindProperty("orbitLineRendererMat");
			orbitWidthProp = _simControlSerialized.FindProperty("orbitsLinesWidth");
			orbitPointsProp = _simControlSerialized.FindProperty("orbitPointsCount");
			drawDebugPointsProp = _simControlSerialized.FindProperty("drawDebugOrbitPoints");
			arrowsSizeProp = _simControlSerialized.FindProperty("arrowsSize");
			arrowsGlobalProp = _simControlSerialized.FindProperty("globalVelocities");
			calcTypeProp = _simControlSerialized.FindProperty("CalculationType");
		}
		#endregion

		#region scenewindow and editorwindow GUI

		/// <summary>
		/// General window onGUI
		/// </summary>
		void OnGUI() {
			if (!_simControl || _simControlSerialized == null) {
				_simControl = FindOrCreateSimulationControlGameObject();
				if (!_simControl) {//check if creation process failed
					Debug.LogWarning("SpaceGravity2D.Editor: no Simulation Control found or created");
					this.Close();
					return;
				}
				InitializeProperties();
			}
			if (SimulationControl.instance == null) {
				SimulationControl.instance = _simControl;
			}
			_simControlSerialized.Update();
			EditorGUILayout.LabelField("Tools:", EditorStyles.boldLabel);
			if (GUILayout.Button("Inverse velocity for all selected celestial bodies")) {
				InverseVelocityFor(Selection.gameObjects); //Undo functionality is supported
			}
			if (GUILayout.Button(new GUIContent("Place all selected bodies randomly around attractor", "Convinient to create asteroids belts. \nusage: all selected bodies must have same attracor. before making selection and pressing this button, place one body at preffered minimal distance and one body - at preffered max distance. placement distances bounds will be calculated from bodies start positions."))) {
				PlaceBodiesRandomly(Selection.gameObjects);
			}
			try {
				EditorGUILayout.LabelField("Global Parameters:", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(calcTypeProp, new GUIContent("N-Body algorithm", "Euler - fastest, \nVerlet - fast and more stable, \nRungeKutta - more precise"));
				EditorGUILayout.PropertyField(gravConstProp);
				var gravConst = EditorGUILayout.FloatField(new GUIContent("Grav.Const. Proportional", "Change gravitational constant AND keep all orbits unaffected"), gravConstProp.floatValue);
				if (gravConst != gravConstProp.floatValue) {
					if (Mathf.Abs(gravConst) < 1e-8f) {
						gravConst = 1e-4f;
					}
					_simControl.GravitationalConstantProportional = gravConst;
				}
				EditorGUILayout.PropertyField(inflRangeProp);
				EditorGUILayout.PropertyField(inflRangeMinProp);
				EditorGUILayout.PropertyField(timeScaleProp);
				EditorGUILayout.PropertyField(minMassProp);
				EditorGUILayout.PropertyField(drawArrowsProp);
				EditorGUILayout.PropertyField(drawEditorOrbsProp);
				EditorGUILayout.PropertyField(drawDisabledOrbsProp, new GUIContent("Draw disabled orbits in editor", "Use if you want to see all orbits, even if celestial body has disabled orbit display"));
				EditorGUILayout.PropertyField(drawPlayOrbsProp);
				EditorGUILayout.LabelField("Orbit Line Renderer:", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(lineRendMatProp);
				EditorGUILayout.PropertyField(orbitWidthProp);
				EditorGUILayout.PropertyField(orbitPointsProp);
				EditorGUILayout.LabelField("Editor Parameters:", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(drawDebugPointsProp);
				EditorGUILayout.PropertyField(arrowsSizeProp);
				EditorGUILayout.PropertyField(arrowsGlobalProp, new GUIContent("Global velocity vectors", "Toggle global/local space for velocity arrows"));
				_simControl.selectWhenDraggingArrow = EditorGUILayout.Toggle("Select object with arrow", _simControl.selectWhenDraggingArrow);
			}
			catch (Exception ex) {
				Debug.LogError("SpaceGravity2D.Editor: " + ex.Message);
				Close();
			}
			if (GUI.changed) {
				_simControlSerialized.ApplyModifiedProperties();
				SceneView.RepaintAll();
			}
		}


		#endregion
		/// <summary>
		/// Tool: inverse velocity of selection
		/// </summary>
		static public void InverseVelocityFor(GameObject[] objects) {
			foreach (var obj in objects) {
				var cBody = obj.GetComponent<CelestialBody>();
				if (cBody) {
					Undo.RecordObject(cBody, "Inverse velocity");
					cBody.RelativeVelocity = -cBody.RelativeVelocity;
					cBody.TerminateRailMotion();
					Undo.IncrementCurrentGroup();
					EditorUtility.SetDirty(cBody);
				}
			}
		}

		/// <summary>
		/// Tool: place selected bodies in random positions in circle which is restricted by starting min and max distances.
		/// </summary>
		public static void PlaceBodiesRandomly(GameObject[] objects) {
			var cbs = new List<CelestialBody>();
			CelestialBody attr = null;
			var mindist = float.MaxValue;
			var maxdist = 0f;
			foreach (var obj in objects) {
				var cBody = obj.GetComponent<CelestialBody>();
				if (cBody) {
					if (cBody.Attractor != null) {
						if (attr == null) {
							attr = cBody.Attractor;
						}
						else {
							if (attr != cBody.Attractor) {
								Debug.LogWarning("SpaceGravity2D: Operation aborted. Selected bodies has different attractors");
								//abort operation to avoid misselection problems
								return;
							}
						}
						cbs.Add(cBody);
						var dist = ( (Vector2)( cBody.Attractor.transform.position - cBody.transform.position ) ).magnitude;
						mindist = Math.Min(dist, mindist);
						maxdist = Math.Max(dist, maxdist);
					}
				}
			}
			if (mindist < float.MaxValue && maxdist > 0) {
				for (int i = 0; i < cbs.Count; i++) {
					var angle = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
					var dist = UnityEngine.Random.Range(mindist, maxdist);
					Undo.RecordObject(cbs[i]._transform, "Place randomly");
					cbs[i].RelativePosition = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * dist;
				}
			}
		}

	}

	public class SceneViewDisplayManager {
		/// <summary>
		/// this reference is used to access global properties, as it is Singleton class
		/// </summary>
		SimulationControl _simControl;
		/// <summary>
		/// not null only during process of dragging velocity vector in editor
		/// </summary>
		CelestialBody _draggingObject;
		/// <summary>
		/// stores all bodies references in scene and is updated every onGUI frame
		/// </summary>
		List<CelestialBody> _bodies;
		/// <summary>
		/// sceneView buttons texture, which is autoloaded at init time from Assets/Resources folder
		/// </summary>
		Texture2D _arrowsBtnImage;
		Texture2D _orbitsBtnImage;

		double _waitDuration = 1;
		double _waitTime = 0;

		/// <summary>
		/// simControl may be null
		/// </summary>
		public SceneViewDisplayManager(SimulationControl simControl) {
			_simControl = simControl;
			_bodies = new List<CelestialBody>();
			EditorApplication.update += StartupUpdate;
		}

		/// <summary>
		/// Editor update delegate. 
		/// Subscribe OnSceneGUI delegate if _simControl is not null. If SimulationControl is not exists on scene, continuously retry untill it's not created
		/// </summary>
		void StartupUpdate() {
			if (_waitTime > EditorApplication.timeSinceStartup) {
				return; ///Wait for check time
			}
			if (_simControl == null) {
				if (SimulationControl.instance != null) {
					_simControl = SimulationControl.instance;
				}
				else {
					var simControl = GameObject.FindObjectOfType<SimulationControl>();
					if (simControl != null) {
						SimulationControl.instance = simControl;
						_simControl = simControl;
					}
					else {
						_waitTime = EditorApplication.timeSinceStartup + _waitDuration;
						return; ///If simulation control is not created, exit and wait for next check time
					}
				}
			}
			_arrowsBtnImage = Resources.Load("Textures/arrowsBtn") as Texture2D;
			_orbitsBtnImage = Resources.Load("Textures/orbitsBtn") as Texture2D;
			// subscribe our OnSceneGUI for updates callbacks
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			//Instance = this;
			SceneView.RepaintAll();
			EditorApplication.update -= StartupUpdate; //Don't call this function any more.
		}

		/// <summary>
		/// Draw all velocitiy vectors and orbits in scene window and process mouse dragging events.
		/// </summary>
		/// <param name="sceneView"></param>
		public void OnSceneGUI(SceneView sceneView) {
			if (!_simControl) {
				SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
				EditorApplication.update += StartupUpdate;
				return;
			}
			//caching scene celestial bodies for using in different methods
			_bodies.Clear();
			_bodies.AddRange(GameObject.FindObjectsOfType<CelestialBody>());
			if (_simControl.drawOrbitsInEditor) {
				DrawAllOrbitsInEditor();
			}
			if (_simControl.drawDebugOrbitPoints) {
				DrawOrbitsPointsInEditor();
			}
			if (_simControl.drawVelocityVectors) {
				ProcessVelocityArrows();
				if (!_draggingObject) {
					CatchMouseHoversArrows();
				}
			}
			if (_simControl.drawLinesToAttractors) {
				DrawLinesToAttractors();
			}
			//Buttons drawing block:
			Handles.BeginGUI();
			if (GUI.Button(new Rect(10, 10, 40, 40), _arrowsBtnImage)) {
				Undo.RecordObject(_simControl, "toggle velocity arrows display");
				_simControl.drawVelocityVectors = !_simControl.drawVelocityVectors;
				EditorUtility.SetDirty(_simControl);
			}
			if (GUI.Button(new Rect(10, 50, 40, 40), _orbitsBtnImage)) {
				Undo.RecordObject(_simControl, "toggle orbits display");
				_simControl.drawOrbitsInEditor = !_simControl.drawOrbitsInEditor;
				EditorUtility.SetDirty(_simControl);
			}
			Handles.EndGUI();
		}

		#region different stuff

		/// <summary>
		/// Process mouse drag and hover events and draw velocity vectors if enabled
		/// </summary>
		void ProcessVelocityArrows() {
			Event _event = Event.current;
			var eventType = _event.GetTypeForControl(GUIUtility.GetControlID(FocusType.Passive));
			if (eventType == EventType.MouseDown && _draggingObject == null) {
				if (_event.button == 0) {
					Vector2 point = new Vector2();
					if (GetMousePointOnXYPlane(ref point)) {
						CheckBeginDrag(point);
						if (_draggingObject != null) {
							_draggingObject.IsOnRailsMotion = false;
							GUIUtility.hotControl = 0;
							_event.Use();
						}
					}
				}
			}

			if (eventType == EventType.mouseDrag && _draggingObject != null) {
				if (_event.button == 0) {
					Vector2 point = new Vector2();
					if (GetMousePointOnXYPlane(ref point)) {
						_draggingObject.TerminateRailMotion();
						Undo.RecordObject(_draggingObject, "Velocity change");
						if (_simControl.globalVelocities) {
							_draggingObject.Velocity = ( point - (Vector2)_draggingObject.transform.position ) / _simControl.arrowsSize;
						}
						else {
							_draggingObject.RelativeVelocity = ( point - (Vector2)_draggingObject.transform.position ) / _simControl.arrowsSize;
						}
						_event.Use();
					}
				}
			}

			if (eventType == EventType.mouseUp || eventType == EventType.mouseMove) {
				if (_draggingObject != null) {
					if (_event.button == 0) {
						EditorUtility.SetDirty(_draggingObject);
						_draggingObject = null;
						_event.Use();
					}
				}
			}
			ShowAllVelocitiesVectors();
		}

		/// <summary>
		/// Check mouseover by comparing mouse and all velocity vectors positions
		/// </summary>
		void CatchMouseHoversArrows() {
			Vector2 point = new Vector2();
			if (GetMousePointOnXYPlane(ref point)) {
				foreach (var body in _bodies) {
					if (!body.isActiveAndEnabled) {
						continue;
					}
					var velocity = _simControl.globalVelocities ? body.Velocity : body.RelativeVelocity;
					Vector2 velocityPoint = (Vector2)body.transform.position + velocity * _simControl.arrowsSize;
					float minRange = velocity.magnitude * _simControl.arrowsSize / 6f;
					if (( point - velocityPoint ).magnitude < minRange) {
						Handles.DrawWireDisc(velocityPoint, Vector3.forward, minRange);
						Handles.color = new Color(1f, 1f, 1f, 0.1f);
						Handles.DrawSolidDisc(velocityPoint, Vector3.forward, minRange);
						SceneView.RepaintAll();
					}
				}
			}
		}

		bool GetMousePointOnXYPlane(ref Vector2 result) {
			Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			float k = !Mathf.Approximately(ray.direction.z, 0) ? ( ray.origin.z / ray.direction.z ) : 1f;
			//if k >0 mouse ray is not intercepting world XY plane
			if (k <= 0) {
				result.x = ray.origin.x - ray.direction.x * k;
				result.y = ray.origin.y - ray.direction.y * k;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Draw velocity vectors in scene view for all active celestial bodies
		/// </summary>
		void ShowAllVelocitiesVectors() {
			foreach (var body in _bodies) {
				if (body.isActiveAndEnabled && !body.IsFixedPosition) {
					DrawArrow(body.transform.position, body.transform.position + (Vector3)( _simControl.globalVelocities ? body.Velocity : body.RelativeVelocity ), Selection.activeTransform != null && Selection.activeTransform == body._transform ? Color.cyan : Color.green);
				}
			}
		}

		/// <summary>
		/// check if click is picked up arrow of velocity vector
		/// </summary>
		/// <param name="mousepoint"></param>
		void CheckBeginDrag(Vector2 mousepoint) {
			CelestialBody closest = null;
			float minDist = 10000;
			foreach (var body in _bodies) {
				if (!body.IsFixedPosition && body.isActiveAndEnabled) {
					//get world position of velocity vector
					var velocity = _simControl.globalVelocities ? body.Velocity : body.RelativeVelocity;
					Vector2 velocityPoint = (Vector2)body.transform.position + velocity * _simControl.arrowsSize;
					float minRange = velocity.magnitude * _simControl.arrowsSize / 6f;
					//compare it with mouse position and store closest one
					if (( mousepoint - velocityPoint ).magnitude < minRange) {
						float dist = ( mousepoint - velocityPoint ).magnitude;
						if (dist < minDist) {
							minDist = dist;
							closest = body;
						}
					}
				}
			}
			// if some velocity vector is found, its owner will be stored for further use
			_draggingObject = closest;
			if (_simControl.selectWhenDraggingArrow && _draggingObject != null) {
				Selection.activeGameObject = _draggingObject.gameObject;
			}
		}

		/// <summary>
		/// Draw simple arrow in scene window at given world coordinates
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		void DrawArrow(Vector3 from, Vector3 to, Color col) {
			if (_simControl.arrowsSize < 0.01f || _simControl.arrowsSize > 100f) {
				_simControl.arrowsSize = 1f;
			}
			var dir = to - from;
			float dist = dir.magnitude;
			var dirNorm = dir / dist; //normalized vector
			float headSize = dist / 6f;
			var _colBefore = Handles.color;
			Handles.color = col;
			Handles.DrawLine(from, from + dirNorm * ( dist - headSize ) * _simControl.arrowsSize);
			Handles.DrawLine(from + dirNorm * ( dist - headSize ) * _simControl.arrowsSize + new Vector3(dirNorm.y, -dirNorm.x) * headSize * _simControl.arrowsSize / 2f,
							  from + dirNorm * ( dist - headSize ) * _simControl.arrowsSize - new Vector3(dirNorm.y, -dirNorm.x) * headSize * _simControl.arrowsSize / 2f);
			Handles.DrawLine(from + dirNorm * ( dist - headSize ) * _simControl.arrowsSize + new Vector3(dirNorm.y, -dirNorm.x) * headSize * _simControl.arrowsSize / 2f, from + dir * _simControl.arrowsSize);
			Handles.DrawLine(from + dirNorm * ( dist - headSize ) * _simControl.arrowsSize - new Vector3(dirNorm.y, -dirNorm.x) * headSize * _simControl.arrowsSize / 2f, from + dir * _simControl.arrowsSize);
			Handles.color = _colBefore;
		}

		/// <summary>
		/// Draw orbits for bodies which has drawing orbit enabled
		/// </summary>
		void DrawAllOrbitsInEditor() {
			foreach (var body in _bodies) {
				if (body.IsDrawOrbit || _simControl.drawDisabledOrbitsInEditor) {
					DrawOrbitInEditorFor(body);
				}
			}
		}

		/// <summary>
		/// Draw orbit. All needed orbit data will be calculated if is not in playmode.
		/// Orbit points count is provided by SimControl
		/// </summary>
		/// <param name="body"></param>
		void DrawOrbitInEditorFor(CelestialBody body) {
			int pointsCount = (int)_simControl.orbitPointsCount;
			if (body.isActiveAndEnabled && !body.IsFixedPosition) {
				if (!Application.isPlaying) {
					if (_draggingObject == null || body == _draggingObject) {
						body.CalculateNewOrbitData();
					}
				}
				var points = body.GetOrbitPoints(pointsCount);
				for (int i = 1; i < points.Length; i++) {
					Handles.DrawLine((Vector3)points[i - 1], (Vector3)points[i]);
					//Handles.DrawDottedLine( (Vector3)points[i - 1], (Vector3)points[i], 12f ); //pretty expensive for performance
				}
			}
		}

		/// <summary>
		/// Debug draw orbit points for all bodies
		/// </summary>
		void DrawOrbitsPointsInEditor() {
			foreach (var body in _bodies) {
				DrawOrbitPointsInEditorFor(body);
			}
		}

		/// <summary>
		/// Debug draw periapsis, apoapsis and center points in scene window
		/// </summary>
		/// <param name="body"></param>
		static void DrawOrbitPointsInEditorFor(CelestialBody body) {
			if (!body.isActiveAndEnabled || body.IsFixedPosition || body.Attractor == null || body.RelativeVelocity == Vector2.zero || !body.IsValidOrbit()) {
				return;
			}
			var size = 1f;
			DebugDrawX(body.CenterPoint, size, Color.red);
			DebugDrawX(body.Periapsis, size, Color.green);
			Handles.color = Color.blue;
			/*
			Handles.DrawLine( body._transform.position, body.Attractor._transform.position );
			 * //additional optional debug lines:
			Vector2 meanLine = new Vector2( 0, 3 ) * body._radiusVectorMagnitude;
			meanLine = CelestialBody.RotateVectorByAngle( meanLine, body._argumentOfPeriapsis + body._meanAnomaly );
			Handles.color = Color.yellow;
			Handles.DrawLine( body._centerPoint, body._centerPoint + meanLine );
			Vector2 eccLine = new Vector2( 0, 3 ) * body._radiusVectorMagnitude;
			eccLine = CelestialBody.RotateVectorByAngle( eccLine, body._argumentOfPeriapsis + body._eccentricAnomaly );
			Handles.color = Color.white;
			Handles.DrawLine( body._centerPoint, body._centerPoint + eccLine );
			Vector2 trueLine =new Vector2( 0, 2 ) * body._radiusVectorMagnitude;
			trueLine = CelestialBody.RotateVectorByAngle( trueLine, body._argumentOfPeriapsis + body._trueAnomaly );
			Handles.color = Color.cyan;
			Handles.DrawLine( body._focusPoint, body._focusPoint + trueLine );
			 */
			Handles.color = Color.white;
			if (body.Eccentricity < 1) {
				DebugDrawX(body.Apoapsis, size, Color.yellow); //apoasis become infinity if eccentricity >= 1
			}
		}

		void DrawLinesToAttractors() {
			var prevCol = Handles.color;
			Handles.color = Color.magenta;
			foreach (var body in _bodies) {
				if (body.isActiveAndEnabled && body.Attractor != null) {
					Handles.DrawLine(body._transform.position, body.Attractor._transform.position);
				}
			}
			Handles.color = prevCol;
		}

		/// <summary>
		/// Draw two crossing lines in scene view
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="size"></param>
		/// <param name="col"></param>
		static void DebugDrawX(Vector2 pos, float size, Color col) {
			Handles.color = col;
			Handles.DrawLine(pos + Vector2.up * size, pos - Vector2.up * size);
			Handles.DrawLine(pos + Vector2.right * size, pos - Vector2.right * size);
		}



		#endregion
	}
}
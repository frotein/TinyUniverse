using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
namespace SpaceGravity2D.Inspector {

	[CustomEditor(typeof(CelestialBody))]
	[CanEditMultipleObjects()]
	public class CelestialBodyEditor : Editor {

		CelestialBody _body;
		CelestialBody[] _bodies;
		SerializedProperty dynamicChangeIntervalProp;
		SerializedProperty attractorProp;
		SerializedProperty maxInfRangeProp;
		SerializedProperty velocityProp;


		[MenuItem("GameObject/SpaceGravity2D/CelestialBody")]
		public static void CreateGameObject() {
			var go = new GameObject("CelestialBody");
			Undo.RegisterCreatedObjectUndo(go, "new CelestialBody");
			go.AddComponent<CelestialBody>();
			Selection.activeObject = go;
		}

		void OnEnable() {
			//initialize properties to display
			_body = target as CelestialBody;
			var celestials = new List<CelestialBody>();
			for (int i = 0; i < Selection.gameObjects.Length; i++) {
				var gocb = Selection.gameObjects[i].GetComponent<CelestialBody>();
				if (gocb != null) {
					celestials.Add(gocb);
				}
			}
			_bodies = celestials.ToArray();
			for (int i = 0; i < _bodies.Length; i++) {
				_bodies[i].FindReferences();
			}
			if (!_body._simulationControl) {
				_body._simulationControl = SimulationParametersWindow.FindOrCreateSimulationControlGameObject();
			}
			dynamicChangeIntervalProp = serializedObject.FindProperty("SearchAttractorInterval");
			attractorProp = serializedObject.FindProperty("Attractor");
			maxInfRangeProp = serializedObject.FindProperty("MaxAttractionRange");
			velocityProp = serializedObject.FindProperty("Velocity");

			AssignIconImage();
		}

		void AssignIconImage() {
			Texture2D icon = (Texture2D)Resources.Load("Textures/icon");
			if (icon != null) {
				typeof(EditorGUIUtility).InvokeMember(
					"SetIconForObject",
					BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
					null,
					null,
					new object[] { _body, icon }
				);
			}
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			ShowActionsButtons();
			ShowToggles();
			ShowGravityProperties();
			ShowVectors();
			ShowOrbitParameters();

			if (GUI.changed) {
				for (int i = 0; i < _bodies.Length; i++) {
					EditorUtility.SetDirty(_bodies[i]);
				}
			}
		}

		void ShowToggles() {
			EditorGUILayout.LabelField("Toggles:", EditorStyles.boldLabel);
			var isFixedPropValue = GUILayout.Toggle(_body.IsFixedPosition, new GUIContent("Is fixed position", "Relative to attractor"), "Button");
			if (isFixedPropValue != _body.IsFixedPosition) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Toggle fixed position");
					_bodies[i].IsFixedPosition = isFixedPropValue;
				}
			}
			//===============
			var useRailValue = GUILayout.Toggle(_body.UseRailMotion, new GUIContent("Use RailMotion", "Keplerian/Newtonian motion type toggle"), "Button");
			if (useRailValue != _body.UseRailMotion) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Toggle keplerian motion");
					_bodies[i].UseRailMotion = useRailValue;
				}
			}
			//===============
			var drawOrbitValue = GUILayout.Toggle(_body.IsDrawOrbit, new GUIContent("Draw Orbit", "Drawing orbits depends on global settings"), "Button");
			if (drawOrbitValue != _body.IsDrawOrbit) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Toggle object orbit draw");
					_bodies[i].IsDrawOrbit = drawOrbitValue;
				}
			}
			//===============
			if (!_body.UseRailMotion) {
				GUI.enabled = false; //turn off button if not keplerian body
			}
			var ignoreCollValue = GUILayout.Toggle(_body.IgnoreAllCollisions, new GUIContent("Ignore all collision", "Body will not change velocity vector on collisions. Works only for 'rail' motion and when body has attractor."), "Button");
			if (ignoreCollValue != _body.IgnoreAllCollisions) {
				for (int i = 0; i < _bodies.Length; i++) {
					if (_bodies[i].UseRailMotion) {
						Undo.RecordObject(_bodies[i], "Toggle ignore collisions");
						_bodies[i].IgnoreAllCollisions = ignoreCollValue;
					}
				}
			}
			if (!_body.UseRailMotion) {
				GUI.enabled = true;
			}
			//===============
			var dynamicChangingValue = GUILayout.Toggle(_body.IsUsingDynamicAttractorChanging, new GUIContent("Dynamic attractor changing", "search most proper attractor continiously. It is recommended not to use on many objects due to performance. For large amount of bodies better to use spheres-of-influence colliders"), "Button");
			if (dynamicChangingValue != _body.IsUsingDynamicAttractorChanging) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Toggle attractor searching");
					_bodies[i].IsUsingDynamicAttractorChanging = dynamicChangingValue;
				}
			}
			//===============
			if (dynamicChangingValue) {
				EditorGUI.showMixedValue = dynamicChangeIntervalProp.hasMultipleDifferentValues;
				var intervalValue = EditorGUILayout.FloatField(new GUIContent("search interval", "in seconds"), dynamicChangeIntervalProp.floatValue);
				if (intervalValue != dynamicChangeIntervalProp.floatValue) {
					for (int i = 0; i < _bodies.Length; i++) {
						Undo.RecordObject(_bodies[i], "Change search interval");
						_bodies[i].SearchAttractorInterval = intervalValue;
					}
				}
				EditorGUI.showMixedValue = false;
			}
		}

		void ShowGravityProperties() {
			var mixedMass = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].Mass != _body.Mass) {
					mixedMass = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedMass;
			var massValue = EditorGUILayout.Slider("Mass", _body.Mass, 1e-6f, 1e6f);
			if (massValue != _body.Mass) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i]._rigidbody2D, "Mass change");
					_bodies[i].Mass = massValue;
				}
			}
			/// ==============
			EditorGUI.showMixedValue = maxInfRangeProp.hasMultipleDifferentValues;
			var maxInfValue = EditorGUILayout.FloatField(
				new GUIContent("influence range:", "Body's own max influence range of attraction force. this option competes with the same global property"),
				maxInfRangeProp.floatValue
				);
			if (maxInfValue != maxInfRangeProp.floatValue) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Body gravity range change");
					_bodies[i].MaxAttractionRange = maxInfValue;
				}
			}
			/// ==============
			EditorGUI.showMixedValue = attractorProp.hasMultipleDifferentValues;
			var attrValue = EditorGUILayout.ObjectField(new GUIContent("Attractor"), attractorProp.objectReferenceValue, typeof(CelestialBody), true) as CelestialBody;
			if (attrValue != attractorProp.objectReferenceValue) {
				for (int i = 0; i < _bodies.Length; i++) {
					if (_bodies[i] != attrValue) {
						Undo.RecordObject(_bodies[i], "Attractor ref change");
						_bodies[i].Attractor = attrValue;
					}
				}
			}
			if (GUILayout.Button("Remove attractor")) {
				for (int i = 0; i < _bodies.Length; i++) {
					if (_bodies[i].Attractor != null) {
						Undo.RecordObject(_bodies[i], "Removing attractor ref");
						_bodies[i].Attractor = null;
						_bodies[i].TerminateRailMotion();
					}
				}
			}
		}
		void ShowVectors() {
			EditorGUI.showMixedValue = velocityProp.hasMultipleDifferentValues;
			var velocityValue = EditorGUILayout.Vector2Field(new GUIContent("Velocity:", "Global velocity vector"), velocityProp.vector2Value);
			if (velocityValue != velocityProp.vector2Value) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Velocity change");
					_bodies[i].Velocity = velocityValue;
				}
			}
			/// ==============
			var mixedLen = false;
			var lenSqr = _body.Velocity.sqrMagnitude;
			for (int i = 0; i < _bodies.Length; i++) {
				if (lenSqr != _bodies[i].Velocity.sqrMagnitude) {
					mixedLen = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedLen;
			var startLen = _body.Velocity.magnitude;
			var lenValue = EditorGUILayout.FloatField(new GUIContent("Velocity magnitude"), startLen);
			if (lenValue != startLen) {
				for (int i = 0; i < _bodies.Length; i++) {
					var bodyVelocityLen = _bodies[i].Velocity.magnitude;
					if (!Mathf.Approximately(lenValue, bodyVelocityLen)) {
						Undo.RecordObject(_bodies[i], "Velocity magnitude change");
						_bodies[i].Velocity = _bodies[i].Velocity * ( Mathf.Approximately(bodyVelocityLen, 0) ? lenValue : lenValue / bodyVelocityLen );
					}
				}
			}
			/// ==============
			EditorGUILayout.Separator();
			var mixedRelVelocity = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].RelativeVelocity != _body.RelativeVelocity) {
					mixedRelVelocity = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedRelVelocity;
			var relVelocityValue = EditorGUILayout.Vector2Field("Relative velocity:", _body.RelativeVelocity);
			if (relVelocityValue != _body.RelativeVelocity) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Rel.Velocity change");
					_bodies[i].RelativeVelocity = relVelocityValue;
				}
			}
			/// ==============
			var mixedRelLen = false;
			var relLenSqr = _body.RelativeVelocity.sqrMagnitude;
			for (int i = 0; i < _bodies.Length; i++) {
				if (relLenSqr != _bodies[i].RelativeVelocity.sqrMagnitude) {
					mixedRelLen = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedRelLen;
			var startRelLen = _body.RelativeVelocity.magnitude;
			var relLenValue = EditorGUILayout.FloatField(new GUIContent("Rel.Velocity magn."), startRelLen);
			if (relLenValue != startRelLen) {
				for (int i = 0; i < _bodies.Length; i++) {
					var bodyRelVelocityLen = _bodies[i].RelativeVelocity.magnitude;
					if (!Mathf.Approximately(relLenValue, bodyRelVelocityLen)) {
						Undo.RecordObject(_bodies[i], "Rel.Velocity magnitude change");
						_bodies[i].RelativeVelocity = _bodies[i].RelativeVelocity * ( Mathf.Approximately(bodyRelVelocityLen, 0) ? relLenValue : relLenValue / bodyRelVelocityLen );
					}
				}
			}
			/// ==============
			EditorGUILayout.Separator();
			var relPositionValue = EditorGUILayout.Vector2Field("Relative position", _body.RelativePosition);
			if (relPositionValue != _body.RelativePosition) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Rel.Position change");
					_bodies[i].RelativePosition = relPositionValue;
				}
			}
		}

		void ShowActionsButtons() {
			EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);
			if (GUILayout.Button("Reset velocity")) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Reset Velocity");
					_bodies[i].RelativeVelocity = Vector2.zero;
					_bodies[i].TerminateRailMotion();
				}

			}
			if (GUILayout.Button(new GUIContent("Find nearest attractor", "Note: nearest attractor not always most proper"))) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Nearest attractor assign");
					_bodies[i].FindAndSetNearestAttractor();
					_bodies[i].TerminateRailMotion();
				}
			}
			if (GUILayout.Button(new GUIContent("Find biggest attractor"))) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Biggest attractor assign");
					_bodies[i].FindAndSetBiggestAttractor();
					_bodies[i].TerminateRailMotion();
				}
			}
			if (GUILayout.Button(new GUIContent("Find most proper attractor", "Choose most realistic attractor for this body at current position"))) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Most proper attractor assign");
					_bodies[i].FindAndSetMostProperAttractor();
					_bodies[i].TerminateRailMotion();
				}
			}
			if (!_body.Attractor) {
				GUI.enabled = false; //turn button off if attractor object is not assigned
			}
			if (GUILayout.Button("Make Orbit Circle")) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Rounding orbit");
					_bodies[i].MakeOrbitCircle();
					_bodies[i].TerminateRailMotion();
				}
			}
			if (!_body.Attractor) {
				GUI.enabled = true;
			}
		}

		void ShowOrbitParameters() {
			EditorGUILayout.LabelField("Current state:", EditorStyles.boldLabel);
			///==================== Rail motion indicator
			//EditorGUILayout.BeginHorizontal("Box");
			//EditorGUILayout.LabelField("RailMotion is " + ( _body.IsOnRailsMotion ? "On" : "Off" ), GUILayout.Width(105));
			//GUI.backgroundColor = _body.IsOnRailsMotion ? new Color(0.05f, 0.72f, 0f) : new Color(0.7f, 0.08f, 0f);
			//EditorGUILayout.LabelField(" ", EditorStyles.miniButton, GUILayout.Width(55));
			//GUI.backgroundColor = Color.white;
			//EditorGUILayout.EndHorizontal();
			///====================
			if (_body.Attractor == null) {
				EditorGUILayout.LabelField("Eccentricity: -");
				EditorGUILayout.LabelField("Mean Anomaly: -");
				EditorGUILayout.LabelField("True Anomaly: -");
				EditorGUILayout.LabelField("Eccentric Anomaly: -");
				EditorGUILayout.LabelField("Argument of Periapsis: -");
				EditorGUILayout.LabelField("Apoapsis: -");
				EditorGUILayout.LabelField("Periapsis: -");
				EditorGUILayout.LabelField("Period: -");
				EditorGUILayout.LabelField("Energy: -");
				EditorGUILayout.LabelField("Distance to focus: -");
				EditorGUILayout.LabelField("Semi major axys: -");
				EditorGUILayout.LabelField("Semi minor axys: -");
				return;
			}
			///==================== Editable values:
			///====================
			bool mixedEcc = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].Eccentricity != _body.Eccentricity) {
					mixedEcc = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedEcc;
			var eccValue = EditorGUILayout.DoubleField(new GUIContent("Eccentricity"), _body.Eccentricity);
			if (eccValue != _body.Eccentricity) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i]._transform, "Eccentricity change");
					Undo.RecordObject(_bodies[i], "Eccentricity change");
					_bodies[i].SetEccentricity(eccValue);
				}
			}
			///====================
			bool mixedAnomaly_m = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].MeanAnomaly != _body.MeanAnomaly) {
					mixedAnomaly_m = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedAnomaly_m;
			var anomalyValue_m = EditorGUILayout.DoubleField(new GUIContent("Mean Anomaly", "In radians"), _body.MeanAnomaly);
			if (anomalyValue_m != _body.MeanAnomaly) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i]._transform, "Mean anomaly change");
					Undo.RecordObject(_bodies[i], "Mean anomaly change");
					_bodies[i].SetMeanAnomaly(anomalyValue_m);
				}
			}
			///====================
			bool mixedAnomaly_t = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].TrueAnomaly != _body.TrueAnomaly) {
					mixedAnomaly_t = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedAnomaly_t;
			GUI.enabled = false;//temporary disable
			var anomalyValue_t = EditorGUILayout.DoubleField(new GUIContent("True Anomaly", "In radians"), _body.TrueAnomaly);
			GUI.enabled = true;
			if (anomalyValue_t != _body.TrueAnomaly) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i]._transform, "True anomaly change");
					Undo.RecordObject(_bodies[i], "True anomaly change");
					_bodies[i].SetTrueAnomaly(anomalyValue_t);
				}
			}
			///====================
			bool mixedAnomaly_e = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].EccentricAnomaly != _body.EccentricAnomaly) {
					mixedAnomaly_e = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedAnomaly_e;
			var anomalyValue_e = EditorGUILayout.DoubleField(new GUIContent("Eccentric Anomaly", "In radians"), _body.EccentricAnomaly);
			if (anomalyValue_e != _body.EccentricAnomaly) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Eccentric anomaly change");
					_bodies[i].SetEccentricAnomaly(anomalyValue_e);
				}
			}
			bool mixedArgPer = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].ArgumentOfPeriapsis != _body.ArgumentOfPeriapsis) {
					mixedArgPer = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedArgPer;
			var argumentOfPeriapsis = EditorGUILayout.DoubleField(new GUIContent("Argument Of Periapsis", "Rotation of orbit. Angle unit is radian"), _body.ArgumentOfPeriapsis);
			if (argumentOfPeriapsis != _body.ArgumentOfPeriapsis) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i]._transform, "Argument of periapsis change");
					Undo.RecordObject(_bodies[i], "Argument of periapsis change");
					_bodies[i].SetArgumentOfPeriapsis(argumentOfPeriapsis);
				}
			}
			EditorGUI.showMixedValue = false;
			///====================
			EditorGUILayout.LabelField("Apoapsis " + _body.ApoapsisDistance);
			EditorGUILayout.LabelField("Periapsis " + _body.PeriapsisDistance);
			EditorGUILayout.LabelField("Period: " + ( _body.Period > 0 ? _body.Period.ToString("0.000") : "inf" ));
			EditorGUILayout.LabelField("Energy: " + _body.EnergyTotal.ToString("0.000"));
			EditorGUILayout.LabelField("Distance to focus: " + _body.RadiusVector.magnitude.ToString("0.000"));
			EditorGUILayout.LabelField("Semi major axys: " + _body.SemiMajorAxys.ToString("0.000"));
			EditorGUILayout.LabelField("Semi minor axys: " + _body.SemiMinorAxys.ToString("0.000"));
		}
	}

}
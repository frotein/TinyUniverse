using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SpaceGravity2D.Demo {
	public class SandboxControl : MonoBehaviour {

		public GameObject SelectionInfoPanel;
		Toggle _orbToggle;
		Toggle _keplerToggle;
		Toggle _collToggle;
		Toggle _fixedToggle;
		Slider _selMassSlider;
		public GameObject MassPanel;
		public Transform BodyPrefab;
		public Transform SelectionMark;

		CelestialBody _selected;
		CelestialBody _capturedBody;
		bool _lmb = false;
		bool _rmb = false;
		float _leftBtnTime = 0f;
		float _rightBtnTime = 0f;
		public float Sensativity = 1000f;
		public float KeyboardSensativity = 1f;
		float _clickMaxTime = 0.22f;
		Vector2 _lastMiddleBtnPos; // for moving camera
		public float StartingMass = 1f;

		public bool ShowOrbits;

		void Start() {
			if (SelectionMark) {
				SelectionMark.gameObject.SetActive(false);
			}
			if (MassPanel) {
				var slider = MassPanel.GetComponentInChildren<Slider>();
				if (slider) {
					slider.minValue = 1f;
					slider.maxValue = 1e6f;
					slider.value = StartingMass;
					slider.onValueChanged.AddListener((float f) => { StartingMass = f; });
				}
			}
			if (SelectionInfoPanel) {
				var slider = SelectionInfoPanel.GetComponentInChildren<Slider>();
				if (slider) {
					_selMassSlider = slider;
					slider.minValue = 1f;
					slider.maxValue = 1e6f;
					slider.value = 1f;
					slider.onValueChanged.AddListener((float f) => { ResizeSelected(f); });
				}
				var toggleOrb = SelectionInfoPanel.transform.Find("ToggleOrbit");
				if (toggleOrb) {
					var toggle = toggleOrb.GetComponent<Toggle>();
					if (toggle) {
						_orbToggle = toggle;
						toggle.onValueChanged.AddListener((bool b) => { if (_selected) { _selected.IsDrawOrbit = b; } else { DeselectBody(); } });
					}
				}
				var toggleKepler = SelectionInfoPanel.transform.Find("ToggleKepler");
				if (toggleKepler) {
					var toggle = toggleKepler.GetComponent<Toggle>();
					if (toggle) {
						_keplerToggle = toggle;
						toggle.onValueChanged.AddListener((bool b) => { if (_selected) { _selected.UseRailMotion = b; } else { DeselectBody(); } });
					}
				}
				var toggleColl = SelectionInfoPanel.transform.Find("ToggleColl");
				if (toggleColl) {
					var toggle = toggleColl.GetComponent<Toggle>();
					if (toggle) {
						_collToggle = toggle;
						toggle.onValueChanged.AddListener((bool b) => { if (_selected) { _selected.IgnoreAllCollisions = b; } else { DeselectBody(); } });
					}
				}
				var toggleFixed = SelectionInfoPanel.transform.Find("ToggleFixed");
				if (toggleFixed) {
					var toggle = toggleFixed.GetComponent<Toggle>();
					if (toggle) {
						_fixedToggle = toggle;
						toggle.onValueChanged.AddListener((bool b) => { if (_selected) { _selected.IsFixedPosition = b; } else { DeselectBody(); } });
					}
				}
				SelectionInfoPanel.SetActive(false);
			}
		}

		void ResizeSelected(float mass) {
			if (_selected) {
				SetSizeFor(_selected, mass);
			}
			else {
				DeselectBody();
			}
		}

		void SetSizeFor(CelestialBody body, float mass) {
			body.Mass = mass;
			var scale = Mathf.Sqrt(mass / 10000f + 1f);///( EaseInEaseOut( mass ) + 0.1f ) * 4f;
			body._transform.localScale = new Vector3(scale, scale, 1);
		}

		float EaseInEaseOut(float x) {
			var power = 3f;
			x /= 1e6f;
			x = Mathf.Clamp(x, 0f, 1f);
			return Mathf.Pow(x, power) / ( Mathf.Pow(x, power) + ( 1 - Mathf.Pow(1 - x, power) ) );
		}

		void Update() {
			KeyboardControl();
			MouseInput();
			SelectionMarking();
		}

		void SelectionMarking() {
			if (SelectionMark) {
				if (_selected) {
					SelectionMark.gameObject.SetActive(true);
					SelectionMark.position = new Vector3(_selected._transform.position.x, _selected._transform.position.y, SelectionMark.position.z);
				}
				else {
					SelectionMark.gameObject.SetActive(false);
				}
			}
		}

		void MouseInput() {
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
				return;
			}
			if (Input.GetMouseButtonDown(0)) {
				OnLeftMouseDown(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				_lmb = true;
				_leftBtnTime = Time.time;
			}
			if (Input.GetMouseButtonDown(1)) {
				//OnRightMouseDown( Camera.main.ScreenToWorldPoint( Input.mousePosition ) );
				_rmb = true;
				_rightBtnTime = Time.time;
			}
			if (Input.GetMouseButtonUp(0)) {
				_lmb = false;
				//if ( Time.time - _leftBtnTime < _clickMaxTime ) {
				//	OnLeftClick( Camera.main.ScreenToWorldPoint( Input.mousePosition ) );
				//}
				OnLeftMouseUp(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			}
			if (Input.GetMouseButtonUp(1)) {
				_rmb = false;
				if (Time.time - _rightBtnTime < _clickMaxTime) {
					OnRightClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				}
				//OnRightMouseUp( Camera.main.ScreenToWorldPoint( Input.mousePosition ) );
			}
			if (_lmb) {
				if (Time.time - _leftBtnTime >= _clickMaxTime) {
					OnLeftMouseHold(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				}
			}
			if (_rmb) {
				//if ( Time.time - _rightBtnTime >= _clickMaxTime ) {
				//	OnRightMouseHold( Camera.main.ScreenToWorldPoint( Input.mousePosition ) );
				//}
			}
			if (Input.GetMouseButtonDown(2)) {
				DeselectBody();
				_lastMiddleBtnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			}
			if (Input.GetMouseButton(2)) {
				var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Camera.main.transform.Translate(-( (Vector2)pos - _lastMiddleBtnPos ));
				_lastMiddleBtnPos = (Vector2)pos - ( (Vector2)pos - _lastMiddleBtnPos );
			}
		}

		void OnLeftMouseDown(Vector2 worldPos) {
			if (Input.GetKey(KeyCode.R)) {
				startRingPos = worldPos;
			}
			else
				if (Input.GetKey(KeyCode.X)) {
					DestroyInPos(worldPos);
				}
				else
					if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
						SelectDeselect(worldPos);
					}
					else {
						CreateAndCaptureBody(worldPos);
					}

		}

		void OnLeftMouseHold(Vector2 worldPos) {
			if (Input.GetKey(KeyCode.R)) {
				CreateRing(worldPos);
			}
			else {
				SetVelocityForCapturedBody(worldPos);
				ShowCapturedOrbit();
			}
		}

		void OnLeftMouseUp(Vector2 worldPos) {
			if (ring.Count > 0) {
				for (int i = 0; i < ring.Count; i++) {
					ring[i].enabled = true;
				}
				ring.Clear();
			}
			else {
				ActivateAndReleaseBody();
			}
		}
		void OnRightClick(Vector2 worldPos) {
			SelectDeselect(worldPos);
		}

		float _savedTimeScale = 1f;

		void KeyboardControl() {
			if (_selected) {
				var x = Input.GetAxis("Horizontal");
				var y = Input.GetAxis("Vertical");
				if (x != 0f || y != 0f) {
					_selected.AddExternalVelocity(new Vector2(x, y) * KeyboardSensativity * Time.deltaTime);
				}
				if (Input.GetKeyDown(KeyCode.F)) {
					var f = Camera.main.GetComponent<SimpleFollow>();
					if (f) {
						f.target = f.target == _selected.transform ? null : _selected.transform;
						f.enabled = f.target != null;
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Space)) {
				if (SimulationControl.instance) {
					if (SimulationControl.instance.TimeScale != 0) {
						_savedTimeScale = SimulationControl.instance.TimeScale;
					}
					SimulationControl.instance.TimeScale = SimulationControl.instance.TimeScale == 0 ? _savedTimeScale : 0f;
				}
			}
			if (Input.GetKeyDown(KeyCode.C)) {
				Debug.Break();
			}
		}

		Vector2 startRingPos = Vector2.zero;
		List<CelestialBody> ring = new List<CelestialBody>();
		//float lastRadius = 0f;
		float mindist = 5f;

		void CreateRing(Vector2 pos) {
			if (ring.Count == 0) {
				var b = Instantiate(BodyPrefab) as Transform;
				b.gameObject.SetActive(true);
				var cb = b.GetComponent<CelestialBody>();
				cb._transform.position = startRingPos;
				SetSizeFor(cb, 0.5f);
				cb.FindAndSetMostProperAttractor();
				cb.enabled = false;
				b.GetComponentInChildren<SpriteRenderer>().color = new Color(0.6f, 0.9f, 0, 9f);
				var cl = cb.GetComponentsInChildren<Collider2D>();
				for (int i = 0; i < cl.Length; i++) {
					cl[i].enabled = false;
				}
				ring.Add(cb);
			}
			if (ring[0].Attractor == null) {
				return;
			}
			var mouseDist = ( pos - startRingPos ).magnitude;
			var radius = ring[0].Attractor._transform.position - ring[0]._transform.position;
			var r = radius.magnitude;
			var cnt = (int)( 2 * Mathf.PI * r / Mathf.Max(mindist, mouseDist) );
			bool countCange = false;
			while (ring.Count < cnt - 1) {
				var d = Instantiate(ring[0]) as CelestialBody;
				d.gameObject.SetActive(true);
				ring.Add(d);
				countCange = true;
			}
			while (ring.Count > cnt) {
				var d = ring[cnt];
				ring.RemoveAt(cnt);
				Destroy(d.gameObject);
				countCange = true;
			}
			for (int i = 0; i < ring.Count; i++) {
				ring[i]._transform.position = ring[i].Attractor._transform.position + new Vector3(Mathf.Cos(2 * Mathf.PI * ( (float)i / ( cnt - 1 ) )) * r, Mathf.Sin(2 * Mathf.PI * ( (float)i / ( cnt - 1 ) )) * r, 0);
				if (countCange) {
					ring[i].MakeOrbitCircle();
					if (CelestialBody.CrossProductZ(ring[i].Velocity, ring[i].RadiusVector) < 0) {
						ring[i].Velocity = -ring[i].Velocity;
					}
				}
			}
		}

		void DestroyInPos(Vector2 pos) {
			var b = HitTest(pos);
			if (b) {
				GameObject.Destroy(b);
			}
		}

		GameObject HitTest(Vector2 p) {
			var hit = Physics2D.Raycast(p, Vector2.up, 0.1f);
			if (hit) {
				return hit.collider.gameObject;
			}
			return null;
		}

		void ShowCapturedOrbit() {
			if (_capturedBody) {
				_capturedBody.FindAndSetMostProperAttractor();
				_capturedBody.CalculateNewOrbitData();
				_capturedBody.DrawOrbit();
			}
		}

		void SelectDeselect(Vector2 p) {
			var g = HitTest(p);
			if (g) {
				var cb = g.GetComponent<CelestialBody>();
				if (cb) {
					if (_selected != null && _selected == cb) {
						DeselectBody();
					}
					else {
						SelectBody(cb);
					}
				}
			}
			else {
				if (_selected) {
					DeselectBody();
				}
			}
		}

		void SelectBody(CelestialBody cb) {
			_selected = cb;
			if (SelectionInfoPanel) {
				SelectionInfoPanel.SetActive(true);
				if (_selMassSlider) {
					_selMassSlider.value = _selected.Mass;
				}
				if (_orbToggle) {
					_orbToggle.isOn = _selected.IsDrawOrbit;
				}
				if (_keplerToggle) {
					_keplerToggle.isOn = _selected.UseRailMotion;
				}
				if (_collToggle) {
					_collToggle.isOn = _selected.IgnoreAllCollisions;
				}
				if (_fixedToggle) {
					_fixedToggle.isOn = _selected.IsFixedPosition;
				}
			}
			float selSize = _selected._transform.localScale.x * 1.1f;
			SelectionMark.localScale = new Vector3(selSize, selSize, 1);
		}

		void DeselectBody() {
			_selected = null;
			var f = Camera.main.GetComponent<SimpleFollow>();
			if (f) {
				f.target = null;
				f.enabled = false;
			}
			if (SelectionInfoPanel) {
				SelectionInfoPanel.SetActive(false);
			}
		}

		void CreateAndCaptureBody(Vector2 pos) {
			if (BodyPrefab) {
				var body = Instantiate(BodyPrefab) as Transform;
				body.gameObject.SetActive(true);
				var cbody = body.GetComponent<CelestialBody>();
				if (_capturedBody) {
					Destroy(_capturedBody.gameObject);
				}
				if (cbody) {
					_capturedBody = cbody;
					var coll = body.GetComponentsInChildren<Collider2D>();
					foreach (var c in coll) {
						c.enabled = false;
					}
					body.transform.position = pos;
					SetSizeFor(cbody, StartingMass);
					_capturedBody.FindAndSetMostProperAttractor();
					_capturedBody.MakeOrbitCircle();
					_capturedBody.enabled = false;
				}
				else {
					Destroy(body);
					return;
				}
			}
		}

		void SetVelocityForCapturedBody(Vector2 pos) {
			if (_capturedBody) {
				var v = pos - (Vector2)_capturedBody.transform.position;
				_capturedBody.RelativeVelocity = v * Sensativity;
				_capturedBody.CalculateNewOrbitData();
			}
		}

		void ActivateAndReleaseBody() {
			if (_capturedBody) {
				_capturedBody.HideOrbit();
				if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
					_capturedBody.Attractor = null;
					_capturedBody.Velocity = Vector2.zero;
				}
				_capturedBody.enabled = true;
				_capturedBody.IsDrawOrbit = ShowOrbits;
				var coll = _capturedBody.GetComponentsInChildren<Collider2D>();
				foreach (var c in coll) {
					c.enabled = true;
				}
				_capturedBody = null;
			}
		}

		public void ToggleShowOrbits() {
			ShowOrbits = !ShowOrbits;
			var bodies = GameObject.FindObjectsOfType<CelestialBody>();
			if (bodies.Length > 0) {
				for (int i = 0; i < bodies.Length; i++) {
					bodies[i].IsDrawOrbit = ShowOrbits;
				}
			}
		}

		public void TogglePredictionDraw() {
			var predictionSystem = GameObject.FindObjectOfType<PredictionSystem>();
			if (predictionSystem) {
				//if (predictionSystem.enabled) {
				//	predictionSystem.HideAllOrbits();
				//}
				predictionSystem.enabled = !predictionSystem.enabled;
			}
		}
	}
}
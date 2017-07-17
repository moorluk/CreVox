using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CreVox;
using UnityEditor.AI;

namespace CrevoxExtend {
	public class Experiments3 {
		[MenuItem("Dungeon/GA 相關功能面板v2", false, 1000)]
		public static void EditorDashboard() {
			EditorWindow.GetWindow<GAWindow>("GAv2", true);
		}
	}
	public class GAWindow : EditorWindow {

		public static Dictionary<string, RoomPattern> RoomPattern = new Dictionary<string, RoomPattern>();
		public static Vector2 WindowScrollPosition;
		public static int GenerationCount = 20;
		public static int PopulationCount = 250;
		private static string[] ignoreOptions = new string[]
		{
			"是", "否"
		};
		void Awake() {
			RoomPattern = new Dictionary<string, RoomPattern>();
			UpdateExperiments();
		}
		void OnFocus() {
			UpdateExperiments();
		}
		void UpdateExperiments() {
			var volumes = GameObject.Find("VolumeManager(Generated)").GetComponentsInChildren<Volume>();
			foreach (var vdata in volumes) {
				if(!RoomPattern.ContainsKey(vdata.vd.name)) {
					RoomPattern.Add(vdata.vd.name, new RoomPattern(vdata.vd.name));
				}
			}
			foreach (var roomPatternName in new List<string>(RoomPattern.Keys)) {
				List<Volume> volumeList = new List<Volume>(volumes);
				if (volumeList.FindIndex(x => x.vd.name == roomPatternName) == -1)  {
					RoomPattern.Remove(roomPatternName);
				}
			}
		}
		void OnGUI() {
			// TestFeilds.
			GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
			textFieldStyle.fontSize = 12;
			textFieldStyle.margin = new RectOffset(10, 10, 5, 5);
            // Buttons.
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.fontSize = 18;
			buttonStyle.margin = new RectOffset(0, 0, 5, 10);
			// Labels.
			GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.fontSize = 12;
			labelStyle.margin = new RectOffset(10, 10, 5, 5);
            // Popup.
            GUIStyle popupStyle = GUI.skin.GetStyle("popup");
			popupStyle.fontSize = 12;
			popupStyle.margin = new RectOffset(10, 10, 5, 5);

			if (GUILayout.Button("運行 GA 設置遊戲物件", buttonStyle, GUILayout.Height(30))) {
				// Run GA.
				CreVoxGA.Initialize();
				foreach (var roomPatternName in RoomPattern.Keys) {
					var roomPattern = RoomPattern[roomPatternName];
					LaunchGAExperiment(roomPattern);
				}
				
				// Bake.
				SerializedObject settingsObject = new SerializedObject(NavMeshBuilder.navMeshSettingsObject);
				settingsObject.FindProperty("m_BuildSettings.agentRadius").floatValue = 0.30f;
				settingsObject.FindProperty("m_BuildSettings.agentSlope").floatValue = 30.0f;
				// settingsObject.FindProperty("m_BuildSettings.ledgeDropHeight").floatValue       = 0f;
				// settingsObject.FindProperty("m_BuildSettings.agentClimb").floatValue            = 0f;
				// settingsObject.FindProperty("m_BuildSettings.maxJumpAcrossDistance").floatValue = 0.0f;
				// settingsObject.FindProperty("m_BuildSettings.minRegionArea").floatValue         = 0f;
				// settingsObject.FindProperty("m_BuildSettings.widthInaccuracy").floatValue       = 0f;
				// settingsObject.FindProperty("m_BuildSettings.heightInaccuracy").floatValue      = 0f;
				settingsObject.ApplyModifiedProperties();
				// Build the mesh of navigation.
				NavMeshBuilder.BuildNavMesh();

				var gamePatternObjects = GameObject.Find("GamePatternObjects") ?? new GameObject("GamePatternObjects");
				if (gamePatternObjects.GetComponentInParent<GA_Experiment.GA_Runtime>() == null) {
					gamePatternObjects.AddComponent<GA_Experiment.GA_Runtime>();
				}
			}

			// Generation count and population count.
			GenerationCount = Math.Max(1, EditorGUILayout.IntField("世代數量", GenerationCount, textFieldStyle));
			PopulationCount = Math.Max(2, EditorGUILayout.IntField("染色體數量", PopulationCount, textFieldStyle));

			// List of all experiments.
			WindowScrollPosition = EditorGUILayout.BeginScrollView(WindowScrollPosition);
			foreach (var roomPatternName in RoomPattern.Keys) {
				var roomPattern = RoomPattern[roomPatternName];
				var weights = roomPattern.Weights;

				// Foldout text.
				roomPattern.IsFoldout = Foldout(roomPattern.IsFoldout, roomPatternName, true, EditorStyles.foldout);

				if (roomPattern.IsFoldout) {
                    // Ignore.
					roomPattern.Ignore = EditorGUILayout.Popup("略過演化", roomPattern.Ignore? 0 : 1, ignoreOptions, popupStyle) == 0;
                    // Enemy count limit.
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("遊戲物件數量", labelStyle);
                    GUILayout.FlexibleSpace();
					roomPattern.ObjectQuantityMaximum = EditorGUILayout.IntField("上限", roomPattern.ObjectQuantityMaximum, textFieldStyle);
					roomPattern.ObjectQuantityMinimum = EditorGUILayout.IntField("下限", roomPattern.ObjectQuantityMinimum, textFieldStyle);
					EditorGUILayout.EndHorizontal();
					// Fitness weights (-1 ~ 1).
					//weights["neglected"] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("死角點權重", weights["neglected"], textFieldStyle)));
					weights[FitnessFunctionName.Block] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("阻擋點權重", weights[FitnessFunctionName.Block], textFieldStyle)));
					weights[FitnessFunctionName.Intercept] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("攔截點權重", weights[FitnessFunctionName.Intercept], textFieldStyle)));
					weights[FitnessFunctionName.Patrol] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("巡邏點權重", weights[FitnessFunctionName.Patrol], textFieldStyle)));
					weights[FitnessFunctionName.Guard] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("守衛點權重", weights[FitnessFunctionName.Guard], textFieldStyle)));
					//weights["dominated"] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("至高點權重", weights["dominated"], textFieldStyle)));
					weights[FitnessFunctionName.Support] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("支援點權重", weights[FitnessFunctionName.Support], textFieldStyle)));
					//weights["emptyDensity"] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("密度權重", weights["emptyDensity"], textFieldStyle)));
				}
			}
			EditorGUILayout.EndScrollView();
		}

		public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style) {
			Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, style);
			return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, style);
		}

		public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style) {
			return Foldout(foldout, new UnityEngine.GUIContent(content), toggleOnLabelClick, style);
		}

		// Launch a series GA experiment.
		private void LaunchGAExperiment(RoomPattern roomPattern) {
            // Ignore.
            if (roomPattern.Ignore) { return; }
			// Core function.
			CreVoxGA.SetQuantityLimit(roomPattern.ObjectQuantityMinimum, roomPattern.ObjectQuantityMaximum);
			CreVoxGA.SetWeights(roomPattern.Weights);
			CreVoxGA.Segmentism(PopulationCount, GenerationCount, roomPattern.Name);

		}
	}
	public class RoomPattern {
        // Ignore.
		public bool Ignore { get; set; }
        // Game object count limit.
		private int _objectQuantityMin;
		private int _objectQuantityMax;
		public int ObjectQuantityMinimum {
			get { return _objectQuantityMin; }
			set {
				if (value >= 0 && value <= _objectQuantityMax) {
					this._objectQuantityMin = value;
				}
			}
		}
		public int ObjectQuantityMaximum {
			get { return _objectQuantityMax; }
			set {
				if (value >= _objectQuantityMin) {
					this._objectQuantityMax = value;
				}
			}
		}
		// Control the foldout in editor window.
		public bool IsFoldout { get; set; }
		// Basic informations of experiment.
		public string Name { get; set; }
		public Dictionary<FitnessFunctionName, float> Weights { get; private set; }
		// Constructors.
		public RoomPattern() {
			IsFoldout = true;
			Name = string.Empty;
			Weights = new Dictionary<FitnessFunctionName, float>() {
					{ FitnessFunctionName.Block    , 0.0f },
					{ FitnessFunctionName.Guard    , 0.0f },
					{ FitnessFunctionName.Intercept, 0.0f },
					{ FitnessFunctionName.Patrol   , 0.0f },
					{ FitnessFunctionName.Support  , 0.0f },
					{ FitnessFunctionName.Density  , 0.0f }
				};
		}
		public RoomPattern(string name) : this() {
			Name = name;
		}
	}
}

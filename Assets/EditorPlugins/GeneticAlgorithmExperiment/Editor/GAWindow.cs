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
					GUILayout.Label("敵人數量限制", labelStyle);
                    GUILayout.FlexibleSpace();
					roomPattern.EnemyCountMaximum = EditorGUILayout.IntField("上限", roomPattern.EnemyCountMaximum, textFieldStyle);
					roomPattern.EnemyCountMinimum = EditorGUILayout.IntField("下限", roomPattern.EnemyCountMinimum, textFieldStyle);
					EditorGUILayout.EndHorizontal();
					// Fitness weights (-10 ~ 10).
					weights["neglected"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("死角點權重", weights["neglected"], textFieldStyle)));
					weights["block"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("阻擋點權重", weights["block"], textFieldStyle)));
					weights["intercept"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("攔截點權重", weights["intercept"], textFieldStyle)));
					weights["patrol"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("巡邏點權重", weights["patrol"], textFieldStyle)));
					weights["guard"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("守衛點權重", weights["guard"], textFieldStyle)));
					weights["dominated"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("至高點權重", weights["dominated"], textFieldStyle)));
					weights["support"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("支援點權重", weights["support"], textFieldStyle)));
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
			CreVoxGA.SetWeights(roomPattern.Weights);
			CreVoxGA.Segmentism(PopulationCount, GenerationCount, roomPattern.Name);

		}
	}
	public class RoomPattern {
        // Ignore.
		public bool Ignore { get; set; }
        // Enemy count limit.
		private int _enemyCountMin;
		private int _enemyCountMax;
		public int EnemyCountMinimum {
			get { return _enemyCountMin; }
			set {
				if (value >= 0 && value <= _enemyCountMax) {
					this._enemyCountMin = value;
				}
			}
		}
		public int EnemyCountMaximum {
			get { return _enemyCountMax; }
			set {
				if (value >= _enemyCountMin) {
					this._enemyCountMax = value;
				}
			}
		}
		// Control the foldout in editor window.
		public bool IsFoldout { get; set; }
		// Basic informations of experiment.
		public string Name { get; set; }
		public Dictionary<string, int> Weights { get; private set; }
		// Constructors.
		public RoomPattern() {
			IsFoldout = true;
			Name = string.Empty;
			Weights = new Dictionary<string, int>() {
					{ "neglected", 0 },
					{ "block"    , 0 },
					{ "intercept", 0 },
					{ "patrol"   , 0 },
					{ "guard"    , 0 },
					{ "dominated", 0 },
					{ "support"  , 0 }
				};
		}
		public RoomPattern(string name) : this() {
			Name = name;
		}
	}
}

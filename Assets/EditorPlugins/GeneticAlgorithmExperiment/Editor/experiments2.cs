using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Math         = System.Math;
using StreamWriter = System.IO.StreamWriter;
using Process      = System.Diagnostics.Process;
using Stopwatch    = System.Diagnostics.Stopwatch;

using NTUSTGA;

namespace CrevoxExtend {
	public class Experiments2 {
		[MenuItem("Dungeon/GA 相關功能面板 (testing)", false, 1000)]
		public static void EditorDashboard() {
			EditorWindow.GetWindow<EditorDashboardWindow2>("GA", true);
		}
	}

	public class EditorDashboardWindow2 : EditorWindow {
		private static readonly string PYTHON_EXEC_PATH = "C:/Python27/python.exe";
		private static readonly string PYTHON_PLOT_PROGRAM = "D:/XAOCX/CreVox/Assets/Resources/GeneticAlgorithmExperiment/.PythonPlot/maxValue.py";
		private static string EXPERIMENT_EXPORT;

		public static Dictionary<string, Experiment> Experiments = new Dictionary<string, Experiment>();

		public static Vector2 WindowScrollPosition;

		void OnGUI() {
			// Labels.
			GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
			textFieldStyle.fontSize = 12;
			textFieldStyle.margin = new RectOffset(10, 10, 5, 5);
			// Buttons.
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.fontSize = 18;
			buttonStyle.margin = new RectOffset(0, 0, 5, 10);

			if (Experiments.Count == 0) {
				Experiments.Add("實驗 A", new Experiment("實驗_A", true));
				Experiments.Add("實驗 B", new Experiment("實驗_B", false));
				Experiments.Add("實驗 C", new Experiment("實驗_C", false));
				Experiments.Add("實驗 D", new Experiment("實驗_D", false));
				Experiments.Add("實驗 E", new Experiment("實驗_E", false));
				Experiments.Add("實驗 F", new Experiment("實驗_F", false));
				Experiments.Add("實驗 G", new Experiment("實驗_G", false));
				Experiments.Add("實驗 H", new Experiment("實驗_H", false));
			}

			// Run the first experiment.
			if (GUILayout.Button("運行第一筆實驗", buttonStyle, GUILayout.Height(30))) {
				var experiment = Experiments[Experiments.Keys.First()];
				LaunchGAExperiment(experiment, false);
			}
			if (GUILayout.Button("拍攝上視圖", buttonStyle, GUILayout.Height(30))) {
				// Store a screenshot from main camera.
				var volumeManager = GameObject.Find("VolumeManager(Generated)");
				LayoutScreenshot(volumeManager);
			}
			// Write into the files.
			if (GUILayout.Button("多個實驗寫檔輸出", buttonStyle, GUILayout.Height(30))) {
				foreach (var experimentName in Experiments.Keys) {
					var experiment = Experiments[experimentName];
					LaunchGAExperiment(experiment, true);
				}
			}
			// List of all experiments.
			WindowScrollPosition = EditorGUILayout.BeginScrollView(WindowScrollPosition);
			foreach (var experimentName in Experiments.Keys) {
				var experiment = Experiments[experimentName];
				var weights    = experiment.Weights;

				// Foldout text.
				experiment.IsFoldout = Foldout(experiment.IsFoldout, (experiment.IsActived ? "[o] " : "[x] " ) + experimentName, true, EditorStyles.foldout);

				if (experiment.IsFoldout) {
					EditorGUI.BeginDisabledGroup(! experiment.IsActived);
					// Generation count and population count.
					experiment.ExperimentCount = Math.Max(1, EditorGUILayout.IntField("實驗次數", experiment.ExperimentCount, textFieldStyle));
					experiment.GenerationCount = Math.Max(1, EditorGUILayout.IntField("世代數量", experiment.GenerationCount, textFieldStyle));
					experiment.PopulationCount = Math.Max(2, EditorGUILayout.IntField("染色體數量", experiment.PopulationCount, textFieldStyle));
					// Fitness weights (-10 ~ 10).
					weights["neglected"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("死角點權重", weights["neglected"], textFieldStyle)));
					weights["block"]     = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("阻擋點權重", weights["block"],     textFieldStyle)));
					weights["intercept"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("攔截點權重", weights["intercept"], textFieldStyle)));
					weights["patrol"]    = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("巡邏點權重", weights["patrol"],    textFieldStyle)));
					weights["guard"]     = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("守衛點權重", weights["guard"],     textFieldStyle)));
					weights["dominated"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("至高點權重", weights["dominated"], textFieldStyle)));
					weights["support"]   = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("支援點權重", weights["support"],   textFieldStyle)));
					EditorGUI.EndDisabledGroup();
					// Is actived or not.
					experiment.IsActived = EditorGUILayout.Toggle("多實驗模式下，是否生效", experiment.IsActived);
				}
			}
			EditorGUILayout.EndScrollView();
		}

		// Launch a series GA experiment.
		private void LaunchGAExperiment(Experiment experiment, bool isExportFiles) {
			for (int i = 1; i <= experiment.ExperimentCount; i++) {
				Debug.Log("Start running the experiment_" + i + " of " + experiment.Name + ".");

				if (isExportFiles) {
					StreamWriter sw = new StreamWriter(EXPERIMENT_EXPORT + "datasets/experiment_" + i + ".csv");
					sw.WriteLine("run,generation,chromosome,label,score,position,type,volume");

					// Core function.
					CreVoxGA.SetWeights(experiment.Weights);
					var bestChromosome = CreVoxGA.Segmentism(experiment.PopulationCount, experiment.GenerationCount, sw);

					sw.Close();
				} else {
					// Core function.
					CreVoxGA.SetWeights(experiment.Weights);
					var bestChromosome = CreVoxGA.Segmentism(experiment.PopulationCount, experiment.GenerationCount);
				}
			}
		}

		// Create a cemera and take a shot.
		private void LayoutScreenshot(GameObject volumeManager) {
			var screenshotCarema = Camera.main;

			// Get the center point of volume manager.
			Vector3 centerPoint = default(Vector3);
			foreach (Transform volume in volumeManager.transform) {
				var chunk = volume.transform.Find("Chunk(0,0,0)");
				centerPoint = volume.transform.position + chunk.GetComponent<Renderer>().bounds.center;
				break;
			}

			// Set camera info.
			screenshotCarema.orthographic = true;
			screenshotCarema.orthographicSize = 15.0f;
			screenshotCarema.transform.rotation = Quaternion.Euler(90, 0, 0);
			screenshotCarema.transform.position = centerPoint + new Vector3(0, 20, 0);

			// Select this camera.
			Selection.objects = new GameObject[1] { screenshotCarema.gameObject };

			// Record the shot.
			EditorApplication.ExecuteMenuItem("Window/Game");
			Application.CaptureScreenshot(EXPERIMENT_EXPORT + "Screenshot.png", 2);
		}

		public class Experiment {
			// Control the foldout in editor window.
			public bool IsFoldout { get; set; }
			// Basic informations of experiment.
			public string Name { get; set; }
			public bool IsActived { get; set; }
			// Genetic algorithm setting.
			public int ExperimentCount { get; set; }
			public int GenerationCount { get; set; }
			public int PopulationCount { get; set; }
			public Dictionary<string, int> Weights { get; private set; }
			// Constructors.
			public Experiment() {
				IsFoldout = true;
				Name = string.Empty;
				IsActived = true;
				ExperimentCount = 1;
				GenerationCount = 20;
				PopulationCount = 250;
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
			public Experiment(string name, bool isActived) : this() {
				Name = name;
				IsFoldout = isActived;
				IsActived = isActived;
			}
		}

		public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style) {
			Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, style);
			return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, style);
		}

		public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style) {
			return Foldout(foldout, new UnityEngine.GUIContent(content), toggleOnLabelClick, style);
		}
	}
}

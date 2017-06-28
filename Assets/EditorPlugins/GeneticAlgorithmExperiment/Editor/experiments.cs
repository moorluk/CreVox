using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Math           = System.Math;
using StreamWriter   = System.IO.StreamWriter;
using DirectoryInfo  = System.IO.DirectoryInfo;
using Process        = System.Diagnostics.Process;
using Stopwatch      = System.Diagnostics.Stopwatch;
using NavMeshBuilder = UnityEditor.AI.NavMeshBuilder;

using NTUSTGA;

namespace CrevoxExtend {
	public class Experiments2 {
		[MenuItem("Dungeon/GA 相關功能面板", false, 1000)]
		public static void EditorDashboard() {
			EditorWindow.GetWindow<EditorDashboardWindow2>("GA", true);
		}
	}

	public class EditorDashboardWindow2 : EditorWindow {
		private static readonly string DEFAULT_PYTHON_EXEC_PATH = "C:/Python27/python.exe";
		private static string EXPERIMENT_DIR;
		private static string PYTHON_SRC_DIR;

		public static Dictionary<string, Experiment> Experiments = new Dictionary<string, Experiment>();

		public static Vector2 WindowScrollPosition;

		void OnEnable() {
			// Base on the OS environment.
			EXPERIMENT_DIR = Application.persistentDataPath + "/Experiments/";
			PYTHON_SRC_DIR = Application.dataPath + "/Resources/GeneticAlgorithmExperiment/.PythonPlot/";
		}

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

			if (GUILayout.Button("Bake the navigation", buttonStyle, GUILayout.Height(30))) {
				SerializedObject settingsObject = new SerializedObject(NavMeshBuilder.navMeshSettingsObject);
				settingsObject.FindProperty("m_BuildSettings.agentRadius").floatValue           = 0.30f;
				settingsObject.FindProperty("m_BuildSettings.agentSlope").floatValue            = 50.0f;
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
				var activedExperiments = new Dictionary<string, Experiment>();

				foreach (var experimentName in Experiments.Keys) {
					var experiment = Experiments[experimentName];
					if (experiment.IsActived) {
						activedExperiments.Add(experimentName, experiment);
						LaunchGAExperiment(experiment, true);
					}
				}
				
				if (ExistsOnPath("python.exe") || ExistsOnPath(DEFAULT_PYTHON_EXEC_PATH)) {
					// ExecutePythonPlot();
					ExecutePythonPlot2(activedExperiments);
				} else {
					Debug.LogError("Please check your system has 'python' in the environment path.");
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
					experiment.PopulationCount = Math.Max(2, EditorGUILayout.IntField("染色體數量", experiment.PopulationCount%2 == 0? experiment.PopulationCount: experiment.PopulationCount+1, textFieldStyle));
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
			string datasetPath = string.Empty;

			// Delete the directory then recreator again.
			if (isExportFiles) {
				datasetPath = EXPERIMENT_DIR + "datasets/" + experiment.Name;
				DirectoryInfo datasetDirectory = new DirectoryInfo(datasetPath);
				if (datasetDirectory.Exists) { datasetDirectory.Delete(true); }
				datasetDirectory.Create();
				// Open this directory in explorer.
				EditorUtility.RevealInFinder(datasetPath);
				Debug.Log("The export of experiment in '" + datasetPath + "'.");
			}

			// For each experiment.
			for (int i = 1; i <= experiment.ExperimentCount; i++) {
				Debug.Log("Start running the experiment_" + i + " of " + experiment.Name + ".");
				// Write the export or not.
				if (isExportFiles) {
					// Create StreamWriter.
					StreamWriter sw = new StreamWriter(datasetPath + "/experiment_" + i + ".csv");
					sw.WriteLine("run,generation,chromosome,label,score,position,type,volume");
					// Core function.
					CreVoxGA.SetWeights(experiment.Weights);
					var bestChromosome = CreVoxGA.Segmentism(experiment.PopulationCount, experiment.GenerationCount, sw);
					// Close StreamWriter.
					sw.Close();
				} else {
					// Core function.
					CreVoxGA.SetWeights(experiment.Weights);
					var bestChromosome = CreVoxGA.Segmentism(experiment.PopulationCount, experiment.GenerationCount);
				}
			}
		}

		private void ExecutePythonPlot() {
			var pythonPath = ExistsOnPath("python.exe") ? GetFullPath("python.exe") : GetFullPath(DEFAULT_PYTHON_EXEC_PATH);

			Process process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Arguments = "/C  + pythonPath + "  + PYTHON_SRC_DIR + "maxValue.py \"" + EXPERIMENT_DIR + "\"";

			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			// Capture python log from process.StandardOutput and process.StandardError.
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			// // When execute the cmd fail.
			// process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(ErrorReceived);
			// process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(ErrorReceived);
			// // Process finished.
			// process.Exited += new System.EventHandler(ProcessExited);
			process.EnableRaisingEvents = true;
			// Start executing.
			Debug.Log("Subprocess is running:\n" + process.StartInfo.Arguments);
			process.Start();

			var error = process.StandardError.ReadToEnd();
			if (error != string.Empty) {
				Debug.LogError("Execute the python program fail:\n" + error);
			} else {
				Debug.Log("Done.");
			}

			process.WaitForExit();
		}

		private void ExecutePythonPlot2(Dictionary<string, Experiment> experiments) {
			var pythonPath = ExistsOnPath("python.exe") ? GetFullPath("python.exe") : GetFullPath(DEFAULT_PYTHON_EXEC_PATH);

			Process process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Arguments = "/C " + pythonPath + " " + PYTHON_SRC_DIR + "heatmapPlot.py \"" + EXPERIMENT_DIR + "\"";

			foreach (var experimentName in experiments.Keys) {
				process.StartInfo.Arguments += " \"" + experiments[experimentName].Name + "\" ";
			}

			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			// Capture python log from process.StandardOutput and process.StandardError.
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			// // When execute the cmd fail.
			// process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(ErrorReceived);
			// process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(ErrorReceived);
			// // Process finished.
			// process.Exited += new System.EventHandler(ProcessExited);
			process.EnableRaisingEvents = true;
			// Start executing.
			Debug.Log("Subprocess is running:\n" + process.StartInfo.Arguments);
			process.Start();

			var error = process.StandardError.ReadToEnd();
			if (error != string.Empty) {
				Debug.LogError("Execute the python program fail:\n" + error);
			} else {
				Debug.Log("Done.");
			}

			process.WaitForExit();
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
			Application.CaptureScreenshot(EXPERIMENT_DIR + "Screenshot.png", 2);
			// Open this directory in explorer.
			EditorUtility.RevealInFinder(EXPERIMENT_DIR);
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

		private static bool ExistsOnPath(string fileName) {
			return GetFullPath(fileName) != null;
		}

		private static string GetFullPath(string fileName) {
			if (System.IO.File.Exists(fileName)) {
				return System.IO.Path.GetFullPath(fileName);
			}

			var values = System.Environment.GetEnvironmentVariable("PATH");
			foreach (var path in values.Split(';')) {
				var fullPath = System.IO.Path.Combine(path, fileName);
				if (System.IO.File.Exists(fullPath)) {
					return fullPath;
				}
			}
			return null;
		}
	}
}

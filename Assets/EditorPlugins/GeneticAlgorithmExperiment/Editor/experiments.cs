﻿using UnityEngine;
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
		public static CreVoxGA.CreVoxChromosome lastBestChromosome;

		private static string[] ignoreOptions = new string[]
        {
            "是", "否"
        };
		private static Dictionary<FitnessFunctionName, string> FitnessChinese = new Dictionary<FitnessFunctionName, string>() {
			{ FitnessFunctionName.Block,"阻擋點" },
			{ FitnessFunctionName.Guard,"守衛點" },
			{ FitnessFunctionName.Intercept,"攔截點" },
			{ FitnessFunctionName.Patrol,"巡邏點" },
			{ FitnessFunctionName.Support,"支援點" },
			{ FitnessFunctionName.Density,"物件數量" }
		};

        void OnEnable() {
			// Base on the OS environment.
			EXPERIMENT_DIR = Application.persistentDataPath + "/Experiments/";
			PYTHON_SRC_DIR = Application.dataPath + "/Resources/GeneticAlgorithmExperiment/.PythonPlot/";
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

			EditorGUI.BeginDisabledGroup(!(GameObject.Find("VolumeManager(Generated)").transform.childCount == 1));
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

				// Clean the experiment folder.
				DirectoryInfo datasetDirectory = new DirectoryInfo(EXPERIMENT_DIR);
				if (datasetDirectory.Exists) { datasetDirectory.Delete(true); }
				datasetDirectory.Create();

				// Open this directory in explorer.
				EditorUtility.RevealInFinder(EXPERIMENT_DIR);
				Debug.Log("The export of experiments in '" + EXPERIMENT_DIR + "'.");

				// Set artpack.
				var volumeManager = GameObject.Find("VolumeManager(Generated)");
				var volume = volumeManager.GetComponentInChildren<CreVox.Volume>();
				volume.ArtPack = CreVox.PathCollect.artPack + "/AncientPalace";
				CreVox.VGlobal.GetSetting().volumeShowArtPack = true;
				volume.LoadTempWorld();
				volume.transform.Find("BoxCursor(Clone)").gameObject.SetActive(false);


				// Export the raw dataset.
				foreach (var experimentName in Experiments.Keys) {
					var experiment = Experiments[experimentName];
					if (experiment.IsActived) {
						activedExperiments.Add(experimentName, experiment);
						LaunchGAExperiment(experiment, true);
					}
				}

				// Clone the python files.
				string fileA = PYTHON_SRC_DIR + "heatmapPlot.py";
				string fileB = PYTHON_SRC_DIR + "fitnessComparison.py";
				System.IO.File.Copy(fileA, EXPERIMENT_DIR + System.IO.Path.GetFileName(fileA), true);
				System.IO.File.Copy(fileB, EXPERIMENT_DIR + System.IO.Path.GetFileName(fileB), true);

				/*
				 * Export the ploy by python.
				if (ExistsOnPath("python.exe") || ExistsOnPath(DEFAULT_PYTHON_EXEC_PATH)) {
					ExecutePythonPlot("heatmap", activedExperiments);
					ExecutePythonPlot("fitnessComparison", activedExperiments);
				} else {
					Debug.LogError("Please check your system has 'python' in the environment path.");
				}
				*/
			}
			EditorGUI.EndDisabledGroup();

			if(lastBestChromosome != null) {
				string fitnessScoreString = "";
				foreach (var key in lastBestChromosome.FitnessScore.Keys) {
					float score = 0.0f;
					if (CreVoxGA.FitnessWeights[key] == 0) { continue; }
					score = lastBestChromosome.GetFitnessScore(key) * CreVoxGA.FitnessWeights[key];
					fitnessScoreString += string.Format("{0}: {1}\n", key.ToString(), score);
				}
				EditorGUILayout.TextArea(fitnessScoreString);
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

                    // Ignore.
                    experiment.Ignore = EditorGUILayout.Popup("略過演化", experiment.Ignore ? 0 : 1, ignoreOptions, popupStyle) == 0;
					// Enemy count limit.
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("遊戲物件數量", labelStyle);
					GUILayout.FlexibleSpace();
					experiment.ObjectQuantityMaximum = EditorGUILayout.IntField("上限", experiment.ObjectQuantityMaximum, textFieldStyle);
					experiment.ObjectQuantityMinimum = EditorGUILayout.IntField("下限", experiment.ObjectQuantityMinimum, textFieldStyle);
					EditorGUILayout.EndHorizontal();

					// Fitness weights (-1 ~ 1).
					//weights["neglected"] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("死角點權重", weights["neglected"], textFieldStyle)));
					weights[FitnessFunctionName.Block]     = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("阻擋點權重", weights[FitnessFunctionName.Block],     textFieldStyle)));
					weights[FitnessFunctionName.Intercept] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("攔截點權重", weights[FitnessFunctionName.Intercept], textFieldStyle)));
					weights[FitnessFunctionName.Patrol]    = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("巡邏點權重", weights[FitnessFunctionName.Patrol],    textFieldStyle)));
					weights[FitnessFunctionName.Guard]     = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("守衛點權重", weights[FitnessFunctionName.Guard],     textFieldStyle)));
					//weights["dominated"] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("至高點權重", weights["dominated"], textFieldStyle)));
					weights[FitnessFunctionName.Support]   = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("支援點權重", weights[FitnessFunctionName.Support],   textFieldStyle)));
                    //weights["emptyDensity"] = Math.Max(-1, Math.Min(1, EditorGUILayout.FloatField("密度權重", weights["emptyDensity"], textFieldStyle)));
                    EditorGUI.EndDisabledGroup();
					// Is actived or not.
					experiment.IsActived = EditorGUILayout.Toggle("多實驗模式下，是否生效", experiment.IsActived);
				}
			}
			EditorGUILayout.EndScrollView();
		}

		// Launch a series GA experiment.
		private void LaunchGAExperiment(Experiment experiment, bool isExportFiles) {
            // Ignore.
            if (experiment.Ignore) { return; }
			string datasetPath = string.Empty;
			StreamWriter swScore = null, swPosition = null, swResult = null;
			// Delete the directory then recreator again.
			if (isExportFiles) {
				// Create dataset folder.
				datasetPath = EXPERIMENT_DIR + "datasets/" + experiment.Name;
				DirectoryInfo datasetDirectory = new DirectoryInfo(datasetPath);
				if (datasetDirectory.Exists) { datasetDirectory.Delete(true); }
				datasetDirectory.Create();
				// Create screenshot folder.
				string screenshotPath = EXPERIMENT_DIR + "Screenshot/" + experiment.Name;
				datasetDirectory = new DirectoryInfo(screenshotPath);
				if (datasetDirectory.Exists) { datasetDirectory.Delete(true); }
				datasetDirectory.Create();
				// Output setting.csv
				StreamWriter swSetting = new StreamWriter(datasetPath + "/setting.csv");
				swSetting.WriteLine("label,weight,comment");
				CreVoxGA.SetWeights(experiment.Weights);
				foreach (FitnessFunctionName fitnessName in System.Enum.GetValues(typeof(FitnessFunctionName))) {
					string line = string.Format("\"{0}\",{1},", fitnessName, CreVoxGA.FitnessWeights[fitnessName]);
					if (fitnessName == FitnessFunctionName.Density) {
						line += string.Format("\"{0},{1}\"", experiment.ObjectQuantityMinimum, experiment.ObjectQuantityMaximum);
					} else {
						line += "\"\"";
					}
					swSetting.WriteLine(line);
				}
				swSetting.Close();

				// Create StreamWriter.
				swScore = new StreamWriter(datasetPath + "/score.csv");
				swScore.WriteLine("run,generation,chromosome,label,score");
				swPosition = new StreamWriter(datasetPath + "/position.csv");
				swPosition.WriteLine("run,generation,chromosome,position,type");
				swResult = new StreamWriter(datasetPath + "/result.txt");
			}
			// Initialize chromosome count.
			CreVoxGA.AllChromosomeCount = default(uint);
			// For each experiment.
			for (int i = 1; i <= experiment.ExperimentCount; i++) {
				Debug.Log("Start running the experiment_" + i + " of " + experiment.Name + ".");
				// Write the export or not.
				if (isExportFiles) {
					// Timer start.
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();

					// Core function.
					CreVoxGA.SetQuantityLimit(experiment.ObjectQuantityMinimum, experiment.ObjectQuantityMaximum);
					CreVoxGA.SetWeights(experiment.Weights);

					// Run GA.
					var bestChromosome = CreVoxGA.Segmentism(experiment.PopulationCount, experiment.GenerationCount, swScore, swPosition);
					lastBestChromosome = bestChromosome as CreVoxGA.CreVoxChromosome;
					// Time's up.
					stopwatch.Stop();

					// Output result.
					swResult.WriteLine("Run: {0}", i);
					swResult.WriteLine("Genration: {0}", experiment.GenerationCount);
					swResult.WriteLine("Individuals: {0}", experiment.PopulationCount);
					swResult.WriteLine("Run time: {0} (ms)", stopwatch.ElapsedMilliseconds);
					float sum = 0;
					List<string> latexLines = new List<string>();
					foreach (var key in bestChromosome.FitnessScore.Keys) {
						float score = 0.0f;
						if (CreVoxGA.FitnessWeights[key] == 0) { continue; }
						score = (bestChromosome as CreVoxGA.CreVoxChromosome).GetFitnessScore(key) * CreVoxGA.FitnessWeights[key];
						swResult.WriteLine("{0}: {1}\n", key.ToString(), score.ToString("0.0000"));
						sum += score;
						//
						string latexString = latexLines.Count == 0 ? @"\multirow{3}{*}{$" + i + @"$}  " : "                      ";
						latexString += string.Format("& {0} & ${1}$ & ${2}$ ", FitnessChinese[key] + string.Empty.PadRight(8 - System.Text.Encoding.Default.GetByteCount(FitnessChinese[key])),
							score.ToString("0.0000"), CreVoxGA.FitnessWeights[key]);
						latexString += @"\\\cline{2-4}";
						latexLines.Add(latexString);
					}
					swResult.WriteLine("Sum: {0}\n", sum.ToString("0.0000"));
					latexLines.Add(@"                      & \multicolumn{3}{ r| }{$" + sum.ToString("0.0000") + @"$} \\\hline");
					foreach (var line in latexLines) {
						swResult.WriteLine(line);
					}
					// End this run.
					swResult.WriteLine("=============================================");
					// Screenshot.
					var volumeManager = GameObject.Find("VolumeManager(Generated)");
					LayoutScreenshot(volumeManager, EXPERIMENT_DIR + "Screenshot/" + experiment.Name + "/"  + i.ToString() + ".png");

				} else {
					// Core function.
					CreVoxGA.SetQuantityLimit(experiment.ObjectQuantityMinimum, experiment.ObjectQuantityMaximum);
					CreVoxGA.SetWeights(experiment.Weights);
					var bestChromosome = CreVoxGA.Segmentism(experiment.PopulationCount, experiment.GenerationCount);
					lastBestChromosome = bestChromosome as CreVoxGA.CreVoxChromosome;
				}
			}
			if (isExportFiles) {
				// Close StreamWriter.
				swScore.Close();
				swPosition.Close();
				swResult.Close();
			}
		}

		private void ExecutePythonPlot(string format, Dictionary<string, Experiment> experiments) {
			var pythonPath = ExistsOnPath("python.exe") ? GetFullPath("python.exe") : GetFullPath(DEFAULT_PYTHON_EXEC_PATH);

			Process process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			// Switch the python source.
			switch (format) {
			case "heatmap":
				process.StartInfo.Arguments = "/C " + pythonPath + " " + PYTHON_SRC_DIR + "heatmapPlot.py \"" + EXPERIMENT_DIR + "\"";
				break;
			case "fitnessComparison":
				process.StartInfo.Arguments = "/C " + pythonPath + " " + PYTHON_SRC_DIR + "maxValue.py \"" + EXPERIMENT_DIR + "\"";
				break;
			}

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

		// Overloading.
		private void LayoutScreenshot(GameObject volumeManager, string fileName) {
			if (Camera.main == null) {
				GameObject cam = new GameObject("main camera");
				cam.AddComponent<Camera>();
				cam.tag = "MainCamera";
			}
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
			screenshotCarema.transform.position = centerPoint + new Vector3(12, 20, 12);

			// Select this camera.
			Selection.objects = new GameObject[1] { screenshotCarema.gameObject };

			// Record the shot.
			EditorApplication.ExecuteMenuItem("Window/Game");

			// Screenshot.
			const int width = 800;
			const int height = 600;
			RenderTexture rt = new RenderTexture(width, height, 24);
			screenshotCarema.targetTexture = rt;
			Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
			screenshotCarema.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			screenshotCarema.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			DestroyImmediate(rt);
			byte[] bytes = screenShot.EncodeToPNG();
			System.IO.File.WriteAllBytes(fileName, bytes);
		}
		public class Experiment {
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
			public bool IsActived { get; set; }
			// Genetic algorithm setting.
			public int ExperimentCount { get; set; }
			public int GenerationCount { get; set; }
			public int PopulationCount { get; set; }
			public Dictionary<FitnessFunctionName, float> Weights { get; private set; }
			// Constructors.
			public Experiment() {
				IsFoldout = true;
				Name = string.Empty;
				IsActived = true;
				ExperimentCount = 1;
				GenerationCount = 20;
				PopulationCount = 250;
				Weights = new Dictionary<FitnessFunctionName, float>() {
					{ FitnessFunctionName.Block    , 0.0f },
					{ FitnessFunctionName.Guard    , 0.0f },
					{ FitnessFunctionName.Intercept, 0.0f },
					{ FitnessFunctionName.Patrol   , 0.0f },
					{ FitnessFunctionName.Support  , 0.0f },
					{ FitnessFunctionName.Density  , 0.0f }
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

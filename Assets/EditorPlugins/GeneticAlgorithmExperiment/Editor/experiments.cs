using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Math         = System.Math;
using StreamWriter = System.IO.StreamWriter;
using Diagnostics  = System.Diagnostics;
using Stopwatch    = System.Diagnostics.Stopwatch;

using NTUSTGA;

namespace CrevoxExtend {
	public class Experiments {
		[MenuItem("Dungeon/GA 相關功能面板", false, 999)]
		public static void EditorDashboard() {
			EditorWindow window = EditorWindow.GetWindow<EditorDashboardWindow>("GA", true);
		}
	}

	public class EditorDashboardWindow : EditorWindow {
		private static int GenerationCount = 20;
		private static int PopulationCount = 250;

		private static int   EnemyCount    { get; set; }
		private static int   TreasureCount { get; set; }
		private static int   TrapCount     { get; set; }
		private static int   EmptyCount    { get; set; }

		private static long  TimeCost      { get; set; }
		private static float OptimalScore  { get; set; }

		public static Dictionary<string, int> FitnessWeights = new Dictionary<string, int>() {
			{ "neglected", 0 },
			{ "block"    , 0 },
			{ "intercept", 0 },
			{ "patrol"   , 0 },
			{ "guard"    , 0 },
			{ "dominated", 0 },
			{ "support"  , 0 }
		};

		void OnGUI() {
			// GUI styles.
			GUIStyle textStyle = new GUIStyle();
			textStyle.fontSize = 18;
			textStyle.margin = new RectOffset(10, 10, 5, 5);

			GUIStyle labelStyle = new GUIStyle(GUI.skin.textField);
			labelStyle.fontSize = 12;
			labelStyle.margin = new RectOffset(10, 10, 5, 5);

			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.fontSize = 20;
			buttonStyle.margin = new RectOffset(0, 0, 5, 10);

			if (GUILayout.Button("拍張相片紀錄下來", buttonStyle, GUILayout.Height(35))) {
				// Store a screenshot from main camera.
				var volumeManager = GameObject.Find("VolumeManager(Generated)");
				LayoutScreenshot(volumeManager);
				// Execute the python program about plot.
				ExecutePythonPlot();
			}

			GenerationCount = Math.Max(1, EditorGUILayout.IntField("世代數量", GenerationCount, labelStyle));
			PopulationCount = Math.Max(2, EditorGUILayout.IntField("染色體數量", PopulationCount, labelStyle));

			if (GUILayout.Button("跑跑 GA", buttonStyle, GUILayout.Height(35))) {
				// Start timer.
				Stopwatch sw = new Stopwatch();
				sw.Start();
				// Core function.
				CreVoxGA.SetWeights(FitnessWeights);
				var bestChromosome = CreVoxGA.Segmentism(PopulationCount, GenerationCount);
				// Stop timer.
				sw.Stop();
				TimeCost = sw.ElapsedMilliseconds;
				// Update informations.
				UpdateObjectInfo(bestChromosome);
			}

			if (GUILayout.Button("一字一句寫下對你的心意", buttonStyle, GUILayout.Height(35))) {
				LaunchGAExperiments();
			}

			if (GUILayout.Button("揮揮衣袖不帶走一點雲彩", buttonStyle, GUILayout.Height(35))) {
				CreVoxGA.Initialize();
			}

			EditorGUI.BeginDisabledGroup(true);
			FitnessWeights["neglected"] = EditorGUILayout.IntField("死角點權重", FitnessWeights["neglected"], labelStyle);
			EditorGUI.EndDisabledGroup();
			FitnessWeights["block"]     = EditorGUILayout.IntField("阻擋點權重", FitnessWeights["block"], labelStyle);
			EditorGUI.BeginDisabledGroup(true);
			FitnessWeights["intercept"] = EditorGUILayout.IntField("攔截點權重", FitnessWeights["intercept"], labelStyle);
			EditorGUI.EndDisabledGroup();
			FitnessWeights["patrol"]    = EditorGUILayout.IntField("巡邏點權重", FitnessWeights["patrol"], labelStyle);
			FitnessWeights["guard"]     = EditorGUILayout.IntField("守衛點權重", FitnessWeights["guard"], labelStyle);
			EditorGUI.BeginDisabledGroup(true);
			FitnessWeights["dominated"] = EditorGUILayout.IntField("至高點權重", FitnessWeights["dominated"], labelStyle);
			FitnessWeights["support"]   = EditorGUILayout.IntField("支援點權重", FitnessWeights["support"], labelStyle);
			EditorGUI.EndDisabledGroup();

			// Description.
			var description = "敵人數量: " + EnemyCount + "\n"
							+ "寶箱數量: " + TreasureCount + "\n"
							+ "陷阱數量: " + TrapCount + "\n"
							+ "空格數量: " + EmptyCount + "\n\n"
							+ "最佳得分: " + OptimalScore + "\n"
							+ "演化總耗時: " + TimeCost + " ms\n";
			GUILayout.TextField(description, textStyle);
		}

		// 
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
			Application.CaptureScreenshot("Screenshot.png", 2);
		}


		private void AfterPythonPlot(object sender, System.EventArgs e) {
			Debug.Log("Plot done.");
		}

		private void ExecutePythonPlot() {
			Diagnostics.Process process = new Diagnostics.Process();
			Diagnostics.ProcessStartInfo startInfo = new Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = "/C C:/Python27/python.exe D:/XAOCX/CreVox/Assets/Resources/GeneticAlgorithmExperiment/PythonPlot/maxValue.py";
			process.Exited += new System.EventHandler(AfterPythonPlot);
			process.EnableRaisingEvents = true;
			process.StartInfo = startInfo;
			process.Start();
		}

		private void UpdateObjectInfo(NTUSTChromosome chromosome) {
			var enemies = chromosome.Genes
				.Select(g => g as CreVoxGA.CreVoxGene)
				.Where(g => g.Type == GeneType.Enemy).ToList();

			var treasures = chromosome.Genes
				.Select(g => g as CreVoxGA.CreVoxGene)
				.Where(g => g.Type == GeneType.Treasure).ToList();

			var traps = chromosome.Genes
				.Select(g => g as CreVoxGA.CreVoxGene)
				.Where(g => g.Type == GeneType.Trap).ToList();

			var empties = chromosome.Genes
				.Select(g => g as CreVoxGA.CreVoxGene)
				.Where(g => g.Type == GeneType.Empty).ToList();

			EnemyCount    = enemies.Count;
			TreasureCount = treasures.Count;
			TrapCount     = traps.Count;
			EmptyCount    = empties.Count;

			OptimalScore = chromosome.FitnessFunction();
		}

		public void LaunchGAExperiments() {
			int times = 100;
			for (int i = 0; i < times; i++) {
				StreamWriter sw = new StreamWriter("Export/experiment_" + (i + 1) + ".csv");
				sw.WriteLine("Fitness,all");

				// Core function.
				CreVoxGA.SetWeights(FitnessWeights);
				var bestChromosome = CreVoxGA.Segmentism(PopulationCount, GenerationCount);

				sw.Write(CreVoxGA.GenesScore);
				sw.Close();
			}
		}
	}
}

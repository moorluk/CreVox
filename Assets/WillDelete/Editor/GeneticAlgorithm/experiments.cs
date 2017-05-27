using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using StreamWriter = System.IO.StreamWriter;
using Stopwatch    = System.Diagnostics.Stopwatch;

using NTUSTGA;

namespace CrevoxExtend {
	public class Experiments {
		// Add the 'test' in 'Dungeon' menu.
		//test the A* for volume.
		[MenuItem("Dungeon/GA 以及輸出", false, 999)]
		public static void ExperimentAndExport() {
			LaunchGAExperiments();
		}

		public static void LaunchGAExperiments() {
			int times = 1;
			for (int i = 0; i < times; i++) {
				StreamWriter sw = new StreamWriter("Export/experiment_" + (i + 1) + ".csv");
				sw.WriteLine("FitnessSupport,all");
				CreVoxGA.Segmentism();
				sw.Write(CreVoxGA.GenesScore);
				sw.Close();
			}
		}

		[MenuItem("Dungeon/GA 相關功能面板", false, 999)]
		public static void EditorDashboard() {
			EditorWindow window = EditorWindow.GetWindow<EditorDashboardWindow>("GA", true);
		}
	}

	public class EditorDashboardWindow : EditorWindow {
		private static int EnemyCount    { get; set; }
		private static int TreasureCount { get; set; }
		private static int TrapCount     { get; set; }
		private static int EmptyCount    { get; set; }

		void OnGUI() {
			// GUI styles.
			GUIStyle textStyle = new GUIStyle();
			textStyle.fontSize = 18;
			textStyle.margin = new RectOffset(10, 10, 5, 5);

			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.fontSize = 20;
			buttonStyle.margin = new RectOffset(0, 0, 5, 10);

			if (GUILayout.Button("跑跑 GA", buttonStyle, GUILayout.Height(35))) {
				// Start timer.
				Stopwatch sw = new Stopwatch();
				sw.Start();
				// Core function.
				// CreVoxGA.SetWeights();
				var bestChromosome = CreVoxGA.Segmentism();
				UpdateObjectInfo(bestChromosome);
				// Stop timer.
				sw.Stop();
				Debug.Log(sw.ElapsedMilliseconds + " ms");
			}

			if (GUILayout.Button("揮揮衣袖不帶走一點雲彩", buttonStyle, GUILayout.Height(35))) {
				CreVoxGA.Initialize();
			}

			// Description.
			var description = "敵人數量: " + EnemyCount + "\n"
							+ "寶箱數量: " + TreasureCount + "\n"
							+ "陷阱數量: " + TrapCount + "\n"
							+ "空格數量: " + EmptyCount + "\n";
			GUILayout.TextField(description, textStyle);
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
				.Where(g => g.Type == GeneType.Treasure).ToList();

			var empties = chromosome.Genes
				.Select(g => g as CreVoxGA.CreVoxGene)
				.Where(g => g.Type == GeneType.Empty).ToList();

			EnemyCount    = enemies.Count;
			TreasureCount = treasures.Count;
			TrapCount     = traps.Count;
			EmptyCount    = empties.Count;
		}
	}
}

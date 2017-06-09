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

		public static Dictionary<string, Experiment> Experiments = new Dictionary<string, Experiment>();

		void OnGUI() {
			GUIStyle labelStyle = new GUIStyle(GUI.skin.textField);
			labelStyle.fontSize = 12;
			labelStyle.margin = new RectOffset(10, 10, 5, 5);

			if (Experiments.Count == 0) {
				Experiments.Add("實驗 A", new Experiment());
				Experiments.Add("實驗 B", new Experiment());
			}

			foreach (var experimentName in Experiments.Keys) {
				var experiment = Experiments[experimentName];
				var weights    = experiment.Weights;

				experiment.IsFoldout = EditorGUILayout.Foldout(experiment.IsFoldout, experimentName);

				if (experiment.IsFoldout) {
					// Generation count and population count.
					experiment.GenerationCount = Math.Max(1, EditorGUILayout.IntField("世代數量", experiment.GenerationCount, labelStyle));
					experiment.PopulationCount = Math.Max(2, EditorGUILayout.IntField("染色體數量", experiment.PopulationCount, labelStyle));
					// Fitness weights (-10 ~ 10).
					weights["neglected"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("死角點權重", weights["neglected"], labelStyle)));
					weights["block"]     = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("阻擋點權重", weights["block"],     labelStyle)));
					weights["intercept"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("攔截點權重", weights["intercept"], labelStyle)));
					weights["patrol"]    = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("巡邏點權重", weights["patrol"],    labelStyle)));
					weights["guard"]     = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("守衛點權重", weights["guard"],     labelStyle)));
					weights["dominated"] = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("至高點權重", weights["dominated"], labelStyle)));
					weights["support"]   = Math.Max(-10, Math.Min(10, EditorGUILayout.IntField("支援點權重", weights["support"],   labelStyle)));
				}
			}
		}
		public class Experiment {
			// Control the foldout in editor window.
			public bool IsFoldout { get; set; }
			// Basic informations of experiment.
			public string Name { get; set; }
			// Genetic algorithm setting.
			public int GenerationCount { get; set; }
			public int PopulationCount { get; set; }
			public Dictionary<string, int> Weights { get; private set; }

			public Experiment() {
				IsFoldout = true;
				Name = string.Empty;
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
		}
	}
}

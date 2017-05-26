using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using StreamWriter = System.IO.StreamWriter;
using DateTime = System.DateTime;



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
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using StreamWriter = System.IO.StreamWriter;
using DateTime = System.DateTime;



namespace CrevoxExtend {
	public class experiments {
		// Add the 'test' in 'Dungeon' menu.
		//test the A* for volume.
		[MenuItem("Dungeon/GAtest", false, 99)]
		public static void GAtest() {
			//Debug.Log(DateTime.Now.ToString());
			getGAExpriments();
		}

		public static void getGAExpriments() {
			int times = 1;
			for (int i = 0; i < times; i++) {
				StreamWriter sw = new StreamWriter("Export/expriment_" + (i + 1) + ".csv");
				sw.WriteLine("FitnessSupport,all");
				CreVoxGA.Segmentism();
				sw.Write(CreVoxGA.GenesScore);
				sw.Close();
			}
		}
	}
}

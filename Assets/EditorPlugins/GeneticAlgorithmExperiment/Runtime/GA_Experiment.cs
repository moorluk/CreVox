using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CrevoxExtend;

namespace GA_Experiment {
	public class Main : MonoBehaviour {
		// Gene prefab.
		public static Dictionary<GeneType, GameObject> GameobjectPrefabs;

		void Awake() {
			GameobjectPrefabs = new Dictionary<GeneType, GameObject>() {
				{ GeneType.Forbidden, null },
				{ GeneType.Empty    , null },
				{ GeneType.Enemy    , Resources.Load(@"GeneticAlgorithmExperiment/TempFolder/Prefab/enemies/BossAI") as GameObject },
				{ GeneType.Treasure , Resources.Load(@"GeneticAlgorithmExperiment/TempFolder/Prefab/treasure/Invector-Chest") as GameObject },
				{ GeneType.Trap     , Resources.Load(@"GeneticAlgorithmExperiment/TempFolder/Prefab/trap/spike_floor") as GameObject }
			};
			// Replace the gameobjects.
			ReplaceGameobjects();
		}

		void Start() {
		}

		private void ReplaceGameobjects() {
			Regex regex = new Regex(@"^(.+) \(.+\)$");
			var gamePatternObjects = GameObject.Find("GamePatternObjects");

			foreach (Transform gamePatternObject in gamePatternObjects.transform.Cast<Transform>().ToList()) {
				Debug.Log(gamePatternObject.transform.name);
				var match = regex.Match(gamePatternObject.transform.name);
				if (! match.Success) { break; }
				// If match the pattern, extract the type of the gameobject.
				var objectType = match.Groups[1].Value;
				var geneType = (GeneType) System.Enum.Parse(typeof(GeneType), objectType);
				// The gene has prefab.
				GameObject gameobject = GameObject.Instantiate(GameobjectPrefabs[geneType]);
				gameobject.transform.SetParent(gamePatternObjects.transform);
				gameobject.transform.position = gamePatternObject.transform.position;
				gameobject.transform.name     = gamePatternObject.transform.name;
				// Destory the marker.
				GameObject.Destroy(gamePatternObject.gameObject);
			}
		}
	}
}
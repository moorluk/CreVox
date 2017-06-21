using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CrevoxExtend;

namespace GA_Experiment {
	public class GA_Runtime : MonoBehaviour {
		// Gene prefab.
		public static Dictionary<GeneType, GameObject> GameobjectPrefabs;

		void Awake() {
			GameobjectPrefabs = new Dictionary<GeneType, GameObject>() {
				{ GeneType.Forbidden, null },
				{ GeneType.Empty    , null },
				{ GeneType.Enemy    , Resources.Load<GameObject>(@"GeneticAlgorithmExperiment/Prefabs/gameobjects/enemies/BossAI") },
				{ GeneType.Treasure , Resources.Load<GameObject>(@"GeneticAlgorithmExperiment/Prefabs/gameobjects/treasure/Invector-Chest") },
				{ GeneType.Trap     , Resources.Load<GameObject>(@"GeneticAlgorithmExperiment/Prefabs/gameobjects/trap/spike_floor") }
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
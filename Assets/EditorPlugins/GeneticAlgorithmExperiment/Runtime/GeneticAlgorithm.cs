using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using SystemRandom = System.Random;
using StreamWriter = System.IO.StreamWriter;
using GC     = System.GC;
using Math   = System.Math;
using Enum   = System.Enum;
using RegExp = System.Text.RegularExpressions;

using CreVox;
using NTUSTGA;

namespace CrevoxExtend {
	// Enum for type of gene.
	public enum GeneType {
		Forbidden = -1,
		Empty     = 0,
		Enemy     = 1,
		Treasure  = 2,
		Trap      = 3
	}

	public class CreVoxGA {
		private static StreamWriter DatasetExport { get; set; }

		public static int GenerationNumber { get; set; }
		public static int PopulationNumber { get; set; }
		public static Dictionary<string, int> FitnessWeights = new Dictionary<string, int>();

		public static Dictionary<GeneType, GameObject> MarkerPrefabs = new Dictionary<GeneType, GameObject>() {
			{ GeneType.Forbidden, null },
			{ GeneType.Empty    , null },
			{ GeneType.Enemy    , Resources.Load<GameObject>(@"GeneticAlgorithmExperiment/Prefabs/markers/marker_enemy") },
			{ GeneType.Treasure , Resources.Load<GameObject>(@"GeneticAlgorithmExperiment/Prefabs/markers/marker_treasure") },
			{ GeneType.Trap     , Resources.Load<GameObject>(@"GeneticAlgorithmExperiment/Prefabs/markers/marker_trap") }
		};
		public static Dictionary<GeneType, Material> MarkerMaterials = new Dictionary<GeneType, Material>() {
			{ GeneType.Forbidden, null },
			{ GeneType.Empty    , null },
			{ GeneType.Enemy    , Resources.Load<Material>(@"GeneticAlgorithmExperiment/Prefabs/markers/Materials/marker_enemy") },
			{ GeneType.Treasure , Resources.Load<Material>(@"GeneticAlgorithmExperiment/Prefabs/markers/Materials/marker_treasure") },
			{ GeneType.Trap     , Resources.Load<Material>(@"GeneticAlgorithmExperiment/Prefabs/markers/Materials/marker_trap") }
		};

		// Game patterns objects are expressed via Enemy, Treasure, Trap, ... etc.
		private static GameObject GamePatternObjects {
			get {
				return GameObject.Find("GamePatternObjects") ?? new GameObject("GamePatternObjects");
			}
		}
		private static readonly string[] _pieceList = { "Gnd.in.one", "Stair.one" };
		private static Dictionary<Vector3, int> _mainPath = new Dictionary<Vector3, int>();

		//calculate all of chromosome.
		public static uint AllChromosomeCount;

		//enum for fitness function.
		public enum FitnessFunctionName {
			Block,
			Intercept,
			Patrol,
			Guard,
			Support
		}

		// Constructor.
		static CreVoxGA() {
			GenerationNumber = 20;
			PopulationNumber = 250;
			FitnessWeights = new Dictionary<string, int>() {
				{ "neglected", 0 },
				{ "block"    , 0 },
				{ "intercept", 0 },
				{ "patrol"   , 0 },
				{ "guard"    , 0 },
				{ "dominated", 0 },
				{ "support"  , 0 }
			};
		}

		public static void SetWeights(Dictionary<string, int> fitnessWeights) {
			FitnessWeights = fitnessWeights;
		}

		public static void Initialize() {
			AllChromosomeCount = default(uint);
			foreach (var child in GamePatternObjects.transform.Cast<Transform>().ToList()) {
				GameObject.DestroyImmediate(child.gameObject);
			}
		}

		public static NTUSTChromosome Segmentism(int populationNumber, int generationNumber, StreamWriter sw = null) {
			Initialize();

			// Set the number of population and generation.
			PopulationNumber = populationNumber;
			GenerationNumber = generationNumber;
			// Update the StreamWriter of DatasetExport.
			DatasetExport = sw;
			CreVoxChromosome bestChromosome = new CreVoxChromosome();
			foreach (var volume in GetVolumeByVolumeManager()) {
				NTUSTGeneticAlgorithm ntustGA = new CreVoxGAA(0.8f, 0.1f, GetSample(_pieceList, volume), PopulationNumber, GenerationNumber);
				// Populations, Generations.
				bestChromosome = ntustGA.Algorithm() as CreVoxChromosome;
				BestChromosomeToWorldPos(volume, bestChromosome);
			}
			GC.Collect();
			// [Will Modify]
			return bestChromosome;
			//return null;
		}
		// Overloading. No initialize.
		public static NTUSTChromosome Segmentism(int populationNumber, int generationNumber, string roomName, StreamWriter sw = null) {
			// Set the number of population and generation.
			PopulationNumber = populationNumber;
			GenerationNumber = generationNumber;
			// Update the StreamWriter of DatasetExport.
			DatasetExport = sw;
			CreVoxChromosome bestChromosome = new CreVoxChromosome();
			foreach (var volume in GetVolumeByVolumeManager()) {
				// Run specific room only.
				if (volume.vd.name != roomName) { continue; }
				
				NTUSTGeneticAlgorithm ntustGA = new CreVoxGAA(0.8f, 0.1f, GetSample(_pieceList, volume), PopulationNumber, GenerationNumber);
				// Populations, Generations.
				bestChromosome = ntustGA.Algorithm() as CreVoxChromosome;
				BestChromosomeToWorldPos(volume, bestChromosome);
			}
			GC.Collect();
			// [Will Modify]
			return bestChromosome;
			//return null;
		}

		// Get all of volumes frin volume manager.
		public static List<Volume> GetVolumeByVolumeManager() {
			var VolumeManager = GameObject.Find("VolumeManager(Generated)");
			return VolumeManager.GetComponentsInChildren<Volume>().ToList();
		 }

		// use volume to get each block position.
		public static CreVoxChromosome GetSample(string[] blockAirPieceList, Volume volume) {
			List<Vector3> tiles = new List<Vector3>();

			// Parse the decorations, create the passable tiles space.
			var decorations = volume.gameObject.transform.Find("DecorationRoot");
			foreach (Transform decoration in decorations) {
				// Select the piece from block air, then add it.
				Transform tile = null;

				foreach (var pieceName in blockAirPieceList) {
					// If find it in the list.
					if (decoration.Find(pieceName) != null) {
						// Get the local position via the name of the gameObject.
						var match = RegExp.Regex.Match(decoration.name, @"^(\d), (\d), (\d)$");
						if (match.Success) {
							Vector3 tilePos = new Vector3(float.Parse(match.Groups[1].Value), float.Parse(match.Groups[2].Value), float.Parse(match.Groups[3].Value));
							tiles.Add(tilePos);
						}
						break;
					}
				}
			}

			var items = volume.gameObject.transform.Find("ItemRoot");
			Vector3 startPosition = items.Find("Starting Node").transform.position - volume.transform.position;
			Vector3 endPosition;
			// Initial the main path.
			_mainPath.Clear();

			// Each item.
			foreach (Transform item in items) {
				// Ignore it if it is not connection.
				if (! item.name.Contains("Connection_")) { continue; }
				// Get the position of connection.
				endPosition = item.transform.position - volume.transform.position;

				Astar.World world = new Astar.World(9, 9, 9);
				for (int x = 0; x < 9; x++) {
					for (int y = 0; y < 9; y++) {
						for (int z = 0; z < 9; z++) {
							world.MarkPosition(new Astar.Point3D(x, y, z), true);
						}
					}
				}

				foreach (Transform decoration in decorations) {
					// If the voxel not include the decoration piece, then skip this one.
					bool isWalkable = false;
					foreach (var pieceName in _pieceList) {
						if (decoration.Find(pieceName) != null) { isWalkable = true; }
					}
					if (! isWalkable) { continue; }
					// Get the local position via the name of the gameObject.
					var match = RegExp.Regex.Match(decoration.name, @"^(\d), (\d), (\d)$");
					if (match.Success) {
						Vector3 decPos = new Vector3(float.Parse(match.Groups[1].Value), float.Parse(match.Groups[2].Value), float.Parse(match.Groups[3].Value));
						world.MarkPosition(new Astar.Point3D((int) decPos.x, (int) decPos.y, (int) decPos.z), false);
					}
				}

				// Y-axis is special because the position of the conntection.
				Astar.Point3D startPoint = new Astar.Point3D((int) startPosition.x / 3, (int) startPosition.y / 2 + 1, (int) startPosition.z / 3);
				Astar.Point3D endPoint   = new Astar.Point3D((int) endPosition.x   / 3, (int) endPosition.y   / 2 + 1, (int) endPosition.z   / 3);
				Astar.SearchNode pathFlow = Astar.PathFinder.FindPath(world, startPoint, endPoint);

				// Parse the path. Increase 1 if the position is exist; otherwise create a new one.
				while (pathFlow != null) {
					var tilePos = new Vector3(pathFlow.position.X, pathFlow.position.Y, pathFlow.position.Z);
					if (_mainPath.ContainsKey(tilePos)) {
						_mainPath[tilePos] += 1;
					} else {
						_mainPath.Add(tilePos, 1);
					}
					pathFlow = pathFlow.next;
				}
			}

			return new CreVoxChromosome(tiles);
		}

		private static Transform GetPieceFromDecoration(Transform decoration) {
			foreach (var pieceName in _pieceList) {
				if (decoration.Find(pieceName) != null) {
					return decoration.Find(pieceName);
				}
			}
			return null;
		}

		//make the best gene is added into world.
		public static void BestChromosomeToWorldPos(Volume volume, CreVoxChromosome bestChromosome) {
			var decorations = volume.transform.Find("DecorationRoot");
			foreach (CreVoxGene gene in bestChromosome.Genes) {
				if (gene.Type != GeneType.Empty) {
					var decoration = decorations.Find(gene.pos.x + ", " + gene.pos.y + ", " + gene.pos.z);
					var position   = GetPieceFromDecoration(decoration).position;
					GameObject geneWorldPosition = GameObject.Instantiate(MarkerPrefabs[gene.Type]);
					geneWorldPosition.transform.SetParent(GamePatternObjects.transform);
					geneWorldPosition.transform.position = position;
					geneWorldPosition.transform.name = gene.Type.ToString() + " " + position;
					geneWorldPosition.GetComponent<Renderer>().material = MarkerMaterials[gene.Type];
				}
			}
		}

		public class CreVoxGAA : NTUSTGeneticAlgorithm {
			public CreVoxGAA(float crossoverRate, float mutationRate, NTUSTChromosome sample, int countOfChromosome, int countOfGeneration) : base(crossoverRate, mutationRate, sample, countOfChromosome, countOfGeneration) {

			}

			// Two-point crossover.
			public override void Crossover(ref NTUSTChromosome parentCopy1, ref NTUSTChromosome parentCopy2) {
				int min = Random.Range(0, parentCopy1.Genes.Count);
				int max = Random.Range(min, parentCopy1.Genes.Count);

				for (int i = min; i < max; i++) {
					NTUSTChromosome.NTUSTGene swapGene = parentCopy1.Genes[i];
					parentCopy1.Genes[i] = parentCopy2.Genes[i];
					parentCopy2.Genes[i] = swapGene;
				}
			}

			public override void Mutation(ref NTUSTChromosome chrom) {
				CreVoxChromosome CreVoxChrom = chrom as CreVoxChromosome;
				// Start to mutate.
				var random = new SystemRandom();
				// Filtering the percent numbers for genes.
				var genes = CreVoxChrom.getGenes();
				var percent = (int)Math.Ceiling(Random.Range(0.05f, 0.20f) * genes.Count);
				var filteredGenes = genes.OrderBy(g => random.Next()).Take(percent).ToList();
				// Change type each gene.
				foreach (var gene in filteredGenes) {
					var types = System.Enum
						.GetValues(typeof(GeneType))
						.Cast<GeneType>()
						.Where(t => t != GeneType.Forbidden && t != gene.Type)
						.ToArray();

					gene.Type = types[Random.Range(0, types.Length)];
				}
			}
		}

		public class CreVoxChromosome : NTUSTChromosome {
			public List<CreVoxGene> getGenes() {
				return Genes.Select(g => g as CreVoxGene).ToList();
			}

			public CreVoxChromosome() {

			}

			public CreVoxChromosome(List<Vector3> allPossiblePosition) {
				foreach (Vector3 pos in allPossiblePosition) {
					this.Genes.Add(new CreVoxGene(GeneType.Empty, pos));
				}
			}

			public override NTUSTChromosome RandomInitialize() {
				CreVoxChromosome result = new CreVoxChromosome();
				foreach (CreVoxGene gene in this.Genes)
					result.Genes.Add(new CreVoxGene(GeneType.Empty, gene.pos));

				return result;
			}

			public override NTUSTChromosome Copy() {
				CreVoxChromosome result = new CreVoxChromosome();

				foreach (CreVoxGene gene in this.Genes)
					result.Genes.Add(gene.Copy());

				return result;
			}

			public override float FitnessFunction() {
				// Chromosomeinfo for csv file, the number from 1 to end.(run,generation,chromosome)
				string chromosomeInfo = (AllChromosomeCount / (GenerationNumber * PopulationNumber) + 1) + ","
										+ (AllChromosomeCount / (PopulationNumber) % GenerationNumber + 1) + ","
										+ (AllChromosomeCount % PopulationNumber + 1);

				float scoreSum = 0.0f
					+ (FitnessWeights["block"]  != 0 ? FitnessBlock()  * FitnessWeights["block"]  : 0)
					+ (FitnessWeights["intercept"] != 0 ? FitnessIntercept() * FitnessWeights["intercept"] : 0)
					+ (FitnessWeights["patrol"] != 0 ? FitnessPatrol() * FitnessWeights["patrol"] : 0)
					+ (FitnessWeights["guard"]  != 0 ? FitnessGuard()  * FitnessWeights["guard"]  : 0)
					+ (FitnessWeights["support"] != 0 ? FitnessSupport() * FitnessWeights["support"] : 0)
					+ (FitnessEmptyDensity() * 0)
				;

				var fitnessNames = Enum.GetValues(typeof(FitnessFunctionName));

				// If DatasetExport is not null, export the data.
				if (!csvFinished) {
					if (DatasetExport != null) {
						foreach (var gene in Genes.Select(g => g as CreVoxGene)) {
							// All of fitness and it's score in a gene.
							foreach (FitnessFunctionName fitnessName in fitnessNames) {
								DatasetExport.WriteLine(chromosomeInfo + "," + fitnessName + "," + GetFitnessScore(fitnessName) + ",\"" + gene.pos + "\"," + gene.Type);
							}
						}
					}
				}
				// IterrateTime++ for next time.
				AllChromosomeCount++;

				return scoreSum;
			}

			public float GetFitnessScore(FitnessFunctionName functionName) {
				switch (functionName) {
					case FitnessFunctionName.Block:
						return FitnessBlock();
					case FitnessFunctionName.Intercept:
						return FitnessIntercept();
					case FitnessFunctionName.Patrol:
						return FitnessPatrol();
					case FitnessFunctionName.Guard:
						return FitnessGuard();
					case FitnessFunctionName.Support:
						return FitnessSupport();
					default:
						return 0;
				}
			}

			public float FitnessBlock() {
				float fitnessScore = 0.0f;
				float enemyWeightSum = 0.0f;
				float mainPathWeightSum = 0.0f;

				var enemies = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type == GeneType.Enemy).ToList();

				// Must have any enemy.
				if (enemies.Count != 0) {
					// Sum of enemy weight/count.
					enemyWeightSum = enemies.Sum(e => (_mainPath.ContainsKey(e.pos) ? _mainPath[e.pos] : 0));
					/*
					Debug.Log(enemyWeightSum);
					foreach (var enemy in enemies) {
						Debug.Log("Enemy: " + enemy.pos);
					}
					foreach (var tile in _mainPath.Keys) {
						Debug.Log("MP: " + tile + "  " + _mainPath[tile]);
					}
					*/
					// Sum of the visited times in main path.
					mainPathWeightSum = _mainPath.Sum(mp => mp.Value);
					// Calculate the fitness score.
					fitnessScore = (float)Math.Max(Math.Log(enemyWeightSum, mainPathWeightSum), -1.0);
				}

				return fitnessScore;
			}

			public float FitnessIntercept() {
				float fitnessScore = 0.0f;
				float mainPathWeightSum = 0.0f;
				float distanceOfEnemyAndMainPath = 0.0f;
				float flexibilityScore = 0.0f;

				var enemies = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type == GeneType.Enemy).ToList();

				// Must have any enemy.
				if (enemies.Count != 0) {
					// Sum of the visited times in main path.
					mainPathWeightSum = _mainPath.Sum(mp => mp.Value);
					// Different enemy
					for (int i = 0; i < enemies.Count; i++) {
						// Enemy cann't on the mathPath.
						if (!_mainPath.ContainsKey(enemies[i].pos)) {
							// Different point of mainPath
							foreach (KeyValuePair<Vector3, int> pointOfMainPath in _mainPath) {
								// Calculate the distance of enemy and mainPath.
								distanceOfEnemyAndMainPath = Vector3.Distance(enemies[i].pos, pointOfMainPath.Key);
								// Calculate the flexibility score.
								flexibilityScore += (float)(1 / distanceOfEnemyAndMainPath) * (pointOfMainPath.Value / mainPathWeightSum);
							}
						}						
					}
					// Normalize the flexibility score to be fitness Score.
					fitnessScore = (float)Math.Max(Math.Log(flexibilityScore, enemies.Count), -1.0);
				}

				return fitnessScore;
			}

			public float FitnessPatrol() {
				float fitnessScore = 0.0f;
				float radius = 3.0f;
				int neighborCount = 0;

				var enemies = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type == GeneType.Enemy).ToList();
				var passables = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type != GeneType.Forbidden).ToList();

				// Must have any enemy.
				if (enemies.Count != 0) {
					for (int i = 0; i < enemies.Count; i++) {
						// Calculate the amount of neighbor.
						neighborCount = passables.Sum(passable => (Vector3.Distance(passable.pos, enemies[i].pos) <= radius ? 1 : 0));
						// If is the last one in list or not.
						if (i != enemies.Count - 1) {
							fitnessScore += (float)((1.0 / Math.Pow(2, i + 1)) * neighborCount);
						}
						else {
							fitnessScore += (float)((1.0 / Math.Pow(2, i)) * neighborCount);
						}
					}
				}

				return fitnessScore;
			}

			public float FitnessGuard() {
				float fitnessScore = 0.0f;
				float avgProtector;
				Dictionary<CreVoxGene, List<CreVoxGene>> neighbors;

				var enemies = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type == GeneType.Enemy).ToList();

				var objectives = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type == GeneType.Treasure).ToList();

				// Must have any enemy and objective.
				if (enemies.Count != 0 && objectives.Count != 0) {
					// GeneA (Objective) has own GeneB (Enemies).
					neighbors = new Dictionary<CreVoxGene, List<CreVoxGene>>();
					// Create the pair each objective.
					foreach (var objective in objectives) { neighbors.Add(objective, new List<CreVoxGene>()); }
					// How many enemies	protect per objective.
					avgProtector = 1.0f * enemies.Count / objectives.Count;
					// Add the minimum distance into the neighbors of objective.
					foreach (var enemy in enemies) {
						var protectedTarget = (
							from objective in objectives
							let distance = Vector3.Distance(enemy.pos, objective.pos)
							where distance < 10
							orderby distance
							select objective
						).FirstOrDefault();
						// If not found then add this one.
						if (protectedTarget != null) { neighbors[protectedTarget].Add(enemy); }
					}
					// Calculate the fitness score.
					fitnessScore = objectives.Sum(o => (avgProtector - Math.Abs(neighbors[o].Count - avgProtector)) / avgProtector / objectives.Count);
				}

				return fitnessScore;
			}

			public float FitnessSupport() {
				float fitnessScore = 0.0f;
				float radius = 3.0f;
				float distanceBetweenEnemies = 0.0f;
				float mainPathWeightSum = 0.0f;
				float distanceOfEnemyAndMainPath = 0.0f;
				float flexibilityScore = 0.0f;
				// The score of distance between enemies..
				float distanceScore = 0.0f;

				var enemies = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type == GeneType.Enemy).ToList();

				// Must have any enemy.
				if (enemies.Count != 0) {
					// Sum of the visited times in main path.
					mainPathWeightSum = _mainPath.Sum(mp => mp.Value);
					for (int i = 0; i < enemies.Count; i++) {
						for (int j = 0; j < enemies.Count; j++) {
							if (i != j) {
								distanceBetweenEnemies = Vector3.Distance(enemies[i].pos, enemies[j].pos);
								distanceScore += (float)(1 - distanceBetweenEnemies / radius) / enemies.Count;
							}							
						}
					}
					// Calculate the fitness score.
					fitnessScore = (float)(Math.Max(Math.Log((float)(Math.Max((distanceScore + (1 - FitnessIntercept())) / 2, Math.Pow(enemies.Count, -1.0))), enemies.Count), -1.0));
				}

				return fitnessScore;
			}

			public float FitnessEmptyDensity() {
				List<CreVoxGene> empties = this.Genes.Select(g => g as CreVoxGene).Where(g => g.Type == GeneType.Empty).ToList();

				return 1.0f * empties.Count / this.Genes.Count;
			}
		}

		public class CreVoxGene : NTUSTChromosome.NTUSTGene {
			public GeneType Type { get; set; }

			public Vector3 pos { get; set; }

			public CreVoxGene(GeneType type, Vector3 pos) {
				this.Type = type;
				this.pos = pos;
			}

			public override NTUSTChromosome.NTUSTGene Copy() {
				return new CreVoxGene(this.Type, this.pos);
			}
		}
	}
}

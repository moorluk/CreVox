using System.Linq;
using System.Collections.Generic;
using SystemRandom = System.Random;
using StreamWriter = System.IO.StreamWriter;
using GC = System.GC;
using Math = System.Math;
using UnityEngine;
using UnityEditor;
using Enum = System.Enum;

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

		// Game patterns objects are expressed via Enemy, Treasure, Trap, ... etc.
		private static GameObject GamePatternObjects {
			get {
				return GameObject.Find("GamePatternObjects") ?? new GameObject("GamePatternObjects");
			}
		}
		private static readonly string[] _picecName = { "Gnd.in.one" };
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

			foreach (var volume in GetVolumeByVolumeManager()) {
				NTUSTGeneticAlgorithm ntustGA = new CreVoxGAA(0.8f, 0.1f, GetSample(_picecName, volume), PopulationNumber, GenerationNumber);

				// Populations, Generations.
				var bestChromosome = ntustGA.Algorithm() as CreVoxChromosome;

				BestChromosomeToWorldPos(bestChromosome);

				// Temp for first room.
				return bestChromosome;
			}
			GC.Collect();
			return null;
		}

		// Get all of volumes frin volume manager.
		public static List<Volume> GetVolumeByVolumeManager() {
			List<Volume> volumes = new List<Volume>();
			var VolumeManager = GameObject.Find("VolumeManager(Generated)");
			foreach (Transform volume in VolumeManager.transform) {
				volumes.Add(volume.GetComponent<Volume>());
			}
			return volumes;
		}

		// use volume to get each block position.
		public static CreVoxChromosome GetSample(string[] blockAirPieceName, Volume volume) {
			List<Vector3> tiles = new List<Vector3>();

			// Parse the decorations, create the passable tiles space.
			var decorations = volume.gameObject.transform.Find("DecorationRoot");
			foreach (Transform decoration in decorations) {
				// Select the piece from block air, then add it.
				Transform tile = null;
				foreach (string pieceName in blockAirPieceName) {
					tile = decoration.Find(pieceName);
					if (tile != null) {
						tiles.Add(tile.position);
						break;
					}
				}
			}

			// A-star, defines the main path.
			var map = MakeAMap(volume.gameObject);
			var items = volume.gameObject.transform.Find("ItemRoot");
			Vector3 startPosition = items.Find("Starting Node").transform.position;
			Vector3 endPosition;
			// Initial the main path.
			_mainPath.Clear();
			// Each item.
			foreach (Transform item in items) {
				// Ignore it if it is not connection.
				if (!item.name.Contains("Connection_")) { continue; }
				// Get the position of connection.
				endPosition = item.transform.position;
				// Execute the A-Star.
				AStar astar = new AStar(map, startPosition, endPosition);
				// Parse the path. Increase 1 if the position is exist; otherwise create a new one. 
				foreach (var pos in astar.theShortestPath) {
					if (_mainPath.ContainsKey(pos.position3)) {
						_mainPath[pos.position3] += 1;
					}
					else {
						_mainPath.Add(pos.position3, 1);
					}
				}
			}

			return new CreVoxChromosome(tiles);
		}

		//make the best gene is added into world.
		public static void BestChromosomeToWorldPos(CreVoxChromosome bestChromosome) {
			foreach (CreVoxGene gene in bestChromosome.Genes) {
				if (gene.Type != GeneType.Empty) {
					GameObject geneWorldPosition = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					geneWorldPosition.transform.SetParent(GamePatternObjects.transform);
					geneWorldPosition.transform.position = gene.pos;
					geneWorldPosition.transform.name = gene.Type.ToString() + " " + gene.pos;

#if UNITY_EDITOR
					switch (gene.Type) {
					case GeneType.Forbidden:
						geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
						break;
					case GeneType.Enemy:
						geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
						break;
					case GeneType.Treasure:
						geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
						break;
					default:
						geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
						break;
					}
#endif
				}
			}
		}

		//make current volume to a 3d int array map for A*.
		public static int[,,] MakeAMap(GameObject volume) {
			int[,,] tiles = new int[9, 9, 9];
			var decorationRoots = volume.transform.Find("DecorationRoot");
			for (int i = 0; i < decorationRoots.childCount; ++i) {
				var tile = decorationRoots.GetChild(i).Find(_picecName[0]).position;
				// 1 means the tile is passable. (Width: 3 x Height: 2 x Length: 3)
				if (tile != null) {
					// Because all "decorations" are reduced 1.
					tiles[(int)tile.x / 3, (int)(tile.y + 1) / 2, (int)tile.z / 3] = 1;
				}
			}
			return tiles;
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
					this.Genes.Add(new CreVoxGene(GeneType.Enemy, pos));
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
				if (DatasetExport != null) {
					foreach (var gene in Genes.Select(g => g as CreVoxGene)) {
						// All of fitness and it's score in a gene.
						foreach (FitnessFunctionName fitnessName in fitnessNames) {
							DatasetExport.WriteLine(chromosomeInfo + "," + fitnessName + "," + GetFitnessScore(fitnessName) + ",\"" + gene.pos + "\"," + gene.Type);
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
								flexibilityScore += (1 / distanceOfEnemyAndMainPath) * (pointOfMainPath.Value / mainPathWeightSum);
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
								distanceScore += (1 - distanceBetweenEnemies / radius) / enemies.Count;
							}							
						}

						if (!_mainPath.ContainsKey(enemies[i].pos)) {
							// Different point of mainPath
							foreach (KeyValuePair<Vector3, int> pointOfMainPath in _mainPath) {
								// Calculate the distance of enemy and mainPath.
								distanceOfEnemyAndMainPath = Vector3.Distance(enemies[i].pos, pointOfMainPath.Key);
								// Calculate the flexibility score.
								flexibilityScore += (1 / distanceOfEnemyAndMainPath) * (pointOfMainPath.Value / mainPathWeightSum);
							}
						}
					}
					// Calculate the fitness score.
					fitnessScore = (float)Math.Max(Math.Log((distanceScore + (1 - flexibilityScore)) / 2, enemies.Count), -1.0);
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

using System.Linq;
using System.Collections.Generic;
using SystemRandom = System.Random;
using Stopwatch    = System.Diagnostics.Stopwatch;
using GC = System.GC;
using Math = System.Math;
using UnityEngine;
using UnityEditor;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Terminations;

using CreVox;

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
		public static int   GenerationNumber { get; set; }
		public static float CrossoverRate    { get; set; }
		public static float MutationRate     { get; set; }
		// Game patterns objects are expressed via Enemy, 
		private static GameObject GamePatternObjects {
			get {
				return GameObject.Find("GamePatternObjects") ?? new GameObject("GamePatternObjects");
			}
			set {
				GamePatternObjects = value;
			}
		}
		private static readonly string[] _picecName = { "Gnd.in.one" };

		private static Dictionary<Vector3, int> _mainPath = new Dictionary<Vector3, int>();
		public static string GenesScore;
		public static Dictionary<Vector3, CreVoxGene> tiles = new Dictionary<Vector3, CreVoxGene>();

		// Constructor.
		static CreVoxGA() {
			GenerationNumber = 20;
			// Crossover rate is [0, 100)
			CrossoverRate    = 1f;
			// Mutation rate is [0, 100)
			MutationRate     = 1.0f;
		}

		public static void Initialize() {
			foreach (Transform child in GamePatternObjects.transform) {
				GameObject.DestroyImmediate(child.gameObject);
			}
		}

		// Add the 'Level settings' in 'Dungeon' menu.
		[MenuItem("Dungeon/沒有CSV，直接跑GA", false, 998)]
		public static void GoFighting() {
			Segmentism();
		}

		public static void Segmentism() {
			Initialize();
			// Start timer.
			Stopwatch sw = new Stopwatch();
			sw.Start();
			foreach (var volume in getVolumeByVolumeManager()) {
				// Tiles (key: Postion; value: CreVoxGene).
				tiles = GetTiles(_picecName, volume);
				GenesScore = default(string);
				// Instance necessary class for GA.
				var selection  = new RouletteWheelSelection();
				var crossover  = new MyCrossover();
				var mutation   = new MyMutation();
				var fitness    = new MyProblemFitness();
				var chromosome = new MyProblemChromosome(tiles);
				var population = new Population(250, 250, chromosome);
				// Execute GA.
				var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
				ga.Termination = new GenerationNumberTermination(GenerationNumber);
				ga.CrossoverProbability = CrossoverRate;
				ga.MutationProbability  = MutationRate;
				Debug.Log("GA running...");
				ga.Start();
				Debug.Log("Best solution found has " + ga.BestChromosome.Fitness + " fitness.");
				BestChromosomeToWorldPos(tiles, ga.BestChromosome);
			}
			sw.Stop();
			Debug.Log(sw.ElapsedMilliseconds + " ms");
			GC.Collect();
		}

		// Get all of volumes frin volume manager.
		public static List<Volume> getVolumeByVolumeManager() {
			List<Volume> volumes = new List<Volume>();
			var volumeManger = GameObject.Find("VolumeManger(Generated)");
			foreach (Transform volume in volumeManger.transform) {
				volumes.Add(volume.GetComponent<Volume>());
			}
			return volumes;
		}

		// use volume to get each block position.
		public static Dictionary<Vector3, CreVoxGene> GetTiles(string[] blockAirPieceName, Volume volume) {
			Dictionary<Vector3, CreVoxGene> tiles = new Dictionary<Vector3, CreVoxGene>();

			// Parse the decorations, create the passable tiles space.
			var decorations = volume.gameObject.transform.Find("DecorationRoot");
			foreach (Transform decoration in decorations) {
				// Select the piece from block air, then add it.
				Transform tile = null;
				foreach (string pieceName in blockAirPieceName) {
					tile = decoration.Find(pieceName);
					if (tile != null) {
						tiles.Add(tile.position, new CreVoxGene());
						break;
					}
				}
			}

			// A-star, defines the main path.
			var map   = makeAMap(volume.gameObject);
			var items = volume.gameObject.transform.Find("ItemRoot");
			Vector3 startPosition = items.Find("Starting Node").transform.position;
			Vector3 endPosition;
			// Initial the main path.
			_mainPath.Clear();
			// Each item.
			foreach (Transform item in items) {
				// Ignore it if it is not connection.
				if (! item.name.Contains("Connection_")) { continue; }
				// Get the position of connection.
				endPosition = item.transform.position;
				// Execute the A-Star.
				AStar astar = new AStar(map, startPosition, endPosition);
				// Parse the path. Increase 1 if the position is exist; otherwise create a new one. 
				foreach (var pos in astar.theShortestPath) {
					if (_mainPath.ContainsKey(pos.position3)) {
						_mainPath[pos.position3] += 1;
					} else {
						_mainPath.Add(pos.position3, 1);
					}
				}
			}

			return tiles;
		}

		//make the best gene is added into world.
		public static void BestChromosomeToWorldPos(Dictionary<Vector3, CreVoxGene> tiles, IChromosome bestChromosome) {
			// foreach (var gene in bestChromosome.GetGenes()) {
			var genes = bestChromosome.GetGenes();
			for (int i = 0; i < tiles.Count; i++) {
				GameObject geneWorldPosition = null;
				if ((genes[i].Value as CreVoxGene).Type != GeneType.Empty) {
					geneWorldPosition = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					geneWorldPosition.transform.SetParent(GamePatternObjects.transform);
					geneWorldPosition.transform.position = tiles.Keys.ElementAt(i);
					// Set the color.
					switch ((genes[i].Value as CreVoxGene).Type) {
#if UNITY_EDITOR
						case GeneType.Forbidden:
							geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
							break;
						case GeneType.Empty:
							geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
							break;
						case GeneType.Enemy:
							geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
							break;
						case GeneType.Treasure:
							geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
							break;
						default:
							geneWorldPosition.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
							break;
					}
				}
#endif
			}
		}

		//make current volume to a 3d int array map for A*.
		public static int[,,] makeAMap(GameObject volume) {
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

		//implement necessary class for GA.
		// public class MySelection : ISelection {
		// 	public MySelection() {
		// 	}

		// 	public override IList<IChromosome> SelectChromosomes(int number, Generation generation) {
		// 		return PerformSelectChromosomes(number, generation);
		// 	}

		// 	protected override IList<IChromosome> PerformSelectChromosomes(IList<IChromosome> parents) {
		// 		var ordered = generation.Chromosomes.OrderByDescending(c => c.Fitness);
		// 		return ordered.Take(number).ToList();
		// 	}
		// }

		//implement MyProblemFitness for IFitness.
		public class MyProblemFitness : IFitness {
			public double Evaluate(IChromosome chromosome) {
				double fitnessValue = default(double);

				fitnessValue += 0.001
							// + FitnessBlock(chromosome) * 10
							// + FitnessIntercept(chromosome) * 1
							// + FitnessSupport(chromosome) * 10
							// + FitnessEnemyDensity(chromosome) * 5
							+ FitnessPatrol(chromosome) * 3
							+ FitnessGuard(chromosome) * 7
							// + FitnessTesting(chromosome) * 1
							
							+ FitnessEmptyDensity(chromosome) * 10
							;
				GenesScore += fitnessValue + "\n";
				return fitnessValue;
			}

			public double FitnessBlock(IChromosome chromosome) {
				double fitnessScore = 0;
				double enemyWeightTotal = 0;
				double mainPathWeightTotal = 0;
				var enemies = chromosome.GetGenes()
					.Select((gene, index) => new { gene, index })
					.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				// Sum of enemy weight/count.
				enemyWeightTotal = enemies.Sum(e => {
					var position = tiles.Keys.ElementAt(e.index);
					return _mainPath.ContainsKey(position) ? _mainPath[position] : 0;
				});
				// Sum of the visited times in main path.
				mainPathWeightTotal = _mainPath.Sum(p => p.Value);
				// Calculate the fitness score.
				fitnessScore = 1.0f * enemyWeightTotal / mainPathWeightTotal;
				// Write into the csv.
				GenesScore += fitnessScore + ", ";
				return fitnessScore;
			}

			public double FitnessIntercept(IChromosome chromosome) {
				double fitnessScore = 0.0f;
				double enemyWeightTotal = 0;
				var enemies = chromosome.GetGenes()
					.Select((gene, index) => new { gene, index })
					.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Enemy).ToList();

				foreach (var enemy in enemies) {
					var enemyPosition = tiles.Keys.ElementAt(enemy.index);
					//   M
					// SIGMA( (1 / dist(E, MP(j)) * mp(j)  )
					// j = 1
					float enemyAndMainPathDisSum = 0.0f;
					foreach (var mainPathTile in _mainPath) {
						if (enemyPosition != mainPathTile.Key) {
							enemyAndMainPathDisSum = (1 / Vector3.Distance(enemyPosition, mainPathTile.Key)) * mainPathTile.Value;
						}
					}
					fitnessScore += enemyAndMainPathDisSum;
				}
				GenesScore += fitnessScore + ",";
				return fitnessScore;
			}

			public double FitnessSupport(IChromosome chromosome) {
				var enemies = chromosome.GetGenes()
					.Select((gene, index) => new { gene, index })
					.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				float fitnessScore = 0.0f;
				foreach (var enemy1 in enemies) {
					float enemyDisSum = 0.0f;
					foreach (var enemy2 in enemies) {
						if (enemy1 != enemy2) {
							// Position of first enemy.
							var position1 = tiles.Keys.ElementAt(enemy1.index);
							var position2 = tiles.Keys.ElementAt(enemy2.index);

							enemyDisSum += 1.0f / (position1 - position2).magnitude;

						}
					}
					// float mpDisSum = 0.0f;
					// foreach (var tile in path) {
					// 	mpDisSum += (((enemy1.Value as CreVoxGene).Position - tile.Position).magnitude) / 15.588f;
					// }
					// fitnessScore += (enemyDisSum / enemies.Count + mpDisSum / path.Count) / 2.0f;
					fitnessScore += enemyDisSum / enemies.Count;
				}
				fitnessScore /= enemies.Count == 0 ? 1 : enemies.Count;
				GenesScore += fitnessScore + ",";
				return fitnessScore;
			}

			public double FitnessEnemyDensity(IChromosome chromosome) {
				var enemies = chromosome.GetGenes()
					.Select((gene, index) => new { gene, index })
					.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				float fitnessScore = 0.0f;

				foreach (var enemy1 in enemies) {
					float enemyDisSum = 0.0f;
					foreach (var enemy2 in enemies) {
						if (enemy1 != enemy2) {
							// Position of first enemy.
							var position1 = tiles.Keys.ElementAt(enemy1.index);
							var position2 = tiles.Keys.ElementAt(enemy2.index);
							if ((position1 - position2).magnitude < 1.5 * 3) {
								enemyDisSum += 1.0f / (float)Math.Pow((position1 - position2).magnitude, 2);
							}

							fitnessScore += enemyDisSum / enemies.Count;

						}
					}

				}

				GenesScore += fitnessScore + ",";
				return fitnessScore;
			}

			public double FitnessEmptyDensity(IChromosome chromosome) {
				var empties = chromosome.GetGenes()
					.Select((gene, index) => new { gene, index })
					.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Empty).ToList();
				float fitnessScore = 0.0f;
				fitnessScore = (1.0f * empties.Count) / chromosome.GetGenes().ToList().Count;
				GenesScore += fitnessScore + ",";
				return fitnessScore;
			}


			public double FitnessTesting(IChromosome chromosome) {
				var enemies = chromosome.GetGenes()
					.Select((gene, index) => new { gene, index })
					.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				float fitnessScore = 0.0f;
				fitnessScore -= (float)Math.Pow(enemies.Count - 5, 2);
				GenesScore += fitnessScore + ",";
				return fitnessScore;
			}

			private bool isInsideRect(Vector3 start, Vector3 end, Vector3 point) {
				float xMax = start.x > end.x ? start.x : end.x;
				float xMin = start.x < end.x ? start.x : end.x;
				float zMax = start.z > end.z ? start.z : end.z;
				float zMin = start.z < end.z ? start.z : end.z;

				if (point.x >= xMin && point.x <= xMax && point.z >= zMin && point.z <= zMax) {
					return true;
				}

				return false;
			}

			//// Fitness no.1 Neglected.
			//public double FitnessNeglected(IChromosome chromosome) {
			//	var enemies = chromosome.GetGenes()
			//					.Select((gene, index) => new { gene, index })
			//					.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
			//	float fitnessScore = 0.0f;

			//	int pathCount = 0;

			//	foreach (var tile in path) {
			//		pathCount++;
			//		foreach (var enemy in enemies) {
			//			List<Gene> square = chromosome.GetGenes().Where(g => isInsideRect(tile.Position, enemy.Position, (g.Value as CreVoxGene).Position)).ToList();
			//			foreach (var point in square) {
			//				fitnessScore += ((-1 / (GetDistance(tile.Position, (enemy.Value as CreVoxGene).Position, (point.Value as CreVoxGene).Position))) / (square.Count > 0 ? square.Count : 1)) * (point.Value as CreVoxGene).Count * (path.Count - pathCount + 1);
			//			}
			//		}

			//	}

			//	return fitnessScore;
			//}

			// Fitness no.4 Patrol.
			public double FitnessPatrol(IChromosome chromosome) {
				var enemies = chromosome.GetGenes()
								.Select((gene, index) => new { gene, index })
								.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				// Path except wall(forbidden?)
				var ps = chromosome.GetGenes()
								.Select((gene, index) => new { gene, index })
								.Where(p => (p.gene.Value as CreVoxGene).Type != GeneType.Forbidden).ToList();
				double radius = 10;

				float fitnessScore = 0.0f;
				foreach (var enemy in enemies) {
					foreach (var p in ps) {
						if (p != enemy) {
							var enemyPosition = tiles.Keys.ElementAt(enemy.index);
							var pPosition = tiles.Keys.ElementAt(p.index);
							// If dist less than radius.
							if (Vector3.Distance(pPosition, enemyPosition) <= radius) {
								fitnessScore += 1;
							}
						}
					}
				}

				return fitnessScore;
			}

			// Fitness no.5 Guard
			public double FitnessGuard(IChromosome chromosome) {
				var enemies = chromosome.GetGenes()
								.Select((gene, index) => new { gene, index })
								.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				// Objectives include treasure and exit[not add yet];
				var objectives = chromosome.GetGenes()
								.Select((gene, index) => new { gene, index })
								.Where(p => (p.gene.Value as CreVoxGene).Type == GeneType.Treasure).ToList();

				float fitnessScore = 0.0f;

				foreach (var enemy in enemies) {
					foreach (var objective in objectives) {
						var enemyPosition = tiles.Keys.ElementAt(enemy.index);
						var objectivePosition = tiles.Keys.ElementAt(objective.index);
						fitnessScore += 1.0f / (Vector3.Distance(enemyPosition, objectivePosition));
					}
				}

				return fitnessScore;
			}

			//// Fitness no.6 Dominated
			//public double FitnessDominated(IChromosome chromosome) {
			//	var enemies = chromosome.GetGenes().Where(g => (g.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
			//	double fitnessScore = 0.0;
			//	foreach (var enemy in enemies) {
			//		CreVoxGene enemyGene = enemy.Value as CreVoxGene;
			//		double sum = 0.0;
			//		//   M
			//		// SIGMA( ( 1 / dist( E, MP(j) ) * mp(j) * j * high( E, MP(j) ) ) )
			//		// j = 1
			//		for (int j = 0; j < path.Count; j++) {
			//			sum += (1 / Vector3.Distance(enemyGene.Position, path[j].Position))
			//				* path[j].Count
			//				* (j + 1)
			//				* Mathf.Abs(enemyGene.Position.y - path[j].Position.y);
			//		}
			//		fitnessScore += sum;
			//	}
			//	GenesScore += fitnessScore + ",";
			//	return fitnessScore;
			//}
		}

		// Two-point crossover.
		public class MyCrossover : CrossoverBase {
			public int SwapPointOneGeneIndex { get; set; }
			public int SwapPointTwoGeneIndex { get; set; }

			public MyCrossover() : base(2, 2) {

			}

			protected override IList<IChromosome> PerformCross(IList<IChromosome> parents) {
				var firstParent = parents[0];
				var secondParent = parents[1];
				var parentLength = firstParent.Length;
				var swapPointsLength = parentLength - 1;

				return CreateChildren(firstParent, secondParent);
			}

			protected IList<IChromosome> CreateChildren(IChromosome firstParent, IChromosome secondParent) {
				SwapPointOneGeneIndex = Random.Range(0, firstParent.GetGenes().ToList().Count - 1);
				SwapPointTwoGeneIndex = Random.Range(SwapPointOneGeneIndex, firstParent.GetGenes().ToList().Count);
				var firstChild  = CreateChild(firstParent, secondParent);
				var secondChild = CreateChild(secondParent, firstParent);

				return new List<IChromosome>() { firstChild, secondChild };
			}

			protected IChromosome CreateChild(IChromosome leftParent, IChromosome rightParent) {
				var firstCutGenesCount  = SwapPointOneGeneIndex + 1;
				var secondCutGenesCount = SwapPointTwoGeneIndex + 1;
				var child = leftParent.CreateNew();
				child.ReplaceGenes(0, leftParent.GetGenes().Take(firstCutGenesCount).ToArray());
				child.ReplaceGenes(firstCutGenesCount, rightParent.GetGenes().Skip(firstCutGenesCount).Take(secondCutGenesCount - firstCutGenesCount).ToArray());
				child.ReplaceGenes(secondCutGenesCount, leftParent.GetGenes().Skip(secondCutGenesCount).ToArray());

				return child;
			}
		}

		//implement MyProblemChromosome for ChromosomeBase.
		public class MyProblemChromosome : ChromosomeBase {
			private Dictionary<Vector3, CreVoxGene> GenePositions { get; set; }

			public MyProblemChromosome(Dictionary<Vector3, CreVoxGene> genePositions) : base(genePositions.Count) {
				GenePositions = genePositions;
				CreateGenes();
			}

			public override IChromosome CreateNew() {
				return new MyProblemChromosome(GenePositions);
			}

			protected override void CreateGenes() {
				// this index is for gene of chromosome.
				int index = 0;
				foreach (var pair in GenePositions) {
					ReplaceGene(index++, new Gene(new CreVoxGene(GeneType.Empty)));
				}
			}

			public override Gene GenerateGene(int geneIndex) {
				GeneType type = GeneType.Forbidden;
				return new Gene(type);
			}
		}

		//implement MyMutation for IMutation.
		public class MyMutation : IMutation {
			public bool IsOrdered { get; private set; }

			public void Mutate(IChromosome chromosome, float probability) {
				var seed = Random.Range(0.0f, 100.0f);
				// If out of seed range, skip this mutation.
				if (seed > probability) {
					return;
				}
				// Start to mutate.
				var random = new SystemRandom();
				// Filtering the percent numbers for genes.
				var genes         = chromosome.GetGenes().ToList();
				var percent       = (int) Math.Ceiling(Random.Range(0.05f, 0.20f) * genes.Count);
				var filteredGenes = genes.OrderBy(g => random.Next()).Take(percent).ToList();
				// Change type each gene.
				foreach (var gene in filteredGenes) {
					var CVGene = gene.Value as CreVoxGene;

					var types = System.Enum
						.GetValues(typeof(GeneType))
						.Cast<GeneType>()
						.Where(t => t != GeneType.Forbidden && t != CVGene.Type)
						.ToArray();

					CVGene.Type = types[Random.Range(0, types.Length)];
				}
			}
		}

		//using CreVoxGene to be gene of GA.
		public class CreVoxGene {
			public GeneType Type { get; set; }
			// Constructor.
			public CreVoxGene() : this(GeneType.Empty) {

			}

			public CreVoxGene(GeneType type) {
				Type = type;
			}
		}
	}
}

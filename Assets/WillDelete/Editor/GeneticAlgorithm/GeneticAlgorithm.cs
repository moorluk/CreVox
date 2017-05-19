using System.Linq;
using System.Collections.Generic;
using Guid = System.Guid;
using Stopwatch = System.Diagnostics.Stopwatch;
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
	public class CreVoxGA {
		private const int _generationNumber = 20;
		private const float _enemyMutationRate = 5.0f;
		private const float _treasureMutationRate = 2.0f;
		private const float _trapMutationRate = 2.0f;
		private const float _emptyMutationRate = 5.0f;
		private const float _crossOverRate = 100.0f;
		private const string _picecName = "Gnd.in.one";
		private const int _volumeTraget = 0;
		private static List<Volume> volumes;
		private static GameObject genePos = new GameObject("genePos");
		private static Dictionary<Vector3, int> _mainPath = new Dictionary<Vector3, int>();
		public static string GenesScore;

		public static Dictionary<Vector3, CreVoxGene> tiles = new Dictionary<Vector3, CreVoxGene>();

		public enum GeneType {
			Forbidden = -1,
			Empty = 0,
			Enemy = 1,
			Treasure = 2,
			Trap = 3
		}

		// Add the 'Level settings' in 'Dungeon' menu.
		[MenuItem("Dungeon/沒有CSV，直接跑GA", false, 99)]
		public static void GoFighting() {
			Segmentism();
		}


		//generate crossOverIndex1 and crossOverIndex2.
		public static void generateRandomCrossOverIndex(int volumeLength, ref int crossOverIndex1, ref int crossOverIndex2) {
			var randomUpperBound = volumeLength - 1;
			crossOverIndex1 = Random.Range(0, randomUpperBound);
			if (crossOverIndex1 == 0)
				crossOverIndex2 = Random.Range(1, randomUpperBound);
			else if (crossOverIndex1 == randomUpperBound - 1) {
				crossOverIndex2 = crossOverIndex1;
				crossOverIndex1 = Random.Range(0, crossOverIndex2);
			}
			else
				crossOverIndex2 = Random.Range(crossOverIndex1 + 1, randomUpperBound);
		}

		public static void Segmentism() {
			Stopwatch sw = new Stopwatch();
			sw.Start();
			volumes = getVolumeByVolumeManager();
			foreach (var volume in volumes) {
				// Tiles (key: Postion; value: CreVoxGene).
				tiles = GetPositionsByPicecName(_picecName, volume);
				// Calculate the count of tiles in path.
				// getPathCount();
				GenesScore = default(string);
				//instance necessary class for GA.
				var selection  = new EliteSelection();
int crossOverIndex1 = default(int);
int crossOverIndex2 = default(int);
generateRandomCrossOverIndex(tiles.Count, ref crossOverIndex1, ref crossOverIndex2);
				var crossover  = new TwoPointCrossover(crossOverIndex1, crossOverIndex2); //MyCrossover();
				var mutation   = new MyMutation();
				var fitness    = new MyProblemFitness();
				var chromosome = new MyProblemChromosome(tiles);
				var population = new Population(10, 10, chromosome);
				//execute GA.
				var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
				ga.Termination = new GenerationNumberTermination(_generationNumber);
				ga.CrossoverProbability = _crossOverRate;
				Debug.Log("GA running...");
				ga.Start();
				Debug.Log("Best solution found has " + ga.BestChromosome.Fitness + " fitness.");
				BestChromosomeToWorldPos(tiles, ga.BestChromosome);
			}
			sw.Stop();
			Debug.Log(sw.ElapsedMilliseconds + " ms");
			GC.Collect();
		}

		// use volumeManger to find all of volumes.
		public static List<Volume> getVolumeByVolumeManager() {
			List<Volume> volumes = new List<Volume>();
			var volumeManger = GameObject.Find("VolumeManger(Generated)");
			for (int i = 0; i < volumeManger.transform.childCount; ++i) {
				volumes.Add(volumeManger.transform.GetChild(i).GetComponent<Volume>());
			}
			return volumes;
		}

		// use volume to get each block position.
		public static Dictionary<Vector3, CreVoxGene> GetPositionsByPicecName(string blockAirPieceName, Volume volume) {
			_mainPath.Clear();
			Dictionary<Vector3, CreVoxGene> tiles = new Dictionary<Vector3, CreVoxGene>();
			//use volume to find DecorationRoot and find the DecorationRoot's child.
			var decorationRoots = volume.gameObject.transform.FindChild("DecorationRoot");
			for (int i = 0; i < decorationRoots.childCount; ++i) {
				// Select the piece from block air, then add it.
				var tile = decorationRoots.GetChild(i).FindChild(blockAirPieceName);
				if (tile != null) {
					tiles.Add(tile.position, new CreVoxGene());
				}
			}

			// Astar.
			var item = GameObject.Find("VolumeManger(Generated)").transform.FindChild("Entrance_01_vData");
			var starpos = item.FindChild("ItemRoot").transform.FindChild("Starting Node").transform.position;
			var endpos = item.FindChild("ItemRoot").transform.FindChild("Connection_Default").transform.position;
			var map = makeAMap(item.gameObject);

			AStar astar = new AStar(map, starpos, endpos);

			foreach (var i in astar.theShortestPath) {
				if (_mainPath.ContainsKey(i.position3)) {
					_mainPath[i.position3] += 1;
				} else {
					_mainPath.Add(i.position3, 1);
				}
			}
			return tiles;
		}

		//make the best gene is added into world.
		public static void BestChromosomeToWorldPos(Dictionary<Vector3, CreVoxGene> tiles, IChromosome bestChromosome) {
			// foreach (var gene in bestChromosome.GetGenes()) {
			genePos = GameObject.Find("genePos") ?? new GameObject("genePos");
			var genes = bestChromosome.GetGenes();
			for (int i = 0; i < tiles.Count; i++) {
				GameObject geneWorldPosition = null;
				if ((genes[i].Value as CreVoxGene).Type != GeneType.Empty) {
					geneWorldPosition = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					geneWorldPosition.transform.SetParent(genePos.transform);
					geneWorldPosition.transform.position = tiles.Keys.ElementAt(i);
				}

/*
				// Set the color.
				switch ((gene.Value as CreVoxGene).Type) {
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
#endif
				geneWorldPosition.transform.SetParent(genePos.transform);
				geneWorldPosition.transform.position = (gene.Value as CreVoxGene).Position;
*/
			}
		}

		//make current volume to a 3d int array map for A*.
		public static int[,,] makeAMap(GameObject volume) {
			int[,,] tiles = new int[9, 9, 9];
			var decorationRoots = volume.transform.Find("DecorationRoot");
			for (int i = 0; i < decorationRoots.childCount; ++i) {
				var tile = decorationRoots.GetChild(i).Find(_picecName).position;
				// 1 means the tile is passable. (Width: 3 x Height: 2 x Length: 3)
				if (tile != null) {
					// Because all "decorations" are reduced 1.
					tiles[(int) tile.x / 3, (int) (tile.y+1) / 2, (int) tile.z / 3] = 1;
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

				fitnessValue += 0
							+ FitnessBlock(chromosome) * 1
							// + FitnessIntercept(chromosome) * 1
							// + FitnessSupport(chromosome) * 2.5
							// + FitnessEnemyDensity(chromosome) * 1
							// + FitnessEmptyDensity(chromosome) * 3
							+ FitnessTesting(chromosome) * 1
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
							if ((position1 - position2).magnitude < 3 * 3) {
								enemyDisSum += 1.0f / (float) Math.Pow((position1 - position2).magnitude, 2);
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
				fitnessScore -= (float) Math.Pow(enemies.Count - 5, 2);
				GenesScore += fitnessScore + ",";
				return fitnessScore;
			}
		}

		public class MyCrossover : CrossoverBase {
			public MyCrossover() : base(2, 2) {
			}

			protected override IList<IChromosome> PerformCross(IList<IChromosome> parents) {
				var firstParent = parents[0];
				var secondParent = parents[1];
				var parentLength = firstParent.Length;
				var swapPointsLength = parentLength - 1;

				return CreateChildren(firstParent, secondParent);
			}

			protected IList<IChromosome> CreateChildren(IChromosome leftParent, IChromosome rightParent) {
				List<Gene> leftParentGenes = leftParent.GetGenes().ToList();
				List<Gene> rightParentGenes = rightParent.GetGenes().ToList();

				var firstCutGenesCount = Random.Range(0, leftParentGenes.Count);
				var secondCutGenesCount = Random.Range(firstCutGenesCount, rightParentGenes.Count);

				var leftParentRange = leftParentGenes.GetRange(firstCutGenesCount, secondCutGenesCount - firstCutGenesCount);
				var rightParentRange = rightParentGenes.GetRange(firstCutGenesCount, secondCutGenesCount - firstCutGenesCount);

				// // Remove the range in right-hand side from left-hand side.
				// foreach (var gene in leftParentRange) {
				// 	rightParentGenes.RemoveAll(g => (gene.Value as CreVoxGene).Id == (g.Value as CreVoxGene).Id);
				// }
				// foreach (var gene in rightParentRange) {
				// 	leftParentGenes.RemoveAll(g => (gene.Value as CreVoxGene).Id == (g.Value as CreVoxGene).Id);
				// }

				// leftParentGenes.InsertRange(firstCutGenesCount, rightParentRange);
				// rightParentGenes.InsertRange(firstCutGenesCount, leftParentRange);

				var firstChild = leftParent.CreateNew();
				var secondChild = rightParent.CreateNew();

				firstChild.ReplaceGenes(0, leftParentGenes.ToArray());
				secondChild.ReplaceGenes(0, rightParentGenes.ToArray());

				return new List<IChromosome>() { firstChild, secondChild };
			}
		}

		//implement MyProblemChromosome for ChromosomeBase.
		public class MyProblemChromosome : ChromosomeBase {
			private Dictionary<Vector3, CreVoxGene> GenePositions { get; set; }

			public MyProblemChromosome(Dictionary<Vector3, CreVoxGene> genePositions) : base(genePositions.Count) {
				this.GenePositions = genePositions;
				CreateGenes();
			}

			public override IChromosome CreateNew() {
				return new MyProblemChromosome(GenePositions);
			}

			protected override void CreateGenes() {
				// this index is for gene of chromosome.
				int index = 0;
				foreach (var pair in GenePositions) {
					this.ReplaceGene(index++, new Gene(new CreVoxGene(GeneType.Empty)));
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
				var genes = chromosome.GetGenes();
				foreach (var gene in genes) {
					var CVGene = gene.Value as CreVoxGene;
					var seed = Random.Range(0.0f, 100.0f);
					if (seed < _enemyMutationRate) {
						CVGene.Type = GeneType.Enemy;
						continue;
					}
					seed -= _enemyMutationRate;
					if (seed < _emptyMutationRate) {
						CVGene.Type = GeneType.Empty;
						continue;
					}
					// seed -= _enemyMutationRate;
					// if (seed < _treasureMutationRate) {
					// 	CVGene.Type = GeneType.Treasure;
					// 	continue;
					// }
					// seed -= _treasureMutationRate;
					// if (seed < _trapMutationRate) {
					// 	CVGene.Type = GeneType.Trap;
					// 	continue;
					// }

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
				this.Type = type;
			}
		}
	}
}

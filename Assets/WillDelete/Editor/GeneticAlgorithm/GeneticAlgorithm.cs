using System.Linq;
using System.Collections.Generic;
using Guid = System.Guid;
using Stopwatch = System.Diagnostics.Stopwatch;
using GC = System.GC;
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
		private const int _generationNumber = 10;
		private const float _enemyMutationRate = 5.0f;
		private const float _treasureMutationRate = 2.0f;
		private const float _trapMutationRate = 2.0f;
		private const float _emptyMutationRate = 10.0f;
		private const float _crossOverRate = 100.0f;
		private const string _picecName = "Gnd.in.one";
		private const int _volumeTraget = 0;
		private static List<Volume> volumes;
		private static GameObject genePos = new GameObject("genePos");
		private static List<CreVoxGene> path = new List<CreVoxGene>();
		public static string GenesScore;

		public enum GeneType {
			Forbidden = -1,
			Empty = 0,
			Enemy = 1,
			Treasure = 2,
			Trap = 3
		}

		// Add the 'Level settings' in 'Dungeon' menu.
		[MenuItem("Dungeon/Love GA", false, 99)]
		public static void GoFighting() {
			Segmentism();
		}

		// Add the 'test' in 'Dungeon' menu.
		//test the A* for volume.
		[MenuItem("Dungeon/test", false, 99)]
		public static void test() {
			getPathCount();
			foreach (var i in path) {
				Debug.Log(i);
			}
		}

		public static void Segmentism() {
			Stopwatch sw = new Stopwatch();
			volumes = getVolumeByVolumeManager();
			foreach (var volume in volumes) {
				GenesScore = default(string);
				var genePositions = GetPositionsByPicecName(_picecName, volume);
				//instance necessary class for GA.
				var selection  = new EliteSelection();
				var crossover  = new MyCrossover();
				var mutation   = new MyMutation();
				var fitness    = new MyProblemFitness();
				var chromosome = new MyProblemChromosome(genePositions);
				var population = new Population(10, 10, chromosome);
				//execute GA.
				var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
				ga.Termination = new GenerationNumberTermination(_generationNumber);
				ga.CrossoverProbability = _crossOverRate;
				Debug.Log("GA running...");
				ga.Start();
				Debug.Log("Best solution found has " + ga.BestChromosome.Fitness + " fitness.");
				BestGeneToWorldPos(ga.BestChromosome);
			}
			sw.Stop();
			Debug.Log(sw.ElapsedMilliseconds + " ms");
			GC.Collect();
			// Calculate the count of tiles in path.
			getPathCount();
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
		public static Dictionary<Guid, Vector3> GetPositionsByPicecName(string blockAirPieceName, Volume volume) {
			Dictionary<Guid, Vector3> positions = new Dictionary<Guid, Vector3>();
			//use volume to find DecorationRoot and find the DecorationRoot's child.
			var decorationRoots = volume.gameObject.transform.FindChild("DecorationRoot");
			for (int i = 0; i < decorationRoots.childCount; ++i) {
				// Select the piece from block air, then add it.
				var tile = decorationRoots.GetChild(i).FindChild(blockAirPieceName);
				if (tile == null) {
					continue;
				} else {
					positions.Add(Guid.NewGuid(), tile.position);
				}
			}
			return positions;
		}

		//make the best gene is added into world.
		public static void BestGeneToWorldPos(IChromosome bestGene) {
			foreach (var gene in bestGene.GetGenes()) {
				genePos = GameObject.Find("genePos") ?? new GameObject("genePos");
				GameObject geneWorldPosition = GameObject.CreatePrimitive(PrimitiveType.Cube);
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
			}
		}

		//make current volume to a 3d int array map for A*.
		public static int[,,] makeAMap(GameObject volume) {
			int[,,] temps = new int[9, 9, 9];
			var decorationRoots = volume.transform.FindChild("DecorationRoot");
			for (int i = 0; i < decorationRoots.childCount; ++i) {
				var temp = decorationRoots.GetChild(i).FindChild(_picecName).position;
				if (temp == null)
					continue;
				temps[(int)temp.x / 3, (int)temp.y / 2, (int)temp.z / 3] = 1;
			}
			return temps;
		}

		public static void getPathCount() {
			path.Clear();
			var item = GameObject.Find("VolumeManger(Generated)").transform.FindChild("Entrance_01_vData");
			var starpos = item.FindChild("ItemRoot").transform.FindChild("Starting Node").transform.position;
			var endpos = item.FindChild("ItemRoot").transform.FindChild("Connection_Default").transform.position;
			var map = makeAMap(item.gameObject);

			AStar tt = new AStar(map, starpos, endpos);
			foreach (var i in tt.theShortestPath) {
				if (path.Any(x => x.Position.Equals(i.position3))) {
					path.First(x => x.Position == (i.position3)).Count++;
				}
				else {
					path.Add(new CreVoxGene(GeneType.Empty, i.position3, 1, Guid.NewGuid()));
				}
			}
		}

		//implement necessary class for GA.
		//implement MyProblemFitness for IFitness.
		public class MyProblemFitness : IFitness {
			public double Evaluate(IChromosome chromosome) {
				double fitnessValue = default(double);

				fitnessValue += FitnessSupport(chromosome);
				GenesScore += fitnessValue + "\n";
				return fitnessValue;
			}

			public double FitnessSupport(IChromosome chromosome) {
				var enemies = chromosome.GetGenes().Where(g => (g.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				float fitnessScore = 0.0f;
				foreach (var enemy1 in enemies) {
					float enemyDisSum = 0.0f;
					foreach (var enemy2 in enemies) {
						if (enemy1 != enemy2) {
							enemyDisSum += 1.0f / ((enemy1.Value as CreVoxGene).Position - (enemy2.Value as CreVoxGene).Position).magnitude;
						}
					}
					float mpDisSum = 0.0f;
					foreach (var tile in path) {
						mpDisSum += (((enemy1.Value as CreVoxGene).Position - tile.Position).magnitude) / 15.588f;
					}
					fitnessScore += (enemyDisSum / enemies.Count + mpDisSum / path.Count) / 2.0f;
				}
				fitnessScore /= enemies.Count;
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

				// Remove the range in right-hand side from left-hand side.
				foreach (var gene in leftParentRange) {
					rightParentGenes.RemoveAll(g => (gene.Value as CreVoxGene).Id == (g.Value as CreVoxGene).Id);
				}
				foreach (var gene in rightParentRange) {
					leftParentGenes.RemoveAll(g => (gene.Value as CreVoxGene).Id == (g.Value as CreVoxGene).Id);
				}

				leftParentGenes.InsertRange(firstCutGenesCount, rightParentRange);
				rightParentGenes.InsertRange(firstCutGenesCount, leftParentRange);

				var firstChild = leftParent.CreateNew();
				var secondChild = rightParent.CreateNew();

				firstChild.ReplaceGenes(0, leftParentGenes.ToArray());
				secondChild.ReplaceGenes(0, rightParentGenes.ToArray());

				return new List<IChromosome>() { firstChild, secondChild };
			}
		}

		//implement MyProblemChromosome for ChromosomeBase.
		public class MyProblemChromosome : ChromosomeBase {
			private Dictionary<Guid, Vector3> GenePositions { get; set; }

			public MyProblemChromosome(Dictionary<Guid, Vector3> genePositions) : base(genePositions.Count) {
				this.GenePositions = genePositions;
				CreateGenes();
			}

			public override IChromosome CreateNew() {
				return new MyProblemChromosome(GenePositions);
			}

			protected override void CreateGenes() {
				int index = 0;
				foreach (var pair in GenePositions) {
					this.ReplaceGene(index, new Gene(new CreVoxGene(GeneType.Empty, pair.Value, pair.Key)));
					index++;
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
			public Guid Id { get; private set; }
			public GeneType Type { get; set; }
			public Vector3 Position { get; private set; }
			public int Count { get; set; }
			// Constructor.
			public CreVoxGene(GeneType type, Vector3 position, Guid guid) {
				this.Id = guid;
				this.Type = type;
				this.Position = position;
			}

			public CreVoxGene(GeneType type, Vector3 position, int count, Guid guid) {
				this.Id = guid;
				this.Type = type;
				this.Position = position;
				this.Count = count;
			}

			public override string ToString() {
				return Position.ToString() + " count: " + Count;
			}
		}
	}
}

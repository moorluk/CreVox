using System.Linq;
using System.Collections.Generic;
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
		private const int _generationNumber = 20;
		private const float _enemyMutationRate = 5.0f;
		private const float _treasureMutationRate = 2.0f;
		private const float _trapMutationRate = 2.0f;
		private const float _emptyMutationRate = 10.0f;
		private const float _crossOverRate = 5.0f;
		private const string _picecName = "Gnd.in.one";
		private const int _volumeTraget = 0;
		private static List<Volume> volumes;
		private static GameObject genePos = new GameObject("genePos");
		private static List<CreVoxGene> path = new List<CreVoxGene>();

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
            foreach(var i in path) {
                Debug.Log(i);
            }
		}

		public static void Segmentism() {
			Stopwatch sw = new Stopwatch();
			volumes = getVolumeByVolumeManager();
			foreach (var volume in volumes) {
				int crossOverIndex1 = default(int);
				int crossOverIndex2 = default(int);
				var genePositions = GetPositionsByPicecName(_picecName, volume);
				generateRandomCrossOverIndex(genePositions.Count, ref crossOverIndex1, ref crossOverIndex2);
				//instance necessary class for GA.
				var selection = new EliteSelection();
				var crossover = new TwoPointCrossover(crossOverIndex1, crossOverIndex2);
				var mutation = new MyMutation();
				var fitness = new MyProblemFitness();
				var chromosome = new MyProblemChromosome();
				var population = new Population(50, 70, chromosome);
				//execute GA.
				var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
				ga.Termination = new GenerationNumberTermination(_generationNumber);
				ga.CrossoverProbability = _crossOverRate;
				Debug.Log("GA running...");
				ga.Start();
				Debug.Log("Best solution found has " + ga.BestChromosome.Fitness + " fitness.");
				BestGeneToWorldPos(ga.BestChromosome);
				//volumes[volumeTraget].name = "6666666";
				//findTheShortestPath();
				//Debug.Log(GameObject.Find("connection").transform.position);
			}
			sw.Stop();
			Debug.Log(sw.ElapsedMilliseconds + " ms");
			GC.Collect();
			// Calculate the count of tiles in path.
			getPathCount();
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
		public static List<Vector3> GetPositionsByPicecName(string blockAirPieceName, Volume volume) {
			List<Vector3> positions = new List<Vector3>();
			//use volume to find DecorationRoot and find the DecorationRoot's child.
			var decorationRoots = volume.gameObject.transform.FindChild("DecorationRoot");
			for (int i = 0; i < decorationRoots.childCount; ++i) {
				var temp = decorationRoots.GetChild(i).FindChild(blockAirPieceName);
				if (temp == null)
					continue;
				positions.Add(temp.position);
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
                    path.Add(new CreVoxGene(GeneType.Empty, i.position3, 1));
                }
            }
        }

		//implement necessary class for GA.
		//implement MyProblemFitness for IFitness.
		public class MyProblemFitness : IFitness {
			public double Evaluate(IChromosome chromosome) {
				double fitnessValue = default(double);

				fitnessValue += FitnessTrap(chromosome)
							 + FitnessTreasure(chromosome)
							 + FitnessDominator(chromosome);

				return fitnessValue;
			}

			public double FitnessTrap(IChromosome chromosome) {
				double fitnessScore = 0;
				var enemies = chromosome.GetGenes().Where(g => (g.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				var traps = chromosome.GetGenes().Where(g => (g.Value as CreVoxGene).Type == GeneType.Trap).ToList();

				foreach (var enemy in enemies) {
					var enemyGene = enemy.Value as CreVoxGene;
					foreach (var trap in traps) {
						var trapGene = trap.Value as CreVoxGene;
						var distance = (enemyGene.Position - trapGene.Position).magnitude;
						fitnessScore += (distance == 0) ? 0 : 1 / distance;
					}
				}
				return fitnessScore;
			}

			public double FitnessTreasure(IChromosome chromosome) {
				double fitnessScore = 0;
				var enemies = chromosome.GetGenes().Where(g => (g.Value as CreVoxGene).Type == GeneType.Enemy).ToList();
				var treasures = chromosome.GetGenes().Where(g => (g.Value as CreVoxGene).Type == GeneType.Treasure).ToList();

				foreach (var enemy in enemies) {
					var enemyGene = enemy.Value as CreVoxGene;
					foreach (var treasure in treasures) {
						var treasureGene = treasure.Value as CreVoxGene;
						var distance = (enemyGene.Position - treasureGene.Position).magnitude; fitnessScore += (distance == 0) ? 0 : 1 / distance;
						fitnessScore += (distance == 0) ? 0 : 1 / distance;
					}
				}
				return fitnessScore;
			}

			public double FitnessDominator(IChromosome chromosome) {
				double fitnessScore = 0;
				var enemies = chromosome.GetGenes().Where(g => (g.Value as CreVoxGene).Type == GeneType.Enemy).ToList();

				foreach (var enemy in enemies) {
					var enemyGene = enemy.Value as CreVoxGene;
					fitnessScore += enemyGene.Position.y / 8.0f;
				}
				return fitnessScore;
			}
		}

		//implement MyProblemChromosome for ChromosomeBase.
		public class MyProblemChromosome : ChromosomeBase {
			public MyProblemChromosome() : base(GetPositionsByPicecName(_picecName, volumes[_volumeTraget]).Count) {
				CreateGenes();
			}

			public override IChromosome CreateNew() {
				return new MyProblemChromosome();
			}

			protected override void CreateGenes() {
				var genes = GetPositionsByPicecName(_picecName, volumes[_volumeTraget]);
				int index = 0;
				foreach (var gene in genes) {
					this.ReplaceGene(index, new Gene(new CreVoxGene(GeneType.Empty, gene)));
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
			public bool IsOrdered {
				get;
				private set;
			}

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
					if (seed < _treasureMutationRate) {
						CVGene.Type = GeneType.Treasure;
						continue;
					}
					seed -= _treasureMutationRate;
					if (seed < _trapMutationRate) {
						CVGene.Type = GeneType.Trap;
						continue;
					}
				}
			}
		}

		//using CreVoxGene to be gene of GA.
		public class CreVoxGene {
			public GeneType Type { get; set; }
			public Vector3 Position { get; private set; }
			public int Count { get; set; }
			// Constructor.
			public CreVoxGene(GeneType type, Vector3 position) {
				this.Type = type;
				this.Position = position;
			}

			public CreVoxGene(GeneType type, Vector3 position,int count) {
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

using System.Linq;
using System.Collections.Generic;
using SystemRandom = System.Random;
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
		public static Dictionary<string, int> FitnessWeights = new Dictionary<string, int>();

		// Game patterns objects are expressed via Enemy, 
		private static GameObject GamePatternObjects {
			get {
				return GameObject.Find("GamePatternObjects") ?? new GameObject("GamePatternObjects");
			}
		}
		private static readonly string[] _picecName = { "Gnd.in.one" };

		public static string GenesScore;

		// Constructor.
		static CreVoxGA() {
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
			foreach (var child in GamePatternObjects.transform.Cast<Transform>().ToList()) {
				GameObject.DestroyImmediate(child.gameObject);
			}
		}

		public static NTUSTChromosome Segmentism() {
			Initialize();
			foreach (var volume in GetVolumeByVolumeManager()) {
				NTUSTGeneticAlgorithm ntustGA = new CreVoxGAA(0.8f, 0.1f, GetSample(_picecName, volume));

				// Populations, Generations.
				var bestChromosome = ntustGA.Algorithm(250, 20) as CreVoxChromosome;

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
			var volumeManger = GameObject.Find("VolumeManger(Generated)");
			foreach (Transform volume in volumeManger.transform) {
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
					} else {
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

		private static Dictionary<Vector3, int> _mainPath = new Dictionary<Vector3, int>();


		public class CreVoxGAA : NTUSTGeneticAlgorithm {
			public CreVoxGAA(float crossoverRate, float mutationRate, NTUSTChromosome sample) : base(crossoverRate, mutationRate, sample) {

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
				return 0.001f
					+ FitnessGuard() * FitnessWeights["guard"]
					+ FitnessEmptyDensity() * 1
				;
			}

			public float FitnessPatrol() {
				var enemies = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type == GeneType.Empty)
					.ToList();
				var ps = this.Genes
					.Select(g => g as CreVoxGene)
					.Where(g => g.Type != GeneType.Forbidden)
					.ToList();
				// Path except wall(forbidden?)

				double radius = 10;

				float fitnessScore = 0.0f;
				foreach (CreVoxGene enemy in enemies) {
					foreach (CreVoxGene p in ps) {
						if (p != enemy) {
							// If dist less than radius.
							if (Vector3.Distance(p.pos, enemy.pos) <= radius) {
								fitnessScore += 1;
							}
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
							from    objective in objectives
							let     distance = Vector3.Distance(enemy.pos, objective.pos)
							where   distance < 10
							orderby distance
							select  objective
						).FirstOrDefault();
						// If not found then add this one.
						if (protectedTarget != null) { neighbors[protectedTarget].Add(enemy); }
					}
					// Calculate the fitness score.
					fitnessScore = objectives.Sum(o => (avgProtector - Math.Abs(neighbors[o].Count - avgProtector) ) / avgProtector / objectives.Count);
				}

				// Write into the csv.
				GenesScore += fitnessScore + ", ";

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

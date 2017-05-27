using System.Linq;
using System.Collections.Generic;
using SystemRandom = System.Random;
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
using NTUSTGA;

namespace CrevoxExtend
{
    // Enum for type of gene.
    public enum GeneType
    {
        Forbidden = -1,
        Empty = 0,
        Enemy = 1,
        Treasure = 2,
        Trap = 3
    }

    public class CreVoxGA
    {
        // Game patterns objects are expressed via Enemy, 
        private static GameObject GamePatternObjects
        {
            get
            {
                return GameObject.Find("GamePatternObjects") ?? new GameObject("GamePatternObjects");
            }
            set
            {
                GamePatternObjects = value;
            }
        }
        private static readonly string[] _picecName = { "Gnd.in.one" };

        public static string GenesScore;

        // Constructor.
        static CreVoxGA()
        {
        }

        public static void Initialize()
        {
            foreach (Transform child in GamePatternObjects.transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        // Add the 'Level settings' in 'Dungeon' menu.
        [MenuItem("Dungeon/沒有CSV，直接跑GA", false, 998)]
        public static void GoFighting()
        {
            Segmentism();
        }

        public static void Segmentism()
        {
            Initialize();
            // Start timer.
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var volume in getVolumeByVolumeManager())
            {
                NTUSTGeneticAlgorithm ntustGA = new CreVoxGAA(0.8f, 0.1f, getSample(_picecName, volume));

                BestChromosomeToWorldPos(ntustGA.algorithm(250, 20) as CreVoxChromosome);
            }
            sw.Stop();
            Debug.Log(sw.ElapsedMilliseconds + " ms");
            GC.Collect();
        }

        #region Get Dungeon Sturcture
        // Get all of volumes frin volume manager.
        public static List<Volume> getVolumeByVolumeManager()
        {
            List<Volume> volumes = new List<Volume>();
            var volumeManger = GameObject.Find("VolumeManger(Generated)");
            foreach (Transform volume in volumeManger.transform)
            {
                volumes.Add(volume.GetComponent<Volume>());
            }
            return volumes;
        }

        // use volume to get each block position.
        public static CreVoxChromosome getSample(string[] blockAirPieceName, Volume volume)
        {
            List<Vector3> tiles = new List<Vector3>();

            // Parse the decorations, create the passable tiles space.
            var decorations = volume.gameObject.transform.Find("DecorationRoot");
            foreach (Transform decoration in decorations)
            {
                // Select the piece from block air, then add it.
                Transform tile = null;
                foreach (string pieceName in blockAirPieceName)
                {
                    tile = decoration.Find(pieceName);
                    if (tile != null)
                    {
                        tiles.Add(tile.position);
                        break;
                    }
                }
            }

            // A-star, defines the main path.
            var map = makeAMap(volume.gameObject);
            var items = volume.gameObject.transform.Find("ItemRoot");
            Vector3 startPosition = items.Find("Starting Node").transform.position;
            Vector3 endPosition;
            // Initial the main path.
            _mainPath.Clear();
            // Each item.
            foreach (Transform item in items)
            {
                // Ignore it if it is not connection.
                if (!item.name.Contains("Connection_")) { continue; }
                // Get the position of connection.
                endPosition = item.transform.position;
                // Execute the A-Star.
                AStar astar = new AStar(map, startPosition, endPosition);
                // Parse the path. Increase 1 if the position is exist; otherwise create a new one. 
                foreach (var pos in astar.theShortestPath)
                {
                    if (_mainPath.ContainsKey(pos.position3))
                    {
                        _mainPath[pos.position3] += 1;
                    }
                    else
                    {
                        _mainPath.Add(pos.position3, 1);
                    }
                }
            }

            return new CreVoxChromosome(tiles);
        }
        #endregion

        #region A Star

        //make the best gene is added into world.
        public static void BestChromosomeToWorldPos(CreVoxChromosome bestChromosome)
        {
            foreach (CreVoxGene gene in bestChromosome.genes)
            {
                if (gene.type != GeneType.Empty)
                {
                    GameObject geneWorldPosition = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    geneWorldPosition.transform.SetParent(GamePatternObjects.transform);
                    geneWorldPosition.transform.position = gene.pos;

#if UNITY_EDITOR
                    switch (gene.type)
                    {
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
                }
            }
        }

        //make current volume to a 3d int array map for A*.
        public static int[,,] makeAMap(GameObject volume)
        {
            int[,,] tiles = new int[9, 9, 9];
            var decorationRoots = volume.transform.Find("DecorationRoot");
            for (int i = 0; i < decorationRoots.childCount; ++i)
            {
                var tile = decorationRoots.GetChild(i).Find(_picecName[0]).position;
                // 1 means the tile is passable. (Width: 3 x Height: 2 x Length: 3)
                if (tile != null)
                {
                    // Because all "decorations" are reduced 1.
                    tiles[(int)tile.x / 3, (int)(tile.y + 1) / 2, (int)tile.z / 3] = 1;
                }
            }
            return tiles;
        }

        private static Dictionary<Vector3, int> _mainPath = new Dictionary<Vector3, int>();

        #endregion

        #region NTUST GA

        public class CreVoxGAA : NTUSTGeneticAlgorithm
        {
            public CreVoxGAA(float crossoverRate, float mutationRate, NTUSTChromosome sample) : base(crossoverRate, mutationRate, sample)
            {
            }

            public override void crossover(ref NTUSTChromosome parentCopy1, ref NTUSTChromosome parentCopy2)
            {
                int min = Random.Range(0, parentCopy1.genes.Count);
                int max = Random.Range(min, parentCopy1.genes.Count);

                for (int i = min; i < max; i++)
                {
                    NTUSTChromosome.NTUSTGene swapGene = parentCopy1.genes[i];
                    parentCopy1.genes[i] = parentCopy2.genes[i];
                    parentCopy2.genes[i] = swapGene;
                }
            }

            public override void mutation(ref NTUSTChromosome chrom)
            {
                CreVoxChromosome CreVoxChrom = chrom as CreVoxChromosome;
                // Start to mutate.
                var random = new SystemRandom();
                // Filtering the percent numbers for genes.
                var genes = CreVoxChrom.getGenes();
                var percent = (int)Math.Ceiling(Random.Range(0.05f, 0.20f) * genes.Count);
                var filteredGenes = genes.OrderBy(g => random.Next()).Take(percent).ToList();
                // Change type each gene.
                foreach (var gene in filteredGenes)
                {
                    var types = System.Enum
                        .GetValues(typeof(GeneType))
                        .Cast<GeneType>()
                        .Where(t => t != GeneType.Forbidden && t != gene.type)
                        .ToArray();

                    gene.type = types[Random.Range(0, types.Length)];
                }
            }
        }

        public class CreVoxChromosome : NTUSTChromosome
        {
            public List<CreVoxGene> getGenes()
            {
                return genes.Select(g => g as CreVoxGene).ToList();
            }

            #region Constructor

            public CreVoxChromosome()
            {
            }

            public CreVoxChromosome(List<Vector3> allPossiblePosition)
            {
                foreach (Vector3 pos in allPossiblePosition)
                {
                    genes.Add(new CreVoxGene(GeneType.Enemy, pos));
                }
            }

            #endregion

            #region Override

            public override NTUSTChromosome randomInitialize()
            {
                CreVoxChromosome result = new CreVoxChromosome();
                foreach (CreVoxGene gene in this.genes)
                    result.genes.Add(new CreVoxGene(GeneType.Empty, gene.pos));


                return result;
            }

            public override NTUSTChromosome copy()
            {
                CreVoxChromosome result = new CreVoxChromosome();

                foreach (CreVoxGene gene in this.genes)
                    result.genes.Add(gene.copy());

                return result;
            }

            public override float fitnessFunction()
            {
                return 0.001f + fitnessGuard() + fitnessEmptyDensity() * 50;
            }

            #endregion

            #region Fifness Functions
            public float fitnessPatrol()
            {
                var enemies = genes
                                .Select(g => g as CreVoxGene)
                                .Where(g => g.type == GeneType.Empty)
                                .ToList();
                var ps = genes
                            .Select(g => g as CreVoxGene)
                            .Where(g => g.type != GeneType.Forbidden)
                            .ToList();
                // Path except wall(forbidden?)

                double radius = 10;

                float fitnessScore = 0.0f;
                foreach (CreVoxGene enemy in enemies)
                {
                    foreach (CreVoxGene p in ps)
                    {
                        if (p != enemy)
                        {
                            // If dist less than radius.
                            if (Vector3.Distance(p.pos, enemy.pos) <= radius)
                            {
                                fitnessScore += 1;
                            }
                        }
                    }
                }

                return fitnessScore;
            }

            public float fitnessGuard()
            {
                var enemies = genes
                                .Select(g => g as CreVoxGene)
                                .Where(g => g.type == GeneType.Enemy).ToList();

                var objectives = genes
                                .Select(g => g as CreVoxGene)
                                .Where(g => g.type == GeneType.Treasure).ToList();

                float fitnessScore = 0.0f;

                foreach (var enemy in enemies)
                {
                    foreach (var objective in objectives)
                    {
                        fitnessScore += 1.0f / (Vector3.Distance(enemy.pos, objective.pos));
                    }
                }

                return fitnessScore;
            }

            public float fitnessEmptyDensity()
            {
                List<CreVoxGene> empties = genes.Select(g => g as CreVoxGene).Where(g => g.type == GeneType.Empty).ToList();

                return 1.0f * empties.Count / genes.Count;
            }
            #endregion
        }

        public class CreVoxGene : NTUSTChromosome.NTUSTGene
        {
            public GeneType type { get; set; }

            public Vector3 pos { get; set; }

            public CreVoxGene(GeneType type, Vector3 pos)
            {
                this.type = type;
                this.pos = pos;
            }

            public override NTUSTChromosome.NTUSTGene copy()
            {
                return new CreVoxGene(this.type, this.pos);
            }
        }
        #endregion
    }
}

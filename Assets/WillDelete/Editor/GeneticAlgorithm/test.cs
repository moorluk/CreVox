using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GeneticSharp;
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
        private static double[] fitnessWeight = { 1, 0.2, 0.4, 0.6, 0.8 };
        private static int crossOverIndex1;
        private static int crossOverIndex2;
        private static List<Volume> volumes;
        private static int volumeTraget = 8;
        static GameObject mother = new GameObject("mother");

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

        public static void Segmentism() {
            volumes = getVolumeByVolumeManager();
            generateRandomCrossOverIndex(volumes[volumeTraget]);

            var selection = new EliteSelection();
            //CORSSOVER會有每次的index都是相同的問題
            //設定index有時會超出範圍的例外(?)
            var crossover = new TwoPointCrossover(crossOverIndex1, crossOverIndex2);
            var mutation = new MyMutation();
            var fitness = new MyProblemFitness();
            var chromosome = new MyProblemChromosome();
            var population = new Population(50, 70, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = new GenerationNumberTermination(_generationNumber);
            ga.CrossoverProbability = _crossOverRate;
            ga.MutationProbability = _emptyMutationRate;
            Debug.Log("GA running...");
            ga.Start();
            Debug.Log("Best solution found has " + ga.BestChromosome.Fitness + " fitness.");
            volumes[volumeTraget].name = "6666666";
            Debug.Log(volumes[volumeTraget].name+"  "+ volumes[volumeTraget].transform.position);
            BestGeneToWorldPos(ga.BestChromosome, volumes[volumeTraget]);
            //foreach (var gene in ga.BestChromosome.GetGenes()) {
            //    Debug.Log((gene.Value as CreVoxGene).Type);
            //}
        }

        // use volumeManger to all of volumes.
        public static List<Volume> getVolumeByVolumeManager() {
            List<Volume> volumes = new List<Volume>();
            var volumeManger = GameObject.Find("VolumeManger(Generated)");
            for (int i = 0; i < volumeManger.transform.childCount; ++i) {
                volumes.Add(volumeManger.transform.GetChild(i).GetComponent<Volume>());
            }
            return volumes;
        }

        // use volume to get each block.
        public static List<WorldPos> GetPositionsByPicecName(string blockAirPieceName, Volume volume) {
            List<WorldPos> positions = new List<WorldPos>();
            //each volume
            //use volumeData to get chunkDatas.
            foreach (var chunk in volume.vd.chunkDatas) {
                var floorBlockAirs = chunk.blockAirs.Where(b => b.pieceNames.Any(name => name == blockAirPieceName));
                foreach (var floorBlockAir in floorBlockAirs) {
                    positions.Add(floorBlockAir.BlockPos);
                }
            }
            return positions;
        }

        public static void generateRandomCrossOverIndex(Volume volume) {
            //generate crossOverIndex1 and crossOverIndex2.
            crossOverIndex1 = Random.Range(0, Random.Range(0, GetPositionsByPicecName("Gnd.in.one", volume).Count));
            if (crossOverIndex1 == 0)
                crossOverIndex2 = Random.Range(1, Random.Range(0, GetPositionsByPicecName("Gnd.in.one", volume).Count));
            else if (crossOverIndex1 == GetPositionsByPicecName("Gnd.in.one", volume).Count) {
                crossOverIndex2 = crossOverIndex1;
                crossOverIndex1 = Random.Range(0, Random.Range(0, GetPositionsByPicecName("Gnd.in.one", volume).Count) - 1);
            }
            else
                crossOverIndex2 = Random.Range(crossOverIndex1, Random.Range(0, GetPositionsByPicecName("Gnd.in.one", volume).Count));
        }

        public static void BestGeneToWorldPos(IChromosome bestGene, Volume volume) {
            var volumedata = CrevoxOperation.GetVolumeData(@"Assets/Resources/CreVox/VolumeData/Isaac/Normal_07_vData2.asset");
            
            foreach (var gene in bestGene.GetGenes()) {
                foreach (var chunk in volume.vd.chunkDatas) {
                    var floorBlockAirs = chunk.blockAirs.Where(b => b.pieceNames.Any(name => name == "Gnd.in.one"));
                    foreach (var floorBlockAir in floorBlockAirs) {

                        GameObject test = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        test.transform.SetParent(mother.transform);
                        test.transform.position = volumes[volumeTraget].transform.position+floorBlockAir.BlockPos.ToVector3();
                        MonoBehaviour.Instantiate(test);
                        
                    }
                }
            }
        }

        public class MyProblemFitness : IFitness {
            public double Evaluate(IChromosome chromosome) {
                double fitnessValue = default(double);

                fitnessValue += FitnessTrap(chromosome)
                             + FitnessTreasure(chromosome)
                             + FitnessDominator(chromosome);

                //Debug.Log(fitnessValue);
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

        public class MyProblemChromosome : ChromosomeBase {
            public MyProblemChromosome() : base(GetPositionsByPicecName("Gnd.in.one", volumes[volumeTraget]).Count) {
                //Debug.Log(GetPositionsByPicecName("Gnd.in.one").Count);
                CreateGenes();
            }

            public override IChromosome CreateNew() {
                return new MyProblemChromosome();
            }

            protected override void CreateGenes() {
                var genes = GetPositionsByPicecName("Gnd.in.one", volumes[volumeTraget]);
                int index = 0;
                foreach (var gene in genes) {
                    //this.ReplaceGene(index, new Gene(new CreVoxGene((GeneType) Random.Range(-1,4), gene.x, gene.y, gene.z)));
                    this.ReplaceGene(index, new Gene(new CreVoxGene(GeneType.Empty, gene.x, gene.y, gene.z)));
                    index++;
                }
            }

            public override Gene GenerateGene(int geneIndex) {
                GeneType type = GeneType.Forbidden;
                return new Gene(type);
            }
        }

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

        public class CreVoxGene {
            public GeneType Type { get; set; }
            public Vector3 Position { get; private set; }
            // Constructor.
            public CreVoxGene(GeneType type, int x, int y, int z) {
                this.Type = type;
                this.Position = new Vector3(x, y, z);
            }
        }
    }
}
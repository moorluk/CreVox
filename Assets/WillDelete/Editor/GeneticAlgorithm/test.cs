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
        private const string _picecName = "Gnd.in.one";
        private const int _volumeTraget = 5;
        private static int _crossOverIndex1;
        private static int _crossOverIndex2;
        private static List<Volume> volumes;
        private static GameObject genePos = new GameObject("genePos");

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
            generateRandomCrossOverIndex(volumes[_volumeTraget]);

            var selection = new EliteSelection();
            var crossover = new TwoPointCrossover(_crossOverIndex1, _crossOverIndex2);
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
            //volumes[volumeTraget].name = "6666666";
            BestGeneToWorldPos(ga.BestChromosome, volumes[_volumeTraget]);
        }

        //generate crossOverIndex1 and crossOverIndex2.
        public static void generateRandomCrossOverIndex(Volume volume) {
            var randomUpperBound = GetPositionsByPicecName(_picecName, volume).Count - 1;
            _crossOverIndex1 = Random.Range(0, randomUpperBound);
            if (_crossOverIndex1 == 0)
                _crossOverIndex2 = Random.Range(1, randomUpperBound);
            else if (_crossOverIndex1 == randomUpperBound - 1) {
                _crossOverIndex2 = _crossOverIndex1;
                _crossOverIndex1 = Random.Range(0, _crossOverIndex2);
            }
            else
                _crossOverIndex2 = Random.Range(_crossOverIndex1 + 1, randomUpperBound);
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

        // use volume to get each block.
        public static List<Vector3> GetPositionsByPicecName(string blockAirPieceName, Volume volume) {
            List<Vector3> positions = new List<Vector3>();
            //use volume to find DecorationRoot and find the DecorationRoot's child.
            var decorationRoots = volume.gameObject.transform.FindChild("DecorationRoot");
            for (int i = 0; i < decorationRoots.childCount; ++i) {
                if (decorationRoots.GetChild(i).FindChild(_picecName) == null)
                    continue;
                positions.Add(decorationRoots.GetChild(i).FindChild(_picecName).position);
            }
            return positions;
        }

        public static void BestGeneToWorldPos(IChromosome bestGene, Volume volume) {
            foreach (var gene in bestGene.GetGenes()) {
                genePos = GameObject.Find("genePos");
                if (genePos == null)
                    genePos = new GameObject("genePos");
                GameObject geneWorldPosition = GameObject.CreatePrimitive(PrimitiveType.Cube);
                geneWorldPosition.transform.SetParent(genePos.transform);
                geneWorldPosition.transform.position = (gene.Value as CreVoxGene).Position;
            }
        }

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
            public CreVoxGene(GeneType type, Vector3 position) {
                this.Type = type;
                this.Position = position;
            }
        }
    }
}
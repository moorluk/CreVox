using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTUSTGA
{
    abstract public class NTUSTGeneticAlgorithm
    {
        #region Paremters

        float crossoverRate;
        float mutationRate;
        NTUSTChromosome sample;
        List<NTUSTChromosome> currentGeneration = new List<NTUSTChromosome>();

        #endregion

        public NTUSTGeneticAlgorithm(float crossoverRate, float mutationRate, NTUSTChromosome sample)
        {
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;
            this.sample = sample;
        }

        abstract public void Crossover(ref NTUSTChromosome parentCopy1, ref NTUSTChromosome parentCopy2);

        abstract public void Mutation(ref NTUSTChromosome chrom);

        public NTUSTChromosome Algorithm(int countOfChromosome, int countOfGeneration)
        {
            InitChromosome(countOfChromosome);

            for (int i = 0; i < countOfGeneration; i++)
            {
                //printGeneration(chromosomes, i);
                PrepareSelection();
                List<NTUSTChromosome> newGeneration = new List<NTUSTChromosome>();
                for (int j = 0; j < currentGeneration.Count; j += 2)
                {
                    NTUSTChromosome target1 = currentGeneration[Selection()].Copy();
                    NTUSTChromosome target2 = currentGeneration[Selection()].Copy();

                    if (crossoverRate > Random.Range(0.0f, 1.0f))
                    {
                        Crossover(ref target1, ref target2);
                    }
                    if (mutationRate > Random.Range(0.0f, 1.0f))
                    {
                        Mutation(ref target1);
                    }
                    if (mutationRate > Random.Range(0.0f, 1.0f))
                    {
                        Mutation(ref target2);
                    }
                    newGeneration.Add(target1);
                    newGeneration.Add(target2);
                }
                currentGeneration = newGeneration;
            }
            

            PrepareSelection();
            return currentGeneration[Selection()];
        }

        void InitChromosome(int countOfChromosome)
        {
            currentGeneration.Clear();
            for (int i = 0; i < countOfChromosome; i++)
                currentGeneration.Add(sample.RandomInitialize());
        }

        #region Selection
        List<float> wheel = new List<float>();

        void PrepareSelection()
        {
            float sum = 0.0f;
            wheel.Clear();
            for (int i = 0; i < currentGeneration.Count; ++i)
            {
                sum += currentGeneration[i].FitnessFunction();
                wheel.Add(sum);
            }
        }

        int Selection()
        {

            float random = Random.Range(0.0f, wheel[wheel.Count - 1]);
            for (int index = 0; index < wheel.Count; ++index)
            {
                if (random <= wheel[index])
                    return index;
            }
            return 0;
        }
        #endregion
    }
}

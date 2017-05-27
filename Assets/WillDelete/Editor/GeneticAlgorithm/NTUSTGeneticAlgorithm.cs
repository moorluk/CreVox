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

        abstract public void crossover(ref NTUSTChromosome parentCopy1, ref NTUSTChromosome parentCopy2);

        abstract public void mutation(ref NTUSTChromosome chrom);

        public NTUSTChromosome algorithm(int countOfChromosome, int countOfGeneration)
        {
            initChromosome(countOfChromosome);

            for (int i = 0; i < countOfGeneration; i++)
            {
                //printGeneration(chromosomes, i);
                prepareSelection();
                List<NTUSTChromosome> newGeneration = new List<NTUSTChromosome>();
                for (int j = 0; j < currentGeneration.Count; j += 2)
                {
                    NTUSTChromosome target1 = currentGeneration[selection()].copy();
                    NTUSTChromosome target2 = currentGeneration[selection()].copy();

                    if (crossoverRate > Random.Range(0.0f, 1.0f))
                    {
                        crossover(ref target1, ref target2);
                    }
                    if (mutationRate > Random.Range(0.0f, 1.0f))
                    {
                        mutation(ref target1);
                    }
                    if (mutationRate > Random.Range(0.0f, 1.0f))
                    {
                        mutation(ref target2);
                    }
                    newGeneration.Add(target1);
                    newGeneration.Add(target2);
                }
                currentGeneration = newGeneration;
            }
            

            prepareSelection();
            return currentGeneration[selection()];
        }

        void initChromosome(int countOfChromosome)
        {
            currentGeneration.Clear();
            for (int i = 0; i < countOfChromosome; i++)
                currentGeneration.Add(sample.randomInitialize());
        }

        #region Selection
        List<float> wheel = new List<float>();

        void prepareSelection()
        {
            float sum = 0.0f;
            wheel.Clear();
            for (int i = 0; i < currentGeneration.Count; ++i)
            {
                sum += currentGeneration[i].fitnessFunction();
                wheel.Add(sum);
            }
        }

        int selection()
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTUSTGA {
	abstract public class NTUSTGeneticAlgorithm {
		#region Paremters

		public int currentGenrationID;
		float crossoverRate;
		float mutationRate;
		public readonly int countOfChromosome;
		public readonly int countOfGeneration;
		NTUSTChromosome sample;
		List<NTUSTChromosome> currentGeneration = new List<NTUSTChromosome>();

		#endregion

		public NTUSTGeneticAlgorithm(float crossoverRate, float mutationRate, NTUSTChromosome sample, int countOfChromosome, int countOfGeneration) {
			this.crossoverRate = crossoverRate;
			this.mutationRate = mutationRate;
			this.sample = sample;
			this.countOfChromosome = countOfChromosome;
			this.countOfGeneration = countOfGeneration;
		}

		abstract public void Crossover(ref NTUSTChromosome parentCopy1, ref NTUSTChromosome parentCopy2);

		abstract public void Mutation(ref NTUSTChromosome chrom);

		public NTUSTChromosome Algorithm() {

			InitChromosome(countOfChromosome);

			for (currentGenrationID = 0; currentGenrationID < countOfGeneration; currentGenrationID++) {

				//printGeneration(chromosomes, i);
				PrepareSelection();
				List<NTUSTChromosome> newGeneration = new List<NTUSTChromosome>();
				for (int j = 0; j < currentGeneration.Count; j += 2) {
					NTUSTChromosome target1 = currentGeneration[Selection()].Copy();
					NTUSTChromosome target2 = currentGeneration[Selection()].Copy();

					if (crossoverRate > Random.Range(0.0f, 1.0f)) {
						Crossover(ref target1, ref target2);
					}
					if (mutationRate > Random.Range(0.0f, 1.0f)) {
						Mutation(ref target1);
					}
					if (mutationRate > Random.Range(0.0f, 1.0f)) {
						Mutation(ref target2);
					}
					newGeneration.Add(target1);
					newGeneration.Add(target2);
				}
				onGenrationEnd(currentGenrationID, currentGeneration, newGeneration);
				currentGeneration = newGeneration;
			}
			for(var i = 0; i < currentGeneration.Count; ++i) {
				currentGeneration[i].csvFinished = true;
			}

			PrepareSelection();
			return currentGeneration[SelectTheBestChromosomeIndex()];
		}

		void InitChromosome(int countOfChromosome) {
			currentGeneration.Clear();
			for (int i = 0; i < countOfChromosome; i++)
				currentGeneration.Add(sample.RandomInitialize());
		}

		#region Selection
		List<float> wheel = new List<float>();

		void PrepareSelection() {
			float sum = 0.0f;
			wheel.Clear();
			foreach (var chromosomeme in currentGeneration) {
				if(chromosomeme.FitnessScore == null) {
					chromosomeme.SetFitnessFunctionScore();
				}
			}
			for (int i = 0; i < currentGeneration.Count; ++i) {
				sum += currentGeneration[i].FitnessFunction();
				wheel.Add(sum);
			}
		}

		int Selection() {
			float random = Random.Range(0.0f, wheel[wheel.Count - 1]);
			for (int index = 0; index < wheel.Count; ++index) {
				if (random <= wheel[index])
					return index;
			}
			return 0;
		}

		protected virtual int SelectTheBestChromosomeIndex() {
			return wheel.IndexOf(wheel.Max());
		}
		#endregion

		#region onFunction

		virtual public void onGenrationEnd(int generation, List<NTUSTChromosome> currentGeneration, List<NTUSTChromosome> newGeneration) {

		}

		#endregion
	}
}

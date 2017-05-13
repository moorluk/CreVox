// using System.Linq;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using GeneticSharp;

// namespace GenericAlgorithm {
// 	public enum GeneType {
// 		Forbidden = -1,
// 		Empty     = 0,
// 		Enemy     = 1,
// 		Treasure  = 2,
// 		Trap      = 3
// 	}

// 	public class Population {
// 		public List<Chromosome> Chromosomes { get; private set; }
		
		
// 		// Majar actions.
// 		public void GenerateSearchSpace() {

// 		}
// 		public void Selection() {
// 			double wheelBoundary = Chromosomes.Sum(c => c.fitnessScoreSum);

// 		}
// 		public void Crossover() {
			
// 		}
// 		public void Mutation() {
			
// 		}



// 		// Subclass - Chromosome.
// 		public class Chromosome {
// 			// x, y, z of chromosome.
// 			private MultiKeyDictionary<int, int, int, GeneType> _genes = new MultiKeyDictionary<int, int, int, GeneType>();
// 			public double fitnessScoreSum { get; private set; }
// 			public double[] Weights { get; set; }

// 			public Chromosome() {
// 				this.Genes = new List<PositionType>();
// 				this.fitnessScoreSum = 0.0f;
// 			}

// 			public void Evalute(){
// 				fitnessScoreSum += Weights[0]*Fitness1();
// 				fitnessScoreSum += Weights[1]*Fitness2();
// 				fitnessScoreSum += Weights[2]*Fitness3();
// 				fitnessScoreSum += Weights[3]*Fitness4();
// 				fitnessScoreSum += Weights[4]*Fitness5();
// 				fitnessScoreSum += Weights[5]*Fitness6();
// 				fitnessScoreSum += Weights[6]*Fitness7();
// 				fitnessScoreSum += Weights[7]*Fitness8();
// 				fitnessScoreSum += Weights[8]*Fitness9();
// 			}

// 			private double Fitness1() {
// 				return 0;
// 			}
// 			private double Fitness2() {
// 				return 0;
// 			}
// 			private double Fitness3() {
// 				return 0;
// 			}
// 			private double Fitness4() {
// 				return 0;
// 			}
// 			private double Fitness5() {
// 				return 0;
// 			}
// 			private double Fitness6() {
// 				return 0;
// 			}
// 			private double Fitness7() {
// 				return 0;
// 			}
// 			private double Fitness8() {
// 				return 0;
// 			}
// 			private double Fitness9() {
// 				return 0;
// 			}
// 		}
// 	}






// 	// Multi-keys Dictionary. (Becuase Unity doesn't support 'Tuple class' in .Net 4.0.)
// 	public class MultiKeyDictionary<Key1, Key2, T>: Dictionary<Key1, Dictionary<Key2, T>> {
// 		public T this[Key1 key1, Key2 key2] {
// 			get {
// 				return base[key1][key2];
// 			}
// 			set {
// 				if (! ContainsKey(key1)) {
// 					this[key1] = new Dictionary<Key2, T>();
// 				}
// 				this[key1][key2] = value;
// 			}
// 		}

// 		public void Add(Key1 key1, Key2 key2, T value) {
// 			if (! ContainsKey(key1)) {
// 				this[key1] = new Dictionary<Key2, T>();
// 			}
// 			this[key1][key2] = value;
// 		}

// 		public bool ContainsKey(Key1 key1, Key2 key2) {
// 			return base.ContainsKey(key1) && this[key1].ContainsKey(key2);
// 		}
// 	}

// 	public class MultiKeyDictionary<Key1, Key2, Key3, T> : Dictionary<Key1, MultiKeyDictionary<Key2, Key3, T>> {
// 		public T this[Key1 key1, Key2 key2, Key3 key3] {
// 			get {
// 				return ContainsKey(key1) ? this[key1][key2, key3] : default(T);
// 			}
// 			set {
// 				if (! ContainsKey(key1)) {
// 					this[key1] = new MultiKeyDictionary<Key2, Key3, T>();
// 				}
// 				this[key1][key2, key3] = value;
// 			}
// 		}

// 		public void Add(Key1 key1, Key2 key2, Key3 key3, T value) {
// 			if (! ContainsKey(key1)) {
// 				this[key1] = new MultiKeyDictionary<Key2, Key3, T>();
// 			}
// 			this[key1][key2, key3] = value;
// 		}

// 		public bool ContainsKey(Key1 key1, Key2 key2, Key3 key3) {
// 			return base.ContainsKey(key1) && this[key1].ContainsKey(key2, key3);
// 		}
// 	}
// }
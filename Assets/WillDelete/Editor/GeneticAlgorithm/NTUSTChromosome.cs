using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTUSTGA
{
    abstract public class NTUSTChromosome
    {

        public List<NTUSTGene> Genes = new List<NTUSTGene>();

        public abstract class NTUSTGene
        {

            abstract public NTUSTGene Copy();
        }

        public abstract float FitnessFunction();

        public abstract NTUSTChromosome Copy();

        public abstract NTUSTChromosome RandomInitialize();
    }
}


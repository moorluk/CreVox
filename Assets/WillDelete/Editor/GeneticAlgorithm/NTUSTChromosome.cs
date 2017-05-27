using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTUSTGA
{
    abstract public class NTUSTChromosome
    {

        public List<NTUSTGene> genes = new List<NTUSTGene>();

        abstract public class NTUSTGene
        {

            abstract public NTUSTGene copy();
        }

        abstract public float fitnessFunction();

        abstract public NTUSTChromosome copy();

        abstract public NTUSTChromosome randomInitialize();
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithm
{
    class Chromosome
    {
        private int numGenes;
        private uint[] genes;
        private int fitnessScore;
        private double crossoverRate;
        private double mutationRate;

        private static bool hasMated = false;

        private const int maxGeneVal = 16;

        // New random chromosome
        public Chromosome(int numGenes, double crossoverRate, double mutationRate)
        {
            this.numGenes = numGenes;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;

            genes = new uint[numGenes]; //only works for evens right now
            for(int i = 0; i < numGenes; i++)
            {
                genes[i] = (uint)(Program.rng.Next(maxGeneVal));
            }
            fitnessScore = computeFitnessScore();
        }

        // New chromosome with given bitString
        public Chromosome(uint[] genes, double crossoverRate, double mutationRate)
        {
            this.genes = genes;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;

            numGenes = genes.Length;
            fitnessScore = computeFitnessScore();
        }

        //Working but may be inefficient.
        public static Chromosome[] rouletteSelect(Chromosome[] population, int numToSelect)
        {
            Chromosome[] toReturn = new Chromosome[numToSelect];
            for (int i = 0; i < numToSelect; i++)
                toReturn[i] = null;

            //population contains all fitness scores
            int[] fitnessScores = new int[population.Length];
            for (int i = 0; i < population.Length; i++)
            {
                fitnessScores[i] = population[i].getFitnessScore();
            }
            for (int i = 1; i < population.Length; i++)
            {
                fitnessScores[i] = fitnessScores[i] + fitnessScores[i - 1];
            }
            //final position will have the total for the entire population.
            //each fitness score represents number of balls, then each is also
            //an assignment for a range of ball numbers.

            //Choose the 2 balls, if it was already selected, choose again
            for(int i = 0; i < numToSelect; i++)
            {
                int selected = (Program.rng.Next() % fitnessScores[population.Length - 1]) + 1;
                
                //find which index this is
                for(int j = 0; j < population.Length; j++)
                {
                    if(selected <= fitnessScores[j] && !hasBeenSelected(population[j], toReturn))
                    {
                        toReturn[i] = population[j];
                        j = population.Length;
                    }
                    else if (selected <= fitnessScores[j] && hasBeenSelected(population[j], toReturn))
                    {
                        i--;
                        j = population.Length;
                    }
                }
            }
            return toReturn;
        }

        private static bool hasBeenSelected(Chromosome toCheck, Chromosome[] selected)
        {
            foreach (Chromosome oneSelected in selected)
            {
                if (oneSelected != null && oneSelected.Equals(toCheck))
                    return true;
            }
            return false;
        }

        private int computeFitnessScore()
        {
            // fitness score computed here
            // Thought about trying to create a method so you could put what ever
            // formula you wanted in. Decided that in the final application this
            // will never need to change so it's okay to just hardcode it.
            uint str = genes[0];
            uint spd = genes[1];
            uint end = genes[2];
            uint intel = genes[3];

            int toReturn = (int)((100 * str) + (100 * spd) + (100 * end) + (100 * intel));
            if (toReturn < 0)
                toReturn = 0;
            return toReturn;
        }

        // crossing genes of 2 mates
        public static Chromosome[] mate(Chromosome mate1, Chromosome mate2)
        {
            //crossover points = 2
            //For 2 crossover points, just do the process twice
            
            //note on crossoverRate. Sould be between 0 and 1.
            //0 <-- more weight on parent 1
            //1 <-- more weight on parent 2
            //for now, crossoverRate is going to be ignored and the pivot
            //for the two chromosomes will be random
            uint[] childGenes1 = new uint[mate1.getNumGenes()];
            uint[] childGenes2 = new uint[mate2.getNumGenes()];

            Chromosome[] toReturn = new Chromosome[2];
            /*
            Console.WriteLine("mate1: ");
            foreach (uint gene in mate1.getGenes())
                Console.WriteLine(gene);
            Console.WriteLine("mate2: ");
            foreach (uint gene in mate2.getGenes())
                Console.WriteLine(gene);
            */
            //chose which change to split on
            int geneToSplit = Program.rng.Next(mate1.getNumGenes());
            //Console.Write("Gene to split: ");
            //Console.WriteLine(geneToSplit);
            //calculate how many bits we are using
            int bitPos = 0;
            int bitsCalc = maxGeneVal;
            while (bitsCalc != 1)
            {
                bitsCalc = bitsCalc >> 1;
                bitPos++;
            }
            int pivot = Program.rng.Next(bitPos + 1);
            //Console.Write("Pivot: ");
            //Console.WriteLine(pivot);
            //no mid-gene swap
            if (pivot == 0)
            {
                childGenes1[geneToSplit] = mate1.getGenes()[geneToSplit];
                childGenes2[geneToSplit] = mate2.getGenes()[geneToSplit];
            }//swap all bits of this gene
            else if (pivot == bitPos)
            {
                childGenes1[geneToSplit] = mate2.getGenes()[geneToSplit];
                childGenes2[geneToSplit] = mate1.getGenes()[geneToSplit];
            }//mid bitstring swap, actually have to do stuff here
            else
            {
                uint saveMask = uint.MaxValue << pivot;
                uint swapMask = uint.MaxValue >> 32 - pivot;
                // half of the chunk that is swapping
                uint swapChunk1 = mate1.getGenes()[geneToSplit] & swapMask;
                uint swapChunk2 = mate2.getGenes()[geneToSplit] & swapMask;

                //put two havles together with bitwise 'or'
                childGenes1[geneToSplit] = (mate1.getGenes()[geneToSplit] & saveMask) | swapChunk2;
                childGenes2[geneToSplit] = (mate2.getGenes()[geneToSplit] & saveMask) | swapChunk1;
            }

            //then, copy all genes before the pivot, and swap all genes after pivot
            for (int i = 0; i < mate1.getNumGenes(); i++)
            {
                if (i < geneToSplit)
                {
                    childGenes1[i] = mate1.getGenes()[i];
                    childGenes2[i] = mate2.getGenes()[i];
                }
                else if (i > geneToSplit)
                {
                    childGenes1[i] = mate2.getGenes()[i];
                    childGenes2[i] = mate1.getGenes()[i];
                }
            }
                

            Chromosome newChild1 = new Chromosome(childGenes1, mate1.getCrossoverRate(), mate1.getMutationRate());
            Chromosome newChild2 = new Chromosome(childGenes2, mate2.getCrossoverRate(), mate2.getMutationRate());

            toReturn[0] = newChild1;
            toReturn[1] = newChild2;
            /*
            Console.WriteLine("child1: ");
            foreach (uint gene in newChild1.getGenes())
                Console.WriteLine(gene);
            Console.WriteLine("child2: ");
            foreach (uint gene in newChild2.getGenes())
                Console.WriteLine(gene);
            Console.WriteLine("------------------------");
            */
            if (hasMated)
            {
                hasMated = false;
                return toReturn;
            }
            else
            {
                hasMated = true;
                return mate(newChild1, newChild2);
            }
        }

        // random chance that a bit is flipped
        private void mutateGenes()
        {

        }

        // Get'ers/set'ers

        public int getNumGenes()
        {
            return numGenes;
        }

        public int getFitnessScore()
        {
            return fitnessScore;
        }

        public double getCrossoverRate()
        {
            return crossoverRate;
        }

        public double getMutationRate()
        {
            return mutationRate;
        }

        public uint[] getGenes()
        {
            return genes;
        }
    }
}

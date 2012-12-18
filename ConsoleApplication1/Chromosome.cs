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

        public static Chromosome[] rouletteSelect(Chromosome[] population, int numToSelect)
        {            
            Chromosome[] toReturn = new Chromosome[numToSelect];
            int[] popIndex = new int[population.Length];
            bool[] hasBeenSelected = new bool[population.Length];

            // sort and compute total fitness
            int fitnessSum = 0;
            population = population.OrderByDescending(x => x.getFitnessScore()).ToArray();
            int previousFitness = 0;
            int counter = 0;
            foreach (Chromosome chrome in population)
            {
                popIndex[counter] = chrome.getFitnessScore() + previousFitness;
                previousFitness = popIndex[counter];
                counter++;
                fitnessSum += chrome.getFitnessScore();
            }

            int selected;
            for(int i = 0; i < numToSelect; i++)
            {
                selected = Program.rng.Next(fitnessSum);
                int index = 0;
                while (selected >= popIndex[index])
                {
                    index++;
                }
                // chrome @ current index was selected from the roulette
                // if the chromosome has already been selected, we just randomly
                // generate another number. This is an easy way to do this but
                // can potentially take a long time to finish the selection if
                // the proportion of selected to not selected is high.
                if (!hasBeenSelected[index])
                {
                    toReturn[i] = population[index];
                    hasBeenSelected[index] = true;
                }
                else
                {
                    // we don't want to go to the next index since we tried
                    // to select something already selected
                    i--;
                }                
            }
            return toReturn;
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

            int toReturn = (int)(str + spd + end + intel);
            if (toReturn < 0)
                toReturn = 0;
            return toReturn;
        }

        // crossing genes of 2 mates
        public static Chromosome[] mate(Chromosome mate1, Chromosome mate2)
        {
            //crossover points = 2
            //For 2 crossover points, just do the process twice
            
            uint[] childGenes1 = new uint[mate1.getNumGenes()];
            uint[] childGenes2 = new uint[mate2.getNumGenes()];

            Chromosome[] toReturn = new Chromosome[2];

            //chose which change to split on
            int geneToSplit = Program.rng.Next(mate1.getNumGenes());

            //calculate how many bits we are using
            int bitPos = 0;
            int bitsCalc = maxGeneVal;
            while (bitsCalc != 1)
            {
                bitsCalc = bitsCalc >> 1;
                bitPos++;
            }
            int pivot = Program.rng.Next(bitPos + 1);

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

            if (hasMated)
            {
                hasMated = false;
                toReturn[0] = mutateGenes(toReturn[0], mate1.getMutationRate());
                toReturn[1] = mutateGenes(toReturn[1], mate2.getMutationRate());
                return toReturn;
            }
            else
            {
                hasMated = true;
                return mate(newChild1, newChild2);
            }
        }

        // random chance that a bit is flipped
        private static Chromosome mutateGenes(Chromosome toMutate, double mutationRate)
        {
            double target = 1.0 / mutationRate;
            if (target == Program.rng.Next(1000) + 1)
            {
                // mutation! now we randomly select bit to change
                
                int geneToMutate = Program.rng.Next(toMutate.getNumGenes());
                int bitsCalc = maxGeneVal;
                int bitPos = 0;
                while (bitsCalc != 1)
                {
                    bitsCalc = bitsCalc >> 1;
                    bitPos++;
                }
                
                int bitToFlip = Program.rng.Next(bitPos);
                uint newGeneVal = toMutate.getGenes()[geneToMutate];
                newGeneVal ^= (uint)(1 << bitToFlip);

                toMutate.setGene(geneToMutate, newGeneVal);
            }
            return toMutate;
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

        public void setFitnessScore(int fitnessScore)
        {
            this.fitnessScore = fitnessScore;
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
        public void setGene(int index, uint newVal)
        {
            genes[index] = newVal;
        }
    }
}

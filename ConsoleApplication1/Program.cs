using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithm
{
    class Program
    {
        public static Random rng = new Random();
        
        static void Main(string[] args)
        {
            int better = 0;
            int worse = 0;
            double grandAvgSum = 0;
            int runs = 0;
            for( ; runs < 1; runs++)
            {
                int populationSize = 200;
                int currentGeneration = 1;
                Chromosome[] population = new Chromosome[populationSize];
                int maxScore = 0;
                int minScore = Int32.MaxValue;
                int total = 0;

                //initial setup of the population
                for (int i = 0; i < populationSize; i++)
                {
                    population[i] = new Chromosome(4, .5, .001);

                    if (population[i].getFitnessScore() > maxScore)
                        maxScore = population[i].getFitnessScore();
                    if (population[i].getFitnessScore() < minScore)
                        minScore = population[i].getFitnessScore();
                    total += population[i].getFitnessScore();
                }
                int firstMax = maxScore;
                int lastMax = 0;
                double lastAvg = 0;
                double firstAvg = (double)(total) / (double)(populationSize);
                for (; currentGeneration <= 10000; currentGeneration++)
                {
                    Chromosome[] selected = Chromosome.rouletteSelect(population, populationSize / 2);
                    Chromosome[] nextGen = new Chromosome[populationSize];

                    maxScore = 0;
                    minScore = Int32.MaxValue;
                    total = 0;

                    //first half are selected and mated from previous set
                    for (int j = 0; j < populationSize / 2; j += 2)
                    {
                        Chromosome[] offspring = Chromosome.mate(selected[j], selected[j + 1]);

                        nextGen[j] = offspring[0];
                        nextGen[j + 1] = offspring[1];
                    }
                    //second half are just the 'parents' that live on
                    for (int j = populationSize / 2; j < populationSize; j++)
                    {
                        nextGen[j] = selected[j - (populationSize / 2)];
                    }

                    for (int j = 0; j < populationSize; j++)
                    {
                        if (nextGen[j].getFitnessScore() > maxScore)
                            maxScore = nextGen[j].getFitnessScore();
                        if (nextGen[j].getFitnessScore() < minScore)
                            minScore = nextGen[j].getFitnessScore();
                        total += nextGen[j].getFitnessScore();
                    }

                    population = nextGen;
                    lastMax = maxScore;
                    lastAvg = ((double)(total) / (double)(populationSize));
                }

                if (firstAvg < lastAvg)
                    better++;
                else
                    worse++;
                grandAvgSum += lastAvg;
            }
            Console.Write("Better performance: ");
            Console.WriteLine(better);
            Console.Write("Worse performance: ");
            Console.WriteLine(worse);
            Console.Write("Grand Average: ");
            Console.WriteLine(grandAvgSum/runs);
            Console.ReadLine();
        }
    }
}

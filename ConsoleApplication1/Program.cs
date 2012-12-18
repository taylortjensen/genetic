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
            int populationSize = 20;
            int currentGeneration = 1;
            Chromosome[] population = new Chromosome[populationSize];
            int maxScore = 0;
            int minScore = Int32.MaxValue;
            int total = 0;

            //initial setup of the population
            for (int i = 0; i < populationSize; i++)
            {
                population[i] = new Chromosome(4, .5, .5);
                Console.Write("Fitness Score: ");
                Console.WriteLine(population[i].getFitnessScore());
                if (population[i].getFitnessScore() > maxScore)
                    maxScore = population[i].getFitnessScore();
                if (population[i].getFitnessScore() < minScore)
                    minScore = population[i].getFitnessScore();
                total += population[i].getFitnessScore();
            }

            Console.WriteLine("---------------------");
            Console.Write("Max Score: ");
            Console.WriteLine(maxScore);
            Console.Write("Min Score: ");
            Console.WriteLine(minScore);
            Console.Write("Avg Score: ");
            Console.WriteLine((double)(total) / (double)(populationSize));
            Console.WriteLine("---------------------");
            Console.ReadLine();
            int firstMax = maxScore;
            int lastMax = 0;
            double lastAvg = 0;
            double firstAvg = (double)(total) / (double)(populationSize);
            for (; currentGeneration <= 100; currentGeneration++)
            {
                Chromosome[] selected = Chromosome.rouletteSelect(population, populationSize / 2);
                Chromosome[] nextGen = new Chromosome[populationSize];

                maxScore = 0;
                minScore = Int32.MaxValue;
                total = 0;

                //first half are selected and mated from previous set
                for (int j = 0; j < populationSize / 2; j += 2)
                {
                    Chromosome[] offspring = Chromosome.mate(selected[j], selected[j+1]);
                    /*Console.WriteLine("Parent1 genes: ");
                    foreach (uint gene in selected[j].getGenes())
                        Console.WriteLine(gene);
                    Console.WriteLine("Parent2 genes: ");
                    foreach (uint gene in selected[j+1].getGenes())
                        Console.WriteLine(gene);
                    Console.WriteLine("Child1 genes: ");
                    foreach (uint gene in offspring[0].getGenes())
                        Console.WriteLine(gene);
                    Console.WriteLine("Child2 genes: ");
                    foreach (uint gene in offspring[1].getGenes())
                        Console.WriteLine(gene);
                    Console.ReadLine();*/
                    nextGen[j] = offspring[0];
                    nextGen[j + 1] = offspring[1];
                }
                //second half randomly generated
                for (int j = populationSize / 2; j < populationSize; j++)
                {
                    nextGen[j] = new Chromosome(4, .5, .5);
                }

                for (int j = 0; j < populationSize; j++)
                {
                    Console.WriteLine(nextGen[j].getFitnessScore());
                    if (nextGen[j].getFitnessScore() > maxScore)
                        maxScore = nextGen[j].getFitnessScore();
                    if (nextGen[j].getFitnessScore() < minScore)
                        minScore = nextGen[j].getFitnessScore();
                    total += nextGen[j].getFitnessScore();
                }

                Console.WriteLine("---------------------");
                Console.Write("Max Score: ");
                Console.WriteLine(maxScore);
                Console.Write("Min Score: ");
                Console.WriteLine(minScore);
                Console.Write("Avg Score: ");
                Console.WriteLine((double)(total) / (double)(populationSize));
                Console.WriteLine("---------------------");
               // Console.ReadLine();
                population = nextGen;
                lastMax = maxScore;
                lastAvg = ((double)(total) / (double)(populationSize));
            }
            Console.WriteLine("*****************************");
            Console.WriteLine(firstMax < lastMax);
            Console.WriteLine(firstAvg < lastAvg);

            // Used to stop from closing
            Console.ReadLine();

        }
    }
}

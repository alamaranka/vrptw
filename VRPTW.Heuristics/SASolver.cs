using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Heuristics.LocalSearch;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class SASolver
    {
        private readonly Dataset _dataset;
        private Solution _bestSolution;

        public SASolver(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var stopwatch = Stopwatch.StartNew();
            var totalSecondsElapsed = 0.0;
            var currentSolution = new HSolver(_dataset).Run();
            var temperature = Config.GetSimulatedAnnealingParam().InitialTemperature;
            var alpha = Config.GetSimulatedAnnealingParam().Alpha;
            var iterationCount = Config.GetSimulatedAnnealingParam().IterationCount;
            var diversifyForEachNIteration = Config.GetDiversificationParam().DiversifyForEachNIteration;
            var minCustomersToRemove = Config.GetDiversificationParam().MinCustomersToRemove;
            var maxCustomersToRemove = Config.GetDiversificationParam().MaxCustomersToRemove;

            _bestSolution = currentSolution;

            for (var i = 0; i <= iterationCount; i++)
            {
                totalSecondsElapsed += stopwatch.Elapsed.Milliseconds;

                if (i % diversifyForEachNIteration == 0)
                {
                    currentSolution = new Diversifier(currentSolution, minCustomersToRemove, maxCustomersToRemove).Diverisfy();
                }
                
                var solutionPool = new List<Solution>();
                solutionPool.AddRange(new TwoOptOperator(currentSolution).GenerateFeasibleSolutions());
                solutionPool.AddRange(new ExchangeOperator(currentSolution).GenerateFeasibleSolutions());
                solutionPool.AddRange(new RelocateOperator(currentSolution).GenerateFeasibleSolutions());
                var candidateSolution = Helpers.GetBestNeighbour(solutionPool);

                Console.WriteLine("Iteration: {0}, Time Elapsed: {1} sn, Temp: {2}, {3} candidate, Current Cost: {4}, Best Cost {5}",
                                   i,
                                   totalSecondsElapsed / 1_000.0,
                                   Math.Round(temperature, 3),
                                   solutionPool.Count,
                                   Math.Round(currentSolution.Cost, 3),
                                   Math.Round(_bestSolution.Cost, 3)
                                   );

                var currentCost = currentSolution.Cost;
                var candidateCost = candidateSolution.Cost;
                //var rand = new Random().NextDouble();
                //var threshold = GetThreshold(currentCost, candidateCost, temperature);
                var acceptanceCondition = candidateCost < currentCost; //||
                                            //(candidateCost >= currentCost && rand <= threshold);

                if (acceptanceCondition)
                {
                    currentSolution = Helpers.Clone(candidateSolution);

                    if (currentSolution.Cost < _bestSolution.Cost)
                    {
                        _bestSolution = currentSolution;
                    }
                }

                temperature *= alpha;
            }

            stopwatch.Stop();

            return _bestSolution;
        }

        private double GetThreshold(double currentObj, double candidateObj, double currentTemperature)
        {
            return 1 / Math.Exp((candidateObj - currentObj) / currentTemperature);
        }
    }
}

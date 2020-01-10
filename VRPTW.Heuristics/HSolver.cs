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
    public class HSolver
    {
        private readonly Dataset _dataset;
        private Solution _bestSolution;

        public HSolver(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var stopwatch = Stopwatch.StartNew();
            var totalSecondsElapsed = 0.0;
            var currentSolution = new LSAlgorithm(_dataset).Run();
            var temperature = Config.GetSimulatedAnnealingParam().InitialTemperature;
            var alpha = Config.GetSimulatedAnnealingParam().Alpha;
            var iterationCount = Config.GetSimulatedAnnealingParam().IterationCount;
            var numberOfNonImprovingIters = Config.GetDiversificationParam().NumberOfNonImprovingIters;
            var minCustomersToRemove = Config.GetDiversificationParam().MinCustomersToRemove;
            var maxCustomersToRemove = Config.GetDiversificationParam().MaxCustomersToRemove;
            var numberOfNonImprovingItersCounter = 0;


            _bestSolution = currentSolution;

            for (var i = 0; i <= iterationCount; i++)
            {
                totalSecondsElapsed += stopwatch.Elapsed.Milliseconds;

                if (numberOfNonImprovingItersCounter == numberOfNonImprovingIters)
                {
                    currentSolution = new Diversifier(Helpers.Clone(_bestSolution), minCustomersToRemove, maxCustomersToRemove).Diverisfy();
                    numberOfNonImprovingItersCounter = 0;
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
                var acceptanceCondition = candidateCost < currentCost; 

                if (acceptanceCondition)
                {
                    currentSolution = Helpers.Clone(candidateSolution);

                    if (currentSolution.Cost < _bestSolution.Cost)
                    {
                        _bestSolution = currentSolution;
                    }
                }
                else
                {
                    numberOfNonImprovingItersCounter++;
                }

                temperature *= alpha;
            }

            stopwatch.Stop();

            return _bestSolution;
        }
    }
}

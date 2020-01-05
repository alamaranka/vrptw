using System;
using System.Collections.Generic;
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
            var currentSolution = new HSolver(_dataset).Run();
            var temperature = Config.GetSimulatedAnnealingParam().InitialTemperature;
            var alpha = Config.GetSimulatedAnnealingParam().Alpha;
            var iterationCount = Config.GetSimulatedAnnealingParam().IterationCount;
            _bestSolution = currentSolution;

            for (var i=0; i<iterationCount; i++)
            {
                var solutionPool = new List<Solution>();
                solutionPool.AddRange(new TwoOptOperator(currentSolution).GenerateFeasibleSolutions(5));
                solutionPool.AddRange(new ExchangeOperator(currentSolution).GenerateFeasibleSolutions());
                solutionPool.AddRange(new RelocateOperator(currentSolution).GenerateFeasibleSolutions()); 
                var candidateSolution = Helpers.GetBestNeighbour(solutionPool);

                Console.WriteLine("Iteration: {0}, Temperature: {1}, {2} candidate solutions, current cost: {3}, best cost {4}",
                                   i, 
                                   Math.Round(temperature, 3),
                                   solutionPool.Count,
                                   Math.Round(currentSolution.Cost, 3),
                                   Math.Round(_bestSolution.Cost, 3)
                                   );

                var currentCost = currentSolution.Cost;
                var candidateCost = candidateSolution.Cost;
                var rand = new Random().NextDouble();
                var threshold = GetThreshold(currentCost, candidateCost, temperature);
                var acceptanceCondition = candidateCost < currentCost ||
                                            (candidateCost >= currentCost && rand <= threshold);

                if (acceptanceCondition)
                {
                    currentSolution = Helpers.Clone(candidateSolution);
                    
                    if (currentSolution.Cost < _bestSolution.Cost)
                    {
                        _bestSolution = currentSolution;
                    }
                }

                if (i % 20 == 0)
                {
                    currentSolution = new Diversifier(currentSolution, 10, 20).Diverisfy();
                }

                temperature *= alpha;
            }

            return _bestSolution;
        }

        private double GetThreshold(double currentObj, double candidateObj, double currentTemperature)
        {
            return 1 / Math.Exp((candidateObj - currentObj) / currentTemperature);
        }
    }
}

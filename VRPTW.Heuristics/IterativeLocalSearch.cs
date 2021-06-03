using System;
using System.Collections.Generic;
using System.Diagnostics;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class IterativeLocalSearch
    {
        private readonly Dataset _dataset;
        private Solution _bestSolution;

        public IterativeLocalSearch(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Restart();
            var totalSecondsElapsed = 0.0;
            var currentSolution = new LocalSearch(_dataset).Run();
            var heuristicParams = Config.GetHeuristicsParam();
            var iterationCount = heuristicParams.IterationCount;
            var numberOfNonImprovingIters = heuristicParams.DiversificationParam.NumberOfNonImprovingIters;
            var minCustomersToRemove = heuristicParams.DiversificationParam.MinCustomersToRemove;
            var maxCustomersToRemove = heuristicParams.DiversificationParam.MaxCustomersToRemove;
            var numberOfNonImprovingItersCounter = 0;

            _bestSolution = currentSolution;
            currentSolution = new Diversifier(_bestSolution.Clone(), minCustomersToRemove, maxCustomersToRemove).Diverisfy();

            totalSecondsElapsed += TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalSeconds;
            Console.WriteLine("Local search completed in {0} seconds!", Math.Round(totalSecondsElapsed, 3));

            for (var i = 0; i <= iterationCount; i++)
            {
                stopwatch.Restart();

                if (numberOfNonImprovingItersCounter == numberOfNonImprovingIters)
                {
                    currentSolution = new Diversifier(_bestSolution.Clone(), minCustomersToRemove, maxCustomersToRemove).Diverisfy();
                    numberOfNonImprovingItersCounter = 0;
                }
                
                var solutionPool = new List<Solution>();
                solutionPool.AddRange(new CrossOperator(currentSolution).GenerateFeasibleSolutions());
                solutionPool.AddRange(new TwoOptOperator(currentSolution).GenerateFeasibleSolutions());
                solutionPool.AddRange(new ExchangeOperator(currentSolution).GenerateFeasibleSolutions());
                solutionPool.AddRange(new RelocateOperator(currentSolution).GenerateFeasibleSolutions());
                var candidateSolution = Helpers.GetBestNeighbour(solutionPool);
                var currentCost = currentSolution.Cost;
                var candidateCost = candidateSolution.Cost;
                var acceptanceCondition = candidateCost < currentCost; 

                if (acceptanceCondition)
                {
                    currentSolution = candidateSolution.Clone();

                    if (currentSolution.Cost < _bestSolution.Cost)
                    {
                        _bestSolution = currentSolution;
                    }
                }
                else
                {
                    numberOfNonImprovingItersCounter++;
                }

                totalSecondsElapsed += TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalSeconds;

                Console.WriteLine("Iteration: {0}, Time Elapsed: {1} sn, {2} candidate, Current Cost: {3}, Best Cost {4}",
                                   i,
                                   Math.Round(totalSecondsElapsed, 3),
                                   solutionPool.Count,
                                   Math.Round(currentSolution.Cost, 3),
                                   Math.Round(_bestSolution.Cost, 3)
                                   );
            }

            stopwatch.Stop();

            return _bestSolution;
        }
    }
}

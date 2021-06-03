﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class TabuSearch
    {
        private readonly Dataset _dataset;
        private Solution _bestSolution;

        public TabuSearch(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var stopwatch = Stopwatch.StartNew();
            var totalSecondsElapsed = 0.0;
            var currentSolution = new LocalSearch(_dataset).Run();
            var heuristicParams = Config.GetHeuristicsParam();
            var iterationCount = heuristicParams.IterationCount;
            var tabuListSize = heuristicParams.TabuSearchParam.TabuListSize;
            var numberOfNonImprovingIters = heuristicParams.DiversificationParam.NumberOfNonImprovingIters;
            var minCustomersToRemove = heuristicParams.DiversificationParam.MinCustomersToRemove;
            var maxCustomersToRemove = heuristicParams.DiversificationParam.MaxCustomersToRemove;
            var numberOfNonImprovingItersCounter = 0;
            var tabuList = new Helpers.FixedSizedQueue<string>(tabuListSize);

            currentSolution = new Diversifier(currentSolution, 
                                              minCustomersToRemove, 
                                              maxCustomersToRemove).Diverisfy();
            tabuList.Enqueue(Helpers.GetStringFormOfSolution(currentSolution));
            
            _bestSolution = currentSolution;
            var candidateSolution = new Solution();

            for (var i = 0; i <= iterationCount; i++)
            {
                totalSecondsElapsed += stopwatch.Elapsed.Milliseconds;

                if (numberOfNonImprovingItersCounter == numberOfNonImprovingIters)
                {
                    currentSolution = new Diversifier(_bestSolution.Clone(), minCustomersToRemove, maxCustomersToRemove).Diverisfy();
                    numberOfNonImprovingItersCounter = 0;
                }

                var solutionPool = CollectNeighbourSolutions(currentSolution);

                for (var s = 0; s < solutionPool.Count; s++)
                {
                    if (!tabuList.Contains(Helpers.GetStringFormOfSolution(solutionPool[s])))
                    {
                        candidateSolution = solutionPool[s];
                        break;
                    }
                }

                Console.WriteLine("Iteration: {0}, Time Elapsed: {1} sn, {2} candidate, Current Cost: {3}, Best Cost {4}",
                                   i,
                                   totalSecondsElapsed / 1_000.0,
                                   solutionPool.Count,
                                   Math.Round(currentSolution.Cost, 3),
                                   Math.Round(_bestSolution.Cost, 3)
                                   );

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

                tabuList.Enqueue(Helpers.GetStringFormOfSolution(candidateSolution));
            }

            stopwatch.Stop();

            return _bestSolution;
        }

        private List<Solution> CollectNeighbourSolutions(Solution solution)
        {
            var solutionPool = new List<Solution>();

            solutionPool.AddRange(new CrossOperator(solution).GenerateFeasibleSolutions());
            solutionPool.AddRange(new TwoOptOperator(solution).GenerateFeasibleSolutions());
            solutionPool.AddRange(new ExchangeOperator(solution).GenerateFeasibleSolutions());
            solutionPool.AddRange(new RelocateOperator(solution).GenerateFeasibleSolutions());

            return solutionPool.OrderBy(s => s.Cost).ToList();
        }
    }
}

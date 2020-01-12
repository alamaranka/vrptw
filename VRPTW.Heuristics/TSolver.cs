﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Heuristics.LocalSearch;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class TSolver
    {
        private readonly Dataset _dataset;
        private Solution _bestSolution;

        public TSolver(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var stopwatch = Stopwatch.StartNew();
            var totalSecondsElapsed = 0.0;
            var currentSolution = new LSAlgorithm(_dataset).Run();
            var heuristicParams = Config.GetHeuristicsParam();
            var iterationCount = heuristicParams.IterationCount;
            var tabuListSize = heuristicParams.TabuSearchParam.TabuListSize;
            var numberOfNonImprovingIters = heuristicParams.DiversificationParam.NumberOfNonImprovingIters;
            var minCustomersToRemove = heuristicParams.DiversificationParam.MinCustomersToRemove;
            var maxCustomersToRemove = heuristicParams.DiversificationParam.MaxCustomersToRemove;
            var numberOfNonImprovingItersCounter = 0;

            _bestSolution = currentSolution;

            var tabuList = new Helpers.FixedSizedQueue<string>(tabuListSize);
            var stringFormOfSolution = Helpers.GetStringFormOfSolution(currentSolution);
            tabuList.Enqueue(stringFormOfSolution);

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
                Solution candidateSolution = new Solution();
                solutionPool = solutionPool.OrderBy(s => s.Cost).ToList();

                for (var s = 0; s < solutionPool.Count; s++)
                {
                    stringFormOfSolution = Helpers.GetStringFormOfSolution(solutionPool[s]);

                    if (!tabuList.Contains(stringFormOfSolution))
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
                    currentSolution = Helpers.Clone(candidateSolution);
                    tabuList.Enqueue(Helpers.GetStringFormOfSolution(candidateSolution));

                    if (currentSolution.Cost < _bestSolution.Cost)
                    {
                        _bestSolution = currentSolution;
                    }
                }
                else
                {
                    numberOfNonImprovingItersCounter++;
                }
            }

            stopwatch.Stop();

            return _bestSolution;
        }
    }
}

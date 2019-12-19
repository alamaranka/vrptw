using System;
using System.Collections.Generic;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class SASolver
    {
        private Dataset _dataset;
        

        public SASolver(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var currentSolution = new InitialSolution(_dataset).Get();
            var temperature = Config.GetSimulatedAnnealingParam().InitialTemperature;
            var alpha = Config.GetSimulatedAnnealingParam().Alpha;
            var iterationCount = Config.GetSimulatedAnnealingParam().IterationCount;

            for (var i=0; i<iterationCount; i++)
            {
                var solutionPool = new List<Solution>();
                //TODO: collect solutions from local search operators

                foreach (var candidateSolution in solutionPool)
                {
                    var currentCost = currentSolution.Cost;
                    var candidateCost = candidateSolution.Cost;
                    var rand = new Random().NextDouble();
                    var threshold = GetThreshold(currentCost, candidateCost, temperature);
                    var acceptanceCondition = candidateCost < currentCost ||
                                              (candidateCost >= currentCost && rand <= threshold);

                    if (acceptanceCondition)
                    {
                        currentSolution = Helpers.Clone(candidateSolution);
                    }
                }

                temperature *= alpha;
            }

            return null;
        }

        private double GetThreshold(double currentObj, double candidateObj, double currentTemperature)
        {
            return 1 / Math.Exp((candidateObj - currentObj) / currentTemperature);
        }
    }
}

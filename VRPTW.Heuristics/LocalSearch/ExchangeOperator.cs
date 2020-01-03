using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class ExchangeOperator
    {
        public Solution _solution;

        public ExchangeOperator(Solution solution)
        {
            _solution = solution;
        }

        public Solution ApplySwapOperator()
        {
            Console.WriteLine("Applying Exchange Operator. Initial cost: {0}", 
                              _solution.Routes.Sum(r => r.Distance));

            var improved = true;
            var iterationCount = 0;
            var numberOfActualRoutes = 0;

            while (improved)
            {
                improved = false;
                numberOfActualRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();

                for (var r1 = 0; r1 < numberOfActualRoutes - 1; r1++)
                {
                    for (var r2 = r1 + 1; r2 < numberOfActualRoutes; r2++)
                    {
                        for (var i = 1; i < _solution.Routes[r1].Customers.Count - 1; i++)
                        {
                            for (var j = 1; j < _solution.Routes[r2].Customers.Count - 1; j++)
                            {
                                var currentDistance = _solution.Routes[r1].Distance + _solution.Routes[r2].Distance;
                                var cloneOfRoute1 = Helpers.Clone(_solution.Routes[r1]);
                                var cloneOfRoute2 = Helpers.Clone(_solution.Routes[r2]);
                                var customerInRoute1 = cloneOfRoute1.Customers[i];
                                var customerInRoute2 = cloneOfRoute2.Customers[j];

                                var newRoute1 = ApplyOperator(cloneOfRoute1, customerInRoute1, customerInRoute2);
                                var newRoute2 = ApplyOperator(cloneOfRoute2, customerInRoute2, customerInRoute1);

                                if (newRoute1 != null && newRoute2 != null)
                                {
                                    if (newRoute1.Distance + newRoute2.Distance < currentDistance)
                                    {
                                        _solution.Routes[r1] = newRoute1;
                                        _solution.Routes[r2] = newRoute2;
                                        improved = true;
                                        numberOfActualRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();

                                        Console.WriteLine("Iteration number: {0}. Improved cost: {1}", 
                                                          iterationCount, _solution.Routes.Sum(r => r.Distance));
                                    }
                                }

                                iterationCount++;
                            }
                        }
                    }
                }
            }
            _solution.Cost = _solution.Routes.Sum(r => r.Distance);
            return _solution;
        }

        public List<Solution> GenerateFeasibleSolutions()
        {
            var solutionPool = new List<Solution>();
            var numberOfActualRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();

            for (var r1 = 0; r1 < numberOfActualRoutes - 1; r1++)
            {
                for (var r2 = r1 + 1; r2 < numberOfActualRoutes; r2++)
                {
                    for (var i = 1; i < _solution.Routes[r1].Customers.Count - 1; i++)
                    {
                        for (var j = 1; j < _solution.Routes[r2].Customers.Count - 1; j++)
                        {
                            var solution = Helpers.Clone(_solution);
                            var cloneOfRoute1 = Helpers.Clone(solution.Routes[r1]);
                            var cloneOfRoute2 = Helpers.Clone(solution.Routes[r2]);
                            var customerInRoute1 = cloneOfRoute1.Customers[i];
                            var customerInRoute2 = cloneOfRoute2.Customers[j];
                            var newRoute1 = ApplyOperator(cloneOfRoute1, customerInRoute1, customerInRoute2);
                            var newRoute2 = ApplyOperator(cloneOfRoute2, customerInRoute2, customerInRoute1);

                            if (newRoute1 != null && newRoute2 != null)
                            {                              
                                solution.Routes[r1] = newRoute1;
                                solution.Routes[r2] = newRoute2;
                                solution.Cost = solution.Routes.Sum(r => r.Distance);
                                solutionPool.Add(solution);
                            }
                        }
                    }
                }
            }

            return solutionPool;
        }

        private Route ApplyOperator(Route route, Customer current, Customer candidate)
        {
            var customersInNewOrder = route.Customers;
            var currentInRoute = route.Customers.Where(c => c.Id == current.Id).FirstOrDefault();
            var indexOfCurrent = route.Customers.IndexOf(currentInRoute);

            customersInNewOrder.Remove(currentInRoute);
            customersInNewOrder.Insert(indexOfCurrent, candidate);

            return Helpers.ConstructRoute(customersInNewOrder, route.Capacity);
        }
    }
}

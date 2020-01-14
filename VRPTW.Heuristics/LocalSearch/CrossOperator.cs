using System;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class CrossOperator
    {
        public Solution _solution;
        private bool _improved = true;
        private int _iterationCount = 0;

        public CrossOperator(Solution solution)
        {
            _solution = solution;
        }

        public Solution ApplyCrossOperator()
        {
            Console.WriteLine("Applying Cross Operator. Initial cost: {0}",
                              _solution.Routes.Sum(r => r.Distance));

            while (_improved)
            {
                _improved = false;
                Iterate();
            }

            _solution.Cost = _solution.Routes.Sum(r => r.Distance);

            return _solution;
        }

        private void Iterate()
        {
            var numberOfRoutes = _solution.Routes.Count();

            for (var r1 = 0; r1 < numberOfRoutes - 1; r1++)
            {
                for (var r2 = r1 + 1; r2 < numberOfRoutes; r2++)
                {
                    for (var i = 1; i < _solution.Routes[r1].Customers.Count - 2; i++)
                    {
                        for (var j = i + 1; j < _solution.Routes[r1].Customers.Count - 1; j++)
                        {
                            for (var k = 1; k < _solution.Routes[r2].Customers.Count - 2; k++)
                            {
                                for (var m = k + 1; m < _solution.Routes[r2].Customers.Count - 1; m++)
                                {
                                    var currentDistance = _solution.Routes[r1].Distance + _solution.Routes[r2].Distance;
                                    var cloneOfRoute1 = Helpers.Clone(_solution.Routes[r1]);
                                    var cloneOfRoute2 = Helpers.Clone(_solution.Routes[r2]);

                                    var newRoute1 = ApplyOperator(cloneOfRoute1, cloneOfRoute2.Customers.GetRange(k, m - k + 1), i, j);
                                    var newRoute2 = ApplyOperator(cloneOfRoute2, cloneOfRoute1.Customers.GetRange(i, j - i + 1), k, m);

                                    if (newRoute1 != null && newRoute2 != null)
                                    {
                                        if (newRoute1.Distance + newRoute2.Distance < currentDistance)
                                        {
                                            _solution.Routes[r1] = newRoute1;
                                            _solution.Routes[r2] = newRoute2;
                                            _improved = true;
                                            numberOfRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();

                                            Console.WriteLine("Iteration number: {0}. Improved cost: {1}",
                                                              _iterationCount, _solution.Routes.Sum(r => r.Distance));
                                            return;
                                        }
                                    }

                                    _iterationCount++;
                                }
                            }
                        }
                    }
                }
            }
        }

    public List<Solution> GenerateFeasibleSolutions()
        {
            var solutionPool = new List<Solution>();
            var numberOfRoutes = _solution.Routes.Count();

            for(var r1 = 0; r1 < numberOfRoutes - 1; r1++)
            {
                for (var r2 = r1 + 1; r2 < numberOfRoutes; r2++)
                {
                    for (var i = 1; i < _solution.Routes[r1].Customers.Count - 2; i++)
                    {
                        for (var j = i + 1; j < _solution.Routes[r1].Customers.Count - 1; j++)
                        {
                            for (var k = 1; k < _solution.Routes[r2].Customers.Count - 2; k++)
                            {
                                for (var m = k + 1; m < _solution.Routes[r2].Customers.Count - 1; m++)
                                {
                                    var solution = Helpers.Clone(_solution);
                                    var cloneOfRoute1 = Helpers.Clone(solution.Routes[r1]);
                                    var cloneOfRoute2 = Helpers.Clone(solution.Routes[r2]);

                                    var newRoute1 = ApplyOperator(cloneOfRoute1, cloneOfRoute2.Customers.GetRange(k, m - k + 1), i, j);
                                    var newRoute2 = ApplyOperator(cloneOfRoute2, cloneOfRoute1.Customers.GetRange(i, j - i + 1), k, m);

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
                }
            }

            return solutionPool;
        }

        private Route ApplyOperator(Route route, List<Customer> customersToInsert, int start, int end)
        {
            var customersInNewOrder = new List<Customer>();
            customersInNewOrder.AddRange(route.Customers.GetRange(0, start));
            customersInNewOrder.AddRange(customersToInsert);
            customersInNewOrder.AddRange(route.Customers.GetRange(end + 1, route.Customers.Count - end - 1));

            return Helpers.ConstructRoute(customersInNewOrder, route);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            ApplySwapOperator();
        }

        private void ApplySwapOperator()
        {
            var improved = true;

            while (improved)
            {
                improved = false;
                var numberOfActualRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();

                for (var r1 = 0; r1 < numberOfActualRoutes - 1; r1++)
                {
                    for (var r2 = r1 + 1; r2 < numberOfActualRoutes; r2++)
                    {
                        Console.WriteLine("---------- Analyzing Route {0} and Route {1} ----------", 
                                          _solution.Routes[r1].Id,
                                          _solution.Routes[r2].Id);
                        for (var i = 1; i < _solution.Routes[r1].Customers.Count - 1; i++)
                        {
                            for (var j = 1; j < _solution.Routes[r2].Customers.Count - 1; j++)
                            {
                                Console.Write("Swapping Route {0}-{1} with Route {2}-{3}: ",
                                               _solution.Routes[r1].Id, _solution.Routes[r1].Customers[i].Name,
                                               _solution.Routes[r2].Id, _solution.Routes[r2].Customers[j].Name);

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
                                        Console.Write("Sum of 2 routes distance reduced from {0} to {1}!",
                                                      Math.Round(currentDistance, 2), 
                                                      Math.Round(newRoute1.Distance + newRoute2.Distance, 2));

                                        _solution.Routes[r1] = newRoute1;
                                        _solution.Routes[r2] = newRoute2;
                                        improved = true;
                                    }
                                    else
                                    {
                                        Console.Write("Feasible, but no improvement!");
                                    }
                                } 
                                else
                                {
                                    Console.Write("Not feasible to swap!");
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
            _solution.Cost = _solution.Routes.Sum(r => r.Distance);
        }

        private Route ApplyOperator(Route route, Customer current, Customer candidate)
        {
            var currentInRoute = route.Customers.Where(c => c.Name == current.Name).FirstOrDefault();
            var indexOfCurrent = route.Customers.IndexOf(currentInRoute);

            route.Customers.Remove(currentInRoute);
            route.Customers.Insert(indexOfCurrent, candidate);

            var constructedRoute = Helpers.ConstructRoute(route);
            var isFeasible = constructedRoute.Item1;
            var newRoute = constructedRoute.Item2;

            if (isFeasible)
            {
                return newRoute;
            }

            return newRoute;
        }
    }
}

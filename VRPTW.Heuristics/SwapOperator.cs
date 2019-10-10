using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class SwapOperator
    {
        public Solution _solution;

        public SwapOperator(Solution solution)
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

                                var newRoute1 = RemoveCurrentInsertCandidate(cloneOfRoute1, cloneOfRoute1.Customers[i], cloneOfRoute2.Customers[j]);
                                var newRoute2 = RemoveCurrentInsertCandidate(cloneOfRoute2, cloneOfRoute2.Customers[j], cloneOfRoute1.Customers[i]);

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

        private Route RemoveCurrentInsertCandidate(Route route, Customer current, Customer candidate)
        {
            var currentInRoute = route.Customers.Where(c => c.Name == current.Name).FirstOrDefault();
            var indexOfCurrent = route.Customers.IndexOf(currentInRoute);

            route.Customers.Remove(currentInRoute);
            route.Customers.Insert(indexOfCurrent, candidate);

            var constructedRoute = ConstructRoute(route);
            var isFeasible = constructedRoute.Item1;
            var newRoute = constructedRoute.Item2;

            if (isFeasible)
            {
                return newRoute;
            }

            return null;
        }

        private (bool, Route) ConstructRoute(Route route)
        {
            var load = 0.0;
            var distance = 0.0;

            for (var c = 1; c < route.Customers.Count; c++)
            {
                route.Customers[c].ServiceStart = Helpers.CalculateServiceStart(route.Customers[c - 1], route.Customers[c]);
                load += route.Customers[c].Demand;
                distance += Helpers.CalculateDistance(route.Customers[c - 1], route.Customers[c]);
                
                if (!IsFeasible(route, load, c))
                {
                    return (false, null);
                }
            }

            route.Load = load;
            route.Distance = distance;

            return (true, route);
        }

        private bool IsFeasible(Route route, double load, int c)
        {
            var isCapacityExceeded = load > route.Capacity;
            var isBeforeTimeStart = route.Customers[c].ServiceStart < route.Customers[c].TimeStart;
            var isAfterTimeEnd = route.Customers[c].ServiceStart > route.Customers[c].TimeEnd;

            if (isCapacityExceeded || isBeforeTimeStart || isAfterTimeEnd)
            {
                return false;
            }

            return true;
        }
    }
}

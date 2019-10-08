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
                var numberOFActualRoutes = _solution.Routes.Select(r => r.Customers.Count > 2).Count();

                for (var r1 = 0; r1 < numberOFActualRoutes - 1; r1++)
                {
                    for (var r2 = r1 + 1; r2 < numberOFActualRoutes; r2++)
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

                                var isFeasibleToInsertInRoute1 = IsFeasibleToInsert(Helpers.Clone(_solution.Routes[r1]),
                                                                                    _solution.Routes[r1].Customers[i],
                                                                                    _solution.Routes[r2].Customers[j]);
                                var isFeasibleToInsertInRoute2 = IsFeasibleToInsert(Helpers.Clone(_solution.Routes[r2]),
                                                                                    _solution.Routes[r2].Customers[j],
                                                                                    _solution.Routes[r1].Customers[i]);

                                if (isFeasibleToInsertInRoute1 && isFeasibleToInsertInRoute2)
                                {
                                    var tempRoute1 = InsertCustomerToTheRoute(Helpers.Clone(_solution.Routes[r1]),
                                                                          _solution.Routes[r1].Customers[i - 1],
                                                                          _solution.Routes[r1].Customers[i],
                                                                          _solution.Routes[r2].Customers[j],
                                                                          _solution.Routes[r1].Customers[i + 1]);
                                    var tempRoute2 = InsertCustomerToTheRoute(Helpers.Clone(_solution.Routes[r2]),
                                                                              _solution.Routes[r2].Customers[j - 1],
                                                                              _solution.Routes[r2].Customers[j],
                                                                              _solution.Routes[r2].Customers[i],
                                                                              _solution.Routes[r2].Customers[j + 1]);

                                    if (tempRoute1.Distance + tempRoute2.Distance < currentDistance)
                                    {
                                        Console.Write("Sum of 2 routes distance reduced from {0} to {1}!",
                                                      Math.Round(currentDistance, 2), 
                                                      Math.Round(tempRoute1.Distance + tempRoute2.Distance, 2));

                                        UpdateRoute(_solution.Routes[r1], tempRoute1);
                                        UpdateRoute(_solution.Routes[r2], tempRoute2);
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

        private Route InsertCustomerToTheRoute(Route route, Customer previous, Customer current, Customer candidate, Customer next)
        {
            route.Customers.Remove(current);
            route.Distance -= current.Demand;

            route.Customers.Insert(route.Customers.IndexOf(next), candidate);

            for (var p = route.Customers.IndexOf(candidate); p < route.Customers.Count; p++)
            {
                route.Customers[p].ServiceStart = Helpers.CalculateServiceStart(route.Customers[p - 1], route.Customers[p]);
            }

            route.Load += candidate.Demand;
            route.Distance = route.Distance -
                              Helpers.CalculateDistance(previous, next) +
                              Helpers.CalculateDistance(previous, candidate) +
                              Helpers.CalculateDistance(candidate, next);
            return route;
        }

        private bool IsFeasibleToInsert(Route route, Customer current, Customer candidate)
        {
            var indexOfInsertion = route.Customers.IndexOf(current) + 1;
            route.Customers.Remove(current);
            route.Distance -= current.Demand;

            if (route.Load + candidate.Demand > route.Capacity)
            {
                return false;
            }

            route.Customers.Insert(indexOfInsertion, candidate);

            for (var p = route.Customers.IndexOf(candidate); p < route.Customers.Count; p++)
            {
                var newServiceStartTime = Helpers.CalculateServiceStart(route.Customers[p - 1], route.Customers[p]);
                var isBeforeTimeStart = newServiceStartTime < route.Customers[p].TimeStart;
                var isAfterTimeEnd = newServiceStartTime > route.Customers[p].TimeEnd;
                route.Customers[p].ServiceStart = newServiceStartTime;
                if (isBeforeTimeStart || isAfterTimeEnd)
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateRoute(Route route, Route tempRoute)
        {
            for (var c = 0; c < tempRoute.Customers.Count; c++)
            {
                route.Customers[c] = tempRoute.Customers[c];
            }
            route.Distance = tempRoute.Distance;
        }
    }
}

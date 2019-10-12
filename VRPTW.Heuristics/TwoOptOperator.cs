using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class TwoOptOperator
    {
        public Solution _solution;

        public TwoOptOperator(Solution solution)
        {
            _solution = solution;
            Apply2OptOperator();
        }

        private void Apply2OptOperator()
        {
            var numberOfActualRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();

            for (var r = 0; r < numberOfActualRoutes; r++)
            {
                var improved = true;
                Console.WriteLine("---------- Analyzing Route {0} ----------", _solution.Routes[r].Id);

                while (improved)
                {
                    improved = false;

                    for (var i = 1; i < _solution.Routes[r].Customers.Count - 2; i++)
                    {
                        for (var j = i + 1; j < _solution.Routes[r].Customers.Count - 1; j++)
                        {
                            Console.Write("Reversing {0} - {1}: ",
                                          _solution.Routes[r].Customers[i].Name, _solution.Routes[r].Customers[j].Name);

                            var currentDistance = _solution.Routes[r].Distance;
                            
                            var newRoute = ApplyOperator(Helpers.Clone(_solution.Routes[r]), i, j);

                            if (newRoute != null)
                            {
                                if (newRoute.Distance < currentDistance)
                                {
                                    Console.Write("Route distance reduced from {0} to {1}!", 
                                                  Math.Round(currentDistance, 2), Math.Round(newRoute.Distance, 2));
                                    _solution.Routes[r] = newRoute;
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
            _solution.Cost = _solution.Routes.Sum(r => r.Distance);
        }

        private Route ApplyOperator(Route route, int i, int j)
        {
            var customersInNewOrder = new List<Customer>();
            customersInNewOrder.AddRange(route.Customers.GetRange(0, i));
            customersInNewOrder.AddRange(ReverseOrder(route, i, j));
            customersInNewOrder.AddRange(route.Customers.GetRange(j + 1, route.Customers.Count - j - 1));
            route.Customers = customersInNewOrder;

            var constructedRoute = Helpers.ConstructRoute(route);
            var isFeasible = constructedRoute.Item1;
            var newRoute = constructedRoute.Item2;

            if (isFeasible)
            {
                return newRoute;
            }

            return newRoute;
        }

        private List<Customer> ReverseOrder(Route route, int i, int j)
        {
            var reversedORder = route.Customers.GetRange(i, j - i + 1);
            reversedORder.Reverse();
            return reversedORder;
        }
    }
}

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
            foreach (var route in _solution.Routes.Where(r => r.Customers.Count > 2))
            {
                var improved = true;
                Console.WriteLine("---------- Analyzing Route {0} ----------", route.Id);

                while (improved)
                {
                    improved = false;
                    for (var i = 1; i < route.Customers.Count - 2; i++)
                    {
                        for (var j = i + 1; j < route.Customers.Count - 1; j++)
                        {
                            var currentDistance = route.Distance;
                            Console.Write("Swapping {0} with {1}: ",
                                          route.Customers[i].Name, route.Customers[j].Name);
                            var tempRoute = GenerateNewRoute(Helpers.Clone(route), i, j);
                            if (tempRoute != null)
                            {
                                if (tempRoute.Distance < currentDistance)
                                {
                                    Console.Write("Route distance reduced from {0} to {1}!", 
                                                  Math.Round(currentDistance, 2), Math.Round(tempRoute.Distance, 2));
                                    UpdateRoute(route, tempRoute);
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
        
        private Route GenerateNewRoute(Route route, int i, int j)
        {
            var distance = 0.0;
            var customersInNewOrder = new List<Customer>();
            var opt = route.Customers.GetRange(i, j - i + 1);
            opt.Reverse();
            customersInNewOrder.AddRange(opt);
            customersInNewOrder.AddRange(route.Customers.GetRange(j + 1, route.Customers.Count - j - 1));

            for (var c = 0; c < i - 1; c++)
            {
                distance += Helpers.CalculateDistance(route.Customers[c], route.Customers[c + 1]);
            }

            for (var c = i; c < route.Customers.Count; c++)
            {
                var customer = customersInNewOrder[c - i];
                customer.ServiceStart = Helpers.CalculateServiceStart(route.Customers[c - 1], customer);
                if (customer.ServiceStart < customer.TimeStart || customer.ServiceStart > customer.TimeEnd)
                {
                    return null;
                }
                route.Customers[c] = customersInNewOrder[c - i];
                distance += Helpers.CalculateDistance(route.Customers[c - 1], route.Customers[c]);
            }
            route.Distance = distance;

            return route;
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

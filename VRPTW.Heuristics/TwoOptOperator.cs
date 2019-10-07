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
        private Route _route;

        public TwoOptOperator(Solution solution)
        {
            _solution = solution;
            _route = new Route();
            Apply2OptOperator();
        }

        private void Apply2OptOperator()
        {
            var routeCount = 1;
            var newRoutes = new List<Route>();

            foreach (var route in _solution.Routes.Where(r => r.Customers.Count > 2))
            {
                var improved = true;
                var clone = Helpers.Clone(route);

                while (improved)
                {
                    improved = false;
                    Console.WriteLine("Route {0} -------------", routeCount++);
                    for (var i = 1; i < route.Customers.Count - 2; i++)
                    {
                        for (var j = i + 1; j < route.Customers.Count - 1; j++)
                        {
                            var currentDistance = clone.Distance;
                            Console.Write("Swapping {0} with {1}: ", route.Customers[i].Name, route.Customers[j].Name);
                            var temp = GenerateNewRoute(clone, i, j);
                            if (temp.Item1)
                            {
                                if (clone.Distance < currentDistance)
                                {
                                    Console.Write("Route distance reduced from {0} to {1}. Net of {2} improvement!", 
                                        currentDistance, clone.Distance, currentDistance - clone.Distance);
                                    improved = true;
                                    clone = temp.Item2;
                                }
                                else
                                {
                                    Console.Write("Feasible but no improvement!");
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

                newRoutes.Add(clone);
            }

            for (var i = 0; i < newRoutes.Count; i++)
            {
                _solution.Routes[i] = newRoutes[i];
            }
        }
        
        private (bool, Route) GenerateNewRoute(Route route, int i, int j)
        {
            var clone = Helpers.Clone(route);
            var distance = 0.0;
            var customersInNewOrder = new List<Customer>();
            var opt = clone.Customers.GetRange(i, j - i + 1);
            opt.Reverse();
            customersInNewOrder.AddRange(opt);
            customersInNewOrder.AddRange(clone.Customers.GetRange(j + 1, clone.Customers.Count - j - 1));

            for (var x = 0; x < i - 1; x++)
            {
                distance += Helpers.CalculateDistance(clone.Customers[x], clone.Customers[x + 1]);
            }

            var count = 0;
            for (var x = i; x < clone.Customers.Count; x++)
            {
                var customer = customersInNewOrder[count++];
                customer.ServiceStart = Helpers.CalculateServiceStart(clone.Customers[x - 1], customer);
                if (customer.ServiceStart < customer.TimeStart || customer.ServiceStart > customer.TimeEnd)
                {
                    return (false, null);
                }
                clone.Customers[x] = customersInNewOrder[x - i];
                distance += Helpers.CalculateDistance(clone.Customers[x - 1], clone.Customers[x]);
            }
            clone.Distance = distance;
            return (true, clone);
        }
    }
}

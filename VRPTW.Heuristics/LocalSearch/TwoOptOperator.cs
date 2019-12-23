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
        }

        public void Apply2OptOperator()
        {
            var numberOfActualRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();

            Console.WriteLine("Applying 2-Opt Operator" + new string('.', 10));

            for (var r = 0; r < numberOfActualRoutes; r++)
            {
                var improved = true;

                while (improved)
                {
                    improved = false;

                    for (var i = 1; i < _solution.Routes[r].Customers.Count - 2; i++)
                    {
                        for (var j = i + 1; j < _solution.Routes[r].Customers.Count - 1; j++)
                        {
                            var currentDistance = _solution.Routes[r].Distance;                            
                            var newRoute = ApplyOperator(Helpers.Clone(_solution.Routes[r]), i, j);

                            if (newRoute != null)
                            {
                                if (newRoute.Distance < currentDistance)
                                {
                                    Console.WriteLine("Distance of Route{0} reduced from {1} to {2} as a result of {3}<->{4}.", 
                                                      r, Math.Round(currentDistance, 2), Math.Round(newRoute.Distance, 2), i, j);
                                    _solution.Routes[r] = newRoute;
                                    improved = true;
                                }
                            }
                        }
                    }
                }
            }
            _solution.Cost = _solution.Routes.Sum(r => r.Distance);
        }

        public List<Solution> GenerateFeasibleSolutions()
        {
            var solutionPool = new List<Solution>();
            var numberOfActualRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();

            for (var r = 0; r < numberOfActualRoutes; r++)
            {
                for (var i = 1; i < _solution.Routes[r].Customers.Count - 2; i++)
                {
                    for (var j = i + 1; j < _solution.Routes[r].Customers.Count - 1; j++)
                    {
                        var solution = Helpers.Clone(_solution);
                        var newRoute = ApplyOperator(Helpers.Clone(solution.Routes[r]), i, j);

                        if (newRoute != null)
                        {
                            solution.Routes[r] = newRoute;
                            solution.Cost = solution.Routes.Sum(r => r.Distance);
                            solutionPool.Add(solution);
                        }
                    }
                }              
            }

            return solutionPool;
        }

        private Route ApplyOperator(Route route, int i, int j)
        {
            var customersInNewOrder = new List<Customer>();
            customersInNewOrder.AddRange(route.Customers.GetRange(0, i));
            customersInNewOrder.AddRange(ReverseOrder(route, i, j));
            customersInNewOrder.AddRange(route.Customers.GetRange(j + 1, route.Customers.Count - j - 1));

            return Helpers.ConstructRoute(customersInNewOrder, route.Capacity);
        }

        private List<Customer> ReverseOrder(Route route, int i, int j)
        {
            var reversedORder = route.Customers.GetRange(i, j - i + 1);
            reversedORder.Reverse();
            return reversedORder;
        }
    }
}

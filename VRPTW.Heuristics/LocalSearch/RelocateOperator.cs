using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics.LocalSearch
{
    class RelocateOperator
    {
        public Solution _solution;
        private bool _improved = true;
        private int _iterationCount = 0;

        public RelocateOperator(Solution solution)
        {
            _solution = solution;
        }

        public Solution ApplyRelocateOperator()
        {
            Console.WriteLine("Applying Relocate Operator. Initial cost: {0}",
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
                    for (var i = 1; i < _solution.Routes[r1].Customers.Count - 1; i++)
                    {
                        for (var j = 1; j < _solution.Routes[r2].Customers.Count - 1; j++)
                        {
                            var currentDistance = _solution.Routes[r1].Distance + _solution.Routes[r2].Distance;
                            var cloneOfRoute1 = Helpers.Clone(_solution.Routes[r1]);
                            var cloneOfRoute2 = Helpers.Clone(_solution.Routes[r2]);
                            var customerInRoute1 = cloneOfRoute1.Customers[i];
                            var customerInRoute2 = cloneOfRoute2.Customers[j];

                            var newRoute1 = ApplyOperatorRemove(cloneOfRoute1, customerInRoute1);
                            var newRoute2 = ApplyOperatorInsert(cloneOfRoute2, customerInRoute2, customerInRoute1);

                            if (newRoute1 != null && newRoute2 != null)
                            {
                                if (newRoute1.Distance + newRoute2.Distance < currentDistance)
                                {
                                    _solution.Routes[r1] = newRoute1;
                                    _solution.Routes[r2] = newRoute2;
                                    _improved = true;
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

        public List<Solution> GenerateFeasibleSolutions()
        {
            var solutionPool = new List<Solution>();
            var numberOfRoutes = _solution.Routes.Count();

            for (var r1 = 0; r1 < numberOfRoutes - 1; r1++)
            {
                for (var r2 = r1 + 1; r2 < numberOfRoutes; r2++)
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
                            var newRoute1 = ApplyOperatorRemove(cloneOfRoute1, customerInRoute1);
                            var newRoute2 = ApplyOperatorInsert(cloneOfRoute2, customerInRoute2, customerInRoute1);

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

        private Route ApplyOperatorRemove(Route route, Customer current)
        {
            var customersInNewOrder = route.Customers;
            var currentInRoute = route.Customers.Where(c => c.Id == current.Id).FirstOrDefault();
            var indexOfCurrent = route.Customers.IndexOf(currentInRoute);
            customersInNewOrder.Remove(currentInRoute);

            return Helpers.ConstructRoute(customersInNewOrder, route);
        }

        private Route ApplyOperatorInsert(Route route, Customer current, Customer candidate)
        {
            var customersInNewOrder = route.Customers;
            var currentInRoute = route.Customers.Where(c => c.Id == current.Id).FirstOrDefault();
            var indexOfCurrent = route.Customers.IndexOf(currentInRoute);
            customersInNewOrder.Insert(indexOfCurrent, candidate);

            return Helpers.ConstructRoute(customersInNewOrder, route);
        }
    }
}

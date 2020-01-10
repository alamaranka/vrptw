using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class Diversifier
    {
        private Solution _solution;
        private readonly int _dMin;
        private readonly int _dMax;

        public Diversifier(Solution solution, int dMin, int dMax)
        {
            _solution = solution;
            _dMin = dMin;
            _dMax = dMax;
        }

        public Solution Diverisfy()
        {
            var allCustomers = _solution.Routes
                .SelectMany(c => c.Customers)
                .Where(c => !c.IsDepot)
                .GroupBy(i => i.Id)
                .Select(g => g.FirstOrDefault())
                .ToList();

            var numberOfCustomersToRemove = new Random().Next(_dMin, _dMax);
            var removedCustomers = new List<Customer>();

            for (var i = 0; i < numberOfCustomersToRemove; i++)
            {
                var randIndex = new Random().Next(allCustomers.Count);

                if (!removedCustomers.Contains(allCustomers[randIndex]))
                {
                    var removedCustomer = allCustomers[randIndex];
                    allCustomers.Remove(removedCustomer);
                    removedCustomers.Add(removedCustomer);
                    _solution.Routes.Where(r => r.Customers.Contains(removedCustomer)).ToList()
                        .FirstOrDefault().Load -= removedCustomer.Demand; 
                    _solution.Routes.Where(r => r.Customers.Contains(removedCustomer)).ToList()
                        .FirstOrDefault().Customers.Remove(removedCustomer);
                }
                else
                {
                    i--;
                }
            }
            return ReInsertRemovedCustomers(removedCustomers);
        }

        private Solution ReInsertRemovedCustomers(List<Customer> customers)
        {
            var numberOfActualRoutes = _solution.Routes.Where(r => r.Customers.Count > 2).Count();
            var solution = Helpers.Clone(_solution);

            while (customers.Count > 0)
            {
                for (var r = 0; r < numberOfActualRoutes; r++)
                {
                    var route = Helpers.Clone(solution.Routes[r]);

                    for (var p = 1; p < route.Customers.Count; p++)
                    {
                        var previous = route.Customers[p - 1];
                        var next = route.Customers[p];
                        var feasibleCustomersToInsert = new List<Customer>();
                        var insertionValueOfFeasibleCustomers = new List<double>();
                        foreach (var candidate in customers)
                        {
                            if (Helpers.IsFeasibleToInsert(route, candidate, next))
                            {
                                feasibleCustomersToInsert.Add(candidate);
                                insertionValueOfFeasibleCustomers
                                    .Add(Helpers.InsertionValueOfCustomer(previous, candidate, next, route.Customers[0]));
                            }
                        }
                        if (feasibleCustomersToInsert.Count > 0)
                        {
                            var indexOfBestFeasibleCustomer = insertionValueOfFeasibleCustomers
                                                                    .IndexOf(insertionValueOfFeasibleCustomers.Max());
                            var bestCustomerToInsert = feasibleCustomersToInsert[indexOfBestFeasibleCustomer];
                            route = Helpers.InsertCustomerToTheRoute(route, bestCustomerToInsert, next);
                            customers.Remove(bestCustomerToInsert);
                        }
                    }

                    solution.Routes[r] = route;
                    solution.Cost = solution.Routes.Sum(r => r.Distance);
                }
            }
            return solution;
        }
    }
}
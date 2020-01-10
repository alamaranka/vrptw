using System.Collections.Generic;
using System.Linq;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class InsertionHeuristics
    {
        private readonly Customer _depot;
        private readonly List<Customer> _candidateCustomers;
        private readonly double _routeCapacity;
        public Route _route;

        public InsertionHeuristics(Customer depot, List<Customer> unRoutedCustomers, double routeCapacity)
        {
            _depot = depot;
            _candidateCustomers = unRoutedCustomers;
            _routeCapacity = routeCapacity;
        }

        public Route Generate()
        {
            InitializeRoute();

            while (_candidateCustomers.Count > 0)
            {
                var anyFeasibleCustomer = false;

                for (var p = 1; p < _route.Customers.Count; p++)
                {
                    var previous = _route.Customers[p - 1];
                    var next = _route.Customers[p];
                    var feasibleCustomersToInsert = new List<Customer>();
                    var insertionValueOfFeasibleCustomers = new List<double>();
                    foreach (var candidate in _candidateCustomers)
                    {
                        if (Helpers.IsFeasibleToInsert(_route, candidate, next))
                        {
                            feasibleCustomersToInsert.Add(candidate);
                            insertionValueOfFeasibleCustomers
                                .Add(Helpers.InsertionValueOfCustomer(previous, candidate, next, _depot));
                        }
                    }
                    if (feasibleCustomersToInsert.Count > 0)
                    {
                        var indexOfBestFeasibleCustomer = insertionValueOfFeasibleCustomers
                                                                .IndexOf(insertionValueOfFeasibleCustomers.Max());
                        var bestCustomerToInsert = feasibleCustomersToInsert[indexOfBestFeasibleCustomer];
                        _route = Helpers.InsertCustomerToTheRoute(_route, bestCustomerToInsert, next);
                        _candidateCustomers.Remove(bestCustomerToInsert);
                        anyFeasibleCustomer = true;
                    }
                }
                if (!anyFeasibleCustomer) 
                {
                    break; 
                }
            }
            return _route;
        }

        private void InitializeRoute()
        {
            _route = new Route()
            {
                Customers = new List<Customer>() { _depot, Helpers.Clone(_depot) },
                Load = 0.0,
                Distance = 0.0,
                Capacity = _routeCapacity
            };
            _route = Helpers.InsertCustomerToTheRoute(_route, GetSeedCustomer(), _route.Customers[1]);
        }

        private Customer GetSeedCustomer()
        {
            var maxDistance = 0.0;
            var seedCustomer = new Customer();
            foreach (var customer in _candidateCustomers)
            {
                var distance = Helpers.CalculateDistance(_depot, customer);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    seedCustomer = customer;
                }
            }
            _candidateCustomers.Remove(seedCustomer);
            return seedCustomer;
        }
    }
}

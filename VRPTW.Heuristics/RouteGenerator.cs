using System;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class RouteGenerator
    {
        private Customer _depot;
        private List<Customer> _candidateCustomers;
        private double _routeMaxCapacity;
        public Route _route;

        public RouteGenerator(Customer depot, List<Customer> unRoutedCustomers, double routeMaxCapacity)
        {
            _depot = depot;
            _candidateCustomers = unRoutedCustomers;
            _routeMaxCapacity = routeMaxCapacity;
            Generate();
        }

        public void Generate()
        {
            InitializeRoute();

            while (_candidateCustomers.Count > 0)
            {
                var anyFeasibleCustomer = false;

                for (var p = 1; p < _route.Customers.Count; p++)
                {
                    var feasibleCustomersToInsert = new List<Customer>();
                    var insertionValueOfFeasibleCustomers = new List<double>();
                    foreach (var u in _candidateCustomers)
                    {
                        if (IsFeasibleToInsert(p, u)) 
                        {
                            feasibleCustomersToInsert.Add(u);
                            insertionValueOfFeasibleCustomers
                                .Add(InsertionValueOfCustomer(_route.Customers[p - 1], u, _route.Customers[p])); 
                        }
                    }
                    if (feasibleCustomersToInsert.Count > 0) 
                    {
                        var indexOfBestFeasibleCustomer = insertionValueOfFeasibleCustomers.IndexOf(insertionValueOfFeasibleCustomers.Max());
                        var bestCustomerToInsert = feasibleCustomersToInsert[indexOfBestFeasibleCustomer];
                        InsertCustomerToTheRoute(_route.Customers[p - 1], bestCustomerToInsert, _route.Customers[p]);
                        anyFeasibleCustomer = true;
                    }
                }
                if (!anyFeasibleCustomer) { break; }
            }
        }

        private void InitializeRoute()
        {
            _route = new Route()
            {
                Customers = new List<Customer>() { _depot, CloneDepot() },
                Capacity = 0.0,
                Distance = 0.0
            };
            InsertCustomerToTheRoute(_route.Customers[0], GetSeedCustomer(), _route.Customers[1]);
        }

        private Customer CloneDepot()
        {
            return new Customer()
            {
                Name = _depot.Name,
                Latitude = _depot.Latitude,
                Longitude = _depot.Longitude,
                Demand = _depot.Demand,
                TimeStart = _depot.TimeStart,
                TimeEnd = _depot.TimeEnd,
                ServiceTime = _depot.ServiceTime
            };
        }

        private void InsertCustomerToTheRoute(Customer previous, Customer candidate, Customer next)
        {
            _route.Customers.Insert(_route.Customers.IndexOf(previous) + 1, candidate);
            for (var p = _route.Customers.IndexOf(candidate); p < _route.Customers.Count; p++)
            {
                _route.Customers[p].ServiceStart = CalculateServiceStart(_route.Customers[p - 1], _route.Customers[p]);
            }
            _route.Capacity += candidate.Demand;
            _route.Distance = _route.Distance -
                                DistanceCalculator.Calculate(previous, next) +
                                DistanceCalculator.Calculate(previous, candidate) +
                                DistanceCalculator.Calculate(candidate, next);
            Console.WriteLine("Customer {0} inserted to the route between {1} and {2}", candidate.Name, previous.Name, next.Name);
            _candidateCustomers.Remove(candidate);
        }
        
        private bool IsFeasibleToInsert(int insertionIndex, Customer candidate)
        {
            if (_route.Capacity + candidate.Demand > _routeMaxCapacity)
            {
                return false;
            }

            _route.Customers.Insert(insertionIndex, candidate);
            for (var p = insertionIndex; p < _route.Customers.Count; p++)
            {
                var serviceStart = CalculateServiceStart(_route.Customers[p-1], _route.Customers[p]);
                _route.Customers[p].ServiceStart = serviceStart;
                if (serviceStart < _route.Customers[p].TimeStart || serviceStart > _route.Customers[p].TimeEnd)
                {
                    _route.Customers.Remove(candidate);
                    return false;
                }
            }
            _route.Customers.Remove(candidate);
            return true;
        }

        private double CalculateServiceStart(Customer previous, Customer candidate)
        {
            return Math.Max(previous.ServiceStart +
                                   previous.ServiceTime +
                                   DistanceCalculator.Calculate(previous, candidate),
                                   candidate.TimeStart);
        }

        private double InsertionValueOfCustomer(Customer previous, Customer candidate, Customer next)
        {
            var alpha1 = Config.GetHeuristicsParam().InitialSolutionParam.Alpha1;
            var alpha2 = Config.GetHeuristicsParam().InitialSolutionParam.Alpha2;
            var mu = Config.GetHeuristicsParam().InitialSolutionParam.Mu;
            var lambda = Config.GetHeuristicsParam().InitialSolutionParam.Lambda;
            var c11 = DistanceCalculator.Calculate(previous, candidate) +
                      DistanceCalculator.Calculate(candidate, next) -
                      DistanceCalculator.Calculate(previous, next) * mu;
            var candidateStartTime = Math.Max(previous.ServiceStart +
                                     previous.ServiceTime +
                                     DistanceCalculator.Calculate(previous, candidate),
                                     candidate.TimeStart);
            var c12 = Math.Max(candidateStartTime +
                      candidate.ServiceTime +
                      DistanceCalculator.Calculate(candidate, next),
                      candidate.TimeStart) -
                      next.ServiceStart;
            var c1 = alpha1 * c11 + alpha2 * c12;
            return lambda * DistanceCalculator.Calculate(_depot, candidate) - c1;
        }

        private Customer GetSeedCustomer()
        {
            var maxDistance = 0.0;
            var seedCustomer = new Customer();
            foreach (var customer in _candidateCustomers)
            {
                var distance = DistanceCalculator.Calculate(_depot, customer);
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

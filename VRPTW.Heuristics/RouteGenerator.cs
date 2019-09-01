using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class RouteGenerator
    {
        private Customer _depot;
        private List<Customer> _unRoutedCustomers;
        private double _routeMaxCapacity;
        public Route _route;

        public RouteGenerator(Customer depot, List<Customer> unRoutedCustomers, double routeMaxCapacity)
        {
            _depot = depot;
            _unRoutedCustomers = unRoutedCustomers;
            _routeMaxCapacity = routeMaxCapacity;
            Generate();
        }

        public void Generate()
        {
            InitializeRoute();
            while (_route.Capacity < _routeMaxCapacity)
            {
                for (var p = 1; p < _route.Customers.Count; p++)
                {
                    foreach (var u in _unRoutedCustomers)
                    {
                        Console.WriteLine("Value of Customer {0}: {1}",
                                          u.Name,  
                                          InsertionValueOfCustomer(_route.Customers[p - 1], u, _route.Customers[p]));
                    }
                }
            }
        }

        private void InitializeRoute()
        {
            _route = new Route()
            {
                Customers = new List<Customer>() { _depot },
                Capacity = 0.0,
                Distance = 0.0
            };
            AddCustomerToTheRoute(GetSeedCustomer());
            AddCustomerToTheRoute(MakeACopyOfDepot());
        }

        private Customer MakeACopyOfDepot()
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

        private void AddCustomerToTheRoute(Customer customer)
        {
            var previousCustomer = _route.Customers.Last();
            customer.ServiceStart = Math.Max(previousCustomer.ServiceStart +
                                    previousCustomer.ServiceTime +
                                    DistanceCalculator.Calculate(previousCustomer, customer),
                                    customer.TimeStart);
            _route.Customers.Add(customer);
            _route.Capacity += customer.Demand;
            _route.Distance += DistanceCalculator.Calculate(previousCustomer, customer);
        }

        private double InsertionValueOfCustomer(Customer previous, Customer candidate, Customer next)
        {
            if (!IsFeasibleToInsert()) { return -Double.MaxValue; }
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

        private bool IsFeasibleToInsert()
        {
            return true;
        }

        private Customer GetSeedCustomer()
        {
            var maxDistance = 0.0;
            var seedCustomer = new Customer();
            foreach (var customer in _unRoutedCustomers)
            {
                var distance = DistanceCalculator.Calculate(_depot, customer);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    seedCustomer = customer;
                }
            }
            _unRoutedCustomers.Remove(seedCustomer);
            return seedCustomer;
        }
    }
}

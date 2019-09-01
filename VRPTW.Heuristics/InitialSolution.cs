using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Data;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class InitialSolution
    {
        private Dataset _dataset;
        private Customer _depot;
        private List<Vehicle> _vehicles;
        private List<Customer> _customers;
        private List<Customer> _unRoutedCustomers;

        public List<Route> Routes { get; set; }
        public double Cost { get; set; }
        private InitialSolution() { }

        public InitialSolution(Dataset dataset)
        {
            _dataset = dataset;
        }

        private void SetInputData()
        {
            _customers = _dataset.Vertices;
            _vehicles = _dataset.Vehicles;
            _depot = _customers[0];
            _customers.RemoveAt(0);
            _unRoutedCustomers = _customers.ToList();
        }

        public InitialSolution Get()
        {
            SetInputData();
            var routes = new List<Route>();
            while (_unRoutedCustomers.Count > 0)
            {
                routes.Add(new RouteGenerator(_unRoutedCustomers).Generate());
            }
            return new InitialSolution() {
                Routes = routes,
                Cost = CalculateCost(routes)
            };
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

        private double CalculateCost(List<Route> routes)
        {
            return 0.0;
        }
    }
}

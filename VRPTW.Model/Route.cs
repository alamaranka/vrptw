using System;
using System.Collections.Generic;

namespace VRPTW.Model
{
    public class Route : IClonable<Route>
    {
        public int Id { get; set; }
        public Vehicle Vehicle { get; set; }
        public double Capacity { get; set; }
        public List<Customer> Customers { get; set; }
        public double Load { get; set; }
        public double Distance { get; set; }

        public Route Clone()
        {
            var route = new Route
            {
                Id = Id,
                Vehicle = Vehicle,
                Capacity = Capacity,
                Customers = new List<Customer>(),
                Load = Load,
                Distance = Distance
            };

            foreach (var customer in Customers)
            {
                route.Customers.Add(customer.Clone());
            }

            return route;
        }
    }
}

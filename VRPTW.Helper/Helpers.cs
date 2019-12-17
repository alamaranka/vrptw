using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using VRPTW.Model;

namespace VRPTW.Helper
{
    public static class Helpers
    {
        public static double CalculateDistance(Customer cStart, Customer cEnd)
        {
            double term1 = Math.Pow(cStart.Latitude - cEnd.Latitude, 2);
            double term2 = Math.Pow(cStart.Longitude - cEnd.Longitude, 2);
            return Math.Sqrt(term1 + term2);
        }

        [DebuggerStepThrough]
        public static T Clone<T> (T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            System.Runtime.Serialization.IFormatter formatter = 
                            new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static double[,,] ExtractVehicleTraverseFromSolution(Solution solution, int vehicleCount, int verticesCount)
        {
            var vehicleTraverse = new double[vehicleCount, verticesCount + 1, verticesCount + 1];

            for (int r = 0; r < solution.Routes.Count; r++)
            {
                for (int c = 0; c < solution.Routes[r].Customers.Count - 1; c++)
                {
                    int start = solution.Routes[r].Customers[c].Id;
                    int end = solution.Routes[r].Customers[c + 1].Id;
                    vehicleTraverse[r, start, end] = 1.0;
                }
            }
            return vehicleTraverse;
        }

        public static double CalculateServiceStart(Customer previous, Customer next)
        {
            return Math.Max(next.TimeStart,
                   previous.ServiceStart +
                   previous.ServiceTime +
                   Helpers.CalculateDistance(previous, next));
        }

        public static Route ConstructRoute(List<Customer> customers, double capacity)
        {
            var route = new Route()
            {
                Customers = new List<Customer>{ customers[0] },
                Load = 0.0,
                Distance = 0.0,
                Capacity = capacity
            };

            for (var c = 1; c < customers.Count; c++)
            {
                route.Customers.Add(customers[c]);
                route.Customers[c].ServiceStart = CalculateServiceStart(route.Customers[c - 1], route.Customers[c]);
                route.Load += route.Customers[c].Demand;
                route.Distance += CalculateDistance(route.Customers[c - 1], route.Customers[c]);

                if (!IsFeasible(customers[c], route.Load, route.Capacity))
                {
                    return null;
                }
            }

            return route;
        }

        public static bool IsFeasible(Customer customer, double load, double capacity)
        {
            var isCapacityExceeded = load > capacity;
            var isAfterTimeEnd = customer.ServiceStart > customer.TimeEnd;

            if (isCapacityExceeded || isAfterTimeEnd)
            {
                return false;
            }

            return true;
        }
    }
}

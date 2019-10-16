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
                    int start = solution.Routes[r].Customers[c].Name;
                    int end = solution.Routes[r].Customers[c + 1].Name;
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

        public static (bool, Route) ConstructRoute(Route route)
        {
            var load = 0.0;
            var distance = 0.0;

            for (var c = 1; c < route.Customers.Count; c++)
            {
                route.Customers[c].ServiceStart = Helpers.CalculateServiceStart(route.Customers[c - 1], route.Customers[c]);
                load += route.Customers[c].Demand;
                distance += Helpers.CalculateDistance(route.Customers[c - 1], route.Customers[c]);

                if (!IsFeasible(route, load, c))
                {
                    return (false, null);
                }
            }

            route.Load = load;
            route.Distance = distance;

            return (true, route);
        }

        public static bool IsFeasible(Route route, double load, int c)
        {
            var isCapacityExceeded = load > route.Capacity;
            var isAfterTimeEnd = route.Customers[c].ServiceStart > route.Customers[c].TimeEnd;

            if (isCapacityExceeded || isAfterTimeEnd)
            {
                return false;
            }

            return true;
        }
    }
}

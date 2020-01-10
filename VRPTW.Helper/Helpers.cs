using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VRPTW.Configuration;
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

        //[DebuggerStepThrough]
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
                   CalculateDistance(previous, next));
        }

        public static Route ConstructRoute(List<Customer> customers, Route route)
        {
            var constructedRoute = new Route()
            {
                Id = route.Id,
                Customers = new List<Customer>{ customers[0] },
                Load = 0.0,
                Distance = 0.0,
                Capacity = route.Capacity
            };

            for (var c = 1; c < customers.Count; c++)
            {
                constructedRoute.Customers.Add(customers[c]);
                constructedRoute.Customers[c].ServiceStart = CalculateServiceStart(constructedRoute.Customers[c - 1], constructedRoute.Customers[c]);
                constructedRoute.Customers[c].RouteId = constructedRoute.Id;
                constructedRoute.Load += constructedRoute.Customers[c].Demand;
                constructedRoute.Distance += CalculateDistance(constructedRoute.Customers[c - 1], constructedRoute.Customers[c]);

                if (!IsFeasible(customers[c], constructedRoute.Load, constructedRoute.Capacity))
                {
                    return null;
                }
            }

            return constructedRoute;
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

        public static Solution GetRandomNeighbour(List<Solution> solutionPool)
        {
            var rand = new Random();
            var rIndex = rand.Next(solutionPool.Count);
            return solutionPool[rIndex];
        }

        public static Solution GetBestNeighbour(List<Solution> solutionPool)
        {
            return solutionPool.OrderByDescending(s => -s.Cost).First();
        }

        public static bool IsFeasibleToInsert(Route route, Customer candidate, Customer beforeCustomer)
        {
            if (route.Load + candidate.Demand > route.Capacity)
            {
                return false;
            }

            var cloneRoute = Clone(route);
            var customersInNewOrder = cloneRoute.Customers;
            customersInNewOrder.Insert(route.Customers.IndexOf(beforeCustomer), candidate);
            var constructedRoute = ConstructRoute(customersInNewOrder, cloneRoute);

            if (constructedRoute == null)
            {
                return false;
            }

            return true;
        }

        public static double InsertionValueOfCustomer(Customer previous, Customer candidate, Customer next, Customer depot)
        {
            var alpha1 = Config.GetHeuristicsParam().InitialSolutionParam.Alpha1;
            var alpha2 = Config.GetHeuristicsParam().InitialSolutionParam.Alpha2;
            var mu = Config.GetHeuristicsParam().InitialSolutionParam.Mu;
            var lambda = Config.GetHeuristicsParam().InitialSolutionParam.Lambda;
            var c11 = CalculateDistance(previous, candidate) +
                      CalculateDistance(candidate, next) -
                      CalculateDistance(previous, next) * mu;
            var candidateStartTime = Math.Max(previous.ServiceStart +
                                     previous.ServiceTime +
                                     CalculateDistance(previous, candidate),
                                     candidate.TimeStart);
            var c12 = Math.Max(candidateStartTime +
                      candidate.ServiceTime +
                      CalculateDistance(candidate, next),
                      candidate.TimeStart) -
                      next.ServiceStart;
            var c1 = alpha1 * c11 + alpha2 * c12;

            return lambda * CalculateDistance(depot, candidate) - c1;
        }

        public static Route InsertCustomerToTheRoute(Route route, Customer candidate, Customer next)
        {
            var cloneRoute = Clone(route);
            var customersInNewOrder = cloneRoute.Customers;
            customersInNewOrder.Insert(route.Customers.IndexOf(next), candidate);

            return ConstructRoute(customersInNewOrder, cloneRoute);
        }

        public static double GetThresholdForAcceptance(double currentObj, double candidateObj, double currentTemperature)
        {
            return 1 / Math.Exp((candidateObj - currentObj) / currentTemperature);
        }
    }
}

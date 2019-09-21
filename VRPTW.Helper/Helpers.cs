using System;
using System.Collections.Generic;
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

        public static double[,,] ExtractVehicleTraverseFromSolution(Solution solution, 
                                                                    List<Vehicle> vehicles, List<Customer> vertices)
        {
            var vehicleTraverse = new double[vehicles.Count, vertices.Count, vertices.Count];

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
    }
}

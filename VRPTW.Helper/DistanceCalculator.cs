using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Model;

namespace VRPTW.Helper
{
    public static class DistanceCalculator
    {
        public static double Calculate(Customer cStart, Customer cEnd)
        {
            double term1 = Math.Pow(cStart.Latitude - cEnd.Latitude, 2);
            double term2 = Math.Pow(cStart.Longitude - cEnd.Longitude, 2);
            return Math.Sqrt(term1 + term2);
        }
    }
}
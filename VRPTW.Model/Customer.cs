using System;

namespace VRPTW.Data
{
    [Serializable]
    public class Customer
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Demand { get; set; }
        public int TimeStart { get; set; }
        public int TimeEnd { get; set; }
        public double ServiceTime { get; set; }
        public double ServiceStart { get; set; }
        public bool IsDepot { get; set; }
    }
}
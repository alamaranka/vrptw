﻿using System;

namespace VRPTW.Model
{
    [Serializable]
    public class Customer
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public int Demand { get; set; }
        public int TimeStart { get; set; }
        public int TimeEnd { get; set; }
        public int ServiceTime { get; set; }
        public double ServiceStart { get; set; }
        public bool IsDepot { get; set; }
    }
}
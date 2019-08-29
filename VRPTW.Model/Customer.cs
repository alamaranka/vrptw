using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.Model
{
    public class Customer
    {
        public String Name { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public int Demand { get; set; }
        public int TimeStart { get; set; }
        public int TimeEnd { get; set; }
        public int ServiceTime { get; set; }
    }
}
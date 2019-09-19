using System;
using System.Collections.Generic;

namespace VRPTW.Model
{
    [Serializable]
    public class Dataset
    {
        public List<Customer> Vertices { get; set; }
        public List<Vehicle> Vehicles { get; set; }
    }
}

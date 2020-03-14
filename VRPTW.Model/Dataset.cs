using System;
using System.Collections.Generic;
using VRPTW.DTO;

namespace VRPTW.Data
{
    [Serializable]
    public class Dataset
    {
        public List<Customer> Vertices { get; set; }
        public List<Vehicle> Vehicles { get; set; }
    }
}

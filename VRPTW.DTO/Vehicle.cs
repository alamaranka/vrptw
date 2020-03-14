using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.DTO
{
    public class Vehicle
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string NOM { get; set; }
        public double Capacity { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.DTO
{
    public class Location
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public DateTime Openning { get; set; }
        public DateTime Closing { get; set; }
        public string Resp { get; set; }
    }
}

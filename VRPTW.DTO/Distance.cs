using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.DTO
{
    public class Distance
    {
        public int LocationCode1 { get; set; }
        public int LocationCode2 { get; set; }
        public double Amount { get; set; }
        public double Speed { get; set; }
    }
}

using System;

namespace VRPTW.DTO
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime Hour { get; set; }
        public int Location { get; set; }
        public string LocationType { get; set; }
        public int ServiceType { get; set; }
        public int NOM { get; set; }
        public string Note { get; set; }
        public double Amount { get; set; }
        public double ServiceDuration { get; set; }
        public string Employee { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Penalty { get; set; }
    }
}

namespace VRPTW.Model
{
    public class Customer
    {
        public string Name { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public int Demand { get; set; }
        public int TimeStart { get; set; }
        public int TimeEnd { get; set; }
        public int ServiceTime { get; set; }
        public double ServiceStart { get; set; }
        public Customer Clone() { return this.MemberwiseClone() as Customer; }
    }
}
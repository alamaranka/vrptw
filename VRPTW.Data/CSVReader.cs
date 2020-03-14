using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.DTO;
using VRPTW.Helper;
using VRPTW.Data;

namespace VRPTW.Data
{
    public class CSVReader
    {
        private List<Order> _orders;
        private List<DTO.Vehicle> _vehicles;
        private List<Location> _locations;
        private List<Distance> _distances;
        
        public CSVReader()
        {
            _orders = PrepareOrders();
            _vehicles = PrepareVehicles();
            _locations = PrepareLocations();
        }

        public List<Customer> GetVertices()
        {
            var vertices = new List<Customer>();

            var depotDTO = _locations.Where(l => l.Type.Equals("NOM")).SingleOrDefault();

            var depot = new Customer()
            {
                Id = depotDTO.Id,
                Latitude = depotDTO.Lat,
                Longitude = depotDTO.Lon,
                TimeStart = depotDTO.Openning.Hour,
                TimeEnd = depotDTO.Closing.Hour,
                IsDepot = true
            };

            vertices.Add(depot);

            foreach (var order in _orders)
            {
                var location = _locations.Where(l => l.Id == order.Location).SingleOrDefault();

                var customer = new Customer()
                {
                    Id = location.Id,
                    Latitude = location.Lat,
                    Longitude = location.Lon,
                    Demand = order.Amount,
                    ServiceTime = order.ServiceDuration / 60.0,
                    TimeStart = order.StartTime.Hour,
                    TimeEnd = order.EndTime.Hour,
                    IsDepot = false
                };

                vertices.Add(customer);
            }

            var depotClone = Helpers.Clone(depot);
            depotClone.TimeEnd = _vehicles.SingleOrDefault()
                                          .EndTime.Hour;

            vertices.Add(depotClone);

            return vertices;
        }

        public List<Data.Vehicle> GetVehicles()
        {
            var vehicles = new List<Data.Vehicle>();

            var vehiclesDTO = PrepareVehicles();

            foreach (var vehicleDTO in vehiclesDTO)
            {
                var vehicle = new Data.Vehicle()
                {
                    Capacity = vehicleDTO.Capacity
                };

                vehicles.Add(vehicle);
            }
            
            return vehicles;
        }

        private List<Order> PrepareOrders()
        {
            var orders = new List<Order>();

            using var reader = new StreamReader(Config.GetFileOperation().InstancePath +
                                                "orders.csv");
            var header = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var values = reader.ReadLine().Split(';');

                var order = new Order()
                {
                    Id = (int)Convert.ToDouble(values[0]),
                    Date = DateTime.Parse(values[1]),
                    Hour = DateTime.Parse(values[2]),
                    Location = (int)Convert.ToDouble(values[3]),
                    LocationType = values[4],
                    ServiceType = (int)Convert.ToDouble(values[5]),
                    NOM = (int)Convert.ToDouble(values[6]),
                    Note = values[7],
                    Amount = Convert.ToDouble(values[8]),
                    ServiceDuration = Convert.ToDouble(values[9]),
                    Employee = values[10],
                    StartTime = DateTime.Parse(values[11]),
                    EndTime = DateTime.Parse(values[12]),
                    Penalty = Convert.ToDouble(values[13])
                };

                orders.Add(order);
            }

            return orders;
        }

        private List<Location> PrepareLocations()
        {
            var locations = new List<Location>();

            using var reader = new StreamReader(Config.GetFileOperation().InstancePath +
                                                "locations.csv");
            var header = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var values = reader.ReadLine().Split(';');

                var location = new Location()
                {
                    Id = (int)Convert.ToDouble(values[0]),
                    Type = values[1],
                    Lat = Convert.ToDouble(values[2]),
                    Lon = Convert.ToDouble(values[3]),
                    Openning = DateTime.Parse(values[4]),
                    Closing = DateTime.Parse(values[5]),
                    Resp = values[6],
                };

                locations.Add(location);
            }

            return locations;
        }

        private List<DTO.Vehicle> PrepareVehicles()
        {
            var vehicles = new List<DTO.Vehicle>();

            using var reader = new StreamReader(Config.GetFileOperation().InstancePath +
                                                "vehicles.csv");
            var header = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var values = reader.ReadLine().Split(';');

                var vehicle = new DTO.Vehicle()
                {
                    Id = values[0],
                    StartTime = DateTime.Parse(values[1]),
                    EndTime = DateTime.Parse(values[2]),
                    NOM = values[3],
                    Capacity = Convert.ToDouble(values[4]),
                };

                vehicles.Add(vehicle);
            }

            return vehicles;
        }        
    }
}

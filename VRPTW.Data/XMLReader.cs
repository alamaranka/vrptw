using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VRPTW.Configuration;
using VRPTW.Model;

namespace VRPTW.Data
{
    public class XMLReader
    {
        private readonly string _filePath;
        private readonly string _fileName;
        private readonly XDocument _doc;

        public XMLReader()
        {
            var config = new Config().GetFileOperations();
            _filePath = config.FilePath;
            _fileName = config.FileName;
            _doc = XDocument.Load(_filePath + _fileName);
        }

        public List<Customer> GetVertices()
        {
            var vertices = new List<Customer>();
            var name = _doc.Descendants("node").Select(c => c.Attribute("id").Value).ToList();
            var latidute = _doc.Descendants("cx").Select(x => x.Value).ToList();
            var longitude = _doc.Descendants("cy").Select(c => c.Value).ToList();
            var demand = _doc.Descendants("quantity").Select(c => c.Value).ToList();
            var timeStart = _doc.Descendants("start").Select(c => c.Value).ToList();
            var timeEnd = _doc.Descendants("end").Select(c => c.Value).ToList();
            var serviceTime = _doc.Descendants("service_time").Select(c => c.Value).ToList();

            demand.Insert(0, "0");
            timeStart.Insert(0, "0");
            timeEnd.Insert(0, _doc.Descendants("max_travel_time").Select(c => c.Value).ToList()[0]);
            serviceTime.Insert(0, "0");

            for (var record = 0; record < name.Count; record++)
            {
                var customer = new Customer
                {
                    Name = (string)name[record],
                    Latitude = (int)Convert.ToDouble(latidute[record]),
                    Longitude = (int)Convert.ToDouble(longitude[record]),
                    Demand = (int)Convert.ToDouble(demand[record]),
                    TimeStart = (int)Convert.ToDouble(timeStart[record]),
                    TimeEnd = (int)Convert.ToDouble(timeEnd[record]),
                    ServiceTime = (int)Convert.ToDouble(serviceTime[record])
                };
                vertices.Add(customer);
            }
            vertices.Add(vertices[0]);
            return vertices;
        }

        public List<Vehicle> GetVehicles()
        {
            var vehicles = new List<Vehicle>();
            var numberOfVehicles = (int)Convert.ToDouble
                                   (_doc.Descendants("vehicle_profile")
                                   .Select(x => x.Attribute("number").Value).ToList()[0]);
            var capacity = _doc.Descendants("capacity").Select(x => x.Value).ToList()[0];
            for (var record = 0; record < numberOfVehicles; record++)
            {
                var vehicle = new Vehicle
                {
                    Capacity = (int)Convert.ToDouble(capacity)
                };
                vehicles.Add(vehicle);
            }
            return vehicles;
        }

        private void WriteToDb()
        {
            WriteVerticesToDb();
            WriteVehiclesToDb();
        }

        private void WriteVerticesToDb()
        {
            //TODO: fill the method
        }

        private void WriteVehiclesToDb()
        {
            //TODO: fill the method
        }
    }
}
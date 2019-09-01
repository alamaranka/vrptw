using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Model;

namespace VRPTW.Data
{
    public class DataPreparer
    {
        private readonly string _dataSource;

        public DataPreparer(string dataSource)
        {
            _dataSource = dataSource;
        }

        public Dataset GetCustomerAndVehicleData()
        {
            var vertices = new List<Customer>();
            var vehicles = new List<Vehicle>();
            switch (_dataSource)
            {
                case "database":
                    var dBReader = new DBReader();
                    vertices = dBReader.GetVertices();
                    vehicles = dBReader.GetVehicles();
                    new DBManager().Close();
                    break;
                case "xml":
                    XMLReader xMLReader = new XMLReader();
                    vertices = xMLReader.GetVertices();
                    vehicles = xMLReader.GetVehicles();
                    break;
                default:
                    break;
            }
            return new Dataset()
            {
                Vertices = vertices,
                Vehicles = vehicles
            };
        }
    }
}

using System.Collections.Generic;
using VRPTW.DTO;
using VRPTW.Data;

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
            var vehicles = new List<Data.Vehicle>();
            var distances = new List<Distance>();

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
                case "csv":
                    CSVReader csvReader = new CSVReader();
                    vertices = csvReader.GetVertices();
                    vehicles = csvReader.GetVehicles();
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

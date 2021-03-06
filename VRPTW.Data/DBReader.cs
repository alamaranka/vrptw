﻿using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using VRPTW.Model;

namespace VRPTW.Data
{
    public class DBReader
    {
        private readonly SqlConnection _conn = DBManager.GetInstance().GetConnection();
        private SqlDataAdapter _sqlDataAdapter;

        public List<Customer> GetVertices()
        {
            var vertices = new List<Customer>();

            var query = "SELECT * FROM Customer";
            _sqlDataAdapter = new SqlDataAdapter(new SqlCommand(query, _conn));
            var dataTable = new DataTable();
            _sqlDataAdapter.Fill(dataTable);

            for (var row = 0; row < dataTable.Rows.Count; row++)
            {
                var customer = new Customer
                {
                    Id = (int)dataTable.Rows[row].ItemArray[1],
                    Latitude = (int)dataTable.Rows[row].ItemArray[2],
                    Longitude = (int)dataTable.Rows[row].ItemArray[3],
                    Demand = (int)dataTable.Rows[row].ItemArray[4],
                    TimeStart = (int)dataTable.Rows[row].ItemArray[5],
                    TimeEnd = (int)dataTable.Rows[row].ItemArray[6],
                    ServiceTime = (int)dataTable.Rows[row].ItemArray[4]
                };
                vertices.Add(customer);
            }
            return vertices;
        }

        public List<Vehicle> GetVehicles()
        {
            var vehicles = new List<Vehicle>();
            var query = "SELECT * FROM Vehicle";
            _sqlDataAdapter = new SqlDataAdapter(new SqlCommand(query, _conn));
            var dataTable = new DataTable();
            _sqlDataAdapter.Fill(dataTable);

            for (var row = 0; row < dataTable.Rows.Count; row++)
            {
                var vehicle = new Vehicle
                {
                    Capacity = (int)dataTable.Rows[row].ItemArray[1]
                };
                vehicles.Add(vehicle);
            }
            return vehicles;
        }
    }
}
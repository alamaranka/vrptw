using System;

namespace VRPTW.Configuration
{
    public class Config
    {
        public ConnectionString GetConnectionString()
        {
            return new ConnectionString()
            {
                DNS = ConfigManager.AppSetting["ConfigurationManager:DNS"],
                Port = ConfigManager.AppSetting["ConfigurationManager:Port"],
                DBName = ConfigManager.AppSetting["ConfigurationManager:DBName"],
                Username = ConfigManager.AppSetting["ConfigurationManager:Username"],
                Password = ConfigManager.AppSetting["ConfigurationManager:Password"]
            };
        }
        public FileOperations GetFileOperations()
        {
            return new FileOperations()
            {
                FilePath = ConfigManager.AppSetting["FileOperations:FilePath"],
                FileName = ConfigManager.AppSetting["FileOperations:FileName"]
            };
        }

        public SolverParameters GetSolverParameters()
        {
            return new SolverParameters()
            {
                Source = ConfigManager.AppSetting["SolverParameters:Source"],
                TimeLimit = Convert.ToDouble(ConfigManager.AppSetting["SolverParameters:TimeLimit"]),
                MIPGap = Convert.ToDouble(ConfigManager.AppSetting["SolverParameters:MIPGap"]),
                Threads = (int)Convert.ToDouble(ConfigManager.AppSetting["SolverParameters:Threads"])
            };
        }
    }
}

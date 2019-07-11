using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace OnTargetDataLibrary.DataAccess
{
    class AppConfiguration
    {
        public readonly string _connectionString = string.Empty;
        public AppConfiguration(string databaseConnection)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            _connectionString = root.GetSection("ConnectionStrings").GetSection(databaseConnection).Value;
        }
        public string ConnectionString
        {
            get => _connectionString;
        }

    }
}

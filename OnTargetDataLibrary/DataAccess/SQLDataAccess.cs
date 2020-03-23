using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Linq;

namespace OnTargetDataLibrary.DataAccess
{
    public static class SQLDataAccess
    {
        public static string GetConnectionString(string databaseConnection)
        {

            AppConfiguration appconfiguration = new AppConfiguration(databaseConnection);

            return appconfiguration.ConnectionString;
        }

        public static List<T> LoadDataDatawarehouse<T>(string sql, DynamicParameters p)
        {
            string databaseConnection = "DataWarehouseConnection";

            using (IDbConnection cnn = new SqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Query<T>(sql, p, commandType: CommandType.Text).ToList();
            }       
        }

        public static int InsertDataDatawarehouse<T>(string sql, T data)
        {
            string databaseConnection = "DataWarehouseConnection";

            using (IDbConnection cnn = new SqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Execute(sql, data);
            }
        }

        public static int UpdateDataDatawarehouse<T>(string sql, T data)
        {
            string databaseConnection = "DataWarehouseConnection";

            using (IDbConnection cnn = new SqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Execute(sql, data);
            }
        }

        public static int DeleteDataDatawarehouse<T>(string sql, T data)
        {
            string databaseConnection = "DataWarehouseConnection";

            using (IDbConnection cnn = new SqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Execute(sql, data);
            }
        }

        public static List<T> LoadDataSCMDB<T>(string sql, DynamicParameters p)
        {
            string databaseConnection = "SCMDBConnection";

            using (IDbConnection cnn = new SqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Query<T>(sql, p, commandType: CommandType.Text).ToList();
            }
        }

        public static int InsertDataSCMDB<T>(string sql, T data)
        {
            string databaseConnection = "SCMDBConnection";

            using (IDbConnection cnn = new SqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Execute(sql, data);
            }
        }

        public static int UpdateDataSCMDB<T>(string sql, T data)
        {
            string databaseConnection = "SCMDBConnection";

            using (IDbConnection cnn = new SqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Execute(sql, data);
            }
        }

        public static List<T> LoadDataBTIntranet<T>(string sql, DynamicParameters p, string databaseConnection)
        {
           
            using (IDbConnection cnn = new MySqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Query<T>(sql, p, commandType: CommandType.Text).ToList();
            }
        }

        public static int InsertDataBTIntranet<T>(string sql, T data, string databaseConnection)
        {
           
            using (IDbConnection cnn = new MySqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Execute(sql, data);
            }
        }

        public static int UpdateDataBTIntranet<T>(string sql, T data, string databaseConnection)
        {
            using (IDbConnection cnn = new MySqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Execute(sql, data);
            }
        }

        public static int DeleteDataBTIntranet<T>(string sql, T data, string databaseConnection)
        {
           
            using (IDbConnection cnn = new MySqlConnection(GetConnectionString(databaseConnection)))
            {
                return cnn.Execute(sql, data);
            }
        }
    }
}

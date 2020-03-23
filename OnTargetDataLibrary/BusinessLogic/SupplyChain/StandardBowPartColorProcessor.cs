using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.SupplyChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.BusinessLogic.SupplyChain
{
    public static class StandardBowPartColorProcessor
    {
        public static List<StandardBowPartColorsModel> LoadStandardBowPartColors()
        {

            string databaseConnection = "";

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                databaseConnection = @"BTIntranetMySQLConnectionDevelopment";
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                databaseConnection = @"BTIntranetMySQLConnectionProduction";
            }
            else
            {
                databaseConnection = @"BTIntranetMySQLConnectionProduction";
            }

            var p = new DynamicParameters();

            string sql = @"SELECT ID, 
                                  ParentItemCode, 
                                  ChildItemCode, 
                                  PartOrder, 
                                  Color
                            FROM btintranet.standardbowpartcolors
                            ORDER BY ParentItemCode, PartOrder;";

            return SQLDataAccess.LoadDataBTIntranet<StandardBowPartColorsModel>(sql, p, databaseConnection);

        }

        public static int InsertStandardBowPartColors(string parentItemCode, string childItemCode, int partOrder, string color )
        {

            string databaseConnection = "";

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                databaseConnection = @"BTIntranetMySQLConnectionDevelopment";
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                databaseConnection = @"BTIntranetMySQLConnectionProduction";
            }
            else
            {
                databaseConnection = @"BTIntranetMySQLConnectionProduction";
            }

            StandardBowPartColorsModel data = new StandardBowPartColorsModel
            {
                ParentItemCode = parentItemCode,
                ChildItemCode = childItemCode,
                PartOrder = partOrder,
                Color = color
            };

            string sql = @"INSERT INTO standardbowpartcolors (ParentItemCode, ChildItemCode, PartOrder, Color) 
                            VALUES (@ParentItemCode, @ChildItemCode, @PartOrder, @Color); ";

            return SQLDataAccess.InsertDataBTIntranet(sql, data, databaseConnection);

        }

        public static int UpdateStandardBowPartColors(int id, string parentItemCode, string childItemCode, int partOrder, string color)
        {
            string databaseConnection = "";

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                databaseConnection = @"BTIntranetMySQLConnectionDevelopment";
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                databaseConnection = @"BTIntranetMySQLConnectionProduction";
            }
            else
            {
                databaseConnection = @"BTIntranetMySQLConnectionProduction";
            }

            StandardBowPartColorsModel data = new StandardBowPartColorsModel
            {
                ID = id,
                ParentItemCode = parentItemCode,
                ChildItemCode = childItemCode,
                PartOrder = partOrder,
                Color = color
            };

            string sql = @"UPDATE standardbowpartcolors 
                           SET  ParentItemCode = @ParentItemCode,
                                ChildItemCode = @ChildItemCode,
                                PartOrder = @PartOrder,
                                Color = @Color
                           WHERE ID = @ID ;";

            return SQLDataAccess.UpdateDataBTIntranet(sql, data, databaseConnection);
        }

        public static int DeleteStandardBowPartColors(int id)
        {
            string databaseConnection = "";

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                databaseConnection = @"BTIntranetMySQLConnectionDevelopment";
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                databaseConnection = @"BTIntranetMySQLConnectionProduction";
            }
            else
            {
                databaseConnection = @"BTIntranetMySQLConnectionProduction";
            }

            StandardBowPartColorsModel data = new StandardBowPartColorsModel
            {
                ID = id,
            };

            string sql = @"DELETE 
                           FROM standardbowpartcolors
                           WHERE ID = @ID;";

            return SQLDataAccess.DeleteDataBTIntranet(sql, data, databaseConnection);
        }


    }
}

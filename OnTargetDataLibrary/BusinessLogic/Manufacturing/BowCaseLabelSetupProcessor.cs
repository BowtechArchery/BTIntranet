using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.Manufacturing;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.BusinessLogic.Manufacturing
{
    public class BowCaseLabelSetupProcessor
    {
        public static int InsertBowCaseWorkOrderData(string workOrderNumber, bool customBow, string customDescription)
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

            BowCaseLabelWorkOrderModel data = new BowCaseLabelWorkOrderModel
            {
                WorkOrderNumber = workOrderNumber,
                CustomBow = customBow,
                CustomDescription = customDescription,
                CurrentCaseNumber = 1,
                Complete = false
            };

            string sql = @"INSERT INTO labelbowcaseworkorder (WorkOrderNumber, CustomBow, CustomDescription, CurrentCaseNumber, Complete) 
                            VALUES (@WorkOrderNumber, @CustomBow, @CustomDescription, @CurrentCaseNumber, @Complete);";

            return SQLDataAccess.InsertDataBTIntranet(sql, data, databaseConnection);

        }

    }
}

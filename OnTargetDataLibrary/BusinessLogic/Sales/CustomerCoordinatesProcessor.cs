using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.Sales;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.BusinessLogic.Sales
{
    public static class CustomerCoordinatesProcessor
    {
        public static List<CustomerCoordinatesModel> LoadCustomerCoordinates()
        {
            var p = new DynamicParameters();

            string sql = @"SELECT 'BT' AS Site,
		                    cb.CustNum,
		                    cb.CustName,
		                    CASE 
			                    WHEN cb.Address1 = cb.CustName THEN
				                    CASE
					                    WHEN cb.Address2 LIKE '%[0-9]%' THEN cb.Address2
					                    WHEN cb.Address3 LIKE '%[0-9]%' THEN cb.Address3
					                    ELSE NULL
				                    END
			                    WHEN cb.Address1 LIKE '%[0-9]%' THEN cb.Address1
			                    WHEN cb.Address2 LIKE '%[0-9]%' THEN cb.Address2
			                    WHEN cb.Address3 LIKE '%[0-9]%' THEN cb.Address3
			                    ELSE NULL
		                    END AS Address,
		                    cb.City,
		                    cb.State,
		                    cb.Zip,
		                    cb.Country,
		                    cc.Latitude,
		                    cc.Longitude
                    FROM	Customers_BT cb
                    INNER JOIN Customers_Coordinates cc ON cb.CustNum = cc.CustNum

                    UNION ALL

                    SELECT  'EXC' AS Site,
		                    ce.CustNum,
		                    ce.CustName,
		                    CASE 
			                    WHEN ce.Address1 = ce.CustName THEN
				                    CASE
					                    WHEN ce.Address2 LIKE '%[0-9]%' THEN ce.Address2
					                    WHEN ce.Address3 LIKE '%[0-9]%' THEN ce.Address3
					                    ELSE NULL
				                    END
			                    WHEN ce.Address1 LIKE '%[0-9]%' THEN ce.Address1
			                    WHEN ce.Address2 LIKE '%[0-9]%' THEN ce.Address2
			                    WHEN ce.Address3 LIKE '%[0-9]%' THEN ce.Address3
			                    ELSE NULL
		                    END AS Address,
		                    ce.City,
		                    ce.State,
		                    ce.Zip,
		                    ce.Country,
		                    cc.Latitude,
		                    cc.Longitude
                    FROM	Customers_EXC ce
                    INNER JOIN Customers_Coordinates cc ON ce.CustNum = cc.CustNum
                   
                    UNION ALL

                    SELECT  (CASE
			                    WHEN cm.Facility = 'Black Gold' THEN 'BG'
			                    WHEN cm.Facility = 'RipCord' THEN 'RC'
			                    WHEN cm.Facility = 'Tight Spot' THEN 'TS'
			                    ELSE ''
		                    END) AS Site,
		                    cm.CustNum,
		                    cm.CustName,
		                    CASE 
			                    WHEN cm.Address1 = cm.CustName THEN
				                    CASE
					                    WHEN cm.Address2 LIKE '%[0-9]%' THEN cm.Address2
					                    WHEN cm.Address3 LIKE '%[0-9]%' THEN cm.Address3
					                    ELSE NULL
				                    END
			                    WHEN cm.Address1 LIKE '%[0-9]%' THEN cm.Address1
			                    WHEN cm.Address2 LIKE '%[0-9]%' THEN cm.Address2
			                    WHEN cm.Address3 LIKE '%[0-9]%' THEN cm.Address3
			                    ELSE NULL
		                    END AS Address,
		                    cm.City,
		                    cm.State,
		                    cm.Zip,
		                    cm.Country,
		                    cc.Latitude,
		                    cc.Longitude
                    FROM Customers_MT cm
                    INNER JOIN Customers_Coordinates cc ON cm.CustNum = cc.CustNum
                    ;";

            return SQLDataAccess.LoadDataDatawarehouse<CustomerCoordinatesModel>(sql, p);
        }

        public static int InsertCustomerCoordinates(string custNum, string latitude, string longitude)
        {

            CustomerCoordinatesModel data = new CustomerCoordinatesModel
            {
                CustNum = custNum.ToUpper(),
                Latitude = latitude,
                Longitude = longitude
            };

            string sql = @"INSERT INTO dbo.Customers_Coordinates (CustNum, Latitude, Longitude)
                        VALUES (@CustNum, @Latitude, @Longitude);";

            return SQLDataAccess.InsertDataDatawarehouse(sql, data);

        }

        public static int UpdateCustomerCoordinates(string custNum, string latitude, string longitude)
        {
            CustomerCoordinatesModel data = new CustomerCoordinatesModel
            {
                CustNum = custNum,
                Latitude = latitude,
                Longitude = longitude
            };

            string sql = @"UPDATE dbo.Customers_Coordinates
                           SET Latitude = @Latitude,
                           Longitude = @Longitude
                           WHERE CustNum = @CustNum;";

            return SQLDataAccess.UpdateDataDatawarehouse(sql, data);
        }

        public static int DeleteCustomerCoordinates(string custNum)
        {
            CustomerCoordinatesModel data = new CustomerCoordinatesModel
            {
                CustNum = custNum
            };

            string sql = @"DELETE
                           FROM dbo.Customers_Coordinates
                           WHERE CustNum = @CustNum;";

            return SQLDataAccess.DeleteDataDatawarehouse(sql, data);
        }

        public static bool CheckCustNum(string custNum)
        {
            bool isCustomer = false;
            List<CustomerCoordinatesModel> output;

            var p = new DynamicParameters();

            p.Add("@CustNum", custNum);

            string sql = @"SELECT CustNum
                            FROM dbo.Customers_BT
                            WHERE CustNum = @CustNum
                            AND (CustomerStatus = 'Active' OR CustomerStatus = 'Hold');";

            //check to see if the customer exists in Customers BT
            output = SQLDataAccess.LoadDataDatawarehouse<CustomerCoordinatesModel>(sql, p);

            if (output.Count > 0)
            {
                isCustomer = true;
            }

            sql = @"SELECT CustNum
                    FROM dbo.Customers_EXC
                    WHERE CustNum = @CustNum
                    AND CustomerStatus = 'Active';";

            //check to see if the customer exists in Customers EXC
            output = SQLDataAccess.LoadDataDatawarehouse<CustomerCoordinatesModel>(sql, p);

            if (output.Count > 0)
            {
                isCustomer = true;
            }

            sql = @"SELECT CustNum
                    FROM dbo.Customers_MT
                    WHERE CustNum = @CustNum
                    AND CustomerStatus = 'Active';";

            //check to see if the customer exists in Customers MT
            output = SQLDataAccess.LoadDataDatawarehouse<CustomerCoordinatesModel>(sql, p);

            if (output.Count > 0)
            {
                isCustomer = true;
            }

            return isCustomer;
        }

        public static bool CheckCustNumDupes(string custNum)
        {
            bool isNotDupe = true;
            List<CustomerCoordinatesModel> output;

            var p = new DynamicParameters();

            p.Add("@CustNum", custNum);

            string sql = @"SELECT CustNum
                            FROM dbo.Customers_Coordinates
                            WHERE CustNum = @CustNum;";

            //check to see if the customer exists in Customers MT
            output = SQLDataAccess.LoadDataDatawarehouse<CustomerCoordinatesModel>(sql, p);

            if (output.Count > 0)
            {
                isNotDupe = false;
            }

            return isNotDupe;
        }
    }
}

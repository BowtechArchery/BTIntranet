using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.Manufacturing;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.BusinessLogic.Manufacturing
{
    public static class WorkOrderSerialModifyProcessor
    {

        public static List<WorkOrderSerialModel> LoadWorkOrderSerials()
        {
            var p = new Dapper.DynamicParameters();

            string sql = @"SELECT	us.serial_num AS ID,
                                    us.serial_num AS SerialNumber,
		                            us.wo_num AS WorkOrderNumber,
		                            us.build_date AS BuildDate,
		                            ptwh.woh_item_no AS ItemCode,
		                            ptwh.woh_item_short_desc AS ItemDescription,
		                            us.builder_name AS BuilderName
                            FROM _ud_serialnumbers as us
                            INNER JOIN pmd_twoh_wo_headr ptwh on us.wo_num = ptwh.woh_wo_no
                            WHERE build_date >= DATEADD(YEAR, -2, GETDATE())
                            ORDER BY build_date DESC;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderSerialModel>(sql, p);
        }

        public static int UpdateWorkOrderSerials(string ID, string serialNumber, string workOrderNumber, string buildDate, string builderName)
        {
            WorkOrderSerialModel data = new WorkOrderSerialModel
            {
                ID = ID,
                SerialNumber = serialNumber,
                WorkOrderNumber = workOrderNumber,
                BuildDate = buildDate,
                BuilderName = builderName
            };

            string sql = @"UPDATE dbo._ud_serialnumbers
                           SET serial_num = @SerialNumber,
                           wo_num = @WorkOrderNumber,
                           build_date = @BuildDate,
                           builder_name = @BuilderName
                           WHERE serial_num = @ID;";

            return SQLDataAccess.UpdateDataSCMDB(sql, data);
        }
    }
}

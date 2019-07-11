using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.Sales;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.BusinessLogic.Sales
{
    public static class CustomBowFieldsProcessor
    {
        public static List<CustomBowFieldModel> LoadCustomBowFields()
        {
            var p = new Dapper.DynamicParameters();

            string sql = @"SELECT pod.IndexKey, 
	                          pod.PepperiOrderNum, 
	                          pod.ItemCode, 
                              pod.ItemDesc, 
	                          pod.CustomBowModel, 
	                          pod.CustomBowHand, 
	                          pod.CustomBowDrawWeight,
	                          pod.CustomBowRiserColor, 
	                          pod.CustomBowLimbColor
                        FROM [RVWDB].[DataWarehouse].dbo.Pepperi_OrderDetail pod
                        WHERE CONVERT(VARCHAR(25), pod.PepperiOrderNum) IN (SELECT DISTINCT dbo.fn_StripChars(soh.sohdr_order_no,'^--=0-9')
													                        FROM so_order_hdr soh
													                        INNER JOIN so_order_item_dtl soid ON soh.sohdr_order_no = soid.sodtl_order_no
													                        WHERE soid.sodtl_item_code LIKE '40000%'
													                        AND soh.sohdr_order_status IN ('FR', 'AM', 'DR'))
                        AND pod.ItemCode LIKE '40000%'
                        ORDER BY PepperiOrderNum;";

            return SQLDataAccess.LoadDataSCMDB<CustomBowFieldModel>(sql, p);
        }

        public static int UpdateCustomBowFields(string indexKey, string customBowModel, string customBowHand, string customBowDrawWeight, string customBowRiserColor, string customBowLimbColor)
        {
            CustomBowFieldModel data = new CustomBowFieldModel
            {
                IndexKey = indexKey,
                CustomBowModel = customBowModel,
                CustomBowHand = customBowHand,
                CustomBowDrawWeight = customBowDrawWeight,
                CustomBowRiserColor = customBowRiserColor,
                CustomBowLimbColor = customBowLimbColor
            };

            string sql = @"UPDATE dbo.Pepperi_OrderDetail
                           SET CustomBowModel = @CustomBowModel,
                           CustomBowHand = @CustomBowHand,
                           CustomBowDrawWeight = @CustomBowDrawWeight,
                           CustomBowRiserColor = @CustomBowRiserColor,
                           CustomBowLimbColor = @CustomBowLimbColor
                           WHERE IndexKey = @IndexKey;";

            return SQLDataAccess.UpdateDataDatawarehouse(sql, data);
        }
    }
}

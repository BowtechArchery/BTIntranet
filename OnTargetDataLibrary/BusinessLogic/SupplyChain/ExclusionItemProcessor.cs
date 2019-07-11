using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.SupplyChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetDataLibrary.BusinessLogic.SupplyChain
{
    public static class ExclusionItemProcessor
    {
        public static List<ExclusionItemModel> LoadExclusionItems()
        {
            var p = new DynamicParameters();

            string sql = @"SELECT ItemCode, ili.loi_itemdesc AS ItemDesc
                            FROM dbo.ExclusionItems ei
                            INNER JOIN [SCMDB].dbo.itm_loi_itemhdr ili ON ei.ItemCode = ili.loi_itemcode;";

            return SQLDataAccess.LoadDataDatawarehouse<ExclusionItemModel>(sql, p);
        }

        public static int InsertExclusionItem(string itemCode)
        {
            
            ExclusionItemModel data = new ExclusionItemModel
            {
                ItemCode = itemCode
            };

            string sql = @"INSERT INTO dbo.ExclusionItems (ItemCode)
                        VALUES (@ItemCode);";

            return SQLDataAccess.InsertDataDatawarehouse(sql, data);
           
        }

        public static int UpdateExclusionItem(string itemCode, string itemDesc)
        {
            ExclusionItemModel data = new ExclusionItemModel
            {
                ItemCode = itemCode,
                ItemDesc = itemDesc
            };

            string sql = @"UPDATE dbo.ExclusionItems 
                           SET ItemDesc = @ItemDesc
                           WHERE ItemCode = @ItemCode;";

            return SQLDataAccess.UpdateDataDatawarehouse(sql, data);
        }

        public static int DeleteExclusionItem(string itemCode)
        {
            ExclusionItemModel data = new ExclusionItemModel
            {
                ItemCode = itemCode
            };

            string sql = @"DELETE
                           FROM dbo.ExclusionItems 
                           WHERE ItemCode = @ItemCode;";

            return SQLDataAccess.DeleteDataDatawarehouse(sql, data);
        }

        public static bool CheckItemCode(string itemCode)
        {
            bool isItem = false;
            List<ExclusionItemModel> output;

            var p = new DynamicParameters();

            p.Add("@ItemCode", itemCode);

            string sql = @"SELECT iou_itemcode
                            FROM itm_iou_itemvarhdr
                            WHERE iou_itemcode = @ItemCode;";

            //check to see if the item code exists in the Ramco item master
            output = SQLDataAccess.LoadDataSCMDB<ExclusionItemModel>(sql, p);

            if (output.Count > 0)
            {
                isItem = true;
            }

            return isItem;
        }
    }
}

using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.LabelPrinting;
using OnTargetDataLibrary.Models.SupplyChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace OnTargetDataLibrary.BusinessLogic.SupplyChain
{
    public static class GenerateDecorationTicketProcessor
    {
        
        public static List<LabelPrinterModel> LoadLabelPrinters(string labelClassID)
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

            p.Add("@ClassID", labelClassID);

            string sql = @"SELECT lp.PrinterID, lp.PrinterDescription, lp.PrinterName
                            FROM labelprinters lp
                            INNER JOIN labelprinterclass lpc ON lp.PrinterID = lpc.PrinterID
                            WHERE lpc.ClassID = @ClassID;";

            return SQLDataAccess.LoadDataBTIntranet<LabelPrinterModel>(sql, p, databaseConnection);

        }

        public static bool CheckWorkOrder(string workOrderNumber)
        {
            bool isWorkOrder = false;
            List<WorkOrderNumberModel> output;

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT woh_wo_no
                            FROM pmd_twoh_wo_headr
                            WHERE woh_wo_no = @WorkOrderNumber;";

            //check to see if the item code exists in the Ramco item master
            output = SQLDataAccess.LoadDataSCMDB<WorkOrderNumberModel>(sql, p);

            if (output.Count > 0)
            {
                isWorkOrder = true;
            }

            return isWorkOrder;
        }

        public static List<WorkOrderModel> LoadWorkOrder(string workOrderNumber)
        {

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT woh_wo_no AS WorkOrderNumber,
		                        woh_item_no AS ItemCode,
		                        woh_item_short_desc AS ItemDescription,
		                        woh_order_qty AS Quantity,
                                woh_so_no AS SalesOrderNumber,
                                woh_created_by AS IssuedBy
                        FROM pmd_twoh_wo_headr
                        WHERE woh_wo_no = @WorkOrderNumber;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderModel>(sql, p);
        }

        public static List<CustomBowModel> LoadCustomBowData(string salesOrderNumber, string itemCode)
        {

            var p = new DynamicParameters();

            p.Add("@SalesOrderNumber", salesOrderNumber);
            p.Add("@ItemCode", itemCode);

            string sql = @"SELECT CustomBowModel AS BowModel,
		                        CustomBowHand AS BowHand,
		                        CustomBowDrawWeight AS BowDrawWeight,
		                        CustomBowRiserColor AS BowRiserColor,
		                        CustomBowLimbColor AS BowLimbColor
                        FROM Pepperi_OrderDetail
                        WHERE PepperiOrderNum = @SalesOrderNumber
                        AND ItemCode = @ItemCode;";

            return SQLDataAccess.LoadDataDatawarehouse<CustomBowModel>(sql, p);
        }

        public static List<PrinterNameModel> LoadLabelPrinterName(string printerID)
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

            p.Add("@PrinterID", printerID);

            string sql = @"SELECT PrinterName
                        FROM labelPrinters
                        WHERE PrinterID = @PrinterID;";

            return SQLDataAccess.LoadDataBTIntranet<PrinterNameModel>(sql, p, databaseConnection);
        }

        public static string RemoveTextFromSalesOrder(string salesOrderNumber)
        {
            var allowedChars = "0123456789-";

            return new string(salesOrderNumber.Where(c => allowedChars.Contains(c)).ToArray());

        }

        public static bool GenerateCustomBowDecorationTicket(string labelCustomBowData, string printerName)
        {
            bool ticketPrinted = false;

            string xmlOutputPath = "";

            XmlWriterSettings setting = new XmlWriterSettings();
            setting.ConformanceLevel = ConformanceLevel.Auto;
            setting.Encoding = Encoding.UTF8;
            setting.Indent = true;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                xmlOutputPath = @"S:\";
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                xmlOutputPath = @"C:\BarTender\Scan\";
            }
            else
            {
                xmlOutputPath = @"C:\BarTender\Scan\";
            }

            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + "200 " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {
                
                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                    writer.WriteStartElement("Command");
                    writer.WriteAttributeString("Name", @"Job1");
                        writer.WriteStartElement("Print");
                            writer.WriteElementString("Format", @"C:\BarTender\BTW\200\200.btw");
                        writer.WriteStartElement("RecordSet");
                        writer.WriteAttributeString("Name", @"200");
                        writer.WriteAttributeString("Type", @"btTextFile");
                            writer.WriteElementString("Delimitation", "btDelimCustom");
                            writer.WriteElementString("FieldDelimiter", "|");
                            writer.WriteElementString("UseFieldNamesFromFirstRecord", "true");
                                writer.WriteStartElement("TextData");
                                writer.WriteCData($"{labelCustomBowData}");
                                writer.WriteEndElement(); //</TextData>
                        writer.WriteEndElement(); //</RecordSet>
                        writer.WriteStartElement("PrintSetup");
                            writer.WriteElementString("Printer", $"{printerName}");
                            writer.WriteElementString("IdenticalCopiesOfLabel", "4");
                            if(printerName == "\\\\BTPrintserver\\Planning (Lexmark M5255)")
                            {
                                writer.WriteElementString("PaperTray", "Manual Feed");
                            }
                        writer.WriteEndElement(); //</PrintSetup>   
                        writer.WriteEndElement(); //</Print>   
                    writer.WriteEndElement(); //</Command>
                writer.WriteEndElement(); //</XMLScript>
                writer.Flush();
                writer.Close();

            }

            //put some error handling here

            ticketPrinted = true;

            return ticketPrinted;
        }

        public static List<WorkOrderChildPartsModel> LoadWorkOrderChildParts(string workOrderNumber)
        {

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);
          
            string sql = @"SELECT ptwp.wos_wo_no AS WorkOrderNumber, 
		                        ptwp.wos_item_no AS ItemCode, 
		                        ili.loi_itemdesc AS ItemDescription, 
		                        CONVERT(NUMERIC(28,4),ptwp.wos_tot_qty) AS Quantity
                        FROM pmd_twos_wops_postn ptwp (NOLOCK)	
                        LEFT JOIN itm_loi_itemhdr ili ON ptwp.wos_item_no = ili.loi_itemcode
                        LEFT JOIN SAV_MINI_BOM_UDS smbu ON ptwp.wos_item_no = smbu.Item_code
                        WHERE ptwp.wos_io_flag = 'I' 
                        AND ptwp.wos_wo_no = @WorkOrderNumber
                        AND (ili.loi_itemdesc LIKE '%RSR%' OR ili.loi_itemdesc LIKE '%LMB%' OR ili.loi_itemdesc LIKE '%CAM%' OR ili.loi_itemdesc LIKE '%HDW FG Body%')
                        AND smbu.prod_class IN ('RSR', 'LMB', 'HDW', 'CAM')
                        ORDER BY CASE WHEN ili.loi_itemdesc LIKE '%RSR%' THEN '1'
			                          WHEN ili.loi_itemdesc LIKE '%LMB%' THEN '2'
			                          WHEN ili.loi_itemdesc LIKE 'HDW FG Body%' THEN '3'
			                          WHEN ili.loi_itemdesc LIKE '%CAM%' THEN '4'
			                          ELSE ili.loi_itemdesc END ASC;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderChildPartsModel>(sql, p);
        }

    }
}

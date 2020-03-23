using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.LabelPrinting;
using OnTargetDataLibrary.Models.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace OnTargetDataLibrary.BusinessLogic.Shipping
{
    public class ShippingLabelProcessor
    {
        public static List<LabelClassModel> LoadLabelClasses(string groupID)
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

            p.Add("@GroupID", groupID);

            string sql = @"SELECT lc.ClassID, lc.ClassName
                            FROM labelclass lc
                            WHERE lc.GroupID = @GroupID
                            ORDER BY ClassID;";

            return SQLDataAccess.LoadDataBTIntranet<LabelClassModel>(sql, p, databaseConnection);

        }

        public static List<LabelDataModel> LoadLabelDataWorkOrder(string workOrderNumber)
        {
            var p = new Dapper.DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT	woh_wo_no AS WorkOrderNumber, 
		                            woh_item_no AS PartNumber,
		                            ptwh.woh_item_short_desc AS PartDescription,
		                            ptwh.woh_order_qty AS Quantity,
                                    ptwh.woh_so_no AS SalesOrderNumber,
		                            smbu.upc_no AS UPC
                            FROM pmd_twoh_wo_headr ptwh
                            INNER JOIN SAV_MINI_BOM_UDS smbu ON ptwh.woh_item_no = smbu.Item_code
                            WHERE ptwh.woh_wo_no = @WorkOrderNumber;";

            return SQLDataAccess.LoadDataSCMDB<LabelDataModel>(sql, p);
        }

        public static List<LabelDataModel> LoadLabelDataPartNumber(string itemCode)
        {
            var p = new Dapper.DynamicParameters();

            p.Add("@ItemCode", itemCode);

            string sql = @"SELECT ili.loi_itemcode AS PartNumber,
                            ili.loi_itemdesc AS PartDescription,           
                            iii.iou_stdwhcode AS Warehouse,
                            iii.iou_stdcostuom AS UOM,
                            iww.wpp_zone AS Zone,
                            smbu.upc_no AS UPC
                            FROM itm_loi_itemhdr ili
                            INNER JOIN itm_wpp_whplanparam iww ON ili.loi_itemcode = iww.wpp_itemcode
                            INNER JOIN itm_iou_itemvarhdr iii on ili.loi_itemcode = iii.iou_itemcode
                            INNER JOIN SAV_MINI_BOM_UDS smbu ON ili.loi_itemcode = smbu.Item_code
                            WHERE ili.loi_itemcode = @ItemCode;";

            return SQLDataAccess.LoadDataSCMDB<LabelDataModel>(sql, p);
        }

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

        public static List<WorkOrderNumberModel> LoadWorkOrderNumbers(string text)
        {
            var p = new DynamicParameters();

            p.Add("@Text", text);

            string sql = @"SELECT woh_wo_no AS WorkOrderNumber
                            FROM pmd_twoh_wo_headr
                            WHERE woh_wo_no LIKE + '%' + @Text + '%';";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderNumberModel>(sql, p);
        }

        public static List<PartNumbersModel> LoadPartNumbers(string text)
        {
            var p = new Dapper.DynamicParameters();

            p.Add("@Text", text);

            string sql = @"SELECT iou_itemcode AS PartNumber
                            FROM itm_iou_itemvarhdr
                            WHERE iou_itemcode LIKE + '%' + @Text + '%'
                            AND iou_status = 'AC'
                            ORDER BY iou_itemcode;";

            return SQLDataAccess.LoadDataSCMDB<PartNumbersModel>(sql, p);
        }

        public static bool CheckWorkOrderNumber(string workOrderNumber)
        {
            bool isWorkOrderNumber = false;
            List<WorkOrderNumberModel> output;

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT woh_wo_no AS WorkOrderNumber
                            FROM pmd_twoh_wo_headr
                            WHERE woh_wo_no = @WorkOrderNumber;";

            //check to see if the item code exists in the Ramco item master
            output = SQLDataAccess.LoadDataSCMDB<WorkOrderNumberModel>(sql, p);

            if (output.Count > 0)
            {
                isWorkOrderNumber = true;
            }

            return isWorkOrderNumber;
        }

        public static bool CheckPartNumber(string partNumber)
        {
            bool isPartNumber = false;
            List<PartNumbersModel> output;

            var p = new DynamicParameters();

            p.Add("@PartNumber", partNumber);

            string sql = @"SELECT iou_itemcode AS PartNumber
                            FROM itm_iou_itemvarhdr
                            WHERE iou_itemcode = @PartNumber;";

            //check to see if the item code exists in the Ramco item master
            output = SQLDataAccess.LoadDataSCMDB<PartNumbersModel>(sql, p);

            if (output.Count > 0)
            {
                isPartNumber = true;
            }

            return isPartNumber;
        }

        public static bool GenerateShippingLabel(string labelShippingData, string labelClass, string printerName, string numberOfCopies)
        {
            bool labelPrinted = false;

            string xmlOutputPath = "";
            string labelFileName = "";
            string dataFileName = "";

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

            //determine which label format to use
            if (labelClass == "300")
            {
                labelFileName = "300 Shipping Restock Case Label Zebra GX420D.btw";
                dataFileName = "300 Shipping Restock";
            }
            else if (labelClass == "301")
            {
                labelFileName = "301 Shipping Stocking Label Zebra LP2844.btw";
                dataFileName = "301 Shipping Stocking";
            }
            else if (labelClass == "302")
            {
                labelFileName = "302 Shipping Large Bin Label Zebra LP2844.btw";
                dataFileName = "302 Shipping Large Bin";
            }
            else if (labelClass == "303")
            {
                labelFileName = "303 Shipping Small Bin Label Zebra LP2844.btw";
                dataFileName = "303 Shipping Small Bin";
            }
            else if (labelClass == "304")
            {
                labelFileName = "304 Shipping PO Number Label Zebra LP2844.btw";
                dataFileName = "304 Shipping PO Number";
            }
            else if (labelClass == "305")
            {
                labelFileName = "305 Shipping MM Stocking Label Zebra LP2844.btw";
                dataFileName = "305 Shipping MM Stocking";
            }
            else if (labelClass == "306")
            {
                labelFileName = "306 Shipping MM Missing Parts Label Zebra LP2844.btw";
                dataFileName = "306 Shipping MM Missing Parts";
            }

            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + labelClass + " " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {

                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                writer.WriteStartElement("Command");
                writer.WriteAttributeString("Name", @"Job1");
                writer.WriteStartElement("Print");
                writer.WriteElementString("Format", @"C:\BarTender\BTW\300\" + labelFileName);
                writer.WriteStartElement("RecordSet");
                writer.WriteAttributeString("Name", $"{dataFileName}");
                writer.WriteAttributeString("Type", @"btTextFile");
                writer.WriteElementString("Delimitation", "btDelimCustom");
                writer.WriteElementString("FieldDelimiter", "|");
                writer.WriteElementString("UseFieldNamesFromFirstRecord", "true");
                writer.WriteStartElement("TextData");
                writer.WriteCData($"{labelShippingData}");
                writer.WriteEndElement(); //</TextData>
                writer.WriteEndElement(); //</RecordSet>
                writer.WriteStartElement("PrintSetup");
                writer.WriteElementString("Printer", $"{printerName}");
                writer.WriteElementString("IdenticalCopiesOfLabel", numberOfCopies);
                writer.WriteEndElement(); //</PrintSetup>   
                writer.WriteEndElement(); //</Print>   
                writer.WriteEndElement(); //</Command>
                writer.WriteEndElement(); //</XMLScript>
                writer.Flush();
                writer.Close();

            }

            //put some error handling here

            labelPrinted = true;

            return labelPrinted;
        }

    }
}

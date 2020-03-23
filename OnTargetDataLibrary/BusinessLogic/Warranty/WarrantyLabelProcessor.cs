using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.Warranty;
using OnTargetDataLibrary.Models.LabelPrinting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace OnTargetDataLibrary.BusinessLogic.Warranty
{
    public static class WarrantyLabelProcessor
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

        public static List<LabelDataModel> LoadLabelData(string itemCode)
        {
            var p = new Dapper.DynamicParameters();

            p.Add("@ItemCode", itemCode);

            string sql = @"SELECT ili.loi_itemdesc AS PartDescription,           
		                    smbu.upc_no AS UPC,
							iii.ibu_category AS BowModel
                            FROM itm_loi_itemhdr ili
                            INNER JOIN SAV_MINI_BOM_UDS smbu ON ili.loi_itemcode = smbu.Item_code
							INNER JOIN itm_ibu_itemvarhdr iii on ili.loi_itemcode = iii.ibu_itemcode
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

        public static bool GenerateWarrantyLabel(string labelWarrantyData, string labelClass, string printerName, string numberOfCopies)
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
            if (labelClass == "400")
            {
                labelFileName = "400 Warranty Standard Case Label Zebra GK420D.btw";
                dataFileName = "400 Warranty Standard";
            }
            else if (labelClass == "401")
            {
                labelFileName = "401 Warranty Bass Pro Case Label Zebra GK420D.btw";
                dataFileName = "401 Warranty Bass Pro";

            }
            else if (labelClass == "402")
            {
                labelFileName = "402 Warranty Dicks Sporting Goods Label Zebra GK420D.btw";
                dataFileName = "402 Warranty Dicks Sporting Goods";

            }
            else if (labelClass == "403")
            {
                labelFileName = "403 Warranty Academy Sports Outdoors Label Zebra GK420D.btw";
                dataFileName = "403 Warranty Academy Sports Outdoors";

            }
            else if (labelClass == "404")
            {
                labelFileName = "404 Warranty Custom Case Label Zebra GK420D.btw";
                dataFileName = "404 Warranty Custom";

            }
            else
            {
                labelFileName = "400 Warranty Standard Case Label Zebra GK420D.btw";
                dataFileName = "400 Warranty Standard";
            }


            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + labelClass + " " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {

                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                writer.WriteStartElement("Command");
                writer.WriteAttributeString("Name", @"Job1");
                writer.WriteStartElement("Print");
                writer.WriteElementString("Format", @"C:\BarTender\BTW\400\" + labelFileName);
                writer.WriteStartElement("RecordSet");
                writer.WriteAttributeString("Name", $"{dataFileName}");
                writer.WriteAttributeString("Type", @"btTextFile");
                writer.WriteElementString("Delimitation", "btDelimCustom");
                writer.WriteElementString("FieldDelimiter", "|");
                writer.WriteElementString("UseFieldNamesFromFirstRecord", "true");
                writer.WriteStartElement("TextData");
                writer.WriteCData($"{labelWarrantyData}");
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

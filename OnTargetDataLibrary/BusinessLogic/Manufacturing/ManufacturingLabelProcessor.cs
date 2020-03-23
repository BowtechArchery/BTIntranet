using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.Manufacturing;
using OnTargetDataLibrary.Models.LabelPrinting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace OnTargetDataLibrary.BusinessLogic.Manufacturing
{
    public static class ManufacturingLabelProcessor
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

            string sql = @"SELECT PrinterDescription, PrinterName
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

        public static List<SerialNumberModel> LoadSerialNumbers(string text)
        {
            var p = new DynamicParameters();

            p.Add("@Text", text);

            string sql = @"SELECT serial_num AS SerialNumber
                            FROM _ud_serialnumbers
                            WHERE serial_num LIKE + '%' + @Text + '%'
                            ORDER BY serial_num"; ;

            return SQLDataAccess.LoadDataSCMDB<SerialNumberModel>(sql, p);
        }

        public static List<BowCaseLabelWorkOrderModel> LoadBowCaseLabelSetup()
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

            //p.Add();

            string sql = @"SELECT ID, 
	                              WorkOrderNumber, 
	                              CustomBow,
	                              CustomDescription, 
	                              CurrentCaseNumber, 
	                              Complete
                            FROM labelbowcaseworkorder
                            ORDER BY WorkOrderNumber;";

            return SQLDataAccess.LoadDataBTIntranet<BowCaseLabelWorkOrderModel>(sql, p, databaseConnection); ;
        }

        public static List<WorkOrderNumberModel> LoadBowCaseWorkOrderNumbers(string text)
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

            p.Add("@Text", text);

            string sql = @"SELECT WorkOrderNumber
                        FROM labelbowcaseworkorder
                        WHERE WorkOrderNumber LIKE CONCAT('%', @Text, '%')
                        AND Complete = 0;";

            return SQLDataAccess.LoadDataBTIntranet<WorkOrderNumberModel>(sql, p, databaseConnection);
        }

        public static int UpdateBowCaseWorkOrderSetup(int iD, bool customBow, string customDescription, int currentCaseNumber, bool complete)
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
                ID = iD,
                CustomBow = customBow,
                CustomDescription = customDescription,
                CurrentCaseNumber = currentCaseNumber,
                Complete = complete
            };

            string sql = @"UPDATE labelbowcaseworkorder
                           SET  CustomBow = @CustomBow,
                                CustomDescription = @CustomDescription,
                                CurrentCaseNumber = @CurrentCaseNumber,
                                Complete = @Complete
                           WHERE ID = @ID ;";

            return SQLDataAccess.UpdateDataBTIntranet(sql, data, databaseConnection);
        }

        public static List<BowCaseLabelWorkOrderModel> LoadBowCaseWorkOrderData(string workOrderNumber)
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

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT WorkOrderNumber, CustomBow, CustomDescription, CurrentCaseNumber
                        FROM labelbowcaseworkorder
                        WHERE WorkOrderNumber = @WorkOrderNumber;";

            return SQLDataAccess.LoadDataBTIntranet<BowCaseLabelWorkOrderModel>(sql, p, databaseConnection); ;
        }

        public static int UpdateLabelBowCaseWorkOrder(string workOrderNumber, int nextCaseNumber, bool complete)
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
                CurrentCaseNumber = nextCaseNumber,
                Complete = complete
            };

            string sql = @"UPDATE labelbowcaseworkorder
                           SET  CurrentCaseNumber = @CurrentCaseNumber,
                                Complete = @Complete
                           WHERE WorkOrderNumber = @WorkOrderNumber ;";

            return SQLDataAccess.UpdateDataBTIntranet(sql, data, databaseConnection);
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

        public static bool CheckWorkOrderSetup(string workOrderNumber)
        {
            bool isWorkOrderNotSetup = true;
            
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

            List<WorkOrderNumberModel> output;

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT WorkOrderNumber
                        FROM labelbowcaseworkorder
                        WHERE WorkOrderNumber = @WorkOrderNumber;";

            output = SQLDataAccess.LoadDataBTIntranet<WorkOrderNumberModel>(sql, p, databaseConnection);

            if (output.Count > 0)
            {
                isWorkOrderNotSetup = false;
            }

            return isWorkOrderNotSetup;
        }

        public static bool CheckLabelBowCaseWorkOrderNumber(string workOrderNumber)
        {
            string databaseConnection = "";
            bool isLabelBowCaseWorkOrderNumber = false;

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

            List<WorkOrderNumberModel> output;

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT WorkOrderNumber
                        FROM labelbowcaseworkorder
                        WHERE WorkOrderNumber = @WorkOrderNumber;";

            output = SQLDataAccess.LoadDataBTIntranet<WorkOrderNumberModel>(sql, p, databaseConnection);

            if (output.Count > 0)
            {
                isLabelBowCaseWorkOrderNumber = true;
            }

            return isLabelBowCaseWorkOrderNumber;
        }

        public static List<WorkOrderCompleteModel> CheckLabelBowCaseWorkOrderNumberIsComplete(string workOrderNumber)
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

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT Complete
                        FROM labelbowcaseworkorder
                        WHERE WorkOrderNumber = @WorkOrderNumber;";

            return SQLDataAccess.LoadDataBTIntranet<WorkOrderCompleteModel>(sql, p, databaseConnection);
            
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

        public static bool CheckSerialNumber(string serialNumber)
        {
            bool serialNumberExist = false;
            
            List<SerialNumberModel> output;

            var p = new DynamicParameters();

            p.Add("@SerialNumber", serialNumber);

            string sql = @"SELECT serial_num AS SerialNumber
                            FROM _ud_serialnumbers
                            WHERE serial_num = @SerialNumber;";

            //check to see if the item code exists in the Ramco item master
            output = SQLDataAccess.LoadDataSCMDB<SerialNumberModel>(sql, p);

            if (output.Count > 0)
            {
                serialNumberExist = true;
            }

            return serialNumberExist;
        }

        public static int InsertSerialNumber(string serialNumber, string workOrderNumber, string builderName)
        {

            SerialModel data = new SerialModel
            {
                SerialNumber = serialNumber,
                WorkOrderNumber = workOrderNumber,
                BuilderName = builderName
            };

            string sql = @"INSERT INTO _ud_serialnumbers (serial_num, wo_num, build_date, invoice_num, invoice_date, builder_name) 
                            VALUES (@SerialNumber, @WorkOrderNumber, CURRENT_TIMESTAMP, CONVERT(VARCHAR(25),NULL), CONVERT(DATETIME,NULL), @BuilderName); ";

            return SQLDataAccess.InsertDataSCMDB(sql, data);

        }

        public static List<WorkOrderNumberModel> LoadWorkOrderNumberFromSerial(string serialNumber)
        {
            var p = new Dapper.DynamicParameters();

            p.Add("@SerialNumber", serialNumber);

            string sql = @"SELECT wo_num AS WorkOrderNumber
                            FROM _ud_serialnumbers
                            WHERE serial_num = @SerialNumber;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderNumberModel>(sql, p);
        }

        public static bool GenerateManufacturingBowCaseLabel(string labelShippingData, string printerName)
        {
            bool labelPrinted = false;

            string xmlOutputPath = "";
            string labelFileName = "";
            string dataFileName = "";
            string labelClass = "";

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

            //
            labelClass = "100";
            labelFileName = "100 Manufacturing Bow Case Label Zebra GX420D.btw";
            dataFileName = "100 Manufacturing Bow Case Label";
        
            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + labelClass + " " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {

                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                writer.WriteStartElement("Command");
                writer.WriteAttributeString("Name", @"Job1");
                writer.WriteStartElement("Print");
                writer.WriteElementString("Format", @"C:\BarTender\BTW\100\" + labelFileName);
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
                writer.WriteElementString("IdenticalCopiesOfLabel", "1");
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

        public static bool GenerateManufacturingLabel(string labelShippingData, string labelClass, string printerName, string numberOfCopies)
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
            
            if (labelClass == "101")
            {
                labelFileName = "101 Manufacturing Case Label (Warranty) DataMax DMX-I4208.btw";
                dataFileName = "101 Manufacturing Case Label (Warranty)";
            }
            else if (labelClass == "102")
            {
                labelFileName = "102 Manufacturing Stocking Label Zebra LP2844.btw";
                dataFileName = "102 Manufacturing Stocking";
            }
            else if (labelClass == "103")
            {
                labelFileName = "103 Manufacturing MM Stocking Label Zebra LP2844.btw";
                dataFileName = "103 Manufacturing MM Stocking";
            }
            else if (labelClass == "104")
            {
                labelFileName = "104 Manufacturing Large Blank Bin Label Zebra LP2844.btw";
                dataFileName = "104 Manufacturing Large Blank Bin";
            }
            else if (labelClass == "105")
            {
                labelFileName = "105 Manufacturing Large Bin Label Zebra LP2844.btw";
                dataFileName = "105 Manufacturing Large Bin";
            }
            else if (labelClass == "106")
            {
                labelFileName = "106 Manufacturing Large Bin With Cell Label Zebra LP2844.btw";
                dataFileName = "106 Manufacturing Large Bin With Cell";
            }
            else if (labelClass == "107")
            {
                labelFileName = "107 Manufacturing Small Bin With Cell Label Zebra LP2844.btw";
                dataFileName = "107 Manufacturing Small Bin With Cell";
            }
            else if (labelClass == "108")
            {
                labelFileName = "108 Manufacturing Small Blank Bin Label Zebra LP2844.btw";
                dataFileName = "108 Manufacturing Small Blank Bin";
            }
            else if (labelClass == "109")
            {
                labelFileName = "109 Manufacturing String Stop Label Zebra LP2844.btw";
                dataFileName = "109 Manufacturing String Stop";

                System.Diagnostics.Debug.WriteLine(labelShippingData);

            }
            else
            {

            }

            if (labelClass == "109")
            {
                using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + labelClass + " " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
                {

                    writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                    writer.WriteStartElement("XMLScript");
                    writer.WriteAttributeString("Version", @"2.0");
                    writer.WriteStartElement("Command");
                    writer.WriteAttributeString("Name", @"Job1");
                    writer.WriteStartElement("Print");
                    writer.WriteElementString("Format", @"C:\BarTender\BTW\100\" + labelFileName);
                    writer.WriteStartElement("NamedSubString");
                    writer.WriteAttributeString("Name", $"Text");
                    writer.WriteStartElement("Value");
                    writer.WriteCData($"{labelShippingData}");
                    writer.WriteEndElement(); //</Value>
                    writer.WriteEndElement(); //</NamedSubString>
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
            }
            else
            {
                using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + labelClass + " " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
                {

                    writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                    writer.WriteStartElement("XMLScript");
                    writer.WriteAttributeString("Version", @"2.0");
                    writer.WriteStartElement("Command");
                    writer.WriteAttributeString("Name", @"Job1");
                    writer.WriteStartElement("Print");
                    writer.WriteElementString("Format", @"C:\BarTender\BTW\100\" + labelFileName);
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
            }

            //put some error handling here

            labelPrinted = true;

            return labelPrinted;
        }

    }
}

using Dapper;
using OnTargetDataLibrary.DataAccess;
using OnTargetDataLibrary.Models.LabelPrinting;
using OnTargetDataLibrary.Models.SupplyChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BarcodeLib;
using System.Drawing;
using System.IO;

namespace OnTargetDataLibrary.BusinessLogic.SupplyChain
{
    public static class GenerateDecorationTicketProcessor
    {

        //gets all label printers ID, Description and Name based on user selection
        public static List<LabelPrinterModel> LoadLabelPrinters(string labelClassID)
        {

            string databaseConnection = "";

            //set the database connection string based on if in dev or prod
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

        //verifies if the WO# entered is valid
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

        //get all WO details
        public static List<WorkOrderModel> LoadWorkOrder(string workOrderNumber)
        {

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT ptwh.woh_wo_no AS WorkOrderNumber,
		                        ptwh.woh_item_no AS ItemCode,
		                        ptwh.woh_item_short_desc AS ItemDescription,
		                        ptwh.woh_order_qty AS Quantity,
                                ptwh.woh_so_no AS SalesOrderNumber,
                                ptwh.woh_created_by AS IssuedBy,
		                        iii.ibu_category AS ItemCategory
                        FROM pmd_twoh_wo_headr ptwh
                        LEFT JOIN itm_ibu_itemvarhdr iii ON ptwh.woh_item_no = iii.ibu_itemcode
                        WHERE woh_wo_no = @WorkOrderNumber;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderModel>(sql, p);
        }

        //get part colors from the standardbowpartcolors table in MySQL to print on deco ticket
        public static List<StandardBowModel> LoadStandardBowData(string itemCode)
        {

            string databaseConnection = "";

            //set the database connection string based on if in dev or prod
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

            p.Add("@ItemCode", itemCode);

            string sql = @"SELECT ParentItemCode,
		                        ChildItemCode,
                                PartOrder,
                                Color
                        FROM standardbowpartcolors
                        WHERE ParentItemCode = @ItemCode
                        ORDER BY PartOrder;";

            return SQLDataAccess.LoadDataBTIntranet<StandardBowModel>(sql, p, databaseConnection);
        }

        //Get the custom bow attributes from the Pepperi_OrderDetail table on RVWDB to print on deco ticket
        public static List<CustomBowModel> LoadCustomBowData(string salesOrderNumber, string itemCode)
        {

            var p = new DynamicParameters();

            p.Add("@SalesOrderNumber", salesOrderNumber);
            p.Add("@ItemCode", itemCode);

            string sql = @"SELECT CustomBowModel AS BowModel,
		                        CustomBowHand AS BowHand,
		                        CustomBowDrawWeight AS BowDrawWeight,
		                        CustomBowRiserColor AS BowRiserColor,
		                        CustomBowLimbColor AS BowLimbColor,
                                CustomBowGrip AS BowGrip,
                                CustomBowOrbit AS BowOrbit
                        FROM Pepperi_OrderDetail
                        WHERE PepperiOrderNum = @SalesOrderNumber
                        AND ItemCode = @ItemCode
                        AND RevisionNum = 0;";

            return SQLDataAccess.LoadDataDatawarehouse<CustomBowModel>(sql, p);
        }

        //Get the printer name based on what printer the user selected. Printer Name is used in BarTender to print job
        public static List<PrinterNameModel> LoadLabelPrinterName(string printerID)
        {
            string databaseConnection = "";

            //set the database connection string based on if in dev or prod
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

        //Remove the characters from the sales order number.  The Order Details in the data warehouse has only the numerical value of the sales order number (i.e. in Ramco PEP, PT)
        public static string RemoveTextFromSalesOrder(string salesOrderNumber)
        {
            //set what characters are allowed
            var allowedChars = "0123456789-";

            //remove all other characters
            return new string(salesOrderNumber.Where(c => allowedChars.Contains(c)).ToArray());

        }

        //Creates the XML file for Standard Bows to be sent to BarTender
        public static bool GenerateStandardBowDecorationTicket(string labelStandardBowData, string printerName)
        {
            bool ticketPrinted = false;

            string xmlOutputPath = "";
            string labelFileName = "";

            XmlWriterSettings setting = new XmlWriterSettings();
            setting.ConformanceLevel = ConformanceLevel.Auto;
            setting.Encoding = Encoding.UTF8;
            setting.Indent = true;

            //set the database connection string based on if in dev or prod
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

            //determine which label format to use based on printer name
            if (printerName == "\\\\BTPrintserver\\Planning (Lexmark M5255)")
            {
                labelFileName = "200 A6 Standard Bow Lexmark M5255.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Finance (Canon  iR-ADV C356)")
            {
                labelFileName = "200 A6 Standard Bow Canon iR-ADV C356.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Purchasing (HP E57540)")
            {
                labelFileName = "200 A6 Standard Bow HP E57540.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\TicketTest")
            {
                labelFileName = "200 A6 Standard Bow HP 4050N.btw";
            }
            else
            {
                labelFileName = "200 A6 Standard Bow HP E57540.btw";
            }

            //generate the xml file in the Scan folder on BTIntranet (this is where BarTender is looking for xml files to print)
            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + "200 Standard Bow " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {

                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                writer.WriteStartElement("Command");
                writer.WriteAttributeString("Name", @"Job1");
                writer.WriteStartElement("Print");
                writer.WriteElementString("Format", @"C:\BarTender\BTW\200\" + labelFileName);
                writer.WriteStartElement("RecordSet");
                writer.WriteAttributeString("Name", @"200 Standard Bow");
                writer.WriteAttributeString("Type", @"btTextFile");
                writer.WriteElementString("Delimitation", "btDelimCustom");
                writer.WriteElementString("FieldDelimiter", "|");
                writer.WriteElementString("UseFieldNamesFromFirstRecord", "true");
                writer.WriteStartElement("TextData");
                writer.WriteCData($"{labelStandardBowData}");
                writer.WriteEndElement(); //</TextData>
                writer.WriteEndElement(); //</RecordSet>
                writer.WriteStartElement("PrintSetup");
                writer.WriteElementString("Printer", $"{printerName}");
                writer.WriteElementString("IdenticalCopiesOfLabel", "4");
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

        //Creates the XML file for Eva Shockey Bows to be sent to BarTender
        public static bool GenerateStandardBowEvaShockeyDecorationTicket(string labelStandardBowEvaShockeyData, string printerName)
        {
            bool ticketPrinted = false;

            string xmlOutputPath = "";
            string labelFileName = "";

            XmlWriterSettings setting = new XmlWriterSettings();
            setting.ConformanceLevel = ConformanceLevel.Auto;
            setting.Encoding = Encoding.UTF8;
            setting.Indent = true;

            //set the database connection string based on if in dev or prod
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

            //determine which label format to use based on printer name
            if (printerName == "\\\\BTPrintserver\\Planning (Lexmark M5255)")
            {
                labelFileName = "200 A6 Standard Bow Lexmark M5255.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Finance (Canon  iR-ADV C356)")
            {
                labelFileName = "200 A6 Standard Bow Canon iR-ADV C356.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Purchasing (HP E57540)")
            {
                labelFileName = "200 A6 Standard Bow HP E57540.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\TicketTest")
            {
                labelFileName = "200 A6 Standard Bow HP 4050N.btw";
            }
            else
            {
                labelFileName = "200 A6 Standard Bow HP E57540.btw";
            }

            //generate the xml file in the Scan folder on BTIntranet(this is where BarTender is looking for xml files to print)
            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + "200 Standard Bow Eva Shockey " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {

                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                writer.WriteStartElement("Command");
                writer.WriteAttributeString("Name", @"Job1");
                writer.WriteStartElement("Print");
                writer.WriteElementString("Format", @"C:\BarTender\BTW\200\" + labelFileName);
                writer.WriteStartElement("RecordSet");
                writer.WriteAttributeString("Name", @"200 Standard Bow");
                writer.WriteAttributeString("Type", @"btTextFile");
                writer.WriteElementString("Delimitation", "btDelimCustom");
                writer.WriteElementString("FieldDelimiter", "|");
                writer.WriteElementString("UseFieldNamesFromFirstRecord", "true");
                writer.WriteStartElement("TextData");
                writer.WriteCData($"{labelStandardBowEvaShockeyData}");
                writer.WriteEndElement(); //</TextData>
                writer.WriteEndElement(); //</RecordSet>
                writer.WriteStartElement("PrintSetup");
                writer.WriteElementString("Printer", $"{printerName}");
                writer.WriteElementString("IdenticalCopiesOfLabel", "5");
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

        //Creates the XML file for Revolt Bows to be sent to BarTender
        public static bool GenerateStandardBowRevoltDecorationTicket(string labelStandardBowRevoltData, string printerName)
        {
            bool ticketPrinted = false;

            string xmlOutputPath = "";
            string labelFileName = "";

            XmlWriterSettings setting = new XmlWriterSettings();
            setting.ConformanceLevel = ConformanceLevel.Auto;
            setting.Encoding = Encoding.UTF8;
            setting.Indent = true;

            //set the database connection string based on if in dev or prod
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

            //determine which label format to use based on printer name
            if (printerName == "\\\\BTPrintserver\\Planning (Lexmark M5255)")
            {
                labelFileName = "200 A6 Standard Bow Lexmark M5255.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Finance (Canon  iR-ADV C356)")
            {
                labelFileName = "200 A6 Standard Bow Canon iR-ADV C356.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Purchasing (HP E57540)")
            {
                labelFileName = "200 A6 Standard Bow HP E57540.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\TicketTest")
            {
                labelFileName = "200 A6 Standard Bow HP 4050N.btw";
            }
            else
            {
                labelFileName = "200 A6 Standard Bow HP E57540.btw";
            }

            //generate the xml file in the Scan folder on BTIntranet(this is where BarTender is looking for xml files to print)
            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + "200 Standard Bow Revolt " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {

                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                writer.WriteStartElement("Command");
                writer.WriteAttributeString("Name", @"Job1");
                writer.WriteStartElement("Print");
                writer.WriteElementString("Format", @"C:\BarTender\BTW\200\" + labelFileName);
                writer.WriteStartElement("RecordSet");
                writer.WriteAttributeString("Name", @"200 Standard Bow");
                writer.WriteAttributeString("Type", @"btTextFile");
                writer.WriteElementString("Delimitation", "btDelimCustom");
                writer.WriteElementString("FieldDelimiter", "|");
                writer.WriteElementString("UseFieldNamesFromFirstRecord", "true");
                writer.WriteStartElement("TextData");
                writer.WriteCData($"{labelStandardBowRevoltData}");
                writer.WriteEndElement(); //</TextData>
                writer.WriteEndElement(); //</RecordSet>
                writer.WriteStartElement("PrintSetup");
                writer.WriteElementString("Printer", $"{printerName}");
                writer.WriteElementString("IdenticalCopiesOfLabel", "5");
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

        public static bool GenerateCustomBowRevoltDecorationTicket(string labelCustomBowData, string printerName)
        {
            bool ticketPrinted = false;

            //string numberOfIdenticalCopies = "";
            string xmlOutputPath = "";
            string labelFileName = "";

            XmlWriterSettings setting = new XmlWriterSettings();
            setting.ConformanceLevel = ConformanceLevel.Auto;
            setting.Encoding = Encoding.UTF8;
            setting.Indent = true;

            //set the database connection string based on if in dev or prod
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

            //determine which label format to use based on printer name
            if (printerName == "\\\\BTPrintserver\\Planning (Lexmark M5255)")
            {
                labelFileName = "200 A6 Custom Bow Lexmark M5255.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Finance (Canon  iR-ADV C356)")
            {
                labelFileName = "200 A6 Custom Bow Canon iR-ADV C356.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Purchasing (HP E57540)")
            {
                labelFileName = "200 A6 Custom Bow HP E57540.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\TicketTest")
            {
                labelFileName = "200 A6 Custom Bow HP 4050N.btw";
            }
            else
            {
                labelFileName = "200 A6 Custom Bow HP E57540.btw";
            }


            //generate the xml file in the Scan folder on BTIntranet(this is where BarTender is looking for xml files to print)
            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + "200 Customer Bow Revolt " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {

                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                writer.WriteStartElement("Command");
                writer.WriteAttributeString("Name", @"Job1");
                writer.WriteStartElement("Print");
                writer.WriteElementString("Format", @"C:\BarTender\BTW\200\" + labelFileName);
                writer.WriteStartElement("RecordSet");
                writer.WriteAttributeString("Name", @"200 Custom Bow");
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
                writer.WriteElementString("IdenticalCopiesOfLabel", "5");
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

        public static bool GenerateCustomBowDecorationTicket(string labelCustomBowData, string printerName)
        {
            bool ticketPrinted = false;

            //string numberOfIdenticalCopies = "";
            string xmlOutputPath = "";
            string labelFileName = "";

            XmlWriterSettings setting = new XmlWriterSettings();
            setting.ConformanceLevel = ConformanceLevel.Auto;
            setting.Encoding = Encoding.UTF8;
            setting.Indent = true;

            //set the database connection string based on if in dev or prod
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

            //determine which label format to use based on printer name
            if (printerName == "\\\\BTPrintserver\\Planning (Lexmark M5255)")
            {
                labelFileName = "200 A6 Custom Bow Lexmark M5255.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Finance (Canon  iR-ADV C356)")
            {
                labelFileName = "200 A6 Custom Bow Canon iR-ADV C356.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\Purchasing (HP E57540)")
            {
                labelFileName = "200 A6 Custom Bow HP E57540.btw";
            }
            else if (printerName == "\\\\BTPrintserver\\TicketTest")
            {
                labelFileName = "200 A6 Custom Bow HP 4050N.btw";
            }
            else
            {
                labelFileName = "200 A6 Custom Bow HP E57540.btw";
            }


            //generate the xml file in the Scan folder on BTIntranet(this is where BarTender is looking for xml files to print)
            using (XmlWriter writer = XmlWriter.Create(xmlOutputPath + "200 Custom Bow " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".xml", setting))
            {

                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                writer.WriteStartElement("XMLScript");
                writer.WriteAttributeString("Version", @"2.0");
                writer.WriteStartElement("Command");
                writer.WriteAttributeString("Name", @"Job1");
                writer.WriteStartElement("Print");
                writer.WriteElementString("Format", @"C:\BarTender\BTW\200\" + labelFileName);
                writer.WriteStartElement("RecordSet");
                writer.WriteAttributeString("Name", @"200 Custom Bow");
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


        //Gets all children parts that go into the parent for the WO
        public static List<WorkOrderChildPartsModel> LoadWorkOrderChildParts(string workOrderNumber)
        {

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT ptwp.wos_wo_no AS WorkOrderNumber, 
		                        ptwp.wos_item_no AS ItemCode, 
		                        ili.loi_itemdesc AS ItemDescription, 
		                        CONVERT(NUMERIC(28,4),ptwp.wos_tot_qty) AS Quantity,
                                CASE 
			                        WHEN ili.loi_itemdesc LIKE '%RSR%' THEN 'Riser'
			                        WHEN ili.loi_itemdesc LIKE '%LMB%' THEN 'Limb'
			                        WHEN ili.loi_itemdesc LIKE 'HDW FG Body%' THEN 'Flex Guard'
			                        WHEN ili.loi_itemdesc LIKE '%CAM%' THEN 'Cam'
			                        ELSE 'Unknown Part Type'
		                        END AS PartType
                        FROM pmd_twos_wops_postn ptwp (NOLOCK)	
                        LEFT JOIN itm_loi_itemhdr ili ON ptwp.wos_item_no = ili.loi_itemcode
                        LEFT JOIN SAV_MINI_BOM_UDS smbu ON ptwp.wos_item_no = smbu.Item_code
                        WHERE ptwp.wos_io_flag = 'I' 
                        AND ptwp.wos_wo_no = @WorkOrderNumber
                        AND (ili.loi_itemdesc LIKE '%RSR%' OR ili.loi_itemdesc LIKE '%LMB%' OR ili.loi_itemdesc LIKE '%CAM%' OR ili.loi_itemdesc LIKE '%HDW FG Body%')
                        AND (ili.loi_itemdesc NOT LIKE '%CAM Anchor%')
                        AND smbu.prod_class IN ('RSR', 'LMB', 'HDW', 'CAM')
                        ORDER BY CASE WHEN ili.loi_itemdesc LIKE '%RSR%' THEN '1'
			                          WHEN ili.loi_itemdesc LIKE '%LMB%' THEN '2'
			                          WHEN ili.loi_itemdesc LIKE 'HDW FG Body%' THEN '3'
			                          WHEN ili.loi_itemdesc LIKE '%CAM%' THEN '4'
			                          ELSE ili.loi_itemdesc END ASC;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderChildPartsModel>(sql, p);
        }

        //Generates the PDF for the Work Order Pick Sheet
        public static string GetWorkOrderPickSheetString(List<string> workOrderList)
        {

            bool isValidWorkOrder = false;
           
            //create a string builder object to generate the PDF
            var sb = new StringBuilder();

            var lastWorkOrder = workOrderList.Last();

            //loop through each WO 
            foreach (var workOrder in workOrderList)
            {
                //check if the work order is valid
                isValidWorkOrder = CheckWorkOrder(workOrder);

                //if the WO is valid then continue
                if (isValidWorkOrder == true)
                {
                    //create the barcode lib object
                    BarcodeLib.Barcode b = new BarcodeLib.Barcode();
                    Image img = b.Encode(BarcodeLib.TYPE.CODE39, workOrder, Color.Black, Color.White, 350, 30);

                    //convert the image into a byte array
                    byte[] imgBytes = TurnImageToByteArray(img);

                    //convert the byte array into a string
                    string imgString = Convert.ToBase64String(imgBytes);

                    //add the html to the string
                    imgString = String.Format("<img src=\"data:image/Bmp;base64,{0}\">", imgString);

                    //create an HTML string that will be converted to PDF
                    sb.AppendFormat(@"
                                <html>
                                    <head>
                                    </head>
                                    <body>
                                        <div class='header'><h1>{0}</h1></div>
                                        <div align='center'>{1}</div>
                                        <p>Work Order Information:</p>
                                        <table align='center'>
                                            <tr>
                                                <th style='font-size: 11px;'>Work Order Number</th>
                                                <th style='font-size: 11px;'>Item Code</th>
                                                <th style='font-size: 11px;'>Item Description</th>
                                                <th style='font-size: 11px;'>Quantity</th>
                                                <th style='font-size: 11px;'>UOM</th>
                                                <th style='font-size: 11px;'>Warehouse</th>
                                                <th style='font-size: 11px;'>Zone</th>
                                            </tr>", workOrder, imgString);

                    //Get the WO header information from Ramco
                    var workOrderHeaderInfoData = LoadWorkOrderHeaderInfo(workOrder);

                    foreach (var row in workOrderHeaderInfoData)
                    {
                        sb.AppendFormat(@"
                                    <tr>
                                    <td style='font-size: 11px;'>{0}</td>
                                    <td style='font-size: 11px;'>{1}</td>
                                    <td style='font-size: 11px;'>{2}</td>
                                    <td style='font-size: 11px;'>{3}</td>
                                    <td style='font-size: 11px;'>{4}</td>
                                    <td style='font-size: 11px;'>{5}</td>
                                    <td style='font-size: 11px;'>{6}</td>
                                    </tr>", row.WorkOrderNumber, row.ItemCode, row.ItemDescription, row.Quantity, row.UOM, row.Warehouse, row.Zone);
                    }

                    sb.Append(@"
                            </table>
                        ");

                    sb.Append(@"
                            <p>Routing Steps:</p>
                            <table align='center'>
                                <tr>
                                    <th>Work Order Number</th>
                                    <th>Step</th>
                                    <th>Costed</th>                                    
                                    <th>Resource Number</th>
                                    <th style='width: 75px;'>Activity Number</th>
                                    <th style='width: 150px;'>Activity Description</th>
                                    <th>Minute Per Activity</th>
                                    <th>Total Minutes</th>
                                    <th>Total Hours</th>
                                    <th>Accepted Quantity</th>
                                    <th>Rejected Quantity</th>
                                </tr>");

                    //Get the WO Routing steps from Ramco
                    var workOrderRoutingStepsData = LoadWorkOrderRoutingSteps(workOrder);

                    foreach (var row in workOrderRoutingStepsData)
                    {

                        sb.AppendFormat(@"
                                    <tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    <td>{6}</td>
                                    <td>{7}</td>
                                    <td>{8}</td>
                                    <td>{9}</td>
                                    <td>{10}</td>
                                    </tr>", row.WorkOrderNumber, row.Step, row.Costed, row.ResourceNumber, row.ActivityNumber, row.ActivityDescription, row.MinutePerActivity, row.TotalMinutes, row.TotalHours, row.AcceptedQuantity, row.RejectedQuantity);
                    }

                    sb.Append(@"
                            </table>
                        ");

                    sb.Append(@"
                            <p>Input Items:</p>
                            <table align='center'>
                                <tr>
                                    <th>Work Order Number</th>
                                    <th>Step</th>
                                    <th>Item Number</th>                                    
                                    <th style='width: 150px;'>Item Description</th>
                                    <th>Quantity</th>
                                    <th>Source</th>
                                    <th>UOM</th>
                                    <th>Warehouse</th>
                                    <th>Zone</th>
                                    <th>Stock Quantity</th>
                                    <th>Short ?</th>
                                </tr>");

                    //Get the WO input items from Ramco
                    var workOrderInputItemsData = LoadWorkOrderInputItems(workOrder);

                    foreach (var row in workOrderInputItemsData)
                    {
                        sb.AppendFormat(@" 
                                    <tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    <td>{6}</td>
                                    <td>{7}</td>
                                    <td>{8}</td>
                                    <td>{9}</td>
                                    <td>{10}</td>
                                    </tr>", row.WorkOrderNumber, row.Step, row.ItemCode, row.ItemDescription, row.Quantity, row.Source, row.UOM, row.Warehouse, row.Zone, row.StockQuantity, row.Short);
                    }

                    sb.Append(@"
                            </table>
                        ");

                    sb.Append(@"
                            <p>Enter Scrap Reporting:</p>
                            <table align='center'>
		                        <tr>
			                    <td width='130'>Date</td>
			                    <td width='130'>Op #</td>
			                    <td width='130'>Quantity</td>
			                    <td width='130'>Scrap Code</td>
			                    <td width='130'>Initials/Shift</td>
			                    <td width='130'>Lead Initials</td>
		                        </tr>
		                        <tr>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
		                        </tr>
		                        <tr>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
		                        </tr>
		                        <tr>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
		                        </tr>
		                        <tr>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
			                    <td height='20'>&nbsp;</td>
		                        </tr>
		                        <tr>
                                </table>
                            ");


                    sb.Append(@"
                            <p>Scrap Code Legend:</p>
                            <table align='center'>
		                        <tr>
			                    <td width='250'>A - Defective Raw Mtl (Warped, Damaged)</td>
			                    <td width='250'>B - Program Error</td>
			                    <td width='250'>C - Broken Tool</td>
		                        </tr>
		                        <tr>
			                    <td>D - Job Set-up Error</td>
			                    <td>D1 - Bad Mill Part</td>
			                    <td>D2 - Paint Poor Texture</td>
		                        </tr>
		                        <tr>
			                    <td>D3 - Paint Too Thin</td>
			                    <td>D4 - Paint Too Thick</td>
			                    <td>D5 - Paint Wrong Color</td>
		                        </tr>
		                        <tr>
			                    <td>D6 - Dip Wrong Film</td>
			                    <td>D7 - Coating Too Thick</td>
			                    <td>D8 - Coating Poor Texture</td>
		                        </tr>
		                        <tr>
			                    <td>D9 - Coating Contamination</td>
			                    <td>D10 - Coating Poor Coverage</td>
			                    <td>D11 - Dye Sub Failed Transfer</td>
		                        </tr>
		                        <tr>
			                    <td>E - Part Loading Error</td>
			                    <td>F - Out of Dimension Tolerance</td>
			                    <td>G - Cracked Limb</td>
		                        </tr>
		                        <tr>
			                    <td>H - Tool Calibration Error</td>
			                    <td>I - Prototyping</td>
			                    <td>J - Part Pulled from Fixture</td>
		                        </tr>
		                        <tr>
			                    <td>K - Fixturing Defect</td>
			                    <td>L - Drops</td>
			                    <td>M - WJ - Tool Wear</td>
		                        </tr>
		                        <tr>
			                    <td>N - Fiber Tear Outs</td>
			                    <td>O - Laminating Issue</td>
			                    <td>OE - Operator Error</td>
		                        </tr>
		                        <tr>
			                    <td>P - Low Def</td>
			                    <td>Q - Step in Limb</td>
			                    <td>R - OP-2 Drilling</td>
		                        </tr>
		                        <tr>
			                    <td>S1 - Strings Wind</td>
			                    <td>S2 - Strings Build</td>
			                    <td>S3 - Strings Twist</td>
		                        </tr>
		                        <tr>
			                    <td>S4 - Strings Serve</td>
			                    <td>S5 - Strings Prep</td>
			                    <td>S6 - Strings Spec</td>
		                        </tr>
		                    </table>
                            
                        ");

                    //if it is not the last WO in the list of work orders add a page break for the next work order pick list
                    if (workOrder != lastWorkOrder)
                    {
                        sb.Append(@"
                            <div class='pagebreak'></div>
                        ");
                    }
                }
                else
                {   
                    //if the WO entered into the list is not valid, create a page in the PDF document letting the user know that the WO was not valid
                    sb.AppendFormat(@"   
                       <p>{0} is not a valid Work order. Please verify and try again.</p>
                        ", workOrder);

                    if (workOrder != lastWorkOrder)
                    {
                        sb.Append(@"
                            <div class='pagebreak'></div>
                        ");
                    }
                }
            }

            //end the PDF document on the last WO in the list
            sb.Append(@"   
                        </body>
                    </html>
                        ");

            return sb.ToString();
           
        }

        //method that turns an image into a byte array
        private static byte[] TurnImageToByteArray(System.Drawing.Image img)
        {
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            return ms.ToArray();
        }

        //Get the WO header info from Ramco
        public static List<WorkOrderPickSheetHeaderInfoModel> LoadWorkOrderHeaderInfo(string workOrderNumber)
        {

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT ptwp.wos_wo_no AS WorkOrderNumber, 
		                        ptwp.wos_item_no AS ItemCode, 
		                        ili.loi_itemdesc AS ItemDescription, 
		                        CONVERT(INT,ptwp.wos_tot_qty) AS Quantity, 
		                        ptwp.wos_uom AS UOM, 
		                        iii.iou_stdwhcode AS Warehouse, 
		                        iww.wpp_zone AS Zone
                        FROM pmd_twos_wops_postn ptwp (NOLOCK)
                        LEFT JOIN itm_iou_itemvarhdr iii  (NOLOCK) ON ptwp.wos_item_no = iii.iou_itemcode
                        LEFT JOIN itm_wpp_whplanparam iww  (NOLOCK) ON ptwp.wos_item_no = iww.wpp_itemcode AND iii.iou_stdwhcode=iww.wpp_warehouse
                        LEFT JOIN itm_loi_itemhdr ili (NOLOCK) ON ptwp.wos_item_no = ili.loi_itemcode
                        WHERE ptwp.wos_io_flag='O' 
                        AND ptwp.wos_wo_no = @WorkOrderNumber 
                        ORDER BY ptwp.wos_wo_no, ili.loi_itemdesc;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderPickSheetHeaderInfoModel>(sql, p);
        }

        //Get the WO routing steps from Ramco
        public static List<WorkOrderPickSheetRoutingStepsModel> LoadWorkOrderRoutingSteps(string workOrderNumber)
        {

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT ptwp.wop_wo_no AS WorkOrderNumber, 
		                            ptwp.wop_pp_pos_no AS Step, 
		                            CASE WHEN pmam.ams_costing_reqd = 1 THEN 'Y' ELSE 'N' END AS Costed, 
		                            pmrr.rrq_res_no AS ResourceNumber, 
		                            ptwp.wop_acty_no AS ActivityNumber, 
		                            pmam.ams_full_desc AS ActivityDescription,
		                            ((ptwp.wop_tot_time / 60) * ptwp.wop_qty_per) / wop_planned_qty AS MinutePerActivity,
		                            (ptwp.wop_tot_time / 60) * ptwp.wop_qty_per AS TotalMinutes, 
		                            (ptwp.wop_tot_time / 3600) * ptwp.wop_qty_per AS TotalHours, 
		                            '' AS AcceptedQuantity, 
		                            '' AS RejectedQuantity
                            FROM pmd_twop_wopp_postn ptwp 
                            LEFT JOIN pmd_mams_acty_mastr pmam ON ptwp.wop_acty_no = pmam.ams_acty_skey
                            LEFT JOIN pmd_mrrq_res_reqmt pmrr ON pmam.ams_acty_no = pmrr.rrq_acty_no
                            WHERE ptwp.wop_pp_pos_no <> 0 AND 
                            ptwp.wop_wo_no = @WorkOrderNumber
                            ORDER BY ptwp.wop_pp_pos_no;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderPickSheetRoutingStepsModel>(sql, p);
        }

        //Get the WO order input items from Ramco
        public static List<WorkOrderPickSheetInputItemsModel> LoadWorkOrderInputItems(string workOrderNumber)
        {

            var p = new DynamicParameters();

            p.Add("@WorkOrderNumber", workOrderNumber);

            string sql = @"SELECT	T1.wos_wo_no AS WorkOrderNumber, 
		                            T1.wos_pp_pos_no AS Step, 
		                            T1.wos_item_no AS ItemCode, 
		                            T5.loi_itemdesc AS ItemDescription, 
		                            CONVERT(NUMERIC(28,4),T1.wos_tot_qty) AS Quantity, 
		                            CASE 
			                            WHEN T2.iou_mfg_source='1' AND T2.iou_purchase_source = '0' THEN 'M'
			                            WHEN T2.iou_mfg_source='1' AND T2.iou_purchase_source = '1' THEN 'M&P'
			                            WHEN T2.iou_mfg_source='0' AND T2.iou_purchase_source = '1' THEN 'P'
			                            ELSE '?'
		                            END AS Source, 
		                            T1.wos_uom AS UOM, 
		                            T2.iou_stdwhcode AS Warehouse, 
		                            T3.wpp_zone AS Zone, 
		                            ISNULL(T4.sbn_quantity,0) AS StockQuantity,
		                            CASE
			                            WHEN ISNULL(T4.sbn_quantity,0)<T1.wos_tot_qty THEN 'YES'
			                            ELSE ''
		                            END AS [Short]
                            FROM pmd_twos_wops_postn T1 (NOLOCK)
	                            LEFT JOIN itm_iou_itemvarhdr T2 (NOLOCK) ON T1.wos_item_no = T2.iou_itemcode
	                            LEFT JOIN itm_wpp_whplanparam T3 (NOLOCK) ON T1.wos_item_no = T3.wpp_itemcode AND T2.iou_stdwhcode = T3.wpp_warehouse
	                            LEFT JOIN skm_stockbal_nonctrl T4 (NOLOCK) ON T1.wos_item_no = T4.sbn_item_code AND T2.iou_stdwhcode = T4.sbn_wh_code AND T3.wpp_zone = T4.sbn_zone AND T4.sbn_stock_status = 'ACC'
	                            LEFT JOIN itm_loi_itemhdr T5 ON T1.wos_item_no = T5.loi_itemcode
                            WHERE T1.wos_io_flag = 'I' AND T1.wos_wo_no = @WorkOrderNumber
                            ORDER BY T1.wos_wo_no, T1.wos_full_desc;";

            return SQLDataAccess.LoadDataSCMDB<WorkOrderPickSheetInputItemsModel>(sql, p);
        }

    }
}

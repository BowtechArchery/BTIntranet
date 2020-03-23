using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using OnTarget.Models.SupplyChain;
using Microsoft.Extensions.Configuration;
using static OnTargetDataLibrary.BusinessLogic.SupplyChain.ExclusionItemProcessor;
using static OnTargetDataLibrary.BusinessLogic.SupplyChain.OpenOrderProcessor;
using static OnTargetDataLibrary.BusinessLogic.SupplyChain.GenerateDecorationTicketProcessor;
using static OnTargetDataLibrary.BusinessLogic.SupplyChain.StandardBowPartColorProcessor;
using OnTargetLibrary.Security;
using Microsoft.AspNetCore.Mvc.Rendering;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.IO;
using BarcodeLib;
using System.Drawing;

namespace OnTarget.Controllers.SupplyChain
{
    public class SupplyChainController : Controller
    {
        //reads from the appsettings.json (see action ItemExclusions at the bottom of this controller) this is currently not used and is an example only
        private readonly IConfiguration configuration;

        private readonly IConverter _converter;

        //reads from the appsettings.json (see action ItemExclusions at the bottom of this controller) this is currently not used and is an example only
        public SupplyChainController(IConfiguration config, IConverter converter)
        {
            this.configuration = config;
            _converter = converter;
        }

        //[Authorize(Policy = "Administration")] 
        //[MultiplePolicysAuthorize("Administration;Supply Chain", true)]
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult Index()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult ExclusionItems()
        {
            return View();
        }

        //loads the exclusion items into the Telerik Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult ExclusionItems_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetExclusionItems().ToDataSourceResult(request));
        }

        //gets all exclusion items and puts them in a list
        private static IEnumerable<ExclusionItemModel> GetExclusionItems()
        {
            var data = LoadExclusionItems();

            List<ExclusionItemModel> exclusionitems = new List<ExclusionItemModel>();

            foreach (var row in data)
            {
                exclusionitems.Add(new ExclusionItemModel
                {
                    ItemCode = row.ItemCode,
                    ItemDesc = row.ItemDesc
                });
            }

            return exclusionitems;
        }

        //inserts an exclusion item in the Telerick Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        [AcceptVerbs("Post")]
        public IActionResult ExclusionItems_Insert([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ExclusionItemModel> exclusionItems)
        {
    
            var results = new List<ExclusionItemModel>();

            if (exclusionItems != null && ModelState.IsValid)
            {
                foreach (var exclusionItem in exclusionItems)
                {
                    bool isValidItemCode = false;

                    //check if the item code is valid
                    isValidItemCode = CheckItemCode(exclusionItem.ItemCode);

                    if(isValidItemCode == true)
                    { 
                        int recordsCreated = InsertExclusionItem(exclusionItem.ItemCode);
                        results.Add(exclusionItem);
                    }
                    else
                    {
                        results.Remove(exclusionItem);
                        ModelState.AddModelError("Invalid Item Code", $"{exclusionItem.ItemCode} is an invalid Item Code.");
                    }
                }
            }

            return Json(results.ToDataSourceResult(request, ModelState));
        }

        //updates an exclusion item in the Telerick Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        [AcceptVerbs("Post")]
        public IActionResult ExclusionItems_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ExclusionItemModel> exclusionItems)
        {
            if (exclusionItems != null && ModelState.IsValid)
            {
                foreach (var exclusionItem in exclusionItems)
                {
                    int recordsUpdated = UpdateExclusionItem(exclusionItem.ItemCode, exclusionItem.ItemDesc);
                }
            }

            return Json(exclusionItems.ToDataSourceResult(request, ModelState));
        }

        //deletes an exclusion item in the Telerick Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        [AcceptVerbs("Post")]
        public IActionResult ExclusionItems_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ExclusionItemModel> exclusionItems)
        {
            if (exclusionItems.Any())
            {
                foreach (var exclusionItem in exclusionItems)
                {
                    int recordsUpdated = DeleteExclusionItem(exclusionItem.ItemCode);
                }
            }

            return Json(exclusionItems.ToDataSourceResult(request, ModelState));
        }

        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult ViewOpenOrders()
        {

            return View();
        }

        //loads open orders into the Telerik Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult OpenOrders_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetOpenOrders().ToDataSourceResult(request));
        }

        //gets all open orders and puts them in a list
        private static IEnumerable<OpenOrderModel> GetOpenOrders()
        {
            var data = LoadOpenOrders();

            List<OpenOrderModel> openorders = new List<OpenOrderModel>();

            foreach (var row in data)
            {
                openorders.Add(new OpenOrderModel
                {
                    SalesOrderNumber = row.SalesOrderNumber,
                    SalesOrderLineNumber = row.SalesOrderLineNumber,
                    ItemCode = row.ItemCode,
                    ItemDescription = row.ItemDescription,
                    CustomerName = row.CustomerName,
                    CustomerNumber = row.CustomerNumber,
                    DealerStatus = row.DealerStatus,
                    OrderStatus = row.OrderStatus,
                    AllocatedQuantity = row.AllocatedQuantity,
                    Priority = row.Priority,
                    OpenQuantity = row.OpenQuantity,
                    OrderDate = row.OrderDate,
                    RequiredDate = row.RequiredDate,
                    PromiseDate = row.PromiseDate,
                    ModifiedDate = row.ModifiedDate,
                    DNSBefore = row.DNSBefore,
                    Commments = row.Commments,
                    WorkOrderNumber = row.WorkOrderNumber,
                    CustomBowModel = row.CustomBowModel,
                    CustomBowHand = row.CustomBowHand,
                    CustomBowDrawWeight = row.CustomBowDrawWeight,
                    CustomBowRiserColor = row.CustomBowRiserColor,
                    CustomBowLimbColor = row.CustomBowLimbColor,
                    CustomBowGrip = row.CustomBowGrip,
                    CustomBowOrbit = row.CustomBowOrbit
                });
            }

            return openorders;
        }

        //exports the contents of the Telerik Grid into an Excel spreadsheet
        [HttpPost]
        public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        //get printers that can print class 200 lables and load them in the drop down for user selection
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult GenerateDecorationTicket()
        {

            //set the label class as 200 for decoration ticket
            string labelClassID = "200";

            var data = LoadLabelPrinters(labelClassID);

            var decorationTicketViewModel = new DecorationTicketViewModel();

            decorationTicketViewModel.LabelPrinters = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                decorationTicketViewModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            return View(decorationTicketViewModel);
        }

        //generate the decoration ticket
        [HttpPost]
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult GenerateDecorationTicket(DecorationTicketViewModel decorationTicketViewModel)
        {
           
            if(decorationTicketViewModel != null && ModelState.IsValid)
            {
                bool isWorkOrderError = false;
                bool customBowTicketPrinted = false;
                bool customBowRevoltTicketPrinted = false;
                bool standardBowEvaShockeyTicketPrinted = false;
                bool standardBowRevoltTicketPrinted = false;
                bool standardBowTicketPrinted = false;
                bool ticketPrintError = false;

                string errors = "";
                string printerName = "";

                string labelCustomBowData = "";
                string labelCustomBowRevoltData = "";
                string labelStandardBowEvaShockeyData = "";
                string labelStandardBowRevoltData = "";
                string labelStandardBowData = "";

                //define the field names for both the custom and standard bow data
                string labelCustomBowFieldNames = $"FGPartNumber|FGPartDescription|FGUnits|SalesOrder|WorkOrderNumber|ChildPartNumber1|ChildPartDescription1|ChildQty1|ChildPartNumber2|ChildPartDescription2|ChildQty2|ChildPartNumber3|ChildPartDescription3|ChildQty3|ChildPartNumber4|ChildPartDescription4|ChildQty4|ChildPartNumber5|ChildPartDescription5|ChildQty5|ChildPartNumber6|ChildPartDescription6|ChildQty6|ChildPartNumber7|ChildPartDescription7|ChildQty7|BowModel|BowHand|BowWeight|BowRiserColor|BowLimbColor|BowGrip|BowOrbit|IssueDate|IssueBy|CompletionDate";
                string labelCustomBowRevoltFieldNames = $"FGPartNumber|FGPartDescription|FGUnits|SalesOrder|WorkOrderNumber|ChildPartNumber1|ChildPartDescription1|ChildQty1|ChildPartNumber2|ChildPartDescription2|ChildQty2|ChildPartNumber3|ChildPartDescription3|ChildQty3|ChildPartNumber4|ChildPartDescription4|ChildQty4|ChildPartNumber5|ChildPartDescription5|ChildQty5|ChildPartNumber6|ChildPartDescription6|ChildQty6|ChildPartNumber7|ChildPartDescription7|ChildQty7|BowModel|BowHand|BowWeight|BowRiserColor|BowLimbColor|BowGrip|BowOrbit|IssueDate|IssueBy|CompletionDate";
                string labelStandardBowEvaShockeyFieldNames = $"FGPartNumber|FGPartDescription|FGUnits|WorkOrderNumber|ChildPartNumber1|ChildPartDescription1|ChildQty1|ChildPartNumber2|ChildPartDescription2|ChildQty2|ChildPartNumber3|ChildPartDescription3|ChildQty3|ChildPartNumber4|ChildPartDescription4|ChildQty4|ChildPartNumber5|ChildPartDescription5|ChildQty5|ChildPartNumber6|ChildPartDescription6|ChildQty6|ChildPartNumber7|ChildPartDescription7|ChildQty7|PartColorLabel1|PartColor1|PartColorLabel2|PartColor2|PartColorLabel3|PartColor3|PartColorLabel4|PartColor4|PartColorLabel5|PartColor5|IssueDate|IssueBy|CompletionDate";
                string labelStandardBowRevoltFieldNames = $"FGPartNumber|FGPartDescription|FGUnits|WorkOrderNumber|ChildPartNumber1|ChildPartDescription1|ChildQty1|ChildPartNumber2|ChildPartDescription2|ChildQty2|ChildPartNumber3|ChildPartDescription3|ChildQty3|ChildPartNumber4|ChildPartDescription4|ChildQty4|ChildPartNumber5|ChildPartDescription5|ChildQty5|ChildPartNumber6|ChildPartDescription6|ChildQty6|ChildPartNumber7|ChildPartDescription7|ChildQty7|PartColorLabel1|PartColor1|PartColorLabel2|PartColor2|PartColorLabel3|PartColor3|PartColorLabel4|PartColor4|PartColorLabel5|PartColor5|IssueDate|IssueBy|CompletionDate";
                string labelStandardBowFieldNames = $"FGPartNumber|FGPartDescription|FGUnits|WorkOrderNumber|ChildPartNumber1|ChildPartDescription1|ChildQty1|ChildPartNumber2|ChildPartDescription2|ChildQty2|ChildPartNumber3|ChildPartDescription3|ChildQty3|ChildPartNumber4|ChildPartDescription4|ChildQty4|ChildPartNumber5|ChildPartDescription5|ChildQty5|ChildPartNumber6|ChildPartDescription6|ChildQty6|ChildPartNumber7|ChildPartDescription7|ChildQty7|PartColorLabel1|PartColor1|PartColorLabel2|PartColor2|PartColorLabel3|PartColor3|PartColorLabel4|PartColor4|PartColorLabel5|PartColor5|IssueDate|IssueBy|CompletionDate";

                //get the printer that was selected on post
                var labelPrinterData = LoadLabelPrinterName(decorationTicketViewModel.SelectedPrinterID.ToString());

                //load the printer name into a string
                foreach (var row in labelPrinterData)
                {
                    printerName =  row.PrinterName;
                }

                //put the date selected from the view into a string.  This will pass as data to the BarTender label
                string completionDate = decorationTicketViewModel.CompletionDate.ToString();

                //this is the dictionary that will hold data for the BarTender label
                Dictionary<string, string> DecorationTicketData = new Dictionary<string, string>();

                //split the Work Order numbers into a list
                List<string> workOrderList = decorationTicketViewModel.WorkOrders.ToString().Replace("\r\n", "\n").Split("\n").ToList();

                //remove any null or white space values from the list if the user accidentally entered them into the text area
                workOrderList = workOrderList.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                //loop through each work order in the list
                foreach (var workOrder in workOrderList)
                {

                    bool isValidWorkOrder = false;

                    //check if the work order is valid
                    isValidWorkOrder = CheckWorkOrder(workOrder);

                    //if the work order is valid then get details and start creating xml for BarTender
                    //else append invalid work order to error string
                    if (isValidWorkOrder == true)
                    {

                        //clear the dictionary for each work order
                        DecorationTicketData.Clear();

                        //load the work order header data into variable
                        var workOrderData = LoadWorkOrder(workOrder);

                        //add the work order header data into the dictionary
                        foreach (var row in workOrderData)
                        {
                            DecorationTicketData.Add("WorkOrderNumber", row.WorkOrderNumber);
                            DecorationTicketData.Add("ItemCode", row.ItemCode);
                            DecorationTicketData.Add("ItemDescription", row.ItemDescription);
                            DecorationTicketData.Add("SalesOrderNumber", row.SalesOrderNumber);
                            DecorationTicketData.Add("Quantity", row.Quantity.ToString());
                            DecorationTicketData.Add("IssuedBy", row.IssuedBy);
                            DecorationTicketData.Add("ItemCategory", row.ItemCategory);

                        }

                        //if the item code is a 40000 then it is a custom bow and we have to get the decoration data from order details table in the data warehouse
                        //otherwise it is a standard bow and we get the decoration data from the standardbowpartcolors table on the intranet


                        //*****************Custom Bow Logic*****************
                        if (DecorationTicketData["ItemCode"].IndexOf("40000") != -1)
                        {
                            if (DecorationTicketData["SalesOrderNumber"] != null)
                            {
                                int i = 1;

                                //get all the child parts for the work order
                                var workOrderChildPartData = LoadWorkOrderChildParts(workOrder);

                                //loop through all child parts in the work order
                                foreach (var row in workOrderChildPartData)
                                {
                                    DecorationTicketData.Add("ChildPartNumber" + i.ToString(), row.ItemCode);

                                    //have to check if the string length is greater than 18 before performing substring
                                    if (row.ItemDescription.Length > 36)
                                    {
                                        DecorationTicketData.Add("ChildPartDescription" + i.ToString(), row.ItemDescription.Substring(0, 36));
                                    }
                                    else
                                    {
                                        DecorationTicketData.Add("ChildPartDescription" + i.ToString(), row.ItemDescription);
                                    }

                                    DecorationTicketData.Add("ChildPartQuantity" + i.ToString(), row.Quantity.ToString());

                                    i += 1;
                                }

                                //Remove the characters from the order number.  The Order Details in the data warehouse has only the numerical value of the sales order number (i.e. in Ramco PEP, PT)
                                string salesOrderNumberNoChars = RemoveTextFromSalesOrder(DecorationTicketData["SalesOrderNumber"]);

                                //load the custom bow attributes from the data warehouse
                                var customBowData = LoadCustomBowData(salesOrderNumberNoChars, DecorationTicketData["ItemCode"]);

                                //check to make sure that there was only one custom bow item code order line returned from the Order Details table
                                if (customBowData.Count == 1)
                                {

                                    //loop through the custom bow attributes and load them into the dictionary
                                    foreach (var row in customBowData)
                                    {
                                        DecorationTicketData.Add("BowModel", row.BowModel);
                                        DecorationTicketData.Add("BowHand", row.BowHand);
                                        DecorationTicketData.Add("BowWeight", row.BowDrawWeight);
                                        DecorationTicketData.Add("BowRiserColor", row.BowRiserColor);
                                        DecorationTicketData.Add("BowLimbColor", row.BowLimbColor);
                                        DecorationTicketData.Add("BowGrip", row.BowGrip);
                                        DecorationTicketData.Add("BowOrbit", row.BowOrbit);


                                    }

                                    //check to see if all dictionary keys are populated, if not then create the key and give it a "" value
                                    for (int index = 1; index < 8; index++)
                                    {
                                        if (!DecorationTicketData.ContainsKey("ChildPartNumber" + index.ToString()))
                                        {
                                            DecorationTicketData.Add("ChildPartNumber" + index.ToString(), "");
                                        }

                                        if (!DecorationTicketData.ContainsKey("ChildPartDescription" + index.ToString()))
                                        {
                                            DecorationTicketData.Add("ChildPartDescription" + index.ToString(), "");
                                        }

                                        if (!DecorationTicketData.ContainsKey("ChildPartQuantity" + index.ToString()))
                                        {
                                            DecorationTicketData.Add("ChildPartQuantity" + index.ToString(), "");
                                        }
                                    }

                                    //check to see if all dictionary keys are populated, if not then create the key and give it a "" value
                                    if (!DecorationTicketData.ContainsKey("BowModel"))
                                    {
                                        DecorationTicketData.Add("BowModel", "");
                                    }

                                    if (!DecorationTicketData.ContainsKey("BowHand"))
                                    {
                                        DecorationTicketData.Add("BowHand", "");
                                    }

                                    if (!DecorationTicketData.ContainsKey("BowWeight"))
                                    {
                                        DecorationTicketData.Add("BowWeight", "");
                                    }

                                    if (!DecorationTicketData.ContainsKey("BowRiserColor"))
                                    {
                                        DecorationTicketData.Add("BowRiserColor", "");
                                    }

                                    if (!DecorationTicketData.ContainsKey("BowLimbColor"))
                                    {
                                        DecorationTicketData.Add("BowLimbColor", "");
                                    }

                                    if (!DecorationTicketData.ContainsKey("BowGrip"))
                                    {
                                        DecorationTicketData.Add("BowGrip", "");
                                    }

                                    if (!DecorationTicketData.ContainsKey("BowOrbit"))
                                    {
                                        DecorationTicketData.Add("BowOrbit", "");
                                    }

                                    if (DecorationTicketData["ItemCategory"].IndexOf("REVOLT") != -1 || DecorationTicketData["ItemCategory"].IndexOf("REVTX") != -1)
                                    {
                                        //load the label data into a string
                                        labelCustomBowRevoltData = $"{labelCustomBowRevoltData} {Environment.NewLine} {DecorationTicketData["ItemCode"]}|{DecorationTicketData["ItemDescription"]}|{DecorationTicketData["Quantity"]}|{DecorationTicketData["SalesOrderNumber"]}|{DecorationTicketData["WorkOrderNumber"]}|{DecorationTicketData["ChildPartNumber1"]}|{DecorationTicketData["ChildPartDescription1"]}|{DecorationTicketData["ChildPartQuantity1"]}|{DecorationTicketData["ChildPartNumber2"]}|{DecorationTicketData["ChildPartDescription2"]}|{DecorationTicketData["ChildPartQuantity2"]}|{DecorationTicketData["ChildPartNumber3"]}|{DecorationTicketData["ChildPartDescription3"]}|{DecorationTicketData["ChildPartQuantity3"]}|{DecorationTicketData["ChildPartNumber4"]}|{DecorationTicketData["ChildPartDescription4"]}|{DecorationTicketData["ChildPartQuantity4"]}|{DecorationTicketData["ChildPartNumber5"]}|{DecorationTicketData["ChildPartDescription5"]}|{DecorationTicketData["ChildPartQuantity5"]}|{DecorationTicketData["ChildPartNumber6"]}|{DecorationTicketData["ChildPartDescription6"]}|{DecorationTicketData["ChildPartQuantity6"]}|{DecorationTicketData["ChildPartNumber7"]}|{DecorationTicketData["ChildPartDescription7"]}|{DecorationTicketData["ChildPartQuantity7"]}|{DecorationTicketData["BowModel"]}|{DecorationTicketData["BowHand"]}|{DecorationTicketData["BowWeight"]}|{DecorationTicketData["BowRiserColor"]}|{DecorationTicketData["BowLimbColor"]}|{DecorationTicketData["BowGrip"]}|{DecorationTicketData["BowOrbit"]}|{DateTime.Now.ToString("M/d/yyyy")}|{DecorationTicketData["IssuedBy"]}|{String.Format("{0:M/d/yyyy}", completionDate)}";

                                    }
                                    else 
                                    {
                                        //load the label data into a string
                                        labelCustomBowData = $"{labelCustomBowData} {Environment.NewLine} {DecorationTicketData["ItemCode"]}|{DecorationTicketData["ItemDescription"]}|{DecorationTicketData["Quantity"]}|{DecorationTicketData["SalesOrderNumber"]}|{DecorationTicketData["WorkOrderNumber"]}|{DecorationTicketData["ChildPartNumber1"]}|{DecorationTicketData["ChildPartDescription1"]}|{DecorationTicketData["ChildPartQuantity1"]}|{DecorationTicketData["ChildPartNumber2"]}|{DecorationTicketData["ChildPartDescription2"]}|{DecorationTicketData["ChildPartQuantity2"]}|{DecorationTicketData["ChildPartNumber3"]}|{DecorationTicketData["ChildPartDescription3"]}|{DecorationTicketData["ChildPartQuantity3"]}|{DecorationTicketData["ChildPartNumber4"]}|{DecorationTicketData["ChildPartDescription4"]}|{DecorationTicketData["ChildPartQuantity4"]}|{DecorationTicketData["ChildPartNumber5"]}|{DecorationTicketData["ChildPartDescription5"]}|{DecorationTicketData["ChildPartQuantity5"]}|{DecorationTicketData["ChildPartNumber6"]}|{DecorationTicketData["ChildPartDescription6"]}|{DecorationTicketData["ChildPartQuantity6"]}|{DecorationTicketData["ChildPartNumber7"]}|{DecorationTicketData["ChildPartDescription7"]}|{DecorationTicketData["ChildPartQuantity7"]}|{DecorationTicketData["BowModel"]}|{DecorationTicketData["BowHand"]}|{DecorationTicketData["BowWeight"]}|{DecorationTicketData["BowRiserColor"]}|{DecorationTicketData["BowLimbColor"]}|{DecorationTicketData["BowGrip"]}|{DecorationTicketData["BowOrbit"]}|{DateTime.Now.ToString("M/d/yyyy")}|{DecorationTicketData["IssuedBy"]}|{String.Format("{0:M/d/yyyy}", completionDate)}";

                                    }

                                }
                                //if there is more than one record in customBowData that means there were duplicate custom item code lines on the order and they didn't get split
                                else if (customBowData.Count > 1)
                                {
                                    isWorkOrderError = true;
                                    errors += $"{workOrder} has duplicate custom item code lines. Please create separate orders for each code and try again" + "\\r\\n";
                                }
                                //if customBowData.count is not > 1 then there is a missing Sales Order Number in the Work Order
                                else
                                {
                                    isWorkOrderError = true;
                                    errors += $"{workOrder} is missing a Sales Order Number. Please edit the WO with a Sales Order and try again" + "\\r\\n";
                                }
                            }
                            //if DecorationTicketData["SalesOrderNumber"] is null then the sales order field in the WO header in ramco is not populated
                            else
                            {
                                isWorkOrderError = true;
                                errors += $"{workOrder} is missing a Sales Order Number. Please edit the WO with a Sales Order and try again" + "\\r\\n";
                            }

                        }

                        //*****************Eva Shockey Standard Bow Logic*****************
                        else if (DecorationTicketData["ItemCategory"].IndexOf("ESHOCK") != -1)
                        {
                            int i = 1;

                            //get all the child parts for the work order
                            var workOrderChildPartData = LoadWorkOrderChildParts(workOrder);

                            //loop through all child parts in the work order
                            foreach (var row in workOrderChildPartData)
                            {
                                DecorationTicketData.Add("ChildPartNumber" + i.ToString(), row.ItemCode);

                                DecorationTicketData.Add("ChildPartDescription" + i.ToString(), row.ItemDescription);

                                DecorationTicketData.Add("ChildPartQuantity" + i.ToString(), row.Quantity.ToString());

                                i += 1;
                            }

                            //check to see if all dictionary keys are populated, if not then create the key and give it a "" value
                            for (int index = 1; index < 8; index++)
                            {
                                if (!DecorationTicketData.ContainsKey("ChildPartNumber" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartNumber" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("ChildPartDescription" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartDescription" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("ChildPartQuantity" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartQuantity" + index.ToString(), "");
                                }
                            }

                            //load the standard bow attributes from the intranet database
                            var standardBowData = LoadStandardBowData(DecorationTicketData["ItemCode"]);

                            //LINQ join the workOrderChildPart data with standardBowData on the ItemCode and ChildItem code. This relates the color to the child part so colors
                            //can print with the respective parts on the decoration ticket
                            var workOrderChildrenJoinStandardBowData = from woChildPartData in workOrderChildPartData
                                                                       join stdBowData in standardBowData on woChildPartData.ItemCode equals stdBowData.ChildItemCode
                                                                       orderby stdBowData.PartOrder
                                                                       select new { PartColor = stdBowData.Color, Part = woChildPartData.PartType, Description = woChildPartData.ItemDescription };


                            i = 1;

                            //loop through the standard bow attributes and load them into the dictionary
                            foreach (var row in workOrderChildrenJoinStandardBowData)
                            {
                                DecorationTicketData.Add("PartColorLabel" + i.ToString(), row.Part);

                                DecorationTicketData.Add("PartColor" + i.ToString(), row.PartColor + " (" + row.Description + ")");

                                i += 1;

                            }

                            //check to see if all dictionary keys are populated, if not then create the key and give it a "" value
                            for (int index = 1; index < 6; index++)
                            {
                                if (!DecorationTicketData.ContainsKey("PartColorLabel" + index.ToString()))
                                {
                                    DecorationTicketData.Add("PartColorLabel" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("PartColor" + index.ToString()))
                                {
                                    DecorationTicketData.Add("PartColor" + index.ToString(), "");
                                }
                            }

                            //load the label data into a string
                            labelStandardBowEvaShockeyData = $"{labelStandardBowEvaShockeyData} {Environment.NewLine} {DecorationTicketData["ItemCode"]}|{DecorationTicketData["ItemDescription"]}|{DecorationTicketData["Quantity"]}|{DecorationTicketData["WorkOrderNumber"]}|{DecorationTicketData["ChildPartNumber1"]}|{DecorationTicketData["ChildPartDescription1"]}|{DecorationTicketData["ChildPartQuantity1"]}|{DecorationTicketData["ChildPartNumber2"]}|{DecorationTicketData["ChildPartDescription2"]}|{DecorationTicketData["ChildPartQuantity2"]}|{DecorationTicketData["ChildPartNumber3"]}|{DecorationTicketData["ChildPartDescription3"]}|{DecorationTicketData["ChildPartQuantity3"]}|{DecorationTicketData["ChildPartNumber4"]}|{DecorationTicketData["ChildPartDescription4"]}|{DecorationTicketData["ChildPartQuantity4"]}|{DecorationTicketData["ChildPartNumber5"]}|{DecorationTicketData["ChildPartDescription5"]}|{DecorationTicketData["ChildPartQuantity5"]}|{DecorationTicketData["ChildPartNumber6"]}|{DecorationTicketData["ChildPartDescription6"]}|{DecorationTicketData["ChildPartQuantity6"]}|{DecorationTicketData["ChildPartNumber7"]}|{DecorationTicketData["ChildPartDescription7"]}|{DecorationTicketData["ChildPartQuantity7"]}|{DecorationTicketData["PartColorLabel1"]}|{DecorationTicketData["PartColor1"]}|{DecorationTicketData["PartColorLabel2"]}|{DecorationTicketData["PartColor2"]}|{DecorationTicketData["PartColorLabel3"]}|{DecorationTicketData["PartColor3"]}|{DecorationTicketData["PartColorLabel4"]}|{DecorationTicketData["PartColor4"]}|{DecorationTicketData["PartColorLabel5"]}|{DecorationTicketData["PartColor5"]}|{DateTime.Now.ToString("M/d/yyyy")}|{DecorationTicketData["IssuedBy"]}|{String.Format("{0:M/d/yyyy}", completionDate)}";
                            
                        }

                        //*****************Revolt Standard Bow Logic*****************
                        else if (DecorationTicketData["ItemCategory"].IndexOf("REVOLT") != -1 || DecorationTicketData["ItemCategory"].IndexOf("REVTX") != -1 )
                        {
                            int i = 1;

                            //get all the child parts for the work order
                            var workOrderChildPartData = LoadWorkOrderChildParts(workOrder);

                            //loop through all child parts in the work order
                            foreach (var row in workOrderChildPartData)
                            {
                                DecorationTicketData.Add("ChildPartNumber" + i.ToString(), row.ItemCode);

                                DecorationTicketData.Add("ChildPartDescription" + i.ToString(), row.ItemDescription);

                                DecorationTicketData.Add("ChildPartQuantity" + i.ToString(), row.Quantity.ToString());

                                i += 1;
                            }

                            //check to see if all dictionary keys are populated, if not then create the key and give it a "" value
                            for (int index = 1; index < 8; index++)
                            {
                                if (!DecorationTicketData.ContainsKey("ChildPartNumber" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartNumber" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("ChildPartDescription" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartDescription" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("ChildPartQuantity" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartQuantity" + index.ToString(), "");
                                }
                            }

                            //load the standard bow attributes from the intranet database
                            var standardBowData = LoadStandardBowData(DecorationTicketData["ItemCode"]);

                            //LINQ join the workOrderChildPart data with standardBowData on the ItemCode and ChildItem code. This relates the color to the child part so colors
                            //can print with the respective parts on the decoration ticket
                            var workOrderChildrenJoinStandardBowData = from woChildPartData in workOrderChildPartData
                                                                       join stdBowData in standardBowData on woChildPartData.ItemCode equals stdBowData.ChildItemCode
                                                                       orderby stdBowData.PartOrder
                                                                       select new { PartColor = stdBowData.Color, Part = woChildPartData.PartType, Description = woChildPartData.ItemDescription };


                            i = 1;

                            //loop through the standard bow attributes and load them into the dictionary
                            foreach (var row in workOrderChildrenJoinStandardBowData)
                            {
                                DecorationTicketData.Add("PartColorLabel" + i.ToString(), row.Part);

                                DecorationTicketData.Add("PartColor" + i.ToString(), row.PartColor + " (" + row.Description + ")");

                                i += 1;

                            }

                            //check to see if all dictionary keys are populated, if not then create the key and give it a "" value
                            for (int index = 1; index < 6; index++)
                            {
                                if (!DecorationTicketData.ContainsKey("PartColorLabel" + index.ToString()))
                                {
                                    DecorationTicketData.Add("PartColorLabel" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("PartColor" + index.ToString()))
                                {
                                    DecorationTicketData.Add("PartColor" + index.ToString(), "");
                                }
                            }

                            //load the label data into a string
                            labelStandardBowRevoltData = $"{labelStandardBowRevoltData} {Environment.NewLine} {DecorationTicketData["ItemCode"]}|{DecorationTicketData["ItemDescription"]}|{DecorationTicketData["Quantity"]}|{DecorationTicketData["WorkOrderNumber"]}|{DecorationTicketData["ChildPartNumber1"]}|{DecorationTicketData["ChildPartDescription1"]}|{DecorationTicketData["ChildPartQuantity1"]}|{DecorationTicketData["ChildPartNumber2"]}|{DecorationTicketData["ChildPartDescription2"]}|{DecorationTicketData["ChildPartQuantity2"]}|{DecorationTicketData["ChildPartNumber3"]}|{DecorationTicketData["ChildPartDescription3"]}|{DecorationTicketData["ChildPartQuantity3"]}|{DecorationTicketData["ChildPartNumber4"]}|{DecorationTicketData["ChildPartDescription4"]}|{DecorationTicketData["ChildPartQuantity4"]}|{DecorationTicketData["ChildPartNumber5"]}|{DecorationTicketData["ChildPartDescription5"]}|{DecorationTicketData["ChildPartQuantity5"]}|{DecorationTicketData["ChildPartNumber6"]}|{DecorationTicketData["ChildPartDescription6"]}|{DecorationTicketData["ChildPartQuantity6"]}|{DecorationTicketData["ChildPartNumber7"]}|{DecorationTicketData["ChildPartDescription7"]}|{DecorationTicketData["ChildPartQuantity7"]}|{DecorationTicketData["PartColorLabel1"]}|{DecorationTicketData["PartColor1"]}|{DecorationTicketData["PartColorLabel2"]}|{DecorationTicketData["PartColor2"]}|{DecorationTicketData["PartColorLabel3"]}|{DecorationTicketData["PartColor3"]}|{DecorationTicketData["PartColorLabel4"]}|{DecorationTicketData["PartColor4"]}|{DecorationTicketData["PartColorLabel5"]}|{DecorationTicketData["PartColor5"]}|{DateTime.Now.ToString("M/d/yyyy")}|{DecorationTicketData["IssuedBy"]}|{String.Format("{0:M/d/yyyy}", completionDate)}";

                        }
                        
                        //*****************Standard Bow Logic*****************
                        else
                        {
                            int i = 1;

                            //get all the child parts for the work order
                            var workOrderChildPartData = LoadWorkOrderChildParts(workOrder);

                            //loop through all child parts in the work order
                            foreach (var row in workOrderChildPartData)
                            {
                                DecorationTicketData.Add("ChildPartNumber" + i.ToString(), row.ItemCode);

                                DecorationTicketData.Add("ChildPartDescription" + i.ToString(), row.ItemDescription);
                                
                                DecorationTicketData.Add("ChildPartQuantity" + i.ToString(), row.Quantity.ToString());

                                i += 1;
                            }

                            //check to see if all dictionary keys are populated, if not then create the key and give it a "" value
                            for (int index = 1; index < 8; index++)
                            {
                                if (!DecorationTicketData.ContainsKey("ChildPartNumber" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartNumber" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("ChildPartDescription" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartDescription" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("ChildPartQuantity" + index.ToString()))
                                {
                                    DecorationTicketData.Add("ChildPartQuantity" + index.ToString(), "");
                                }
                            }

                            //load the standard bow attributes from the intranet database
                            var standardBowData = LoadStandardBowData(DecorationTicketData["ItemCode"]);

                            //LINQ join the workOrderChildPart data with standardBowData on the ItemCode and ChildItem code. This relates the color to the child part so colors
                            //can print with the respective parts on the decoration ticket
                            var workOrderChildrenJoinStandardBowData = from woChildPartData in workOrderChildPartData
                                       join stdBowData in standardBowData on woChildPartData.ItemCode equals stdBowData.ChildItemCode
                                       orderby stdBowData.PartOrder
                                       select new { PartColor = stdBowData.Color, Part = woChildPartData.PartType, Description = woChildPartData.ItemDescription };


                            i = 1;

                            //loop through the standard bow attributes and load them into the dictionary
                            foreach (var row in workOrderChildrenJoinStandardBowData)
                            {
                                DecorationTicketData.Add("PartColorLabel" + i.ToString(), row.Part);

                                DecorationTicketData.Add("PartColor" + i.ToString(), row.PartColor + " (" + row.Description + ")");

                                i += 1;

                            }

                            //check to see if all dictionary keys are populated, if not then create the key and give it a "" value
                            for (int index = 1; index < 6; index++)
                            {
                                if (!DecorationTicketData.ContainsKey("PartColorLabel" + index.ToString()))
                                {
                                    DecorationTicketData.Add("PartColorLabel" + index.ToString(), "");
                                }

                                if (!DecorationTicketData.ContainsKey("PartColor" + index.ToString()))
                                {
                                    DecorationTicketData.Add("PartColor" + index.ToString(), "");
                                }
                            }
                             
                            //load the label data into a string
                            labelStandardBowData = $"{labelStandardBowData} {Environment.NewLine} {DecorationTicketData["ItemCode"]}|{DecorationTicketData["ItemDescription"]}|{DecorationTicketData["Quantity"]}|{DecorationTicketData["WorkOrderNumber"]}|{DecorationTicketData["ChildPartNumber1"]}|{DecorationTicketData["ChildPartDescription1"]}|{DecorationTicketData["ChildPartQuantity1"]}|{DecorationTicketData["ChildPartNumber2"]}|{DecorationTicketData["ChildPartDescription2"]}|{DecorationTicketData["ChildPartQuantity2"]}|{DecorationTicketData["ChildPartNumber3"]}|{DecorationTicketData["ChildPartDescription3"]}|{DecorationTicketData["ChildPartQuantity3"]}|{DecorationTicketData["ChildPartNumber4"]}|{DecorationTicketData["ChildPartDescription4"]}|{DecorationTicketData["ChildPartQuantity4"]}|{DecorationTicketData["ChildPartNumber5"]}|{DecorationTicketData["ChildPartDescription5"]}|{DecorationTicketData["ChildPartQuantity5"]}|{DecorationTicketData["ChildPartNumber6"]}|{DecorationTicketData["ChildPartDescription6"]}|{DecorationTicketData["ChildPartQuantity6"]}|{DecorationTicketData["ChildPartNumber7"]}|{DecorationTicketData["ChildPartDescription7"]}|{DecorationTicketData["ChildPartQuantity7"]}|{DecorationTicketData["PartColorLabel1"]}|{DecorationTicketData["PartColor1"]}|{DecorationTicketData["PartColorLabel2"]}|{DecorationTicketData["PartColor2"]}|{DecorationTicketData["PartColorLabel3"]}|{DecorationTicketData["PartColor3"]}|{DecorationTicketData["PartColorLabel4"]}|{DecorationTicketData["PartColor4"]}|{DecorationTicketData["PartColorLabel5"]}|{DecorationTicketData["PartColor5"]}|{DateTime.Now.ToString("M/d/yyyy")}|{DecorationTicketData["IssuedBy"]}|{String.Format("{0:M/d/yyyy}", completionDate)}";

                        }
                    }
                    else
                    {
                        //the valid work order check failed and it was not a valid work order. Add message to the errors string.
                        isWorkOrderError = true;
                        errors += $"Work Order {workOrder} is not valid" + "\\r\\n";
                    }

                }

                //trim the label data
                labelCustomBowData = labelCustomBowData.Trim();
                
                //*****************Custom Bow Call to Generate XML*****************
                //only call the custom deco ticket if the the data string is populated
                if (labelCustomBowData.Length > 0)
                {
                    
                    //Concatenate the Field Name string with the data string
                    labelCustomBowData = $"{labelCustomBowFieldNames} {Environment.NewLine} {labelCustomBowData}";

                    customBowTicketPrinted = GenerateCustomBowDecorationTicket(labelCustomBowData, printerName);
                }
                else
                {
                    customBowTicketPrinted = true;
                }

                //if custombowticketprinted is false then there was an error set the ticketPrint error to true and append to the errors string
                if (customBowTicketPrinted == false)
                {
                    ticketPrintError = true;

                    errors += $"Custom bow labels did not print. Try re-printing again." + "\\r\\n";

                }


                //trim the label data
                labelCustomBowRevoltData = labelCustomBowRevoltData.Trim();

                //*****************Custom Bow Revolt Call to Generate XML*****************
                //only call the custom deco ticket if the the data string is populated
                if (labelCustomBowRevoltData.Length > 0)
                {
                   
                    //Concatenate the Field Name string with the data string
                    labelCustomBowRevoltData = $"{labelCustomBowRevoltFieldNames} {Environment.NewLine} {labelCustomBowRevoltData}";

                    customBowRevoltTicketPrinted = GenerateCustomBowRevoltDecorationTicket(labelCustomBowRevoltData, printerName);
                }
                else
                {
                    customBowRevoltTicketPrinted = true;
                }

                //if custombowticketprinted is false then there was an error set the ticketPrint error to true and append to the errors string
                if (customBowRevoltTicketPrinted == false)
                {
                    ticketPrintError = true;

                    errors += $"Custom bow Revolt labels did not print. Try re-printing again." + "\\r\\n";

                }

                //trim the label data
                labelStandardBowEvaShockeyData = labelStandardBowEvaShockeyData.Trim();

                //*****************Standard Bow Eva Shockey Call to Generate XML*****************
                //only call the standard bow ticket if the the data string is populated
                if (labelStandardBowEvaShockeyData.Length > 0)
                {
                    //Concatenate the Field Name string with the data string
                    labelStandardBowEvaShockeyData = $"{labelStandardBowEvaShockeyFieldNames} {Environment.NewLine} {labelStandardBowEvaShockeyData}";

                    standardBowEvaShockeyTicketPrinted = GenerateStandardBowEvaShockeyDecorationTicket(labelStandardBowEvaShockeyData, printerName);
                }
                else
                {
                    standardBowEvaShockeyTicketPrinted = true;
                }

                //if standardbowEvaShockeyticketprinted is false then there was an error set the ticketPrint error to true and append to the errors string
                if (standardBowEvaShockeyTicketPrinted == false)
                {
                    ticketPrintError = true;

                    errors += $"Standard Eva Shockey labels did not print. Try re-printing again." + "\\r\\n";

                }

                //trim the label data
                labelStandardBowRevoltData = labelStandardBowRevoltData.Trim();

                //*****************Standard Bow Revolt Call to Generate XML*****************
                //only call the standard bow ticket if the the data string is populated
                if (labelStandardBowRevoltData.Length > 0)
                {
                    //Concatenate the Field Name string with the data string
                    labelStandardBowRevoltData = $"{labelStandardBowRevoltFieldNames} {Environment.NewLine} {labelStandardBowRevoltData}";
                    
                    standardBowRevoltTicketPrinted = GenerateStandardBowRevoltDecorationTicket(labelStandardBowRevoltData, printerName);
                }
                else
                {
                    standardBowRevoltTicketPrinted = true;
                }

                //if standardbowEvaShockeyticketprinted is false then there was an error set the ticketPrint error to true and append to the errors string
                if (standardBowRevoltTicketPrinted == false)
                {
                    ticketPrintError = true;

                    errors += $"Standard Revolt labels did not print. Try re-printing again." + "\\r\\n";

                }

                //trim the label data
                labelStandardBowData = labelStandardBowData.Trim();

                //*****************Standard Bow Call to Generate XML*****************
                //only call the custom deco ticket if the the data string is populated
                if (labelStandardBowData.Length > 0)
                {
                    
                    //Concatenate the Field Name string with the data string
                    labelStandardBowData = $"{labelStandardBowFieldNames} {Environment.NewLine} {labelStandardBowData}";

                    standardBowTicketPrinted = GenerateStandardBowDecorationTicket(labelStandardBowData, printerName);
                }
                else
                {
                    standardBowTicketPrinted = true;
                }

                //if standardbowticketprinted is false then there was an error set the ticketPrint error to true and append to the errors string
                if (standardBowTicketPrinted == false)
                {
                    ticketPrintError = true;

                    errors += $"Standard bow labels did not print. Try re-printing again." + "\\r\\n";

                }

                //if there are errors then the the error string is populated
                if (isWorkOrderError == true || ticketPrintError == true)
                {
                    ViewData["Messages"] = errors;
                }
                else
                {
                    ViewData["Messages"] = "Decoration Tickets printed with no errors.";
                }

            }

            //clear the model state before returning the view.
            ModelState.Clear();

            //must re-populated the model so it has the printers loaded in the dropdown
            //set the label class as 200 for decoration ticket
            string labelClassID = "200";

            var data = LoadLabelPrinters(labelClassID);

            decorationTicketViewModel = new DecorationTicketViewModel();

            decorationTicketViewModel.LabelPrinters = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                decorationTicketViewModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            return View(decorationTicketViewModel);

        }

        //check to make sure a printer is selected on the generate decoration ticket view, gets called from the remote attribute of the decorationticketmodel
        [AcceptVerbs("Get", "Post")]
        public IActionResult PrinterSelected(int selectedPrinterID)
        {
            bool result;          

            if (selectedPrinterID == 0)
            {
                result = false;
            }
            else
            {
                result = true;
            }

            return Json(result);
        }

        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult GenerateWorkOrderPickSheet()
        {
            return View();
        }

        //generates a PDF for the work order pick sheet
        [HttpPost]
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult GenerateWorkOrderPickSheet(WorkOrderPickSheetViewModel workOrderPickSheetViewModel)
        {

            if (workOrderPickSheetViewModel != null && ModelState.IsValid)
            {

                //split the Work Order numbers into a list
                List<string> workOrderList = workOrderPickSheetViewModel.WorkOrders.ToString().Replace("\r\n", "\n").Split("\n").ToList();

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 15, Left = 2, Right = 2 },
                    DocumentTitle = "Work Order Pick Sheet",
                };

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = GetWorkOrderPickSheetString(workOrderList),
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\css", "WorkOrderPickSheet.css") },
                    HeaderSettings = { FontName = "Verdana", FontSize = 6, Right = "Page [page] of [toPage]", Line = true },
                    FooterSettings = { FontName = "Verdana", FontSize = 6, Line = true, Center = "Work Order Pick Sheet" }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                var file = _converter.Convert(pdf);

                ModelState.Clear();

                return File(file, "application/pdf", "Work Order Pick Sheet.pdf");

            }
            else
            {
                ModelState.Clear();

                return View();
            }
           
        }

        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult StandardBowPartColors()
        {
            return View();
        }

        //loads the standard bow part colors into the Telerik Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult StandardBowPartColors_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetStandardBowPartColors().ToDataSourceResult(request));
        }

        //gets all standard bow part colors and puts them in a list
        private static IEnumerable<StandardBowPartColorsModel> GetStandardBowPartColors()
        {
            var data = LoadStandardBowPartColors();

            List<StandardBowPartColorsModel> standardBowPartColors = new List<StandardBowPartColorsModel>();

            foreach (var row in data)
            {
                standardBowPartColors.Add(new StandardBowPartColorsModel
                {
                    ID = row.ID,
                    ParentItemCode = row.ParentItemCode,
                    ChildItemCode = row.ChildItemCode,
                    PartOrder = row.PartOrder,
                    Color = row.Color

                });
            }

            return standardBowPartColors;
        }

        //updates a exclusion item in the Telerick Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        [AcceptVerbs("Post")]
        public IActionResult StandardBowPartColors_Insert([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<StandardBowPartColorsModel> standardBowPartColors)
        {

            var results = new List<StandardBowPartColorsModel>();

            if (standardBowPartColors != null && ModelState.IsValid)
            {
                foreach (var standardBowPartColor in standardBowPartColors)
                {
                    bool isValidParentItemCode = false;
                    bool isValidChildItemCode = false;

                    //check if the item code is valid
                    isValidParentItemCode = CheckItemCode(standardBowPartColor.ParentItemCode);
                    isValidChildItemCode = CheckItemCode(standardBowPartColor.ChildItemCode);

                    if (isValidParentItemCode == true && isValidChildItemCode == true)
                    {
                        int recordsCreated = InsertStandardBowPartColors(standardBowPartColor.ParentItemCode, standardBowPartColor.ChildItemCode, standardBowPartColor.PartOrder, standardBowPartColor.Color);
                        results.Add(standardBowPartColor);
                    }
                    else
                    {
                        results.Remove(standardBowPartColor);

                        if (isValidParentItemCode == false && isValidChildItemCode == false)
                        {
                            ModelState.AddModelError("Invalid Item Code", $"{standardBowPartColor.ParentItemCode} and {standardBowPartColor.ChildItemCode} is an invalid Item Code.");
                        }
                        else if (isValidParentItemCode == false)
                        {
                            ModelState.AddModelError("Invalid Item Code", $"{standardBowPartColor.ParentItemCode} is an invalid Item Code.");
                        }
                        else if (isValidChildItemCode == false)
                        {
                            ModelState.AddModelError("Invalid Item Code", $"{standardBowPartColor.ChildItemCode} is an invalid Item Code.");
                        }
                        else
                        {
                            ModelState.AddModelError("Invalid Item Code", $"There was an error adding the record. Please verify Item Codes and try again.");
                        }

                    }
                }
            }

            return Json(results.ToDataSourceResult(request, ModelState));
        }

        //updates a standard bow part colors in the Telerik Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        [AcceptVerbs("Post")]
        public IActionResult StandardBowPartColors_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<StandardBowPartColorsModel> standardBowPartColors)
        {
            if (standardBowPartColors != null && ModelState.IsValid)
            {
                
                foreach (var standardBowPartColor in standardBowPartColors)
                {

                    bool isValidParentItemCode = false;
                    bool isValidChildItemCode = false;

                    //check if the item code is valid
                    isValidParentItemCode = CheckItemCode(standardBowPartColor.ParentItemCode);
                    isValidChildItemCode = CheckItemCode(standardBowPartColor.ChildItemCode);

                    if (isValidParentItemCode == true && isValidChildItemCode == true)
                    {
                        int recordsUpdated = UpdateStandardBowPartColors(standardBowPartColor.ID, standardBowPartColor.ParentItemCode, standardBowPartColor.ChildItemCode, standardBowPartColor.PartOrder, standardBowPartColor.Color);
                    }
                    else
                    {
                        
                        if (isValidParentItemCode == false && isValidChildItemCode == false)
                        {
                            ModelState.AddModelError("Invalid Item Code", $"{standardBowPartColor.ParentItemCode} and {standardBowPartColor.ChildItemCode} is an invalid Item Code.");
                        }
                        else if (isValidParentItemCode == false)
                        {
                            ModelState.AddModelError("Invalid Item Code", $"{standardBowPartColor.ParentItemCode} is an invalid Item Code.");
                        }
                        else if (isValidChildItemCode == false)
                        {
                            ModelState.AddModelError("Invalid Item Code", $"{standardBowPartColor.ChildItemCode} is an invalid Item Code.");
                        }
                        else
                        {
                            ModelState.AddModelError("Invalid Item Code", $"There was an error updating the record. Please verify Item Codes and try again.");
                        }

                    }

                }   
            }

            return Json(standardBowPartColors.ToDataSourceResult(request, ModelState));
        }

        //deletes a standard bow part colors in the Telerik Grid
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        [AcceptVerbs("Post")]
        public IActionResult StandardBowPartColors_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<StandardBowPartColorsModel> standardBowPartColors)
        {
            if (standardBowPartColors.Any())
            {
                foreach (var standardBowPartColor in standardBowPartColors)
                {
                    int recordsUpdated = DeleteStandardBowPartColors(standardBowPartColor.ID);
                }
            }

            return Json(standardBowPartColors.ToDataSourceResult(request, ModelState));
        }

    }
}

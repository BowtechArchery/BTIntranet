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
using OnTargetLibrary.Security;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnTarget.Controllers.SupplyChain
{
    public class SupplyChainController : Controller
    {
        //reads from the appsettings.json (see action ItemExclusions at the bottom of this controller) example only
        private readonly IConfiguration configuration;

        //reads from the appsettings.json (see action ItemExclusions at the bottom of this controller) example only
        public SupplyChainController(IConfiguration config)
        {
            this.configuration = config;
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

        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult ExclusionItems_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetExclusionItems().ToDataSourceResult(request));
        }

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

        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult OpenOrders_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetOpenOrders().ToDataSourceResult(request));
        }

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
                    CustomBowLimbColor = row.CustomBowLimbColor
                });
            }

            return openorders;
        }

        [HttpPost]
        public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult GenerateDecorationTicket()
        {

            //set the label class as 200 for decoration ticket
            string labelClassID = "200";

            var data = LoadLabelPrinters(labelClassID);

            var decorationTicketModel = new DecorationTicketModel();

            decorationTicketModel.LabelPrinters = new List<SelectListItem>();

            foreach (var row in data)
            {

                decorationTicketModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            return View(decorationTicketModel);
        }

        [HttpPost]
        [MultiplePolicysAuthorize("Administration;Supply Chain")]
        public IActionResult GenerateDecorationTicket(DecorationTicketModel decorationTicketModel)
        {
           
            if(decorationTicketModel != null && ModelState.IsValid)
            {
                bool isWorkOrderError = false;
                bool ticketPrintError = false;

                string errors = "";
                string printerName = "";

                string labelCustomBowData = "";
                string labelStandardBowData = "";
                string labelFieldNames = $"FGPartNumber|FGPartDescription|FGUnits|SalesOrder|WorkOrderNumber|ChildPartNumber1|ChildPartDescription1|ChildQty1|ChildPartNumber2|ChildPartDescription2|ChildQty2|ChildPartNumber3|ChildPartDescription3|ChildQty3|ChildPartNumber4|ChildPartDescription4|ChildQty4|ChildPartNumber5|ChildPartDescription5|ChildQty5|ChildPartNumber6|ChildPartDescription6|ChildQty6|ChildPartNumber7|ChildPartDescription7|ChildQty7|BowModel|BowHand|BowWeight|BowRiserColor|BowLimbColor|IssueDate|IssueBy|CompletionDate{Environment.NewLine}";
               
                var labelPrinterData = LoadLabelPrinterName(decorationTicketModel.SelectedPrinterID.ToString());

                foreach (var row in labelPrinterData)
                {
                    printerName =  row.PrinterName;
                }

                string completionDate =  decorationTicketModel.CompletionDate.ToString();

                Dictionary<string, string> DecorationTicketData = new Dictionary<string, string>();

                //split the Work Order numbers into a list
                List<string> workOrderList = decorationTicketModel.WorkOrders.ToString().Replace("\r\n", "\n").Split("\n").ToList();

                foreach (var workOrder in workOrderList)
                {

                    bool isValidWorkOrder = false;

                    //check if the work order is valid
                    isValidWorkOrder = CheckWorkOrder(workOrder);

                    //if the work order is valid then get details and start creating xml for BarTender
                    //else append invalid work order to error string
                    if (isValidWorkOrder == true)
                    {

                        DecorationTicketData.Clear();

                        var workOrderData = LoadWorkOrder(workOrder);

                        foreach (var row in workOrderData)
                        {
                            DecorationTicketData.Add("WorkOrderNumber", row.WorkOrderNumber);
                            DecorationTicketData.Add("ItemCode", row.ItemCode);
                            DecorationTicketData.Add("ItemDescription", row.ItemDescription);
                            DecorationTicketData.Add("SalesOrderNumber", row.SalesOrderNumber);
                            DecorationTicketData.Add("Quantity", row.Quantity.ToString());
                            DecorationTicketData.Add("IssuedBy", row.IssuedBy);
                                                        
                        }

                        //if the item code is a 40000 then it is a custom bow and we have to get the decoration data from order details table in the data warehouse
                        //otherwise it is a non-custom bow and we get the decoration data on the custom table on the intranet
                        if (DecorationTicketData["ItemCode"].IndexOf("40000") != -1)
                        {
                            int i = 1;

                            //get all the child parts for the work order
                            var workOrderChildPartData = LoadWorkOrderChildParts(workOrder);

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
                            
                            //remove the characters from the order number.  The Order Details in the data warehouse has only the numerical value of the sales order number
                            string salesOrderNumberNoChars = RemoveTextFromSalesOrder(DecorationTicketData["SalesOrderNumber"]);

                            var customBowData = LoadCustomBowData(salesOrderNumberNoChars, DecorationTicketData["ItemCode"]);

                            //check to make sure that there was only one custom bow item code order line returned from the Order Details table
                            if (customBowData.Count == 1)
                            {
                                foreach (var row in customBowData)
                                {
                                    DecorationTicketData.Add("BowModel", row.BowModel);
                                    DecorationTicketData.Add("BowHand", row.BowHand);
                                    DecorationTicketData.Add("BowWeight", row.BowDrawWeight);
                                    DecorationTicketData.Add("BowRiserColor", row.BowRiserColor);
                                    DecorationTicketData.Add("BowLimbColor", row.BowLimbColor);

                                }

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

                                //load the data into a string
                                labelCustomBowData = $"{labelCustomBowData} {Environment.NewLine} {DecorationTicketData["ItemCode"]}|{DecorationTicketData["ItemDescription"]}|{DecorationTicketData["Quantity"]}|{DecorationTicketData["SalesOrderNumber"]}|{DecorationTicketData["WorkOrderNumber"]}|{DecorationTicketData["ChildPartNumber1"]}|{DecorationTicketData["ChildPartDescription1"]}|{DecorationTicketData["ChildPartQuantity1"]}|{DecorationTicketData["ChildPartNumber2"]}|{DecorationTicketData["ChildPartDescription2"]}|{DecorationTicketData["ChildPartQuantity2"]}|{DecorationTicketData["ChildPartNumber3"]}|{DecorationTicketData["ChildPartDescription3"]}|{DecorationTicketData["ChildPartQuantity3"]}|{DecorationTicketData["ChildPartNumber4"]}|{DecorationTicketData["ChildPartDescription4"]}|{DecorationTicketData["ChildPartQuantity4"]}|{DecorationTicketData["ChildPartNumber5"]}|{DecorationTicketData["ChildPartDescription5"]}|{DecorationTicketData["ChildPartQuantity5"]}|{DecorationTicketData["ChildPartNumber6"]}|{DecorationTicketData["ChildPartDescription6"]}|{DecorationTicketData["ChildPartQuantity6"]}|{DecorationTicketData["ChildPartNumber7"]}|{DecorationTicketData["ChildPartDescription7"]}|{DecorationTicketData["ChildPartQuantity7"]}|{DecorationTicketData["BowModel"]}|{DecorationTicketData["BowHand"]}|{DecorationTicketData["BowWeight"]}|{DecorationTicketData["BowRiserColor"]}|{DecorationTicketData["BowLimbColor"]}|{DateTime.Now.ToString("M/d/yyyy")}|{DecorationTicketData["IssuedBy"]}|{String.Format("{0:M/d/yyyy}", completionDate)}";

                            }
                            //if there is more than one record in customBowData that means there were duplicate custom item code lines on the order and they didn't get split
                            else if(customBowData.Count > 1)
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
                        //put non-custom generate ticket code here
                        else
                        {
                            labelStandardBowData = $"{labelStandardBowData} {Environment.NewLine}";
                        }

                    }
                    else
                    {
                        isWorkOrderError = true;
                        errors += $"Work Order {workOrder} is not valid" + "\\r\\n";
                    }

                }

                //call the method to generate the XML for custom bow tickets that goes to BarTender
                labelCustomBowData = $"{labelFieldNames} {Environment.NewLine} {labelCustomBowData}";

                System.Diagnostics.Debug.WriteLine(labelCustomBowData);

                bool customBowTicketPrinted = GenerateCustomBowDecorationTicket(labelCustomBowData, printerName);

                if (customBowTicketPrinted == false)
                {
                    ticketPrintError = true;

                    errors += $"Custom Labels did not print. Try re-printing again" + "\\r\\n";

                }

                //*****put call to GenerateStandardBowDecorationTicket here*****
                //bool standardBowTicketPrinted = GenerateStandardBowDecorationTicket(labelStandardBowData, printerName);

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

            decorationTicketModel = new DecorationTicketModel();

            decorationTicketModel.LabelPrinters = new List<SelectListItem>();

            foreach (var row in data)
            {

                decorationTicketModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            return View(decorationTicketModel);

        }

        //check to make sure a printer is selected on the generate decoration ticket view
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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using OnTarget.Models.Manufacturing;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using static OnTargetDataLibrary.BusinessLogic.Manufacturing.ManufacturingLabelProcessor;
using static OnTargetDataLibrary.BusinessLogic.Manufacturing.WorkOrderSerialModifyProcessor;
using static OnTargetDataLibrary.BusinessLogic.Manufacturing.BowCaseLabelSetupProcessor;
using OnTargetLibrary.Security;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnTarget.Controllers.Manufacturing
{
    public class ManufacturingController : Controller
    {
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult Index()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        public IActionResult BowCaseLabelSetup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        public IActionResult BowCaseLabelSetup(BowCaseLabelWorkOrderModel bowCaseLabelWorkOrderModel)
        {
            bool labelSetupError = false;
            bool isValidWorkOrder = false;
            bool isWorkOrderNotSetup = false;
            
            string partNumber = "";
            string errors = "";

            var labelData = LoadLabelDataWorkOrder(bowCaseLabelWorkOrderModel.WorkOrderNumber);

            foreach (var row in labelData)
            {
                partNumber = row.PartNumber;
            }
                
            //check if the Work Order number is is valid
            isValidWorkOrder = CheckWorkOrderNumber(bowCaseLabelWorkOrderModel.WorkOrderNumber);

            if (isValidWorkOrder == true)
            {
                    
                //check to see if the Work Order has already been setup
                isWorkOrderNotSetup = CheckWorkOrderSetup(bowCaseLabelWorkOrderModel.WorkOrderNumber);

                if (isWorkOrderNotSetup == true)
                {

                    //if it is a standard bow
                    if (partNumber.IndexOf("40000") == -1)
                    {
                        //check to make sure custom bow is set to false
                        if (bowCaseLabelWorkOrderModel.CustomBow.Equals(false))
                        {

                            //make sure that a custom description does not get inserted into the record in case the user accidentally entered a custom description and then selected no under custom
                            bowCaseLabelWorkOrderModel.CustomDescription = null;

                            //create the record in the bowcaseworkorderdata table
                            int recordsCreated = InsertBowCaseWorkOrderData(bowCaseLabelWorkOrderModel.WorkOrderNumber, bowCaseLabelWorkOrderModel.CustomBow, bowCaseLabelWorkOrderModel.CustomDescription);
                        }
                        else
                        {
                            labelSetupError = true;

                            errors += $"The Work Order Number entered is a standard bow.  Please check that Custom is set to No and try again" + "\\r\\n";
                        }
                    }

                    //if it is a custom bow
                    else if (partNumber.IndexOf("40000") != -1) 
                    {
                       
                        //check to make sure custom is set to true
                        if (bowCaseLabelWorkOrderModel.CustomBow.Equals(true))
                        {

                            //check to make sure if it is a custom bow that the custom description is populated
                            if (String.IsNullOrEmpty(bowCaseLabelWorkOrderModel.CustomDescription).Equals(false))
                            {
                                
                                //create the record in the bowcaseworkorderdata table
                                int recordsCreated = InsertBowCaseWorkOrderData(bowCaseLabelWorkOrderModel.WorkOrderNumber, bowCaseLabelWorkOrderModel.CustomBow, bowCaseLabelWorkOrderModel.CustomDescription);

                            }
                            else
                            {

                                labelSetupError = true;

                                errors += $"The Work Order Number entered is a custom bow.  Please enter a custom description and try again" + "\\r\\n";

                            }

                        }
                        else
                        {
                            labelSetupError = true;

                            errors += $"The Work Order Number entered is a custom bow.  Please check that Custom is set to Yes and try again" + "\\r\\n";

                        }

                    }
                    else
                    {
                        labelSetupError = true;

                        errors += $"The Work Order Number entered is for a custom bow.  Please check that Custom is selected and a Custom Description is populated and try again" + "\\r\\n";

                    }
                }
                else
                {
                    labelSetupError = true;

                    errors += $"The Work Order Number entered is already setup.  Please verify that the correct Work Order was entered" + "\\r\\n";
                }
            }
            else
            {
                labelSetupError = true;

                errors += $"The Work Order Number entered is not valid.  Please enter a valid Work Order Number try again" + "\\r\\n";
            }

            //if there are errors then the the error string is populated
            if (labelSetupError == true)
            {
                ViewData["Messages"] = errors;
            }
            else
            {
                ViewData["Messages"] = $"{bowCaseLabelWorkOrderModel.WorkOrderNumber} is now setup and ready to print case labels.";
            }

            //clear the model state before returning the view.
            ModelState.Clear();
            
            return View();
        }
        
        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        public IActionResult BowCaseLabelSetupModify()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        public IActionResult BowCaseLabelSetup_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetBowCaseLabelSetup().ToDataSourceResult(request));
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        private static IEnumerable<BowCaseLabelWorkOrderModel> GetBowCaseLabelSetup()
        {
            var data = LoadBowCaseLabelSetup();

            List<BowCaseLabelWorkOrderModel> bowCaseLabelWorkOrderModel = new List<BowCaseLabelWorkOrderModel>();

            foreach (var row in data)
            {
                bowCaseLabelWorkOrderModel.Add(new BowCaseLabelWorkOrderModel
                {
                    ID = row.ID,
                    WorkOrderNumber = row.WorkOrderNumber,
                    CustomBow = row.CustomBow,
                    CustomDescription = row.CustomDescription,
                    CurrentCaseNumber = row.CurrentCaseNumber,
                    Complete = row.Complete

                });
            }

            return bowCaseLabelWorkOrderModel;
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        [AcceptVerbs("Post")]
        public IActionResult BowCaseLabelSetup_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<BowCaseLabelWorkOrderModel> bowCaseLabelWorkOrders)
        {
            if (bowCaseLabelWorkOrders != null && ModelState.IsValid)
            {
                foreach (var bowCaseLabelWorkOrder in bowCaseLabelWorkOrders)
                {
                   
                    //check to make sure custom description is populated if custom bow is set to true
                    if(bowCaseLabelWorkOrder.CustomBow == true && String.IsNullOrEmpty(bowCaseLabelWorkOrder.CustomDescription) == false)
                    {
                        int recordsUpdated = UpdateBowCaseWorkOrderSetup(bowCaseLabelWorkOrder.ID, bowCaseLabelWorkOrder.CustomBow, bowCaseLabelWorkOrder.CustomDescription, bowCaseLabelWorkOrder.CurrentCaseNumber, bowCaseLabelWorkOrder.Complete);
                    }
                    else if (bowCaseLabelWorkOrder.CustomBow == false && String.IsNullOrEmpty(bowCaseLabelWorkOrder.CustomDescription) == true)
                    {
                        int recordsUpdated = UpdateBowCaseWorkOrderSetup(bowCaseLabelWorkOrder.ID, bowCaseLabelWorkOrder.CustomBow, bowCaseLabelWorkOrder.CustomDescription, bowCaseLabelWorkOrder.CurrentCaseNumber, bowCaseLabelWorkOrder.Complete);
                    }
                    else if (bowCaseLabelWorkOrder.CustomBow == false && String.IsNullOrEmpty(bowCaseLabelWorkOrder.CustomDescription) == false)
                    {
                        ModelState.AddModelError("Bow Case Label Setup Error", $"Work Order {bowCaseLabelWorkOrder.WorkOrderNumber} is not set to Custom, but does have a Custom Description. Please verify if the Work Order is Custom and if so, select custom.");
                    }
                    else if (bowCaseLabelWorkOrder.CustomBow == true && String.IsNullOrEmpty(bowCaseLabelWorkOrder.CustomDescription) == true)
                    {
                        ModelState.AddModelError("Bow Case Label Setup Error", $"Work Order {bowCaseLabelWorkOrder.WorkOrderNumber} is set to Custom, but does not have a Custom Description. Please verify if the Work Order is Custom and if so, add a Custom Description.");                     
                    }
                    else
                    {
                        ModelState.AddModelError("Bow Case Label Setup Error", $" There is a problem with Work Order {bowCaseLabelWorkOrder.WorkOrderNumber} setup. Please verify all Work Order setup is set correctly.  If you continue to recieve this message contact IT");

                    }

                }
            }

            return Json(bowCaseLabelWorkOrders.ToDataSourceResult(request, ModelState));
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult BowCaseLabelsSelectWorkOrder()
        {
            //set the label class as 1 for bow case label
            string labelClassID = "100";

            var data = LoadLabelPrinters(labelClassID);

            var bowCaseSelectWorkOrderViewModel = new BowCaseSelectWorkOrderViewModel();

            bowCaseSelectWorkOrderViewModel.LabelPrinters = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                bowCaseSelectWorkOrderViewModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            return View(bowCaseSelectWorkOrderViewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult BowCaseLabelsSelectWorkOrder(BowCaseSelectWorkOrderViewModel bowCaseSelectWorkOrderViewModel)
        {
            bool isValidWorkOrder = false;
            bool isWorkOrderComplete = false;

            string errors = "";
            
            //check if the Work Order number is is valid
            isValidWorkOrder = CheckLabelBowCaseWorkOrderNumber(bowCaseSelectWorkOrderViewModel.WorkOrderNumber);

            //check the Work Order to make sure it isn't completed
            var workOrderComplete = CheckLabelBowCaseWorkOrderNumberIsComplete(bowCaseSelectWorkOrderViewModel.WorkOrderNumber);

            foreach (var row in workOrderComplete)
            {
                isWorkOrderComplete = Convert.ToBoolean(row.Complete);
            }

            if (isValidWorkOrder == true)
            {

                if (isWorkOrderComplete == false)
                {
                    return RedirectToAction("BowCaseLabels", new { workOrderNumber = bowCaseSelectWorkOrderViewModel.WorkOrderNumber, SelectedPrinterID = bowCaseSelectWorkOrderViewModel.SelectedPrinterID });
                }
                else
                {
                    //clear the model state before returning the view.
                    ModelState.Clear();

                    //set the label class as 200 for decoration ticket
                    string labelClassID = "100";

                    var data = LoadLabelPrinters(labelClassID);

                    var newBowCaseSelectWorkOrderViewModel = new BowCaseSelectWorkOrderViewModel();

                    newBowCaseSelectWorkOrderViewModel.LabelPrinters = new List<SelectListItem>();

                    //load the printer names into the model
                    foreach (var row in data)
                    {

                        newBowCaseSelectWorkOrderViewModel.LabelPrinters.Add(new SelectListItem
                        {
                            Text = row.PrinterDescription.ToString(),
                            Value = row.PrinterID.ToString()
                        });
                    }

                    errors = $"The Work Order Number entered is already Completed.  Please enter a valid Work Order Number try again" + "\\r\\n";
                    ViewData["Messages"] = errors;
                    return View(newBowCaseSelectWorkOrderViewModel);
                }
                
            }
            else
            {
                //clear the model state before returning the view.
                ModelState.Clear();

                //set the label class as 200 for decoration ticket
                string labelClassID = "100";

                var data = LoadLabelPrinters(labelClassID);

                var newBowCaseSelectWorkOrderViewModel = new BowCaseSelectWorkOrderViewModel();

                newBowCaseSelectWorkOrderViewModel.LabelPrinters = new List<SelectListItem>();

                //load the printer names into the model
                foreach (var row in data)
                {

                    newBowCaseSelectWorkOrderViewModel.LabelPrinters.Add(new SelectListItem
                    {
                        Text = row.PrinterDescription.ToString(),
                        Value = row.PrinterID.ToString()
                    });
                }

                errors = $"The Work Order Number entered is not valid.  Please enter a valid Work Order Number try again" + "\\r\\n";
                ViewData["Messages"] = errors;
                return View(newBowCaseSelectWorkOrderViewModel);

            }
                
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult BowCaseLabels(string workOrderNumber, int selectedPrinterID)
        {
            string printerDescription = "";

            BowCaseLabelViewModel bowCaseLabelViewModel = new BowCaseLabelViewModel();

            bowCaseLabelViewModel.WorkOrderNumber = workOrderNumber;
            bowCaseLabelViewModel.SelectedPrinterID = selectedPrinterID;
            
            var labelPrinterData = LoadLabelPrinterName(selectedPrinterID.ToString());

            //load the printer name into a string
            foreach (var row in labelPrinterData)
            {
                printerDescription = row.PrinterDescription;
            }

            bowCaseLabelViewModel.PrinterDescription = printerDescription;

            return View(bowCaseLabelViewModel);
        }

        public JsonResult LoadBowCaseLabelWorkOrderData(string workOrderNumber)
        {
            bool isCustomBow = false;
            
            var labelWorkOrderData = LoadLabelDataWorkOrder(workOrderNumber);

            var bowCaseWorkOrderData = LoadBowCaseWorkOrderData(workOrderNumber);

            foreach (var row in bowCaseWorkOrderData)
            {
                isCustomBow = row.CustomBow;
            }

            List<BowCaseLabelViewModel> bowCaseLabelViewModel = new List<BowCaseLabelViewModel>();

            //if standard bow then use the Part Description from the Ramco Work Order Master
            if (isCustomBow == false)
            {
                //LINQ join
                var bowCaseWorkOrderDataJoinlabelWorkOrderData = from bwCaseWorkOrderData in bowCaseWorkOrderData
                                                                 join lblWorkOrderData in labelWorkOrderData on bwCaseWorkOrderData.WorkOrderNumber equals lblWorkOrderData.WorkOrderNumber
                                                                 select new { WorkOrderNumber = lblWorkOrderData.WorkOrderNumber, PartNumber = lblWorkOrderData.PartNumber, PartDescription = lblWorkOrderData.PartDescription, Quantity = lblWorkOrderData.Quantity, CurrentCaseNumber = bwCaseWorkOrderData.CurrentCaseNumber };

                foreach (var row in bowCaseWorkOrderDataJoinlabelWorkOrderData)
                {

                    bowCaseLabelViewModel.Add(new BowCaseLabelViewModel
                    {
                        WorkOrderNumber = row.WorkOrderNumber,
                        PartNumber = row.PartNumber,
                        PartDescription = row.PartDescription,
                        Quantity = row.Quantity,
                        CurrentCaseNumber = row.CurrentCaseNumber

                    });
                }
            }
            else
            {
                //LINQ join
                var bowCaseWorkOrderDataJoinlabelWorkOrderData = from bwCaseWorkOrderData in bowCaseWorkOrderData
                                                                 join lblWorkOrderData in labelWorkOrderData on bwCaseWorkOrderData.WorkOrderNumber equals lblWorkOrderData.WorkOrderNumber
                                                                 select new { WorkOrderNumber = lblWorkOrderData.WorkOrderNumber, PartNumber = lblWorkOrderData.PartNumber, PartDescription = bwCaseWorkOrderData.CustomDescription, Quantity = lblWorkOrderData.Quantity, CurrentCaseNumber = bwCaseWorkOrderData.CurrentCaseNumber };

                foreach (var row in bowCaseWorkOrderDataJoinlabelWorkOrderData)
                {

                    bowCaseLabelViewModel.Add(new BowCaseLabelViewModel
                    {
                        WorkOrderNumber = row.WorkOrderNumber,
                        PartNumber = row.PartNumber,
                        PartDescription = row.PartDescription,
                        Quantity = row.Quantity,
                        CurrentCaseNumber = row.CurrentCaseNumber

                    });
                }
            }

            return Json(bowCaseLabelViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult PrintBowCaseLabel(BowCaseLabelViewModel bowCaseLabelViewModel)
        {
            
            bool labelPrinted = false;
            bool customBow = false;
            bool complete = false;
            bool isWorkOrderComplete = false;
            bool serialNumberExists = false;

            int quantity = 0;
            int nextCaseNumber = 0;
            int workOrderCompleteMessage = 0;

            string statusMessage = "";
            string printerName = "";
            string partNumber = "";
            string partDescription = "";
            string salesOrderNumber = "";
            string UPC = "";
            string customDescription = "";
            

            string labelManufacturingBowCaseData = "";
            string labelManufacturingBowCaseFieldNames = "";

            var workOrderComplete = CheckLabelBowCaseWorkOrderNumberIsComplete(bowCaseLabelViewModel.WorkOrderNumber);

            foreach (var row in workOrderComplete)
            {
                isWorkOrderComplete = Convert.ToBoolean(row.Complete);
            }

            if (isWorkOrderComplete == false)
            {

                serialNumberExists = CheckSerialNumber(bowCaseLabelViewModel.SerialNumber);

                if (serialNumberExists == false)
                {

                    if (String.Equals(bowCaseLabelViewModel.WorkOrderNumber.ToUpper(), bowCaseLabelViewModel.SerialNumber.ToUpper()) == false)
                    {
                        //get the printer that was selected on post
                        var labelPrinterData = LoadLabelPrinterName(bowCaseLabelViewModel.SelectedPrinterID.ToString());

                        //load the printer name into a string
                        foreach (var row in labelPrinterData)
                        {
                            printerName = row.PrinterName;
                        }

                        var labelData = LoadLabelDataWorkOrder(bowCaseLabelViewModel.WorkOrderNumber);

                        foreach (var row in labelData)
                        {
                            partNumber = row.PartNumber;
                            partDescription = row.PartDescription;
                            quantity = row.Quantity;
                            salesOrderNumber = row.SalesOrderNumber;
                            UPC = row.UPC;
                        }

                        //check to see if a custom bow and if it is replace the part description
                        var labelBowCaseWorkOrderData = LoadBowCaseWorkOrderData(bowCaseLabelViewModel.WorkOrderNumber);

                        foreach (var row in labelBowCaseWorkOrderData)
                        {
                            customBow = row.CustomBow;
                            customDescription = row.CustomDescription;

                        }

                        if (customBow == true)
                        {
                            partDescription = customDescription;
                        }

                        labelManufacturingBowCaseData = $"{partNumber}|{partDescription}|{partDescription}|{bowCaseLabelViewModel.WorkOrderNumber}|{quantity}|{bowCaseLabelViewModel.CurrentCaseNumber}|{salesOrderNumber}|{bowCaseLabelViewModel.SerialNumber}|{partNumber}|{UPC}";

                        if (labelManufacturingBowCaseData.Length > 0)
                        {
                            //define the field names for the selected warranty label
                            labelManufacturingBowCaseFieldNames = $"PartNumber|PartDescription1|PartDescription2|WorkOrderNumber|WorkOrderQty|CurrentCaseNumber|SalesOrderNumber|SerialNumber|Barcode1|Barcode2";

                            labelManufacturingBowCaseData = $"{labelManufacturingBowCaseFieldNames}{Environment.NewLine}{labelManufacturingBowCaseData}";

                            labelPrinted = GenerateManufacturingBowCaseLabel(labelManufacturingBowCaseData, printerName);

                            //update the current case number and if the WO is complete
                            if (labelPrinted == true)
                            {
                                if (bowCaseLabelViewModel.CurrentCaseNumber == quantity)
                                {
                                    complete = true;

                                    nextCaseNumber = bowCaseLabelViewModel.CurrentCaseNumber;

                                    workOrderCompleteMessage = 1;
                                }
                                else
                                {
                                    complete = false;

                                    nextCaseNumber = bowCaseLabelViewModel.CurrentCaseNumber + 1;
                                }

                                //update the labelbowcaseworkorder table on btintranet with casenumber and if the WO is complete or not
                                int recordsUpdated = UpdateLabelBowCaseWorkOrder(bowCaseLabelViewModel.WorkOrderNumber, nextCaseNumber, complete);

                                //insert a record in the _ud_serialnumbers table in SCMDB
                                int recordsCreated = InsertSerialNumber(bowCaseLabelViewModel.SerialNumber, bowCaseLabelViewModel.WorkOrderNumber, bowCaseLabelViewModel.BowBuilder);
                            }

                        }
                        else
                        {
                            labelPrinted = false;

                        }

                        //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                        if (labelPrinted == false)
                        {

                            statusMessage = $"Bow Case Number { bowCaseLabelViewModel.CurrentCaseNumber } Did Not print.";

                        }
                        else
                        {

                            statusMessage = $"Bow Case Label Number { bowCaseLabelViewModel.CurrentCaseNumber } of {quantity} Printed";

                        }
                    }
                    else
                    {
                        
                        statusMessage = $"The Work Order Number and Serial Number are matching.  Please rescan.";

                    }
                }
                else
                {

                    statusMessage = $"Serial Number { bowCaseLabelViewModel.SerialNumber } has already been registered.  Please check for duplication.";

                }
            }
            else
            {

                statusMessage = $"Printing is Complete for this Work Order. No More Case Label Printing Allowed.";
                workOrderCompleteMessage = 1;

            }

            return Json(new { message = statusMessage, workOrderCompleteMessage });

        }

        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult ReprintBowCaseLabel()
        {
            //set the label class as 100 for decoration ticket
            string labelClassID = "100";

            var data = LoadLabelPrinters(labelClassID);

            var reprintCaseLabelViewModel = new ReprintCaseLabelViewModel();

            reprintCaseLabelViewModel.LabelPrinters = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                reprintCaseLabelViewModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            return View(reprintCaseLabelViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult ReprintBowCaseLabel(ReprintCaseLabelViewModel reprintCaseLabelViewModel)
        {
            bool customBow = false;
            bool labelPrinted = false;
            bool labelPrintError = false;
            bool serialNumberExists = false;

            string workOrderNumber = "";
            string partNumber = "";
            string partDescription = "";
            string salesOrderNumber = "";
            string UPC = "";
            string customDescription = "";
            string printerName = "";
            string errors = "";
            string printMessage = "";

            string labelManufacturingBowCaseData = "";
            string labelManufacturingBowCaseFieldNames = "";

            int quantity = 0;

            //get the printer that was selected on post
            var labelPrinterData = LoadLabelPrinterName(reprintCaseLabelViewModel.SelectedPrinterID.ToString());

            //load the printer name into a string
            foreach (var row in labelPrinterData)
            {
                printerName = row.PrinterName;
            }

            serialNumberExists = CheckSerialNumber(reprintCaseLabelViewModel.SerialNumber.Trim());

            if (serialNumberExists == true)
            {

                //get work order from serial number
                var workOrderNumberData = LoadWorkOrderNumberFromSerial(reprintCaseLabelViewModel.SerialNumber.Trim());

                //put the work order number into a string
                foreach (var row in workOrderNumberData)
                {
                    workOrderNumber = row.WorkOrderNumber.Trim();
                }

                //get the label data from the work order
                var labelData = LoadLabelDataWorkOrder(workOrderNumber);

                foreach (var row in labelData)
                {
                    partNumber = row.PartNumber;
                    partDescription = row.PartDescription;
                    quantity = row.Quantity;
                    salesOrderNumber = row.SalesOrderNumber;
                    UPC = row.UPC;
                }

                if (labelData != null && labelData.Count > 0)
                {

                    //check if a custom, if so then replace the item description with the custom description
                    var labelBowCaseWorkOrderData = LoadBowCaseWorkOrderData(workOrderNumber);

                    foreach (var row in labelBowCaseWorkOrderData)
                    {
                        customBow = row.CustomBow;
                        customDescription = row.CustomDescription;

                    }

                    if (labelBowCaseWorkOrderData != null && labelBowCaseWorkOrderData.Count > 0)
                    {

                        if (reprintCaseLabelViewModel.CaseNumber <= quantity)
                        {

                            if (customBow == true)
                            {
                                partDescription = customDescription;
                            }

                            labelManufacturingBowCaseData = $"{partNumber}|{partDescription}|{partDescription}|{workOrderNumber}|{quantity}|{reprintCaseLabelViewModel.CaseNumber}|{salesOrderNumber}|{reprintCaseLabelViewModel.SerialNumber}|{partNumber}|{UPC}";

                            if (labelManufacturingBowCaseData.Length > 0)
                            {
                                //define the field names for the selected warranty label
                                labelManufacturingBowCaseFieldNames = $"PartNumber|PartDescription1|PartDescription2|WorkOrderNumber|WorkOrderQty|CurrentCaseNumber|SalesOrderNumber|SerialNumber|Barcode1|Barcode2";

                                labelManufacturingBowCaseData = $"{labelManufacturingBowCaseFieldNames}{Environment.NewLine}{labelManufacturingBowCaseData}";

                                labelPrinted = GenerateManufacturingBowCaseLabel(labelManufacturingBowCaseData, printerName);

                                printMessage = $"Bow Case Label for Case Number { reprintCaseLabelViewModel.CaseNumber } on {workOrderNumber} has re-printed.";

                            }
                            else
                            {
                                labelPrinted = false;

                            }

                            //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                            if (labelPrinted == false)
                            {
                                labelPrintError = true;

                                errors = $"Bow Case Number { reprintCaseLabelViewModel.CaseNumber } for { workOrderNumber } did dot re-print. Please try again.";

                            }
                        }
                        else
                        {
                            labelPrintError = true;

                            errors = $"Case Number { reprintCaseLabelViewModel.CaseNumber } is greater than the Work Order Quantity.  Please enter a valid Case Number and try again.";
                        }
                    }
                    else
                    {
                        labelPrintError = true;

                        errors = $"The Work Order Number { workOrderNumber } is not setup.  Please setup the Work Order in Setup Bow Case Label Setup and try again.";

                    }
                }
                else
                {
                    labelPrintError = true;

                    errors = $"The Work Order Number { workOrderNumber } associated with Serial Number { reprintCaseLabelViewModel.SerialNumber } does not exist in Ramco.  There may be an error with the relationship.  Please verify the Serial Number is correct and try again.  If you get this error message again, please contact IT";
                }

            }
            else 
            {
                labelPrintError = true;

                errors = $"Serial Number { reprintCaseLabelViewModel.SerialNumber } is invalid.  Please enter a valide Serial Number and try again.";

            }

            //if there are errors then the the error string is populated
            if (labelPrintError == true)
            {
                ViewData["Messages"] = errors;
            }
            else
            {
                ViewData["Messages"] = printMessage;
            }

            //clear the model state before returning the view.
            ModelState.Clear();

            //set the label class as 100 for decoration ticket
            string labelClassID = "100";

            var data = LoadLabelPrinters(labelClassID);

            reprintCaseLabelViewModel = new ReprintCaseLabelViewModel();

            reprintCaseLabelViewModel.LabelPrinters = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                reprintCaseLabelViewModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            return View(reprintCaseLabelViewModel);
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        public IActionResult WorkOrderSerialModify()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        public IActionResult WorkOrderSerial_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetWorkOrderSerial().ToDataSourceResult(request));
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        private static IEnumerable<WorkOrderSerialModel> GetWorkOrderSerial()
        {
            var data = LoadWorkOrderSerials();

            List <WorkOrderSerialModel> workOrderSerialModel = new List<WorkOrderSerialModel> ();

            foreach (var row in data)
            {
                workOrderSerialModel.Add(new WorkOrderSerialModel
                {
                    ID = row.ID,
                    SerialNumber = row.SerialNumber,
                    WorkOrderNumber = row.WorkOrderNumber,
                    BuildDate = row.BuildDate,
                    ItemCode = row.ItemCode,
                    ItemDescription = row.ItemDescription,
                    BuilderName = row.BuilderName
                 
                });
            }

            return workOrderSerialModel;
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing Admin")]
        [AcceptVerbs("Post")]
        public IActionResult WorkOrderSerial_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<WorkOrderSerialModel> workOrderSerials)
        {
            if (workOrderSerials != null && ModelState.IsValid)
            {
                foreach (var workOrderSerial in workOrderSerials)
                {
                    //don't want there to be null fields into the table.  Pepperi has a value of "" for the custom fields we want to do the same
                    if (workOrderSerial.SerialNumber == null)
                    {
                        workOrderSerial.SerialNumber = "";
                    }

                    if (workOrderSerial.WorkOrderNumber == null)
                    {
                        workOrderSerial.WorkOrderNumber = "";
                    }

                    if (workOrderSerial.BuildDate == null)
                    {
                        workOrderSerial.BuildDate = "";
                    }

                    if (workOrderSerial.BuilderName == null)
                    {
                        workOrderSerial.BuilderName = "";
                    }

                    int recordsUpdated = UpdateWorkOrderSerials(workOrderSerial.ID, workOrderSerial.SerialNumber, workOrderSerial.WorkOrderNumber, workOrderSerial.BuildDate, workOrderSerial.BuilderName);
                }
            }

            return Json(workOrderSerials.ToDataSourceResult(request, ModelState));
        }

       
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult ManufacturingLabels()
        {
            //
            string groupID = "1";

            var data = LoadLabelClasses(groupID);

            var manufacturingLabelsViewModel = new ManufacturingLabelsViewModel();

            manufacturingLabelsViewModel.LabelClasses = new List<SelectListItem>();

            //load the class names into the view model
            foreach (var row in data)
            {

                manufacturingLabelsViewModel.LabelClasses.Add(new SelectListItem
                {
                    Text = row.ClassID.ToString() + " - " + row.ClassName.ToString(),
                    Value = row.ClassID.ToString()
                });
            }

            return View(manufacturingLabelsViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult ManufacturingLabels(ManufacturingLabelsViewModel manufacturingLabelsViewModel)
        {
            bool labelPrintError = false;
            string errors = "";
            string printMessage = "";

            //had to take out (ModelState.IsValid & warrantyLabelsViewModel != null) because different classes don't use all model properties
            //if (ModelState.IsValid)

            bool labelPrinted = false;
            bool isValidPartNumber = false;

            string printerName = "";
            string numberOfCopies = "";
            string labelClass = "";
            string partNumber = "";
            string partDescription = "";
            string UPC = "";
            string warehouse = "";
            string zone = "";
            string uom = "";

            string labelManufacturingData = "";
            string labelManufacturingFieldNames = "";

            if (manufacturingLabelsViewModel.NumberOfCopies > 0)
            {

                if ((manufacturingLabelsViewModel.NumberOfCopies % 1) == 0)
                {

                    if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "101")
                    {

                        //check if the Work Order is is valid
                        isValidPartNumber = CheckPartNumber(manufacturingLabelsViewModel.PartNumber);

                        if (isValidPartNumber == true)
                        {

                            //get the printer that was selected on post
                            var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                            //load the printer name into a string
                            foreach (var row in labelPrinterData)
                            {
                                printerName = row.PrinterName;
                            }

                            var labelData = LoadLabelDataPartNumber(manufacturingLabelsViewModel.PartNumber);

                            foreach (var row in labelData)
                            {
                                partNumber = row.PartNumber;
                                partDescription = row.PartDescription;
                                UPC = row.UPC;

                            }

                            labelManufacturingData = $"{partNumber}|{partDescription}|{partDescription}|{manufacturingLabelsViewModel.SerialNumber}|{partNumber}|{UPC}|{manufacturingLabelsViewModel.SerialNumber}";

                            if (labelManufacturingData.Length > 0)
                            {
                                //define the field names for the selected shipping label
                                labelManufacturingFieldNames = $"PartNumber|PartDescription1|PartDescription2|SerialNumber|Barcode1|Barcode2|Barcode3";

                                labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                                labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                                numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                                labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                                printMessage = "Manufacturing Case (Warranty) label(s) have printed with no errors." + "\\r\\n";
                            }
                            else
                            {
                                labelPrinted = false;

                            }

                            //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                            if (labelPrinted == false)
                            {
                                labelPrintError = true;

                                errors += $"The Manufacturing Case (Warranty) label(s) did not print. Try re-printing again" + "\\r\\n";

                            }

                        }
                        else
                        {
                            labelPrintError = true;

                            errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                        }

                    }
                    else if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "102")
                    {

                        //check if the Work Order is is valid
                        isValidPartNumber = CheckPartNumber(manufacturingLabelsViewModel.PartNumber);

                        if (isValidPartNumber == true)
                        {

                            //get the printer that was selected on post
                            var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                            //load the printer name into a string
                            foreach (var row in labelPrinterData)
                            {
                                printerName = row.PrinterName;
                            }

                            var labelData = LoadLabelDataPartNumber(manufacturingLabelsViewModel.PartNumber);

                            foreach (var row in labelData)
                            {
                                partNumber = row.PartNumber;
                                partDescription = row.PartDescription;
                                warehouse = row.Warehouse;
                                zone = row.Zone;
                                uom = row.UOM;

                            }

                            labelManufacturingData = $"{partNumber}|{partDescription}|{warehouse}|{zone}|{manufacturingLabelsViewModel.Quantity}|{uom}|{partNumber}";

                            if (labelManufacturingData.Length > 0)
                            {
                                //define the field names for the selected shipping label
                                labelManufacturingFieldNames = $"PartNumber|PartDescription1|Warehouse|Zone|Quantity|UOM|Barcode1";

                                labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                                labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                                numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                                labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                                printMessage = "Manufacturing Stocking label(s) have printed with no errors." + "\\r\\n";
                            }
                            else
                            {
                                labelPrinted = false;

                            }

                            //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                            if (labelPrinted == false)
                            {
                                labelPrintError = true;

                                errors += $"The Manufacturing Stocking label(s) did not print. Try re-printing again" + "\\r\\n";

                            }

                        }
                        else
                        {
                            labelPrintError = true;

                            errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                        }

                    }
                    else if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "103")
                    {

                        //check if the Work Order is is valid
                        isValidPartNumber = CheckPartNumber(manufacturingLabelsViewModel.PartNumber);

                        if (isValidPartNumber == true)
                        {

                            //get the printer that was selected on post
                            var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                            //load the printer name into a string
                            foreach (var row in labelPrinterData)
                            {
                                printerName = row.PrinterName;
                            }

                            var labelData = LoadLabelDataPartNumber(manufacturingLabelsViewModel.PartNumber);

                            foreach (var row in labelData)
                            {
                                partNumber = row.PartNumber;
                                partDescription = row.PartDescription;
                                UPC = row.UPC;

                            }

                            labelManufacturingData = $"{partNumber}|{partDescription}|{manufacturingLabelsViewModel.Quantity}|{partNumber}|{UPC}";

                            if (labelManufacturingData.Length > 0)
                            {
                                //define the field names for the selected shipping label
                                labelManufacturingFieldNames = $"PartNumber|PartDescription1|Quantity|Barcode1|Barcode2";

                                labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                                labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                                numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                                labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                                printMessage = "Manufacturing MM Stocking label(s) have printed with no errors." + "\\r\\n";
                            }
                            else
                            {
                                labelPrinted = false;

                            }

                            //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                            if (labelPrinted == false)
                            {
                                labelPrintError = true;

                                errors += $"The Manufacturing MM Stocking label(s) did not print. Try re-printing again" + "\\r\\n";

                            }

                        }
                        else
                        {
                            labelPrintError = true;

                            errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                        }


                    }
                    else if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "104")
                    {
                        //get the printer that was selected on post
                        var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                        //load the printer name into a string
                        foreach (var row in labelPrinterData)
                        {
                            printerName = row.PrinterName;
                        }

                        labelManufacturingData = $"{manufacturingLabelsViewModel.LabelText}";

                        if (labelManufacturingData.Length > 0)
                        {

                            //define the field names for the selected shipping label
                            labelManufacturingFieldNames = $"Text";

                            labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                            labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                            numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                            labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                            printMessage = "The Manufacturing Large Blank Bin label(s) have printed with no errors." + "\\r\\n";
                        }
                        else
                        {
                            labelPrinted = false;

                        }

                        //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                        if (labelPrinted == false)
                        {
                            labelPrintError = true;

                            errors += $"The Manufacturing Large Blank Bin label(s) did not print. Try re-printing again" + "\\r\\n";

                        }

                    }
                    else if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "105")
                    {

                        //check if the Work Order is is valid
                        isValidPartNumber = CheckPartNumber(manufacturingLabelsViewModel.PartNumber);

                        if (isValidPartNumber == true)
                        {

                            //get the printer that was selected on post
                            var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                            //load the printer name into a string
                            foreach (var row in labelPrinterData)
                            {
                                printerName = row.PrinterName;
                            }

                            var labelData = LoadLabelDataPartNumber(manufacturingLabelsViewModel.PartNumber);

                            foreach (var row in labelData)
                            {
                                partNumber = row.PartNumber;
                                partDescription = row.PartDescription;

                            }

                            labelManufacturingData = $"{partNumber}|{partDescription}";

                            if (labelManufacturingData.Length > 0)
                            {
                                //define the field names for the selected shipping label
                                labelManufacturingFieldNames = $"PartNumber|PartDescription1";

                                labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                                labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                                numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                                labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                                printMessage = "Manufacturing Large Bin label(s) have printed with no errors." + "\\r\\n";
                            }
                            else
                            {
                                labelPrinted = false;

                            }

                            //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                            if (labelPrinted == false)
                            {
                                labelPrintError = true;

                                errors += $"The Manufacturing Large Bin label(s) did not print. Try re-printing again" + "\\r\\n";

                            }

                        }
                        else
                        {
                            labelPrintError = true;

                            errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                        }

                    }
                    else if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "106")
                    {

                        //check if the Work Order is is valid
                        isValidPartNumber = CheckPartNumber(manufacturingLabelsViewModel.PartNumber);

                        if (isValidPartNumber == true)
                        {

                            //get the printer that was selected on post
                            var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                            //load the printer name into a string
                            foreach (var row in labelPrinterData)
                            {
                                printerName = row.PrinterName;
                            }

                            var labelData = LoadLabelDataPartNumber(manufacturingLabelsViewModel.PartNumber);

                            foreach (var row in labelData)
                            {
                                partNumber = row.PartNumber;
                                partDescription = row.PartDescription;

                            }

                            labelManufacturingData = $"{partNumber}|{partDescription}|{manufacturingLabelsViewModel.LabelText}";

                            if (labelManufacturingData.Length > 0)
                            {
                                //define the field names for the selected shipping label
                                labelManufacturingFieldNames = $"PartNumber|PartDescription1|Text";

                                labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                                labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                                numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                                labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                                printMessage = "Manufacturing Large Bin With Cell label(s) have printed with no errors." + "\\r\\n";
                            }
                            else
                            {
                                labelPrinted = false;

                            }

                            //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                            if (labelPrinted == false)
                            {
                                labelPrintError = true;

                                errors += $"The Manufacturing Large Bin With Cell label(s) did not print. Try re-printing again" + "\\r\\n";

                            }

                        }
                        else
                        {
                            labelPrintError = true;

                            errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                        }


                    }
                    else if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "107")
                    {

                        //check if the Work Order is is valid
                        isValidPartNumber = CheckPartNumber(manufacturingLabelsViewModel.PartNumber);

                        if (isValidPartNumber == true)
                        {

                            //get the printer that was selected on post
                            var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                            //load the printer name into a string
                            foreach (var row in labelPrinterData)
                            {
                                printerName = row.PrinterName;
                            }

                            var labelData = LoadLabelDataPartNumber(manufacturingLabelsViewModel.PartNumber);

                            foreach (var row in labelData)
                            {
                                partNumber = row.PartNumber;
                                partDescription = row.PartDescription;

                            }

                            labelManufacturingData = $"{partNumber}|{partDescription}|{manufacturingLabelsViewModel.LabelText}";

                            if (labelManufacturingData.Length > 0)
                            {
                                //define the field names for the selected shipping label
                                labelManufacturingFieldNames = $"PartNumber|PartDescription1|Text";

                                labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                                labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                                numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                                labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                                printMessage = "Manufacturing Small Bin With Cell label(s) have printed with no errors." + "\\r\\n";
                            }
                            else
                            {
                                labelPrinted = false;

                            }

                            //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                            if (labelPrinted == false)
                            {
                                labelPrintError = true;

                                errors += $"The Manufacturing Small Bin With Cell label(s) did not print. Try re-printing again" + "\\r\\n";

                            }

                        }
                        else
                        {
                            labelPrintError = true;

                            errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                        }

                    }
                    else if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "108")
                    {
                        //get the printer that was selected on post
                        var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                        //load the printer name into a string
                        foreach (var row in labelPrinterData)
                        {
                            printerName = row.PrinterName;
                        }

                        labelManufacturingData = $"{manufacturingLabelsViewModel.LabelText}";

                        if (labelManufacturingData.Length > 0)
                        {

                            //define the field names for the selected shipping label
                            labelManufacturingFieldNames = $"Text";

                            labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                            labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                            numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                            labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                            printMessage = "The Manufacturing Small Blank label(s) have printed with no errors." + "\\r\\n";
                        }
                        else
                        {
                            labelPrinted = false;

                        }

                        //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                        if (labelPrinted == false)
                        {
                            labelPrintError = true;

                            errors += $"The Manufacturing Small Blank label(s) did not print. Try re-printing again" + "\\r\\n";

                        }

                    }
                    else if (manufacturingLabelsViewModel.SelectedClassID.ToString() == "109")
                    {
                        //get the printer that was selected on post
                        var labelPrinterData = LoadLabelPrinterName(manufacturingLabelsViewModel.SelectedPrinterID.ToString());

                        //load the printer name into a string
                        foreach (var row in labelPrinterData)
                        {
                            printerName = row.PrinterName;
                        }

                        labelManufacturingData = $"{manufacturingLabelsViewModel.LabelText}";

                        if (labelManufacturingData.Length > 0)
                        {

                            //define the field names for the selected shipping label
                            //labelManufacturingFieldNames = $"Text";

                            //labelManufacturingData = $"{labelManufacturingFieldNames}{Environment.NewLine}{labelManufacturingData}";

                            labelClass = manufacturingLabelsViewModel.SelectedClassID.ToString();

                            numberOfCopies = manufacturingLabelsViewModel.NumberOfCopies.ToString();

                            labelPrinted = GenerateManufacturingLabel(labelManufacturingData, labelClass, printerName, numberOfCopies);

                            printMessage = "The Manufacturing String Stop label(s) have printed with no errors." + "\\r\\n";
                        }
                        else
                        {
                            labelPrinted = false;

                        }

                        //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                        if (labelPrinted == false)
                        {
                            labelPrintError = true;

                            errors += $"The Manufacturing String Stop label(s) did not print. Try re-printing again" + "\\r\\n";

                        }

                    }
                    //end of else if for label clases

                }
                else
                {
                    labelPrintError = true;

                    errors += $"Number of Copies must be a whole number.  Enter in a whole number and try re-printing again" + "\\r\\n";
                }

            }
            else
            {
                labelPrintError = true;

                errors += $"Number of Copies must be greater than 0.  Enter in a number greater than 0 and try re-printing again" + "\\r\\n";
            }
            
            //if there are errors then the the error string is populated
            if (labelPrintError == true)
            {
                ViewData["Messages"] = errors;
            }
            else
            {
                ViewData["Messages"] = printMessage;
            }

            //clear the model state before returning the view.
            ModelState.Clear();

            //
            string groupID = "1";

            var data = LoadLabelClasses(groupID);

            manufacturingLabelsViewModel = new ManufacturingLabelsViewModel();

            manufacturingLabelsViewModel.LabelClasses = new List<SelectListItem>();

            //load the class names into the view model
            foreach (var row in data)
            {

                manufacturingLabelsViewModel.LabelClasses.Add(new SelectListItem
                {
                    Text = row.ClassID.ToString() + " - " + row.ClassName.ToString(),
                    Value = row.ClassID.ToString()
                });
            }

            return View(manufacturingLabelsViewModel);
        }

        //
        [AcceptVerbs("Get", "Post")]
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult LabelClassSelected(int selectedClassID)
        {
            bool result;

            if (selectedClassID == 0)
            {
                result = false;
            }
            else
            {
                result = true;
            }

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public IActionResult LoadManufacturingLabelsViewComponent(ManufacturingLabelsViewModel manufacturingLabelsViewModel)
        {

            return ViewComponent("ManufacturingLabels", manufacturingLabelsViewModel);
        }

        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public JsonResult SearchWorkOrderNumbers(string text)
        {

            var data = LoadWorkOrderNumbers(text);

            if (!string.IsNullOrEmpty(text))
            {
                var result = data.Where(p => p.WorkOrderNumber.Contains(text));
                return Json(result.ToList());
            }
            else
            {
                var result = "";
                return Json(result.ToList());
            }

        }

        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public JsonResult SearchPartNumbers(string text)
        {

            var data = LoadPartNumbers(text);

            if (!string.IsNullOrEmpty(text))
            {
                var result = data.Where(p => p.PartNumber.Contains(text));
                return Json(result.ToList());
            }
            else
            {
                var result = "";
                return Json(result.ToList());
            }

        }

        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public JsonResult SearchBowCaseWorkOrderNumbers(string text)
        {

            var data = LoadBowCaseWorkOrderNumbers(text);

            if (!string.IsNullOrEmpty(text))
            {
                var result = data.Where(p => p.WorkOrderNumber.Contains(text));
                return Json(result.ToList());
            }
            else
            {
                var result = "";
                return Json(result.ToList());
            }

        }

        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public JsonResult SearchSerialNumbers(string text)
        {

            var data = LoadSerialNumbers(text);

            if (!string.IsNullOrEmpty(text))
            {
                var result = data.Where(p => p.SerialNumber.Contains(text));
                return Json(result.ToList());
            }
            else
            {
                var result = "";
                return Json(result.ToList());
            }

        }

        [HttpPost]
        [MultiplePolicysAuthorize("Administration;Manufacturing")]
        public JsonResult KeepSessionAlive()
        {

            return Json(new { Data = "Success" });

        }
    }


}
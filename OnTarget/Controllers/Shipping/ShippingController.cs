using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using OnTarget.Models.Shipping;
using Microsoft.Extensions.Configuration;
using static OnTargetDataLibrary.BusinessLogic.Shipping.ShippingLabelProcessor;
using OnTargetLibrary.Security;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace OnTarget.Controllers.Shipping
{
    public class ShippingController : Controller
    {
        [MultiplePolicysAuthorize("Administration;Shipping")]
        public IActionResult Index()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Shipping")]
        public IActionResult ShippingLabels()
        {
            //
            string groupID = "3";

            var data = LoadLabelClasses(groupID);

            var shippingLabelsViewModel = new ShippingLabelsViewModel();

            shippingLabelsViewModel.LabelClasses = new List<SelectListItem>();

            //load the class names into the view model
            foreach (var row in data)
            {

                shippingLabelsViewModel.LabelClasses.Add(new SelectListItem
                {
                    Text = row.ClassID.ToString() + " - " + row.ClassName.ToString(),
                    Value = row.ClassID.ToString()
                });
            }

            return View(shippingLabelsViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultiplePolicysAuthorize("Administration;Shipping")]
        public IActionResult ShippingLabels(ShippingLabelsViewModel shippingLabelsViewModel)
        {
            bool labelPrintError = false;
            string errors = "";
            string printMessage = "";

            //had to take out (ModelState.IsValid & warrantyLabelsViewModel != null) because different classes don't use all model properties
            //if (ModelState.IsValid)

            bool labelPrinted = false;
            bool isValidWorkOrder = false;
            bool isValidPartNumber = false;

            int quantity = 0;

            string printerName = "";
            string numberOfCopies = "";
            string labelClass = "";
            string partNumber = "";
            string partDescription = "";
            string salesOrderNumber = "";
            string UPC = "";
            string warehouse = "";
            string zone = "";
            string uom = "";

            string labelShippingData = "";
            string labelShippingFieldNames = "";
            
            if (shippingLabelsViewModel.SelectedClassID.ToString() == "300")
            { 

                //check if the Work Order is is valid
                isValidWorkOrder = CheckWorkOrderNumber(shippingLabelsViewModel.WorkOrderNumber);

                if (isValidWorkOrder == true)
                {

                    //get the printer that was selected on post
                    var labelPrinterData = LoadLabelPrinterName(shippingLabelsViewModel.SelectedPrinterID.ToString());

                    //load the printer name into a string
                    foreach (var row in labelPrinterData)
                    {
                        printerName = row.PrinterName;
                    }

                    var labelData = LoadLabelDataWorkOrder(shippingLabelsViewModel.WorkOrderNumber);

                    foreach (var row in labelData)
                    {
                        partNumber = row.PartNumber;
                        partDescription = row.PartDescription;
                        quantity = row.Quantity;
                        salesOrderNumber = row.SalesOrderNumber;
                        UPC = row.UPC;
                    }

                    labelShippingData = $"{partNumber}|{partDescription}|{partDescription}|{shippingLabelsViewModel.WorkOrderNumber}|{quantity}|{salesOrderNumber}|{partNumber}|{UPC}";

                    if (labelShippingData.Length > 0)
                    {
                        //define the field names for the selected shipping label
                        labelShippingFieldNames = $"PartNumber|PartDescription1|PartDescription2|WorkOrderNumber|WorkOrderQty|SalesOrderNumber|Barcode1|Barcode2";

                        labelShippingData = $"{labelShippingFieldNames}{Environment.NewLine}{labelShippingData}";

                        labelClass = shippingLabelsViewModel.SelectedClassID.ToString();

                        //convert work order quantity into number of copies
                        numberOfCopies = quantity.ToString();

                        labelPrinted = GenerateShippingLabel(labelShippingData, labelClass, printerName, numberOfCopies);

                        printMessage = "Shipping Restock Case label(s) have printed with no errors." + "\\r\\n";
                    }
                    else
                    {
                        labelPrinted = false;

                    }

                    //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                    if (labelPrinted == false)
                    {
                        labelPrintError = true;

                        errors += $"The Shipping Restock Case label(s) did not print. Try re-printing again" + "\\r\\n";

                    }

                }
                else
                {
                    labelPrintError = true;

                    errors += $"The Work Order Number entered is not valid.  Enter a valid Work Order Number try re-printing again" + "\\r\\n";
                }

            }
            else if (shippingLabelsViewModel.SelectedClassID.ToString() == "301")
            {

                //check if the Work Order is is valid
                isValidPartNumber = CheckPartNumber(shippingLabelsViewModel.PartNumber);

                if (isValidPartNumber == true)
                {

                    //get the printer that was selected on post
                    var labelPrinterData = LoadLabelPrinterName(shippingLabelsViewModel.SelectedPrinterID.ToString());

                    //load the printer name into a string
                    foreach (var row in labelPrinterData)
                    {
                        printerName = row.PrinterName;
                    }

                    var labelData = LoadLabelDataPartNumber(shippingLabelsViewModel.PartNumber);

                    foreach (var row in labelData)
                    {
                        partNumber = row.PartNumber;
                        partDescription = row.PartDescription;
                        warehouse = row.Warehouse;
                        zone = row.Zone;
                        
                    }

                    labelShippingData = $"{partNumber}|{partDescription}|{warehouse}|{zone}|{shippingLabelsViewModel.Quantity}|{uom}|{partNumber}";

                    if (labelShippingData.Length > 0)
                    {
                        //define the field names for the selected shipping label
                        labelShippingFieldNames = $"PartNumber|PartDescription1|Warehouse|Zone|Quantity|UOM|Barcode1";

                        labelShippingData = $"{labelShippingFieldNames}{Environment.NewLine}{labelShippingData}";

                        labelClass = shippingLabelsViewModel.SelectedClassID.ToString();

                        numberOfCopies = shippingLabelsViewModel.NumberOfCopies.ToString();

                        labelPrinted = GenerateShippingLabel(labelShippingData, labelClass, printerName, numberOfCopies);

                        printMessage = "Shipping Stocking label(s) have printed with no errors." + "\\r\\n";
                    }
                    else
                    {
                        labelPrinted = false;

                    }

                    //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                    if (labelPrinted == false)
                    {
                        labelPrintError = true;

                        errors += $"The Shipping Stocking label(s) did not print. Try re-printing again" + "\\r\\n";

                    }

                }
                else
                {
                    labelPrintError = true;

                    errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                }
            }
            else if (shippingLabelsViewModel.SelectedClassID.ToString() == "302")
            {

                //check if the Work Order is is valid
                isValidPartNumber = CheckPartNumber(shippingLabelsViewModel.PartNumber);

                if (isValidPartNumber == true)
                {

                    //get the printer that was selected on post
                    var labelPrinterData = LoadLabelPrinterName(shippingLabelsViewModel.SelectedPrinterID.ToString());

                    //load the printer name into a string
                    foreach (var row in labelPrinterData)
                    {
                        printerName = row.PrinterName;
                    }

                    var labelData = LoadLabelDataPartNumber(shippingLabelsViewModel.PartNumber);

                    foreach (var row in labelData)
                    {
                        partNumber = row.PartNumber;
                        partDescription = row.PartDescription;
                        
                    }

                    labelShippingData = $"{partNumber}|{partDescription}";

                    if (labelShippingData.Length > 0)
                    {

                        //define the field names for the selected shipping label
                        labelShippingFieldNames = $"PartNumber|PartDescription1";

                        labelShippingData = $"{labelShippingFieldNames}{Environment.NewLine}{labelShippingData}";

                        labelClass = shippingLabelsViewModel.SelectedClassID.ToString();

                        numberOfCopies = shippingLabelsViewModel.NumberOfCopies.ToString();

                        labelPrinted = GenerateShippingLabel(labelShippingData, labelClass, printerName, numberOfCopies);

                        printMessage = "Shipping Large Bin label(s) have printed with no errors." + "\\r\\n";
                    }
                    else
                    {
                        labelPrinted = false;

                    }

                    //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                    if (labelPrinted == false)
                    {
                        labelPrintError = true;

                        errors += $"The Shipping Large Bin label(s) did not print. Try re-printing again" + "\\r\\n";

                    }

                }
                else
                {
                    labelPrintError = true;

                    errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                }

            }
            else if (shippingLabelsViewModel.SelectedClassID.ToString() == "303")
            {

                //check if the Work Order is is valid
                isValidPartNumber = CheckPartNumber(shippingLabelsViewModel.PartNumber);

                if (isValidPartNumber == true)
                {

                    //get the printer that was selected on post
                    var labelPrinterData = LoadLabelPrinterName(shippingLabelsViewModel.SelectedPrinterID.ToString());

                    //load the printer name into a string
                    foreach (var row in labelPrinterData)
                    {
                        printerName = row.PrinterName;
                    }

                    var labelData = LoadLabelDataPartNumber(shippingLabelsViewModel.PartNumber);

                    foreach (var row in labelData)
                    {
                        partNumber = row.PartNumber;
                        partDescription = row.PartDescription;

                    }

                    labelShippingData = $"{partNumber}|{partDescription}";

                    if (labelShippingData.Length > 0)
                    {
                        //define the field names for the selected shipping label
                        labelShippingFieldNames = $"PartNumber|PartDescription1";

                        labelShippingData = $"{labelShippingFieldNames}{Environment.NewLine}{labelShippingData}";

                        labelClass = shippingLabelsViewModel.SelectedClassID.ToString();

                        numberOfCopies = shippingLabelsViewModel.NumberOfCopies.ToString();

                        labelPrinted = GenerateShippingLabel(labelShippingData, labelClass, printerName, numberOfCopies);

                        printMessage = "Shipping Small Bin label(s) have printed with no errors." + "\\r\\n";
                    }
                    else
                    {
                        labelPrinted = false;

                    }

                    //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                    if (labelPrinted == false)
                    {
                        labelPrintError = true;

                        errors += $"The Shipping Small Bin label(s) did not print. Try re-printing again" + "\\r\\n";

                    }

                }
                else
                {
                    labelPrintError = true;

                    errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                }

            }
            else if (shippingLabelsViewModel.SelectedClassID.ToString() == "304")
            {

                //get the printer that was selected on post
                var labelPrinterData = LoadLabelPrinterName(shippingLabelsViewModel.SelectedPrinterID.ToString());

                //load the printer name into a string
                foreach (var row in labelPrinterData)
                {
                    printerName = row.PrinterName;
                }

                labelShippingData = $"{shippingLabelsViewModel.LabelText}";

                if (labelShippingData.Length > 0)
                {
                   
                    //define the field names for the selected shipping label
                    labelShippingFieldNames = $"Text";

                    labelShippingData = $"{labelShippingFieldNames}{Environment.NewLine}{labelShippingData}";

                    labelClass = shippingLabelsViewModel.SelectedClassID.ToString();

                    numberOfCopies = shippingLabelsViewModel.NumberOfCopies.ToString();

                    labelPrinted = GenerateShippingLabel(labelShippingData, labelClass, printerName, numberOfCopies);

                    printMessage = "The Shipping PO Number label(s) have printed with no errors." + "\\r\\n";
                }
                else
                {
                    labelPrinted = false;

                }

                //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                if (labelPrinted == false)
                {
                    labelPrintError = true;

                    errors += $"The Shipping PO Number label(s) did not print. Try re-printing again" + "\\r\\n";

                }

            }
            else if (shippingLabelsViewModel.SelectedClassID.ToString() == "305")
            {

                //check if the Work Order is is valid
                isValidPartNumber = CheckPartNumber(shippingLabelsViewModel.PartNumber);

                if (isValidPartNumber == true)
                {

                    //get the printer that was selected on post
                    var labelPrinterData = LoadLabelPrinterName(shippingLabelsViewModel.SelectedPrinterID.ToString());

                    //load the printer name into a string
                    foreach (var row in labelPrinterData)
                    {
                        printerName = row.PrinterName;
                    }

                    var labelData = LoadLabelDataPartNumber(shippingLabelsViewModel.PartNumber);

                    foreach (var row in labelData)
                    {
                        partNumber = row.PartNumber;
                        partDescription = row.PartDescription;
                        UPC = row.UPC;

                    }

                    labelShippingData = $"{partNumber}|{partDescription}|{shippingLabelsViewModel.Quantity}|{partNumber}|{UPC}";

                    if (labelShippingData.Length > 0)
                    {

                        //define the field names for the selected shipping label
                        labelShippingFieldNames = $"PartNumber|PartDescription1|Quantity|Barcode1|Barcode2";

                        labelShippingData = $"{labelShippingFieldNames}{Environment.NewLine}{labelShippingData}";

                        labelClass = shippingLabelsViewModel.SelectedClassID.ToString();

                        numberOfCopies = shippingLabelsViewModel.NumberOfCopies.ToString();

                        labelPrinted = GenerateShippingLabel(labelShippingData, labelClass, printerName, numberOfCopies);

                        printMessage = "The Shipping MM Stocking label(s) have printed with no errors." + "\\r\\n";
                    }
                    else
                    {
                        labelPrinted = false;

                    }

                    //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                    if (labelPrinted == false)
                    {
                        labelPrintError = true;

                        errors += $"The Shipping MM Stocking label(s) did not print. Try re-printing again" + "\\r\\n";

                    }

                }
                else
                {
                    labelPrintError = true;

                    errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
                }

            }
            else if (shippingLabelsViewModel.SelectedClassID.ToString() == "306")
            {

                //get the printer that was selected on post
                var labelPrinterData = LoadLabelPrinterName(shippingLabelsViewModel.SelectedPrinterID.ToString());

                //load the printer name into a string
                foreach (var row in labelPrinterData)
                {
                    printerName = row.PrinterName;
                }

                labelShippingData = $"Text";

                //define the field names for the selected shipping label
                labelShippingFieldNames = $"Text";

                labelShippingData = $"{labelShippingFieldNames}{Environment.NewLine}{labelShippingData}";

                labelClass = shippingLabelsViewModel.SelectedClassID.ToString();

                numberOfCopies = shippingLabelsViewModel.NumberOfCopies.ToString();

                labelPrinted = GenerateShippingLabel(labelShippingData, labelClass, printerName, numberOfCopies);

                printMessage = "The Shipping MM Missing Parts label(s) have printed with no errors." + "\\r\\n";
                    

                //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                if (labelPrinted == false)
                {
                    labelPrintError = true;

                    errors += $"The Shipping MM Missing Parts label(s) did not print. Try re-printing again" + "\\r\\n";

                }

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
            string groupID = "3";

            var data = LoadLabelClasses(groupID);

            shippingLabelsViewModel = new ShippingLabelsViewModel();

            shippingLabelsViewModel.LabelClasses = new List<SelectListItem>();

            //load the class names into the view model
            foreach (var row in data)
            {

                shippingLabelsViewModel.LabelClasses.Add(new SelectListItem
                {
                    Text = row.ClassID.ToString() + " - " + row.ClassName.ToString(),
                    Value = row.ClassID.ToString()
                });
            }

            return View(shippingLabelsViewModel);
        }

        [AcceptVerbs("Get", "Post")]
        [MultiplePolicysAuthorize("Administration;Shipping")]
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
        [MultiplePolicysAuthorize("Administration;Shipping")]
        public IActionResult LoadShippingLabelsViewComponent(ShippingLabelsViewModel shippingLabelsViewModel)
        {

            return ViewComponent("ShippingLabels", shippingLabelsViewModel);
        }

        [MultiplePolicysAuthorize("Administration;Shipping")]
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

        [MultiplePolicysAuthorize("Administration;Shipping")]
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

        [HttpPost]
        [MultiplePolicysAuthorize("Administration;Shipping")]
        public JsonResult KeepSessionAlive()
        {

            return Json(new { Data = "Success" });

        }
    }
}
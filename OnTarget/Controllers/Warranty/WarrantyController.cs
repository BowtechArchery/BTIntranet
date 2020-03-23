using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using OnTarget.Models.Warranty;
using Microsoft.Extensions.Configuration;
using static OnTargetDataLibrary.BusinessLogic.Warranty.WarrantyLabelProcessor;
using OnTargetLibrary.Security;
using Microsoft.AspNetCore.Mvc.Rendering;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.IO;

namespace OnTarget.Controllers.Warranty
{
    public class WarrantyController : Controller
    {

        [MultiplePolicysAuthorize("Administration;Warranty")]
        public IActionResult Index()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Warranty")]
        public IActionResult WarrantyLabels()
        {
            //
            string groupID = "4";

            var data = LoadLabelClasses(groupID);

            var warrantyLabelsViewModel = new WarrantyLabelsViewModel();

            warrantyLabelsViewModel.LabelClasses = new List<SelectListItem>();

            //load the class names into the view model
            foreach (var row in data)
            {

                warrantyLabelsViewModel.LabelClasses.Add(new SelectListItem
                {
                    Text = row.ClassID.ToString() + " - " + row.ClassName.ToString(),
                    Value = row.ClassID.ToString()
                });
            }

            return View(warrantyLabelsViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        [MultiplePolicysAuthorize("Administration;Warranty")]
        public IActionResult WarrantyLabels(WarrantyLabelsViewModel warrantyLabelsViewModel)
        {
            bool labelPrintError = false;
            string errors = "";

            //had to take out (ModelState.IsValid & warrantyLabelsViewModel != null) because different classes don't use all model properties
            //if (ModelState.IsValid)
            
            bool labelPrinted = false;
            bool isValidPartNumber = false;

            string printerName = "";
            string numberOfCopies = "";
            string labelClass = "";
            string partDescription = "";
            string UPC = "";
            string bowModel = "";
            string serialNumber = "";
            string serialNumberHumanReadable = "";

            string labelWarrantyData = "";
            string labelWarrantyFieldNames = "";
            string labelWarrantyCustomDescription = "";

            //check if the Part Number is valid
            isValidPartNumber = CheckPartNumber(warrantyLabelsViewModel.PartNumber);

            if (isValidPartNumber == true)
            {

                if (warrantyLabelsViewModel.NumberOfCopies > 0)
                {

                    if ((warrantyLabelsViewModel.NumberOfCopies % 1) == 0)
                    {

                        //get the printer that was selected on post
                        var labelPrinterData = LoadLabelPrinterName(warrantyLabelsViewModel.SelectedPrinterID.ToString());

                        //load the printer name into a string
                        foreach (var row in labelPrinterData)
                        {
                            printerName = row.PrinterName;
                        }

                        //make sure the part number is in upper case.  sometimes users enter them with lowercase letters
                        warrantyLabelsViewModel.PartNumber = warrantyLabelsViewModel.PartNumber.ToUpper();

                        var labelData = LoadLabelData(warrantyLabelsViewModel.PartNumber);

                        foreach (var row in labelData)
                        {
                            partDescription = row.PartDescription;
                            UPC = row.UPC;
                            bowModel = row.BowModel;
                        }

                        labelClass = warrantyLabelsViewModel.SelectedClassID.ToString();

                        numberOfCopies = warrantyLabelsViewModel.NumberOfCopies.ToString();

                        if (warrantyLabelsViewModel.SelectedClassID == 400)
                        {
                            //define the field names for the selected warranty label
                            labelWarrantyFieldNames = $"PartNumber|PartDescription1|PartDescription2|SerialNumber|Barcode1|Barcode2|Barcode3";

                            if (warrantyLabelsViewModel.SerialNumber != null)
                            {
                                serialNumber = warrantyLabelsViewModel.SerialNumber;
                                serialNumberHumanReadable = $"Serial #: {warrantyLabelsViewModel.SerialNumber}";
                            }

                            labelWarrantyData = $"{warrantyLabelsViewModel.PartNumber}|{partDescription}|{partDescription}|{serialNumberHumanReadable}|{warrantyLabelsViewModel.PartNumber}|{UPC}|{serialNumber}";
                        }
                        else if (warrantyLabelsViewModel.SelectedClassID == 401)
                        {
                            //define the field names for the selected warranty label
                            labelWarrantyFieldNames = $"PartNumber|PartDescription1|RA|RTV|Barcode1";

                            //53 characters is the max length for the label
                            labelWarrantyData = $"{warrantyLabelsViewModel.PartNumber}|{partDescription}|{warrantyLabelsViewModel.RANumber}|{warrantyLabelsViewModel.RTV}|{UPC}";
                        }
                        else if (warrantyLabelsViewModel.SelectedClassID == 402)
                        {
                            //define the field names for the selected warranty label
                            labelWarrantyFieldNames = $"PartNumber|PartDescription1|RA|DocID|Barcode1";

                            //53 characters is the max length for the label
                            labelWarrantyData = $"{warrantyLabelsViewModel.PartNumber}|{partDescription}|{warrantyLabelsViewModel.RANumber}|{warrantyLabelsViewModel.DocID}|{UPC}";
                        }
                        else if (warrantyLabelsViewModel.SelectedClassID == 403)
                        {
                            //define the field names for the selected warranty label
                            labelWarrantyFieldNames = $"PartNumber|PartDescription1|RA|DocID|Barcode1";

                            //53 characters is the max length for the label
                            labelWarrantyData = $"{warrantyLabelsViewModel.PartNumber}|{partDescription}|{warrantyLabelsViewModel.RANumber}|{warrantyLabelsViewModel.DocID}|{UPC}";
                        }
                        else if (warrantyLabelsViewModel.SelectedClassID == 404)
                        {
                            //create the custom decription for the custom label.  Bow Model + Hand + Draw Weight + Riser Color + Limb Color
                            labelWarrantyCustomDescription = $"{bowModel} {warrantyLabelsViewModel.SelectedBowHand} {warrantyLabelsViewModel.DrawWeight} {warrantyLabelsViewModel.RiserColor} {warrantyLabelsViewModel.LimbColor}";

                            //define the field names for the selected warranty label
                            labelWarrantyFieldNames = $"PartNumber|CustomDescription1|PartDescription1|Barcode1|Barcode2";

                            //53 characters is the max length for the label
                            labelWarrantyData = $"{warrantyLabelsViewModel.PartNumber}|{ labelWarrantyCustomDescription}|{ partDescription }|{warrantyLabelsViewModel.PartNumber}|{UPC}";
                        }
                        else
                        {
                            //define the field names for the selected warranty label
                            labelWarrantyFieldNames = $"PartNumber|PartDescription1|PartDescription2|Barcode1|Barcode2";

                            //53 characters is the max length for the label
                            labelWarrantyData = $"{warrantyLabelsViewModel.PartNumber}|{partDescription}|{partDescription}|{warrantyLabelsViewModel.PartNumber}|{UPC}";
                        }

                        labelWarrantyData = $"{labelWarrantyFieldNames} {Environment.NewLine} {labelWarrantyData}";

                        if (labelWarrantyData.Length > 0)
                        {
                            labelPrinted = GenerateWarrantyLabel(labelWarrantyData, labelClass, printerName, numberOfCopies);
                        }
                        else
                        {
                            labelPrinted = true;
                        }

                        //if labelPrinted is false then there was an error set the label error to true and append to the errors string
                        if (labelPrinted == false)
                        {
                            labelPrintError = true;

                            errors += $"The Warranty Standard Case label(s) did not print. Try re-printing again" + "\\r\\n";

                        }

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
            }
            else
            {
                labelPrintError = true;

                errors += $"The Part Number entered is not valid.  Enter a valid Part Number try re-printing again" + "\\r\\n";
            }

            //if there are errors then the the error string is populated
            if (labelPrintError == true)
            {
                ViewData["Messages"] = errors;
            }
            else
            {
                ViewData["Messages"] = "Warranty Standard Case label(s) have printed with no errors.";
            }

            //clear the model state before returning the view.
            ModelState.Clear();

            //
            string groupID = "4";

            var data = LoadLabelClasses(groupID);

            warrantyLabelsViewModel = new WarrantyLabelsViewModel();

            warrantyLabelsViewModel.LabelClasses = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                warrantyLabelsViewModel.LabelClasses.Add(new SelectListItem
                {
                    Text = row.ClassID.ToString() + " - " + row.ClassName.ToString(),
                    Value = row.ClassID.ToString()
                });
            }

            return View(warrantyLabelsViewModel);
        }

        //
        [AcceptVerbs("Get", "Post")]
        [MultiplePolicysAuthorize("Administration;Warranty")]
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
        [MultiplePolicysAuthorize("Administration;Warranty")]
        public IActionResult LoadWarrantyLabelsViewComponent(WarrantyLabelsViewModel warrantyLabelsViewModel)
        {

            return ViewComponent("WarrantyLabels", warrantyLabelsViewModel);
        }

        [MultiplePolicysAuthorize("Administration;Warranty")]
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
        [MultiplePolicysAuthorize("Administration;Warranty")]
        public JsonResult KeepSessionAlive()
        {

            return Json(new { Data = "Success" });

        }

    }

}

    
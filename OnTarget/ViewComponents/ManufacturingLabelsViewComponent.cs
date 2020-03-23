using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnTarget.Models;
using OnTarget.Models.Manufacturing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnTargetDataLibrary.BusinessLogic.Manufacturing.ManufacturingLabelProcessor;

namespace OnTarget.ViewComponents
{
    public class ManufacturingLabelsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ManufacturingLabelsViewModel manufacturingLabelsViewModel)
        {

            string SelectedView = "Default";

            var data = LoadLabelPrinters(manufacturingLabelsViewModel.SelectedClassID.ToString());

            manufacturingLabelsViewModel.LabelPrinters = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                manufacturingLabelsViewModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            if (manufacturingLabelsViewModel.SelectedClassID == 101)
            {
                SelectedView = "LabelClass101";
            }
            else if (manufacturingLabelsViewModel.SelectedClassID == 102)
            {
                SelectedView = "LabelClass102";
            }
            else if (manufacturingLabelsViewModel.SelectedClassID == 103)
            {
                SelectedView = "LabelClass103";
            }
            else if (manufacturingLabelsViewModel.SelectedClassID == 104)
            {
                SelectedView = "LabelClass104";
            }
            else if (manufacturingLabelsViewModel.SelectedClassID == 105)
            {
                SelectedView = "LabelClass105";
            }
            else if (manufacturingLabelsViewModel.SelectedClassID == 106)
            {
                SelectedView = "LabelClass106";
            }
            else if (manufacturingLabelsViewModel.SelectedClassID == 107)
            {
                SelectedView = "LabelClass107";
            }
            else if (manufacturingLabelsViewModel.SelectedClassID == 108)
            {
                SelectedView = "LabelClass108";
            }
            else if (manufacturingLabelsViewModel.SelectedClassID == 109)
            {
                manufacturingLabelsViewModel.LabelText = @"String Stop Assy 4.75""" + Environment.NewLine + @"Part# 97081 (HDW String Stop Rod 4.75"")" + Environment.NewLine +  "Qty 1 per" + Environment.NewLine + "Part# 97090S (HDW String Stop TC)" + Environment.NewLine + "Qty 1 per";
                
                SelectedView = "LabelClass109";
            }
            else
            {
                SelectedView = "Default";
            }

            return View(SelectedView, manufacturingLabelsViewModel);
        }
    }
}

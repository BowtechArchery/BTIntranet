using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnTarget.Models;
using OnTarget.Models.Warranty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnTargetDataLibrary.BusinessLogic.Warranty.WarrantyLabelProcessor;

namespace OnTarget.ViewComponents
{
    public class WarrantyLabelsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(WarrantyLabelsViewModel warrantyLabelsViewModel)
        {

            string SelectedView  = "Default";

            var data = LoadLabelPrinters(warrantyLabelsViewModel.SelectedClassID.ToString());

            warrantyLabelsViewModel.LabelPrinters = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                warrantyLabelsViewModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            warrantyLabelsViewModel.BowHands = new List<SelectListItem>();

            //load bow hands into the model
            warrantyLabelsViewModel.BowHands.Add(new SelectListItem
            {
                Text = "RH",
                Value = "RH"
            });

            warrantyLabelsViewModel.BowHands.Add(new SelectListItem
            {
                Text = "LH",
                Value = "LH"
            });


            if (warrantyLabelsViewModel.SelectedClassID == 400)
            {
                SelectedView = "LabelClass400";
            }
            else if (warrantyLabelsViewModel.SelectedClassID == 401)
            {
                SelectedView = "LabelClass401";
            }
            else if (warrantyLabelsViewModel.SelectedClassID == 402)
            {
                SelectedView = "LabelClass402";
            }
            else if (warrantyLabelsViewModel.SelectedClassID == 403)
            {
                SelectedView = "LabelClass403";
            }
            else if (warrantyLabelsViewModel.SelectedClassID == 404)
            {
                SelectedView = "LabelClass404";
            }
            else
            {
                SelectedView = "Default";
            }

            return View(SelectedView, warrantyLabelsViewModel);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnTarget.Models;
using OnTarget.Models.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OnTargetDataLibrary.BusinessLogic.Shipping.ShippingLabelProcessor;

namespace OnTarget.ViewComponents
{
    public class ShippingLabelsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ShippingLabelsViewModel shippingLabelsViewModel)
        {

            string SelectedView = "Default";

            var data = LoadLabelPrinters(shippingLabelsViewModel.SelectedClassID.ToString());

            shippingLabelsViewModel.LabelPrinters = new List<SelectListItem>();

            //load the printer names into the model
            foreach (var row in data)
            {

                shippingLabelsViewModel.LabelPrinters.Add(new SelectListItem
                {
                    Text = row.PrinterDescription.ToString(),
                    Value = row.PrinterID.ToString()
                });
            }

            if (shippingLabelsViewModel.SelectedClassID == 300)
            {
                SelectedView = "LabelClass300";
            }
            else if (shippingLabelsViewModel.SelectedClassID == 301)
            {
                SelectedView = "LabelClass301";
            }
            else if (shippingLabelsViewModel.SelectedClassID == 302)
            {
                SelectedView = "LabelClass302";
            }
            else if (shippingLabelsViewModel.SelectedClassID == 303)
            {
                SelectedView = "LabelClass303";
            }
            else if (shippingLabelsViewModel.SelectedClassID == 304)
            {
                SelectedView = "LabelClass304";
            }
            else if (shippingLabelsViewModel.SelectedClassID == 305)
            {
                SelectedView = "LabelClass305";
            }
            else if (shippingLabelsViewModel.SelectedClassID == 306)
            {
                SelectedView = "LabelClass306";
            }
            else
            {
                SelectedView = "Default";
            }

            return View(SelectedView, shippingLabelsViewModel);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using OnTarget.Models.Sales;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using static OnTargetDataLibrary.BusinessLogic.Sales.CustomBowFieldsProcessor;
using OnTargetLibrary.Security;

namespace OnTarget.Controllers.Sales
{
    public class SalesController : Controller
    {
        [MultiplePolicysAuthorize("Administration;Sales")]
        public IActionResult Index()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Sales")]
        public IActionResult ModifyCustomBowFields()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Sales")]
        public IActionResult CustomBowFields_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetCustomBowFields().ToDataSourceResult(request));
        }

        private static IEnumerable<CustomBowFieldModel> GetCustomBowFields()
        {
            var data = LoadCustomBowFields();

            List<CustomBowFieldModel> custombowfieldmodel = new List<CustomBowFieldModel>();

            foreach (var row in data)
            {
                custombowfieldmodel.Add(new CustomBowFieldModel
                {
                    IndexKey = row.IndexKey,
                    PepperiOrderNum = row.PepperiOrderNum,
                    ItemCode = row.ItemCode,
                    ItemDesc = row.ItemDesc,
                    CustomBowModel = row.CustomBowModel,
                    CustomBowHand = row.CustomBowHand,
                    CustomBowDrawWeight = row.CustomBowHand,
                    CustomBowRiserColor = row.CustomBowRiserColor,
                    CustomBowLimbColor = row.CustomBowLimbColor
                });
            }

            return custombowfieldmodel;
        }

        [MultiplePolicysAuthorize("Administration;Sales")]
        [AcceptVerbs("Post")]
        public IActionResult CustomBowFields_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<CustomBowFieldModel> customBowFields)
        {
            if (customBowFields != null && ModelState.IsValid)
            {
                foreach (var customBowField in customBowFields)
                {
                    //don't want there to be null fields into the table.  Pepperi has a value of "" for the custom fields we want to do the same
                    if(customBowField.CustomBowModel == null)
                    {
                        customBowField.CustomBowModel = "";
                    }

                    if (customBowField.CustomBowHand == null)
                    {
                        customBowField.CustomBowHand = "";
                    }

                    if (customBowField.CustomBowDrawWeight == null)
                    {
                        customBowField.CustomBowDrawWeight = "";
                    }

                    if (customBowField.CustomBowRiserColor == null)
                    {
                        customBowField.CustomBowRiserColor = "";
                    }

                    if (customBowField.CustomBowLimbColor == null)
                    {
                        customBowField.CustomBowLimbColor = "";
                    }

                    int recordsUpdated = UpdateCustomBowFields(customBowField.IndexKey, customBowField.CustomBowModel, customBowField.CustomBowHand, customBowField.CustomBowDrawWeight, customBowField.CustomBowRiserColor, customBowField.CustomBowLimbColor);
                }
            }

            return Json(customBowFields.ToDataSourceResult(request, ModelState));
        }

    }
}
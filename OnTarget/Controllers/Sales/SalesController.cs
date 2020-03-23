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
using static OnTargetDataLibrary.BusinessLogic.Sales.CustomerCoordinatesProcessor;
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

        [MultiplePolicysAuthorize("Administration;Sales")]
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
                    CustomBowDrawWeight = row.CustomBowDrawWeight,
                    CustomBowRiserColor = row.CustomBowRiserColor,
                    CustomBowLimbColor = row.CustomBowLimbColor,
                    CustomBowGrip = row.CustomBowGrip,
                    CustomBowOrbit = row.CustomBowOrbit
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

                    if (customBowField.CustomBowGrip == null)
                    {
                        customBowField.CustomBowGrip = "";
                    }

                    if (customBowField.CustomBowOrbit == null)
                    {
                        customBowField.CustomBowOrbit = "";
                    }

                    int recordsUpdated = UpdateCustomBowFields(customBowField.IndexKey, customBowField.CustomBowModel, customBowField.CustomBowHand, customBowField.CustomBowDrawWeight, customBowField.CustomBowRiserColor, customBowField.CustomBowLimbColor, customBowField.CustomBowGrip, customBowField.CustomBowOrbit);
                }
            }

            return Json(customBowFields.ToDataSourceResult(request, ModelState));
        }

        [MultiplePolicysAuthorize("Administration;Sales")]
        public IActionResult CustomerCoordinates()
        {
            return View();
        }

        [MultiplePolicysAuthorize("Administration;Sales")]
        public IActionResult CustomerCoordinates_Load([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetCustomerCoordinates().ToDataSourceResult(request));
        }

        private static IEnumerable<CustomerCoordinatesModel> GetCustomerCoordinates()
        {
            var data = LoadCustomerCoordinates();

            List<CustomerCoordinatesModel> customercoordinatesmodel = new List<CustomerCoordinatesModel>();

            foreach (var row in data)
            {
                customercoordinatesmodel.Add(new CustomerCoordinatesModel
                {
                    Site = row.Site,
                    CustNum = row.CustNum,
                    CustName = row.CustName,
                    Address = row.Address,
                    City = row.City,
                    State = row.State,
                    Zip = row.Zip,
                    Country = row.Country,
                    Latitude = row.Latitude,
                    Longitude = row.Longitude
                });
            }

            return customercoordinatesmodel;
        }

        //inserts an customer coordinate in the Telerick Grid
        [MultiplePolicysAuthorize("Administration;Sales")]
        [AcceptVerbs("Post")]
        public IActionResult CustomerCoordinates_Insert([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<CustomerCoordinatesModel> customerCoordinates)
        {

            var results = new List<CustomerCoordinatesModel>();

            if (customerCoordinates != null && ModelState.IsValid)
            {
                foreach (var customerCoordinate in customerCoordinates)
                {
                    bool isValidCustNum = false;

                    //check if the cust num is valid
                    isValidCustNum = CheckCustNum(customerCoordinate.CustNum);

                    //check if the custnum is not a dupe
                    isValidCustNum = CheckCustNumDupes(customerCoordinate.CustNum);

                    if (isValidCustNum == true)
                    {
                        int recordsCreated = InsertCustomerCoordinates(customerCoordinate.CustNum, customerCoordinate.Latitude, customerCoordinate.Longitude);
                        results.Add(customerCoordinate);
                    }
                    else
                    {
                        results.Remove(customerCoordinate);
                        ModelState.AddModelError("Invalid Customer Number", $"{customerCoordinate.CustNum} is an invalid Customer Number.");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Error", "There was an error adding the customer coordinantes.  Please verify customer data and try again.");
            }

            return Json(results.ToDataSourceResult(request, ModelState));
        }

        //updates an customer coordinate in the Telerick Grid
        [MultiplePolicysAuthorize("Administration;Sales")]
        [AcceptVerbs("Post")]
        public IActionResult CustomerCoordinates_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<CustomerCoordinatesModel> customerCoordinates)
        {
            if (customerCoordinates != null && ModelState.IsValid)
            {
                foreach (var customerCoordinate in customerCoordinates)
                {
                    int recordsUpdated = UpdateCustomerCoordinates(customerCoordinate.CustNum, customerCoordinate.Latitude, customerCoordinate.Longitude);
                }
            }

            return Json(customerCoordinates.ToDataSourceResult(request, ModelState));
        }

        //deletes an customer coordinate in the Telerick Grid
        [MultiplePolicysAuthorize("Administration;Sales")]
        [AcceptVerbs("Post")]
        public IActionResult CustomerCoordinates_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<CustomerCoordinatesModel> customerCoordinates)
        {
            if (customerCoordinates.Any())
            {
                foreach (var customerCoordinate in customerCoordinates)
                {
                    int recordsUpdated = DeleteCustomerCoordinates(customerCoordinate.CustNum);
                }
            }

            return Json(customerCoordinates.ToDataSourceResult(request, ModelState));
        }


    }
}
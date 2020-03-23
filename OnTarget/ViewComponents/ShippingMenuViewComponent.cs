using Microsoft.AspNetCore.Mvc;
using OnTarget.Models;
using OnTarget.Models.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.ViewComponents
{
    public class ShippingMenuViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {

            ShippingMenuViewModel shippingmenu = new ShippingMenuViewModel();

            shippingmenu.Title = "Shipping";

            return View(shippingmenu);
        }
    }
}

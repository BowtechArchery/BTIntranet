using Microsoft.AspNetCore.Mvc;
using OnTarget.Models;
using OnTarget.Models.Warranty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.ViewComponents
{
    public class WarrantyMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {

            WarrantyMenuViewModel warrantymenu = new WarrantyMenuViewModel();

            warrantymenu.Title = "Warranty";

            return View(warrantymenu);
        }
    }
}

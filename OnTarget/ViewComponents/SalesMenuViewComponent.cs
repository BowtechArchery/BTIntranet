using Microsoft.AspNetCore.Mvc;
using OnTarget.Models;
using OnTarget.Models.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.ViewComponents
{
    public class SalesMenuViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {

            SalesMenuViewModel salesmenu = new SalesMenuViewModel();

            salesmenu.Title = "Sales";

            return View(salesmenu);
        }
    }
}

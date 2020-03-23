using Microsoft.AspNetCore.Mvc;
using OnTarget.Models;
using OnTarget.Models.Manufacturing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.ViewComponents
{
    public class ManufacturingMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {

            ManufacturingMenuViewModel manufacturingmenu = new ManufacturingMenuViewModel();

            manufacturingmenu.Title = "Manufacturing";

            return View(manufacturingmenu);
           
        }
    }
}

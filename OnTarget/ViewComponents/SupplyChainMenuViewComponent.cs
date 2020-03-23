using Microsoft.AspNetCore.Mvc;
using OnTarget.Models;
using OnTarget.Models.SupplyChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnTarget.ViewComponents
{
    public class SupplyChainMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {

            SupplyChainMenuViewModel supplychainmenu = new SupplyChainMenuViewModel();

            supplychainmenu.Title = "Supply Chain";

            return View(supplychainmenu);
        }
    }
}

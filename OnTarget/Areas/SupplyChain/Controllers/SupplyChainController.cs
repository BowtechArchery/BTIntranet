using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OnTarget.Areas.SupplyChain.Controllers
{   
    [Area("SupplyChain")]
    public class SupplyChainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
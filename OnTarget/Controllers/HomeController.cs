using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OnTarget.Models;
using Microsoft.AspNetCore.Authorization;

namespace OnTarget.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            System.Diagnostics.Debug.WriteLine("Environment: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")=="Development")
            {
                System.Diagnostics.Debug.WriteLine("Dev");
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                System.Diagnostics.Debug.WriteLine("Prod");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}

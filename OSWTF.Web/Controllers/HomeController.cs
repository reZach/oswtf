using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OSWTF.Models;
using OSWTF.Server;
using OSWTF.Server.Models;
using OSWTF.Web.Models;


namespace OSWTF.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [TypeFilter(typeof(ServerSideTestFilter),
            Arguments = new object[] { new string[] { "a" } })]
        public IActionResult Privacy()
        {
            TestEligibility a = ViewBag.Eligibilities;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

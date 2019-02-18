using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OSWTF.Demo.Models;
using OSWTF.Server;

namespace OSWTF.Demo.Controllers
{
    public class HomeController : Controller
    {
        [TypeFilter(typeof(ServerSideTestFilter),
            Arguments = new object[] {new[] {"MyFirstTest"}})]
        public IActionResult Index()
        {
            string variation = ViewBag.ServerSideTests["MyFirstTest"];
            string result = string.Empty;

            if (variation == "0")
            {
                result = "I fall in the control.";
            }
            else if (variation == "1")
            {
                result = "I fall in the variation.";
            }

            result +=
                " Please delete the cookie and refresh this page to re-bucket this user into the control or variation";

            return Content(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MemoryInjectionSample.Models;
using My.Extensions.DependencyInjection.ConfigurableInjection;
using MemoryInjectionSample.Interfaces;

namespace MemoryInjectionSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About([FromServices]IServiceProvider provider,[FromServices]IServiceConfigurationProvider<string, MemoryServiceConfigurationProvider.MemoryConfig> settings, string @in, string @out)
        {
            (settings as MemoryServiceConfigurationProvider).SetInAndOut(@in, @out);

            try
            {
                var dummy = provider.GetService(typeof(IDummy)) as IDummy;
                ViewData["Message"] = dummy.GetDummy();
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
            }
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

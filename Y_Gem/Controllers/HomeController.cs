using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMembershipPlans _membershipPlansRepository;

        public HomeController(IMembershipPlans membershipPlansRepository)
        {
            _membershipPlansRepository = membershipPlansRepository;
        }

        public IActionResult Index()
        {
            var plans = _membershipPlansRepository
                .GetAll()
                .Take(3)
                .ToList();

            return View(plans);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}
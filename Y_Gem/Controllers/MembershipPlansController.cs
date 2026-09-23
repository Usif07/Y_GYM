using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class MembershipPlansController : Controller
    {
        private readonly IMembershipPlans _memPlanRepo;

        public MembershipPlansController(
            IMembershipPlans memPlanRepo)
        {
            _memPlanRepo = memPlanRepo;
        }

        // =========================
        // PUBLIC
        // =========================

        [AllowAnonymous]
        public IActionResult Index()
        {
            var membershipPlans =
                _memPlanRepo.GetAll();

            return View(membershipPlans);
        }

        // =========================
        // ADMIN ONLY
        // =========================

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var plan = _memPlanRepo.GetById(id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var plan = _memPlanRepo.GetById(id);

            if (plan != null)
            {
                _memPlanRepo.delete(id);
                _memPlanRepo.save();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id)
        {
            var plan = _memPlanRepo.GetById(id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(MembershipPlan mp)
        {
            if (!ModelState.IsValid)
            {
                return View(mp);
            }

            _memPlanRepo.update(mp);
            _memPlanRepo.save();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult SaveNew(MembershipPlan mp)
        {
            if (!ModelState.IsValid)
            {
                return View("New", mp);
            }

            _memPlanRepo.insert(mp);
            _memPlanRepo.save();

            return RedirectToAction(nameof(Index));
        }
    }
}
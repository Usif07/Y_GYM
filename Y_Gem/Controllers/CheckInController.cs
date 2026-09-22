using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class CheckInController : Controller
    {
        private readonly ICheckInRepository _checkInRepo;
        private readonly IMemberRepository _memberRepo;

        public CheckInController(ICheckInRepository checkInRepo, IMemberRepository memberRepo)
        {
            _checkInRepo = checkInRepo;
            _memberRepo = memberRepo;
        }

        public IActionResult Index()
        {
            return View(_checkInRepo.GetAllWithDetails());
        }

        public IActionResult Create()
        {
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CheckIn item)
        {
            if (ModelState.IsValid)
            {
                _checkInRepo.Add(item);
                _checkInRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _checkInRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _checkInRepo.GetById(id);
            if (item != null)
            {
                _checkInRepo.Delete(item);
                _checkInRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
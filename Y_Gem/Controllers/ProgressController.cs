using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class ProgressController : Controller
    {
        private readonly IProgressRepository _progressRepo;
        private readonly IMemberRepository _memberRepo;

        public ProgressController(IProgressRepository progressRepo, IMemberRepository memberRepo)
        {
            _progressRepo = progressRepo;
            _memberRepo = memberRepo;
        }

        public IActionResult Index()
        {
            return View(_progressRepo.GetAllWithDetails());
        }

        public IActionResult Create()
        {
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Progress item)
        {
            if (ModelState.IsValid)
            {
                _progressRepo.Add(item);
                _progressRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _progressRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _progressRepo.GetById(id);
            if (item != null)
            {
                _progressRepo.Delete(item);
                _progressRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
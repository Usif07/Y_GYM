using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Y_GYM.Models;
using Y_GYM.Repository;


namespace Y_GYM.Controllers
{
    public class DietPlanController : Controller
    {
        private readonly IDietPlanRepository _dietRepo;
        private readonly IMemberRepository _memberRepo;
        private readonly ICoachRepository _coachRepo;

        public DietPlanController(IDietPlanRepository dietRepo, IMemberRepository memberRepo, ICoachRepository coachRepo)
        {
            _dietRepo = dietRepo;
            _memberRepo = memberRepo;
            _coachRepo = coachRepo;
        }

        public IActionResult Index()
        {
            return View(_dietRepo.GetAllWithDetails());
        }

        public IActionResult Details(int id)
        {
            var item = _dietRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName");
            ViewBag.Coaches = new SelectList(_coachRepo.GetAll(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DietPlan item)
        {
            if (ModelState.IsValid)
            {
                _dietRepo.Add(item);
                _dietRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            ViewBag.Coaches = new SelectList(_coachRepo.GetAll(), "Id", "FullName", item.CoachId);
            return View(item);
        }

        public IActionResult Edit(int id)
        {
            var item = _dietRepo.GetById(id);
            if (item == null) return NotFound();

            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            ViewBag.Coaches = new SelectList(_coachRepo.GetAll(), "Id", "FullName", item.CoachId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DietPlan item)
        {
            if (ModelState.IsValid)
            {
                _dietRepo.Update(item);
                _dietRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            ViewBag.Coaches = new SelectList(_coachRepo.GetAll(), "Id", "FullName", item.CoachId);
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _dietRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _dietRepo.GetById(id);
            if (item != null)
            {
                _dietRepo.Delete(item);
                _dietRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
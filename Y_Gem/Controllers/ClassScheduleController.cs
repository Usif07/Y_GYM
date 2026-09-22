using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Y_GYM.Models;
using Y_GYM.Repository;


namespace Y_GYM.Controllers
{
    public class ClassScheduleController : Controller
    {
        private readonly IClassScheduleRepository _scheduleRepo;
        private readonly IClassRepository _classeRepo;
        private readonly ICoachRepository _coachRepo;

        public ClassScheduleController(IClassScheduleRepository scheduleRepo, IClassRepository classeRepo, ICoachRepository coachRepo)
        {
            _scheduleRepo = scheduleRepo;
            _classeRepo = classeRepo;
            _coachRepo = coachRepo;
        }

        public IActionResult Index()
        {
            return View(_scheduleRepo.GetAllWithDetails());
        }

        public IActionResult Details(int id)
        {
            var item = _scheduleRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Classes = new SelectList(_classeRepo.GetAll(), "Id", "Name");
            ViewBag.Coaches = new SelectList(_coachRepo.GetAll(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClassSchedule item)
        {
            if (ModelState.IsValid)
            {
                _scheduleRepo.Add(item);
                _scheduleRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Classes = new SelectList(_classeRepo.GetAll(), "Id", "Name", item.ClassId);
            ViewBag.Coaches = new SelectList(_coachRepo.GetAll(), "Id", "FullName", item.CoachId);
            return View(item);
        }

        public IActionResult Edit(int id)
        {
            var item = _scheduleRepo.GetById(id);
            if (item == null) return NotFound();

            ViewBag.Classes = new SelectList(_classeRepo.GetAll(), "Id", "Name", item.ClassId);
            ViewBag.Coaches = new SelectList(_coachRepo.GetAll(), "Id", "FullName", item.CoachId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ClassSchedule item)
        {
            if (ModelState.IsValid)
            {
                _scheduleRepo.Update(item);
                _scheduleRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Classes = new SelectList(_classeRepo.GetAll(), "Id", "Name", item.ClassId);
            ViewBag.Coaches = new SelectList(_coachRepo.GetAll(), "Id", "FullName", item.CoachId);
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _scheduleRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _scheduleRepo.GetById(id);
            if (item != null)
            {
                _scheduleRepo.Delete(item);
                _scheduleRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
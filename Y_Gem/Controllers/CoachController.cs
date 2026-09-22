using Microsoft.AspNetCore.Mvc;
using Y_GYM.Models;

using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class CoachController : Controller
    {
        private readonly ICoachRepository _coachRepo;

        public CoachController(ICoachRepository coachRepo)
        {
            _coachRepo = coachRepo;
        }

        // GET: Coach
        public IActionResult Index()
        {
            var coaches = _coachRepo.GetAll();
            return View(coaches);
        }

        // GET: Coach/Details/5
        public IActionResult Details(int id)
        {
            var coach = _coachRepo.GetById(id);
            if (coach == null)
            {
                return NotFound();
            }
            return View(coach);
        }

        // GET: Coach/Create
        public IActionResult Create()
        {
            return View();
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Coach coach)
        {
            if (ModelState.IsValid)
            {
                _coachRepo.Add(coach);
                _coachRepo.Save(); 
                return RedirectToAction(nameof(Index));
            }
            return View(coach);
        }

    
        public IActionResult Edit(int id)
        {
            var coach = _coachRepo.GetById(id);
            if (coach == null)
            {
                return NotFound();
            }
            return View(coach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Coach coach)
        {
            if (ModelState.IsValid)
            {
                _coachRepo.Update(coach);
                _coachRepo.Save();
                return RedirectToAction("Index" );
            }
            return View(coach);
        }

       
        public IActionResult Delete(int id)
        {
            var coach = _coachRepo.GetById(id);
            if (coach == null)
            {
                return NotFound();
            }
            return View(coach);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _coachRepo.GetById(id); // بنجيب المدرب الأول
            if (item != null)
            {
                _coachRepo.Delete(item);
                _coachRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
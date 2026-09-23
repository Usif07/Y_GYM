using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class CoachController : Controller
    {
        private readonly ICoachRepository _coachRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoachController(
            ICoachRepository coachRepo,
            UserManager<ApplicationUser> userManager)
        {
            _coachRepo = coachRepo;
            _userManager = userManager;
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

        // POST: Coach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Coach coach)
        {
            if (!ModelState.IsValid)
            {
                return View(coach);
            }

            _coachRepo.Add(coach);
            _coachRepo.Save();

            return RedirectToAction(nameof(Index));
        }

        // GET: Coach/Edit/5
        public IActionResult Edit(int id)
        {
            var coach = _coachRepo.GetById(id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        // POST: Coach/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Coach coach)
        {
            if (!ModelState.IsValid)
            {
                return View(coach);
            }

            _coachRepo.Update(coach);
            _coachRepo.Save();

            return RedirectToAction(nameof(Index));
        }

        // GET: Coach/Delete/5
        public IActionResult Delete(int id)
        {
            var coach = _coachRepo.GetById(id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        // POST: Coach/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var coach = _coachRepo.GetById(id);

            if (coach != null)
            {
                _coachRepo.Delete(coach);
                _coachRepo.Save();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
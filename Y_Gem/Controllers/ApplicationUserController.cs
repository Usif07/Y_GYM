using Microsoft.AspNetCore.Mvc;
using Y_GYM.Models;
using Y_GYM.Repository;


namespace Y_GYM.Controllers
{
    public class ApplicationUserController : Controller
    {
        private readonly IApplicationUserRepository _repo;

        public ApplicationUserController(IApplicationUserRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var items = _repo.GetAll();
            return View(items);
        }

        public IActionResult Details(int id)
        {
            var item = _repo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationUser item)
        {
            if (ModelState.IsValid)
            {
                _repo.Add(item);
                _repo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public IActionResult Edit(int id)
        {
            var item = _repo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationUser item)
        {
            if (ModelState.IsValid)
            {
                _repo.Update(item);
                _repo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _repo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _repo.GetById(id); // استبدل _repo باسم المتغير الصحيح عندك
            if (item != null)
            {
                _repo.Delete(item);
                _repo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
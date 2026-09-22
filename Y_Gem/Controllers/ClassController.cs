using Microsoft.AspNetCore.Mvc;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class ClassController : Controller
    {
        private readonly IClassRepository _ClassRepo;

        public ClassController(IClassRepository ClassRepo)
        {
            _ClassRepo = ClassRepo;
        }

        public IActionResult Index()
        {
            return View(_ClassRepo.GetAll());
        }

        public IActionResult Details(int id)
        {
            var item = _ClassRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Class item)
        {
            if (ModelState.IsValid)
            {
                _ClassRepo.Add(item);
                _ClassRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public IActionResult Edit(int id)
        {
            var item = _ClassRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Class item)
        {
            if (ModelState.IsValid)
            {
                _ClassRepo.Update(item);
                _ClassRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _ClassRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _ClassRepo.GetById(id);
            if (item != null)
            {
                _ClassRepo.Delete(item);
                _ClassRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
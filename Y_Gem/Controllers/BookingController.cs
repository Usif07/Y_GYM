using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IMemberRepository _memberRepo;
        private readonly IClassScheduleRepository _scheduleRepo;

        public BookingController(IBookingRepository bookingRepo, IMemberRepository memberRepo, IClassScheduleRepository scheduleRepo)
        {
            _bookingRepo = bookingRepo;
            _memberRepo = memberRepo;
            _scheduleRepo = scheduleRepo;
        }

        public IActionResult Index()
        {
            return View(_bookingRepo.GetAllWithDetails());
        }

        public IActionResult Details(int id)
        {
            var item = _bookingRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName");
            ViewBag.Schedules = new SelectList(_scheduleRepo.GetAllWithDetails(), "Id", "Id"); // يفضل تعرض تفاصيل الميعاد بدل Id لو متاح
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Booking item)
        {
            if (ModelState.IsValid)
            {
                _bookingRepo.Add(item);
                _bookingRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            ViewBag.Schedules = new SelectList(_scheduleRepo.GetAllWithDetails(), "Id", "Id", item.ScheduleId);
            return View(item);
        }

        public IActionResult Edit(int id)
        {
            var item = _bookingRepo.GetById(id);
            if (item == null) return NotFound();

            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            ViewBag.Schedules = new SelectList(_scheduleRepo.GetAllWithDetails(), "Id", "Id", item.ScheduleId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Booking item)
        {
            if (ModelState.IsValid)
            {
                _bookingRepo.Update(item);
                _bookingRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            ViewBag.Schedules = new SelectList(_scheduleRepo.GetAllWithDetails(), "Id", "Id", item.ScheduleId);
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _bookingRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _bookingRepo.GetById(id);
            if (item != null)
            {
                _bookingRepo.Delete(item);
                _bookingRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
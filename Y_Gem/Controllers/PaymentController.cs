using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IMemberRepository _memberRepo;

        public PaymentController(IPaymentRepository paymentRepo, IMemberRepository memberRepo)
        {
            _paymentRepo = paymentRepo;
            _memberRepo = memberRepo;
        }

        public IActionResult Index()
        {
            return View(_paymentRepo.GetAllWithDetails());
        }

        public IActionResult Create()
        {
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName"); // تأكد إن اسم العضو في الـ Model هو FullName أو Name
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Payment item)
        {
            if (ModelState.IsValid)
            {
                _paymentRepo.Add(item);
                _paymentRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "FullName", item.MemberId);
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _paymentRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _paymentRepo.GetById(id);
            if (item != null)
            {
                _paymentRepo.Delete(item);
                _paymentRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
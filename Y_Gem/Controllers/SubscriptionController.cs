using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Y_GYM.Models;
using Y_GYM.Repository;



namespace Y_GYM.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionRepository _subRepo;
        private readonly IMemberRepository _memberRepo;
        private readonly IMembershipPlans _planRepo; // أو IMembershipPlanRepository حسب ما أنت مسميها

        // ضفنا الـ Repos التانية في الـ Constructor
        public SubscriptionController(ISubscriptionRepository subRepo, IMemberRepository memberRepo, IMembershipPlans planRepo)
        {
            _subRepo = subRepo;
            _memberRepo = memberRepo;
            _planRepo = planRepo;
        }

        public IActionResult Index()
        {
            // استخدمنا الدالة الجديدة عشان نعرض الأسماء بدل الأرقام
            var items = _subRepo.GetAllWithDetails();
            return View(items);
        }

        public IActionResult Details(int id)
        {
            var item = _subRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            // هنجيب الداتا ونبعتها للـ View عشان تظهر في القوائم المنسدلة
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "Name"); // افترضت إن اسم العضو متسجل في Property اسمها Name
            ViewBag.Plans = new SelectList(_planRepo.GetAll(), "Id", "Name"); // افترضت إن اسم الخطة Name
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Subscription item)
        {
            if (ModelState.IsValid)
            {
                _subRepo.Add(item);
                _subRepo.Save();
                return RedirectToAction(nameof(Index));
            }

            // لو الفورم فيها غلط ورجعت تاني، لازم نبعت القوائم تاني عشان متضربش إيرور
            ViewBag.Members = new SelectList(_memberRepo.GetAll(), "Id", "Name", item.MemberId);
            ViewBag.Plans = new SelectList(_planRepo.GetAll(), "Id", "Name", item.PlanId);
            return View(item);
        }

        public IActionResult Edit(int id)
        {
            var item = _subRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Subscription item)
        {
            if (ModelState.IsValid)
            {
                _subRepo.Update(item);
                _subRepo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _subRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // التعديل هنا: بنجيب الأوبجيكت الأول وبعدين نمسحه لأن الـ Repo بتاعك بياخد (T entity)
            var item = _subRepo.GetById(id);
            if (item != null)
            {
                _subRepo.Delete(item);
                _subRepo.Save();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
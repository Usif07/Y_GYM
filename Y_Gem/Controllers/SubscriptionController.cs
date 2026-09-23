using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionRepository _subRepo;
        private readonly IMemberRepository _memberRepo;
        private readonly IMembershipPlans _planRepo;
        private readonly ApplicationDbContext _context;

        public SubscriptionController(
            ISubscriptionRepository subRepo,
            IMemberRepository memberRepo,
            IMembershipPlans planRepo,
            ApplicationDbContext context)
        {
            _subRepo = subRepo;
            _memberRepo = memberRepo;
            _planRepo = planRepo;
            _context = context;
        }


        // ============================================================
        // Index
        // ============================================================

        public IActionResult Index()
        {
            var items = _subRepo
                .GetAllWithDetails()
                .OrderByDescending(s => s.StartDate)
                .ToList();

            UpdateStatuses(items);

            ViewBag.TotalSubscriptions = items.Count;
            ViewBag.ActiveSubscriptions =
                items.Count(s => s.Status == "Active");

            ViewBag.ExpiredSubscriptions =
                items.Count(s => s.Status == "Expired");

            ViewBag.PendingSubscriptions =
                items.Count(s => s.Status == "Pending");

            return View(items);
        }


        // ============================================================
        // Details
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.MembershipPlan)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
                return NotFound();

            UpdateStatus(item);

            return View(item);
        }


        // ============================================================
        // Create - GET
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            var item = new Subscription
            {
                StartDate = DateTime.Today,
                Status = "Pending"
            };

            LoadCreateLists();

            return View(item);
        }


        // ============================================================
        // Create - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subscription item)
        {
            var member = _memberRepo.GetById(item.MemberId);

            if (member == null)
            {
                ModelState.AddModelError(
                    nameof(item.MemberId),
                    "Selected member was not found.");
            }


            var plan = _planRepo
                .GetAll()
                .FirstOrDefault(p => p.Id == item.PlanId);

            if (plan == null)
            {
                ModelState.AddModelError(
                    nameof(item.PlanId),
                    "Selected membership plan was not found.");
            }


            if (plan != null)
            {
                // Calculate EndDate automatically
                item.EndDate =
                    item.StartDate.AddDays(plan.DurationDays);

                item.Status = CalculateStatus(
                    item.StartDate,
                    item.EndDate);
            }


            if (item.StartDate == default)
            {
                ModelState.AddModelError(
                    nameof(item.StartDate),
                    "Start date is required.");
            }


            if (ModelState.IsValid)
            {
                // Make sure navigation properties are not attached
                item.Member = null;
                item.MembershipPlan = null;

                _subRepo.Add(item);
                _subRepo.Save();

                TempData["Success"] =
                    "Subscription has been created successfully.";

                return RedirectToAction(nameof(Index));
            }


            LoadCreateLists(
                item.MemberId,
                item.PlanId);

            return View(item);
        }


        // ============================================================
        // Edit - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
                return NotFound();

            LoadCreateLists(
                item.MemberId,
                item.PlanId);

            return View(item);
        }


        // ============================================================
        // Edit - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Subscription item)
        {
            var memberExists =
                _memberRepo.GetById(item.MemberId) != null;

            if (!memberExists)
            {
                ModelState.AddModelError(
                    nameof(item.MemberId),
                    "Selected member was not found.");
            }


            var plan = _planRepo
                .GetAll()
                .FirstOrDefault(p => p.Id == item.PlanId);

            if (plan == null)
            {
                ModelState.AddModelError(
                    nameof(item.PlanId),
                    "Selected membership plan was not found.");
            }
            else
            {
                item.EndDate =
                    item.StartDate.AddDays(plan.DurationDays);

                item.Status = CalculateStatus(
                    item.StartDate,
                    item.EndDate);
            }


            if (ModelState.IsValid)
            {
                item.Member = null;
                item.MembershipPlan = null;

                _subRepo.Update(item);
                _subRepo.Save();

                TempData["Success"] =
                    "Subscription has been updated successfully.";

                return RedirectToAction(nameof(Index));
            }


            LoadCreateLists(
                item.MemberId,
                item.PlanId);

            return View(item);
        }


        // ============================================================
        // Renew - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Renew(int id)
        {
            var item = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.MembershipPlan)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
                return NotFound();

            UpdateStatus(item);

            return View(item);
        }


        // ============================================================
        // Renew - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewConfirmed(int id)
        {
            var item = await _context.Subscriptions
                .Include(s => s.MembershipPlan)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
                return NotFound();


            var plan = item.MembershipPlan;

            if (plan == null)
            {
                TempData["Error"] =
                    "The membership plan for this subscription was not found.";

                return RedirectToAction(nameof(Index));
            }


            var today = DateTime.Today;

            // If subscription is still active,
            // extend from current EndDate.
            //
            // If it is already expired,
            // start the renewed subscription from today.

            if (item.EndDate.Date >= today)
            {
                item.StartDate = item.StartDate;
                item.EndDate =
                    item.EndDate.AddDays(plan.DurationDays);
            }
            else
            {
                item.StartDate = today;
                item.EndDate =
                    today.AddDays(plan.DurationDays);
            }


            item.Status = CalculateStatus(
                item.StartDate,
                item.EndDate);


            _subRepo.Update(item);
            _subRepo.Save();

            TempData["Success"] =
                "Subscription has been renewed successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // Delete - GET
        // ============================================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.MembershipPlan)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
                return NotFound();

            return View(item);
        }


        // ============================================================
        // Delete - POST
        // ============================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _subRepo.GetById(id);

            if (item == null)
                return NotFound();

            _subRepo.Delete(item);
            _subRepo.Save();

            TempData["Success"] =
                "Subscription has been deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // Load Members + Plans
        // ============================================================

        private void LoadCreateLists(
            int? selectedMemberId = null,
            int? selectedPlanId = null)
        {
            var members = _memberRepo
                .GetAll()
                .OrderBy(m => m.FullName)
                .ToList();


            ViewBag.Members = new SelectList(
                members,
                "Id",
                "FullName",
                selectedMemberId);


            var plans = _planRepo
                .GetAll()
                .OrderBy(p => p.Price)
                .ToList();


            var planItems = plans.Select(p => new
            {
                p.Id,

                DisplayName =
                    $"{p.Name} - {p.Price:N2} - {p.DurationDays} days"
            });


            ViewBag.Plans = new SelectList(
                planItems,
                "Id",
                "DisplayName",
                selectedPlanId);
        }


        // ============================================================
        // Calculate Status
        // ============================================================

        private static string CalculateStatus(
            DateTime startDate,
            DateTime endDate)
        {
            var today = DateTime.Today;

            if (endDate.Date < today)
                return "Expired";

            if (startDate.Date > today)
                return "Pending";

            return "Active";
        }


        // ============================================================
        // Update One Status
        // ============================================================

        private static void UpdateStatus(Subscription item)
        {
            item.Status = CalculateStatus(
                item.StartDate,
                item.EndDate);
        }


        // ============================================================
        // Update All Statuses
        // ============================================================

        private static void UpdateStatuses(
            IEnumerable<Subscription> items)
        {
            foreach (var item in items)
            {
                UpdateStatus(item);
            }
        }
    }
}
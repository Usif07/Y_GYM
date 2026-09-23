using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class PaymentController : Controller
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IMemberRepository _memberRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(
            IPaymentRepository paymentRepo,
            IMemberRepository memberRepo,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _paymentRepo = paymentRepo;
            _memberRepo = memberRepo;
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // INDEX
        // =========================================================

        public IActionResult Index()
        {
            var payments = _paymentRepo
                .GetAllWithDetails()
                .ToList();

            ViewBag.TotalRevenue = payments.Sum(p => p.Amount);
            ViewBag.TotalPayments = payments.Count;

            return View(payments);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateLists();

            return View(new Payment
            {
                PaymentDate = DateTime.Now
            });
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment item)
        {
            // -----------------------------------------------------
            // Validate Member
            // -----------------------------------------------------

            var member = _memberRepo.GetById(item.MemberId);

            if (member == null)
            {
                ModelState.AddModelError(
                    nameof(item.MemberId),
                    "Selected member was not found.");
            }

            // -----------------------------------------------------
            // Validate Subscription
            // -----------------------------------------------------

            if (item.SubscriptionId.HasValue)
            {
                var subscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(
                        s => s.Id == item.SubscriptionId.Value);

                if (subscription == null)
                {
                    ModelState.AddModelError(
                        nameof(item.SubscriptionId),
                        "Selected subscription was not found.");
                }
                else if (subscription.MemberId != item.MemberId)
                {
                    ModelState.AddModelError(
                        nameof(item.SubscriptionId),
                        "The selected subscription does not belong to this member.");
                }
            }

            // -----------------------------------------------------
            // Validate Staff
            // -----------------------------------------------------

            if (item.StaffId.HasValue)
            {
                var staffExists = await _context.Staff
                    .AnyAsync(s => s.Id == item.StaffId.Value);

                if (!staffExists)
                {
                    ModelState.AddModelError(
                        nameof(item.StaffId),
                        "Selected staff member was not found.");
                }
            }

            // -----------------------------------------------------
            // Automatically identify Staff
            // -----------------------------------------------------

            if (!item.StaffId.HasValue)
            {
                var currentUserId = _userManager.GetUserId(User);

                if (!string.IsNullOrWhiteSpace(currentUserId))
                {
                    var currentStaff = await _context.Staff
                        .FirstOrDefaultAsync(
                            s => s.UserId == currentUserId);

                    if (currentStaff != null)
                    {
                        item.StaffId = currentStaff.Id;
                    }
                }
            }

            // -----------------------------------------------------
            // Validate Amount
            // -----------------------------------------------------

            if (item.Amount <= 0)
            {
                ModelState.AddModelError(
                    nameof(item.Amount),
                    "Payment amount must be greater than zero.");
            }

            // -----------------------------------------------------
            // Save
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                item.Member = null;
                item.Subscription = null;
                item.Staff = null;

                _paymentRepo.Add(item);
                _paymentRepo.Save();

                TempData["Success"] =
                    "Payment has been recorded successfully.";

                return RedirectToAction(nameof(Index));
            }

            await LoadCreateLists(
                item.MemberId,
                item.SubscriptionId,
                item.StaffId);

            return View(item);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Member)

                .Include(p => p.Subscription)
                    .ThenInclude(s => s.MembershipPlan)

                .Include(p => p.Staff)
                    .ThenInclude(s => s.User)

                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Member)

                .Include(p => p.Subscription)
                    .ThenInclude(s => s.MembershipPlan)

                .Include(p => p.Staff)
                    .ThenInclude(s => s.User)

                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var payment = _paymentRepo.GetById(id);

            if (payment == null)
            {
                return NotFound();
            }

            _paymentRepo.Delete(payment);
            _paymentRepo.Save();

            TempData["Success"] =
                "Payment has been deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // LOAD CREATE LISTS
        // =========================================================

        private async Task LoadCreateLists(
            int? selectedMemberId = null,
            int? selectedSubscriptionId = null,
            int? selectedStaffId = null)
        {
            // -----------------------------------------------------
            // Members
            // -----------------------------------------------------

            var members = _memberRepo
                .GetAll()
                .OrderBy(m => m.FullName)
                .ToList();

            ViewBag.Members = new SelectList(
                members,
                "Id",
                "FullName",
                selectedMemberId);

            // -----------------------------------------------------
            // Subscriptions
            // -----------------------------------------------------

            var subscriptions = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.MembershipPlan)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            var subscriptionItems = subscriptions.Select(s => new
            {
                s.Id,

                DisplayName =
                    $"#{s.Id} - " +
                    $"{s.Member?.FullName ?? "Unknown Member"} - " +
                    $"{s.MembershipPlan?.Name ?? "No Plan"} - " +
                    $"{s.StartDate:dd/MM/yyyy}"
            });

            ViewBag.Subscriptions = new SelectList(
                subscriptionItems,
                "Id",
                "DisplayName",
                selectedSubscriptionId);

            // -----------------------------------------------------
            // Staff
            // -----------------------------------------------------

            var staff = await _context.Staff
                .Include(s => s.User)
                .OrderBy(s => s.User!.FullName)
                .ToListAsync();

            var staffItems = staff.Select(s => new
            {
                s.Id,

                DisplayName =
                    s.User != null
                        ? $"{s.User.FullName} - {s.JobTitle}"
                        : $"Staff #{s.Id}"
            });

            ViewBag.Staff = new SelectList(
                staffItems,
                "Id",
                "DisplayName",
                selectedStaffId);
        }
    }
}
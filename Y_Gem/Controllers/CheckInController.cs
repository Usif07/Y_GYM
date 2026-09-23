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
    public class CheckInController : Controller
    {
        private readonly ICheckInRepository _checkInRepo;
        private readonly IMemberRepository _memberRepo;
        private readonly ApplicationDbContext _context;

        public CheckInController(
            ICheckInRepository checkInRepo,
            IMemberRepository memberRepo,
            ApplicationDbContext context)
        {
            _checkInRepo = checkInRepo;
            _memberRepo = memberRepo;
            _context = context;
        }

        // =========================
        // INDEX
        // =========================

        public IActionResult Index()
        {
            var items = _checkInRepo
                .GetAllWithDetails()
                .OrderByDescending(c => c.CheckInTime)
                .ToList();

            ViewBag.TotalCheckIns = items.Count;
            ViewBag.AllowedCheckIns =
                items.Count(c => c.Status == "Allowed");

            ViewBag.RejectedCheckIns =
                items.Count(c => c.Status != "Allowed");

            ViewBag.TodayCheckIns =
                items.Count(c => c.CheckInTime.Date == DateTime.Today);

            return View(items);
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            LoadMembers();

            var model = new CheckIn
            {
                CheckInTime = DateTime.Now
            };

            return View(model);
        }

        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckIn item)
        {
            // ---------------------------------
            // Validate Member
            // ---------------------------------

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == item.MemberId);

            if (member == null)
            {
                ModelState.AddModelError(
                    nameof(item.MemberId),
                    "Selected member was not found.");
            }

            // ---------------------------------
            // Validate Staff
            // ---------------------------------

            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.Id == item.StaffId);

            if (staff == null)
            {
                ModelState.AddModelError(
                    nameof(item.StaffId),
                    "Selected staff member was not found.");
            }

            // ---------------------------------
            // Determine subscription status
            // ---------------------------------

            string calculatedStatus = "Rejected-No Active Subscription";

            if (member != null)
            {
                var today = DateTime.Today;

                var activeSubscription = await _context.Subscriptions
                    .Include(s => s.MembershipPlan)
                    .Where(s => s.MemberId == member.Id)
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

                if (activeSubscription == null)
                {
                    calculatedStatus = "Rejected-No Active Subscription";
                }
                else if (activeSubscription.StartDate.Date > today)
                {
                    calculatedStatus = "Rejected-Pending";
                }
                else if (activeSubscription.EndDate.Date < today)
                {
                    calculatedStatus = "Rejected-Expired";
                }
                else
                {
                    calculatedStatus = "Allowed";
                }
            }

            // ---------------------------------
            // Status is NEVER entered by user
            // ---------------------------------

            item.Status = calculatedStatus;

            if (item.CheckInTime == default)
            {
                item.CheckInTime = DateTime.Now;
            }

            // ---------------------------------
            // Save
            // ---------------------------------

            if (ModelState.IsValid)
            {
                _checkInRepo.Add(item);
                _checkInRepo.Save();

                if (item.Status == "Allowed")
                {
                    TempData["Success"] =
                        "Check-in allowed successfully.";
                }
                else
                {
                    TempData["Error"] =
                        $"Check-in rejected: {GetReadableStatus(item.Status)}";
                }

                return RedirectToAction(nameof(Index));
            }

            LoadMembers(item.MemberId);
            LoadStaff(item.StaffId);

            return View(item);
        }

        // =========================
        // DELETE - GET
        // =========================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.CheckIns
                .Include(c => c.Member)
                .Include(c => c.Staff)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // =========================
        // DELETE - POST
        // =========================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _checkInRepo.GetById(id);

            if (item == null)
            {
                return NotFound();
            }

            _checkInRepo.Delete(item);
            _checkInRepo.Save();

            TempData["Success"] =
                "Check-in record has been deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // Helpers
        // =========================

        private void LoadMembers(int? selectedMemberId = null)
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
        }

        private void LoadStaff(int? selectedStaffId = null)
        {
            var staff = _context.Staff
                .Include(s => s.User)
                .OrderBy(s => s.User!.FullName)
                .ToList();

            var staffItems = staff.Select(s => new
            {
                Id = s.Id,
                FullName = s.User != null
                    ? s.User.FullName
                    : $"Staff #{s.Id}"
            });

            ViewBag.Staff = new SelectList(
                staffItems,
                "Id",
                "FullName",
                selectedStaffId);
        }

        private static string GetReadableStatus(string status)
        {
            return status switch
            {
                "Rejected-Expired" =>
                    "Membership subscription has expired.",

                "Rejected-Pending" =>
                    "Membership subscription has not started yet.",

                "Rejected-No Active Subscription" =>
                    "Member does not have an active subscription.",

                _ => "Check-in was rejected."
            };
        }
    }
}
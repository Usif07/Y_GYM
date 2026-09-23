using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly IMemberRepository _memberRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(
            IMemberRepository memberRepo,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _memberRepo = memberRepo;
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // GET: /Member
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
            {
                return RedirectToAction(nameof(CompleteProfile));
            }

            var activeSubscription =
                _memberRepo.GetActiveSubscription(member.Id);

            var attendanceThisMonth =
                _memberRepo.GetAttendanceCountThisMonth(member.Id);

            var nextBooking = await _context.Bookings
                .Include(b => b.ClassSchedule)
                    .ThenInclude(cs => cs.Class)
                .Include(b => b.ClassSchedule)
                    .ThenInclude(cs => cs.Coach)
                        .ThenInclude(c => c.User)
                .Where(b =>
                    b.MemberId == member.Id &&
                    b.ClassSchedule.StartTime > DateTime.Now &&
                    b.Status == "Confirmed")
                .OrderBy(b => b.ClassSchedule.StartTime)
                .FirstOrDefaultAsync();

            ViewBag.ActiveSubscription = activeSubscription;
            ViewBag.AttendanceThisMonth = attendanceThisMonth;
            ViewBag.NextBooking = nextBooking;

            return View(member);
        }

        // =========================================================
        // COMPLETE PROFILE
        // =========================================================

        [HttpGet]
        public IActionResult CompleteProfile()
        {
            return View();
        }

        // =========================================================
        // CLASS SCHEDULE
        // =========================================================

        public async Task<IActionResult> Schedule()
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
            {
                return RedirectToAction(nameof(CompleteProfile));
            }

            var schedules = await _context.ClassSchedules
                .Include(cs => cs.Class)
                .Include(cs => cs.Coach)
                    .ThenInclude(c => c.User)
                .Where(cs => cs.StartTime > DateTime.Now)
                .OrderBy(cs => cs.StartTime)
                .ToListAsync();

            return View(schedules);
        }

        // =========================================================
        // BOOK CLASS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookClass(int scheduleId)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
            {
                return RedirectToAction(nameof(CompleteProfile));
            }

            var strategy =
                _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                try
                {
                    var schedule = await _context.ClassSchedules
                        .FromSqlInterpolated(
                            $"SELECT * FROM ClassSchedules WITH (UPDLOCK, ROWLOCK) WHERE Id = {scheduleId}")
                        .FirstOrDefaultAsync();

                    if (schedule == null)
                    {
                        TempData["Error"] =
                            "The selected class is not available.";

                        await transaction.RollbackAsync();
                        return;
                    }

                    var alreadyBooked = await _context.Bookings
                        .AnyAsync(b =>
                            b.ScheduleId == scheduleId &&
                            b.MemberId == member.Id &&
                            b.Status == "Confirmed");

                    if (alreadyBooked)
                    {
                        TempData["Error"] =
                            "You already have a booking for this class.";

                        await transaction.RollbackAsync();
                        return;
                    }

                    if (schedule.StartTime <= DateTime.Now)
                    {
                        TempData["Error"] =
                            "You cannot book a class that has already started.";

                        await transaction.RollbackAsync();
                        return;
                    }

                    if (schedule.AvailablePlaces <= 0)
                    {
                        TempData["Error"] =
                            "Sorry, this class is full.";

                        await transaction.RollbackAsync();
                        return;
                    }

                    schedule.AvailablePlaces--;

                    var booking = new Booking
                    {
                        ScheduleId = scheduleId,
                        MemberId = member.Id,
                        BookingDate = DateTime.Now,
                        Status = "Confirmed",
                        IsAttended = false
                    };

                    _context.Bookings.Add(booking);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] =
                        "Your class booking was successful.";
                }
                catch
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "An error occurred while booking the class.";
                }
            });

            return RedirectToAction(nameof(Schedule));
        }

        // =========================================================
        // CANCEL BOOKING
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
            {
                return RedirectToAction(nameof(CompleteProfile));
            }

            var booking = await _context.Bookings
                .Include(b => b.ClassSchedule)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.MemberId == member.Id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != "Confirmed")
            {
                TempData["Error"] =
                    "This booking cannot be cancelled.";

                return RedirectToAction(nameof(MyBookings));
            }

            if (booking.ClassSchedule.StartTime
                <= DateTime.Now.AddHours(2))
            {
                TempData["Error"] =
                    "Cancellations cannot be made less than two hours before the class starts.";

                return RedirectToAction(nameof(MyBookings));
            }

            booking.Status = "Cancelled";

            booking.ClassSchedule.AvailablePlaces++;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your booking has been cancelled successfully.";

            return RedirectToAction(nameof(MyBookings));
        }

        // =========================================================
        // MY BOOKINGS
        // =========================================================

        public async Task<IActionResult> MyBookings()
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
            {
                return RedirectToAction(nameof(CompleteProfile));
            }

            var bookings = await _context.Bookings
                .Include(b => b.ClassSchedule)
                    .ThenInclude(cs => cs.Class)
                .Include(b => b.ClassSchedule)
                    .ThenInclude(cs => cs.Coach)
                        .ThenInclude(c => c.User)
                .Where(b => b.MemberId == member.Id)
                .OrderByDescending(b => b.ClassSchedule.StartTime)
                .ToListAsync();

            return View(bookings);
        }

        // =========================================================
        // PROGRESS
        // =========================================================

        public async Task<IActionResult> Progress()
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
            {
                return RedirectToAction(nameof(CompleteProfile));
            }

            var logs = await _context.ProgressLogs
                .Where(p => p.MemberId == member.Id)
                .OrderByDescending(p => p.RecordDate)
                .ToListAsync();

            return View(logs);
        }

        // =========================================================
        // ADD PROGRESS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProgress(double weight)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
            {
                return RedirectToAction(nameof(CompleteProfile));
            }

            if (weight <= 0)
            {
                TempData["Error"] =
                    "Please enter a valid weight.";

                return RedirectToAction(nameof(Progress));
            }

            _context.ProgressLogs.Add(
                new Progress
                {
                    MemberId = member.Id,
                    Weight = weight,
                    RecordDate = DateTime.Now
                });

            await _context.SaveChangesAsync();

            member.Weight = weight;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your weight has been recorded successfully.";

            return RedirectToAction(nameof(Progress));
        }

        // =========================================================
        // CURRENT MEMBER
        // =========================================================

        private async Task<Member?> GetCurrentMemberAsync()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return await _context.Members
                .Include(m => m.User)
                .Include(m => m.Plan)
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }
    }
}
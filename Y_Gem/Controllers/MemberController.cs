using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;//?
using Y_Gem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Y_Gem.Models;
using Y_Gem.Repository;



namespace Y_Gem.Controllers
{
    // [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly IMemberRepository _memberRepo;//?
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

        public IActionResult Index()//?
        {
            var member =  _context.Members.Include(m => m.User).FirstOrDefault();// var userId = _userManager.GetUserId(User);
            // var member =  _memberRepo.GetByUserId(userId);
            if (member == null) return NotFound();

            ViewBag.ActiveSubscription =  _memberRepo.GetActiveSubscription(member.Id);
            ViewBag.AttendanceThisMonth =  _memberRepo.GetAttendanceCountThisMonth(member.Id);
            ViewBag.NextBooking =  _context.Bookings
                .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Classe)
                .Where(b => b.MemberId == member.Id && b.ClassSchedule.StartTime > DateTime.Now && b.Status == "Confirmed")
                .OrderBy(b => b.ClassSchedule.StartTime)
                .FirstOrDefault();

            return View(member);
        }

        public IActionResult Schedule()
        {
            var schedules =  _context.ClassSchedules
                .Include(cs => cs.Classe)
                .Include(cs => cs.Coach)
                .Where(cs => cs.StartTime > DateTime.Now)
                .OrderBy(cs => cs.StartTime)
                .ToList();

            return View(schedules);
        }

        // ---------- الحجز: هنا لازم Transaction + Row Lock عشان منع الـ Race Condition ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookClass(int scheduleId)
        {
            var member = ( _memberRepo.GetAll()).FirstOrDefault();
            // var userId = _userManager.GetUserId(User);
            // var member =  _memberRepo.GetByUserId(userId);

            var strategy = _context.Database.CreateExecutionStrategy();//?

             strategy.Execute( () =>
            {
                using var transaction =  _context.Database.BeginTransaction(//?
                    System.Data.IsolationLevel.Serializable);//?

                try
                {
                    // قفل الصف عشان محدش يقرأ نفس العدد في نفس اللحظة
                    var schedule =  _context.ClassSchedules
                        .FromSqlInterpolated($"SELECT * FROM ClassSchedules WITH (UPDLOCK, ROWLOCK) WHERE Id = {scheduleId}")
                        .FirstOrDefault();

                    if (schedule == null)
                    {
                        TempData["Error"] = "The class is not available";
                         transaction.Rollback();//?
                        return;
                    }

                    var alreadyBooked =  _context.Bookings
                        .Any(b => b.ScheduleId == scheduleId && b.MemberId == member.Id && b.Status == "Confirmed");

                    if (alreadyBooked)
                    {
                        TempData["Error"] = "You already have a booking for this class";
                         transaction.Rollback();
                        return;
                    }

                    if (schedule.AvailablePlaces <= 0)
                    {
                        TempData["Error"] = "Sorry, the class is full.";
                         transaction.Rollback();
                        return;
                    }

                    schedule.AvailablePlaces -= 1;

                    var booking = new Booking
                    {
                        ScheduleId = scheduleId,
                        MemberId = member.Id,
                        BookingDate = DateTime.Now,
                        Status = "Confirmed",
                        IsAttended = false
                    };
                    _context.Bookings.Add(booking);

                     _context.SaveChanges();
                     transaction.Commit();

                    TempData["Success"] = "The booking was successful";
                }
                catch
                {
                     transaction.Rollback();
                    TempData["Error"] = "An error occurred during the booking process";
                }
            });

            return RedirectToAction(nameof(Schedule));
        }

        // ---------- إلغاء الحجز: يرجّع المكان ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelBooking(int bookingId)
        {
            var member = ( _memberRepo.GetAll()).FirstOrDefault();
            // var userId = _userManager.GetUserId(User);
            // var member =  _memberRepo.GetByUserId(userId);

            var booking =  _context.Bookings
                .Include(b => b.ClassSchedule)
                .FirstOrDefault(b => b.Id == bookingId && b.MemberId == member.Id);

            if (booking == null) return NotFound();

            if (booking.ClassSchedule.StartTime <= DateTime.Now.AddHours(2))
            {
                TempData["Error"] = "Cancellations cannot be made less than two hours before the class starts.";
                return RedirectToAction(nameof(MyBookings));
            }

            booking.Status = "Cancelled";
            booking.ClassSchedule.AvailablePlaces += 1;

             _context.SaveChanges();
            TempData["Success"] = "The booking has been canceled";
            return RedirectToAction(nameof(MyBookings));
        }

        public IActionResult MyBookings()
        {
            var member = ( _memberRepo.GetAll()).FirstOrDefault();
            // var userId = _userManager.GetUserId(User);
            // var member =  _memberRepo.GetByUserId(userId);

            var bookings =  _context.Bookings
                .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Classe)
                .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Coach)
                .Where(b => b.MemberId == member.Id)
                .OrderByDescending(b => b.ClassSchedule.StartTime)
                .ToList();

            return View(bookings);
        }

        public IActionResult Progress()
        {
            var member = ( _memberRepo.GetAll()).FirstOrDefault();
            // var userId = _userManager.GetUserId(User);
            // var member =  _memberRepo.GetByUserId(userId);

            var logs =  _context.ProgressLogs
                .Where(p => p.MemberId == member.Id)
                .OrderBy(p => p.RecordDate)
                .ToList();

            return View(logs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProgress(double weight)
        {
            var member = ( _memberRepo.GetAll()).FirstOrDefault();
            // var userId = _userManager.GetUserId(User);
            // var member =  _memberRepo.GetByUserId(userId);

            _context.ProgressLogs.Add(new Progress
            {
                MemberId = member.Id,
                Weight = weight,
                RecordDate = DateTime.Now
            });

             _context.SaveChanges();
            TempData["Success"] = "The weight was recorded";//?
            return RedirectToAction(nameof(Progress));
        }
    }
}
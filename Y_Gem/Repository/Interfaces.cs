using System.Collections.Generic;
using Y_GYM.Models;

namespace Y_GYM.Repository
{
    public interface ICheckInRepository : IGenericRepository<CheckIn> { IEnumerable<CheckIn> GetAllWithDetails(); }
    public interface IPaymentRepository : IGenericRepository<Payment> { IEnumerable<Payment> GetAllWithDetails(); }
    public interface IProgressRepository : IGenericRepository<Progress> { IEnumerable<Progress> GetAllWithDetails(); }
    public interface IClassScheduleRepository : IGenericRepository<ClassSchedule> { IEnumerable<ClassSchedule> GetAllWithDetails(); }
    public interface IBookingRepository : IGenericRepository<Booking> { IEnumerable<Booking> GetAllWithDetails(); }
    public interface IDietPlanRepository : IGenericRepository<DietPlan> { IEnumerable<DietPlan> GetAllWithDetails(); }
    public interface ISubscriptionRepository : IGenericRepository<Subscription> { IEnumerable<Subscription> GetAllWithDetails(); }
    public interface IAdminRepository : IGenericRepository<Admin> { }
    public interface IApplicationUserRepository : IGenericRepository<ApplicationUser> { }
    public interface ICoachRepository : IGenericRepository<Coach> { }


    // Class جدول بسيط مفيهوش Foreign Keys
    public interface IClassRepository : IGenericRepository<Class> { }
}
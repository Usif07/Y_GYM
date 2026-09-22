// Repository/StaffRepository.cs
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;

namespace Y_GYM.Repository
{
    public class StaffRepository : GenericRepository<Staff>, IStaffRepository
    {
        public StaffRepository(ApplicationDbContext context) : base(context) { }

        public  Staff GetByUserId(string userId)
            =>  _dbSet.Include(s => s.User)
                            .FirstOrDefault(s => s.UserId == userId);
    }
}
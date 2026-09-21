// Repository/StaffRepository.cs
using Microsoft.EntityFrameworkCore;
using Y_Gem.Data;
using Y_Gem.Models;

namespace Y_Gem.Repository
{
    public class StaffRepository : GenericRepository<Staff>, IStaffRepository
    {
        public StaffRepository(ApplicationDbContext context) : base(context) { }

        public  Staff GetByUserId(string userId)
            =>  _dbSet.Include(s => s.User)
                            .FirstOrDefault(s => s.UserId == userId);
    }
}
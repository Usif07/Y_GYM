// Repository/IStaffRepository.cs
using Y_GYM.Models;

namespace Y_GYM.Repository
{
    public interface IStaffRepository : IGenericRepository<Staff>
    {
        Staff GetByUserId(string userId);
    }
}
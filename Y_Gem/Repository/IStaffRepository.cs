// Repository/IStaffRepository.cs
using Y_Gem.Models;

namespace Y_Gem.Repository
{
    public interface IStaffRepository : IGenericRepository<Staff>
    {
        Staff GetByUserId(string userId);
    }
}
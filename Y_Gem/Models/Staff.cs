// namespace Y_Gem.Models
// {
//     public class Staff : User
//     {
//         public string JobTitle  { get; set; }
//         public DateTime ShiftTime  { get; set; }
//     }
// }


// Models/Staff.cs
namespace Y_Gem.Models
{
    public class Staff
    {
        public int Id { get; set; }

        public string UserId { get; set; }         // FK -> ApplicationUser.Id
        public ApplicationUser User { get; set; }

        public string JobTitle { get; set; }
        public string ShiftTime { get; set; }       

        public ICollection<CheckIn> CheckIns { get; set; }     
        public ICollection<Payment> PaymentsHandled { get; set; }
    }
}

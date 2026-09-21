// namespace Y_Gem.Models
// {
//     public class CheckIn
//     {
//         public int Id { get; set; }
//         public int StaffId { get; set; }
//         public Staff Staff { get; set; }
//         public int MemberId { get; set; }
//         public Member Member { get; set; }
//         public string Status { get; set; }
//         public DateTime CheckInTime { get; set; }
//     }
// }


// Models/CheckIn.cs
namespace Y_Gem.Models
{
    public class CheckIn
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public int StaffId { get; set; }
        public Staff Staff { get; set; }
        public DateTime CheckInTime { get; set; }
        public string Status { get; set; }   // Allowed / Rejected-Expired
    }
}

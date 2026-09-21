// namespace Y_Gem.Models
// {
//     public class Member :User
//     {
//         public double Weight { get; set; }
//         public double Hight { get; set; }



//     }
// }


// Models/Member.cs
namespace Y_Gem.Models
{
    public class Member
    {
        public int Id { get; set; }

        public string UserId { get; set; }        // FK -> ApplicationUser.Id
        public ApplicationUser User { get; set; }

        public double Weight { get; set; }
        public double Height { get; set; }
        public string Goal { get; set; }           // lose weight / build muscle / stay fit
        public string FitnessLevel { get; set; }
        public string QRCode { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; }//?
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Progress> ProgressLogs { get; set; }
        public ICollection<CheckIn> CheckIns { get; set; }
    }
}
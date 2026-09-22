namespace Y_GYM.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public ClassSchedule ClassSchedule { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public bool IsAttended { get; set; }
    }
}

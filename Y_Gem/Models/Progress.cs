namespace Y_GYM.Models
{
    public class Progress
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public double Weight { get; set; }
        public DateTime RecordDate { get; set; }

    }
}

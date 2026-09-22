namespace Y_GYM.Models
{
    public class ClassSchedule
    {
        public int Id { get; set; }
       
       
        // ده الـ Foreign Key (لو موجود عندك متكتبوش تاني)
        public int ClassId { get; set; }

        // ده السطر اللي ناقص وبيعمل الإيرور
        public Class? Class { get; set; }
        public int CoachId { get; set; }
        public Coach Coach { get; set; }
        public int AvailablePlaces { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}

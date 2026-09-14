namespace Y_Gem.Models
{
    public class ClassSchedule
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public Classe Class { get; set; }
        public int CoachId { get; set; }
        public Coache Coach { get; set; }
        public int AvailablePlaces { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}

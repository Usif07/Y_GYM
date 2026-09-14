namespace Y_Gem.Models
{
    public class Staff : User
    {
        public string JobTitle  { get; set; }
        public DateTime ShiftTime  { get; set; }
    }
}

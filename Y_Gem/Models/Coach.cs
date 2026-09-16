namespace Y_Gem.Models
{
    
    public class Coach : User
    {
        public string Specialty { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }

        
        public ICollection<DietPlan> DietPlans { get; set; } = new List<DietPlan>();

    
        public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
    }
}

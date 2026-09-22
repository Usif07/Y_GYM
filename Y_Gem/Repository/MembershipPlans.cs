using Y_GYM.Data;
using Y_GYM.Models;

namespace Y_GYM.Repository{
    public class MembershipPlans : IMembershipPlans
    {
        private readonly ApplicationDbContext context;
        public MembershipPlans(ApplicationDbContext context) {

            this.context = context;

        }
        public void delete(int id)
        {
            MembershipPlan mp = GetById(id);
            context.MembershipPlans.Remove(mp); 
        }
        public MembershipPlan GetById(int id)
        {
            return context.MembershipPlans.FirstOrDefault(p => p.Id == id); 
        }

        public List<MembershipPlan> GetAll()
        {
            return context.MembershipPlans.ToList();
        }

        public void insert(MembershipPlan obj)
        {
           context.MembershipPlans.Add(obj);
        }

        public void save()
        {
            context.SaveChanges();
        }

        public void update(MembershipPlan obj)
        {
            context.Update(obj);
        }
    }
}

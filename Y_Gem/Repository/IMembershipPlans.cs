using Y_GYM.Models;

namespace Y_GYM.Repository
{
    public interface IMembershipPlans
    {
        MembershipPlan GetById(int id);
        List<MembershipPlan> GetAll();
        void insert(MembershipPlan obj);
        void update(MembershipPlan obj);
        void delete(int id);
        void save();
    }
}
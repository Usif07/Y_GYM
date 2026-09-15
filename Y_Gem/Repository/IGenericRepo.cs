namespace Y_GYM.Repository
{
    public interface IGenericRepo<T> 
    {
        public List<T> GetAll();
        public T GetById(int id);
        void insert(T obj);
        void update(T obj);
        void delete(int id);
        void save();
    }
}

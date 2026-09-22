// namespace Y_GYM.Repository
// {
//     public interface IGenericRepo<T> 
//     {
//         public List<T> GetAll();
//         public T GetById(int id);
//         void insert(T obj);
//         void update(T obj);
//         void delete(int id);
//         void save();
//     }
// }


// Repository/IGenericRepository.cs
using System.Linq.Expressions;

namespace Y_GYM.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        T GetById(int id);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        bool Save();
    }
}
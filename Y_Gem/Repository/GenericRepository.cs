// Repository/GenericRepository.cs
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Y_GYM.Data;

namespace Y_GYM.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class   //?
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public  T GetById(int id) =>  _dbSet.Find(id);

        public  IEnumerable<T> GetAll() =>  _dbSet.ToList();

        public  IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
            =>  _dbSet.Where(predicate).ToList();

        public  void Add(T entity) =>  _dbSet.Add(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public  bool Save() =>  _context.SaveChanges() > 0;
    }
}
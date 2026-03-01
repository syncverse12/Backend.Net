using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SyncVerse.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly DatabaseDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DatabaseDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Infrastructure.Persistence.Repositories
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
            var found = await _dbSet.FindAsync(id);
            return found?.Entity ?? (T?)found;
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
    }
}
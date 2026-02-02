using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Common;
using Graduation_Project.Infrastructure.Data;
using Graduation_Project.Infrastructure.Persistence.Repositories;

namespace Graduation_Project.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(DatabaseDbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repoType = typeof(GenericRepository<>).MakeGenericType(type);
                var repoInstance = Activator.CreateInstance(repoType, _context)!;
                _repositories[type] = repoInstance;
            }

            return (IRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}

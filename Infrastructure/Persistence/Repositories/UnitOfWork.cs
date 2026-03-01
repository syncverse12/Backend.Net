using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Common;
using SyncVerse.Infrastructure.Data;
using SyncVerse.Infrastructure.Persistence.Repositories;

namespace SyncVerse.Infrastructure.Persistence
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

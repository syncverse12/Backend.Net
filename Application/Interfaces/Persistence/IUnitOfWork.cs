using SyncVerse.Domain.Common;

namespace SyncVerse.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
    }
}

using Graduation_Project.Domain.Common;

namespace Graduation_Project.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
    }
}

using System.Linq.Expressions;
using Graduation_Project.Domain.Common;

namespace Graduation_Project.Application.Interfaces.Persistence
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        IQueryable<T> Query();
    }
}

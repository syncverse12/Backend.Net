using System.Linq;
using System.Threading.Tasks;

namespace Graduation_Project.Application.Interfaces.Persistence
{
    public interface IRepository<T> where T : class
    {
        System.Threading.Tasks.Task AddAsync(T entity); 
        Task<T?> GetByIdAsync(object id);
        IQueryable<T> Query();
        void Update(T entity);
        void Delete(T entity);
    }
}

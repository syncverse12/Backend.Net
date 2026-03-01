using SyncVerse.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace SyncVerse.Application.Common.Extensions
{
    public static class IQueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            var total = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, total, pageNumber, pageSize);
        }
    }
}

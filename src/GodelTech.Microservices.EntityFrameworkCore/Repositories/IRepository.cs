using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GodelTech.Microservices.EntityFrameworkCore.Specifications;

namespace GodelTech.Microservices.EntityFrameworkCore.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        ValueTask<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity> GetAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default(CancellationToken));
        Task<TResult> GetAsync<TResult>(ISpecification<TEntity> spec, Expression<Func<TEntity, TResult>> projection, CancellationToken cancellationToken = default(CancellationToken));
        Task<TEntity[]> ListAsync(ISpecification<TEntity> spec, int? skip = null, int? take = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TResult[]> ListAsync<TResult>(ISpecification<TEntity> spec, Expression<Func<TEntity, TResult>> projection, int? skip = null, int? take = null, CancellationToken cancellationToken = default(CancellationToken));
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> ExistsAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default(CancellationToken));
    }
}
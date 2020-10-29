using GodelTech.Microservices.Core;
using GodelTech.Microservices.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GodelTech.Microservices.EntityFrameworkCore
{
    public class RepositoryInitializer<TDbContext, TEntity> : MicroserviceInitializerBase
        where TDbContext : DbContext
        where TEntity : class
    {
        public RepositoryInitializer(IConfiguration configuration)
            : base(configuration)
        {

        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IRepository<TEntity>, Repository<TDbContext, TEntity>>();
        }
    }
}

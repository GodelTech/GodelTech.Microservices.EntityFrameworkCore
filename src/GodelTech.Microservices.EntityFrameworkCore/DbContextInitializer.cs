using GodelTech.Microservices.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GodelTech.Microservices.EntityFrameworkCore
{
    public class DbContextInitializer<TDbContext> : MicroserviceInitializerBase
        where TDbContext : DbContext
    {
        private readonly string _connectionString;
        private readonly bool _applyMigrations;
        private readonly bool _enableHealthChecks;

        public DbContextInitializer(
            IConfiguration configuration,
            string connectionString,
            bool applyMigrations = false,
            bool enableHealthChecks = true)
            : base(configuration)
        {
            _connectionString = connectionString;
            _applyMigrations = applyMigrations;
            _enableHealthChecks = enableHealthChecks;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (_enableHealthChecks)
            {
                services
                    .AddHealthChecks()
                    .AddSqlServer(_connectionString);
            }

            services.AddDbContext<TDbContext>(
                (provider, options) =>
                {
                    options.UseSqlServer(
                        _connectionString,
                        sqlOptions => sqlOptions.EnableRetryOnFailure()
                    );
                    options.EnableSensitiveDataLogging(provider.GetService<IHostEnvironment>().IsDevelopment());
                }
            );
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (_applyMigrations)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetService<TDbContext>();
                    context.Database.Migrate();
                }
            }
        }
    }
}

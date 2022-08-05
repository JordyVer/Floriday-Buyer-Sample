using Axerrio.BB.AspNetCore.EntityFrameworkCore.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Floriday_Buyer_Sample.Infrastructure.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleDbContext<TDbContext>(this IServiceCollection services, string connectionString) where TDbContext : DbContext
        {
            services.AddDbContext<TDbContext>(options =>
            {
                DbContextOptionsActionBuilderFactory.Create<TDbContext>(
                        connectionString,
                        sqlServerRetryOnFailureOptions: new SqlServerRetryOnFailureOptions { MaxRetryCount = 10, MaxRetryDelay = TimeSpan.FromSeconds(30), ErrorNumbersToAdd = null },
                        migrationsAssemblyName: typeof(TDbContext).GetTypeInfo().Assembly.GetName().Name
                    ).Invoke(options);
            });
            return services;
        }
    }
}
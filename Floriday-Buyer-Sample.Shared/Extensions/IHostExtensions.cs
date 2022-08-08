using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Floriday_Buyer_Sample.Shared.Extensions
{
    public static class IHostExtensions
    {
        public static IHost MigrateDbContext<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            return host.ExecuteAction(delegate (IServiceProvider services)
            {
                ILogger<TContext> logger = services.GetRequiredService<ILogger<TContext>>();
                TContext context = services.GetRequiredService<TContext>();
                try
                {
                    logger.LogInformation("Migrating database associated with context " + typeof(TContext).Name);
                    context.Database.Migrate();
                    seeder(context, services);
                    logger.LogInformation("Migrated database associated with context " + typeof(TContext).Name);
                }
                catch (Exception exception)
                {
                    logger.LogCritical(exception, "An error occurred while migrating the database used on context " + typeof(TContext).Name);
                    throw;
                }
            });
        }

        public static IHost ExecuteAction(this IHost host, Action<IServiceProvider> action)
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                action(serviceProvider);
            }

            return host;
        }
    }
}
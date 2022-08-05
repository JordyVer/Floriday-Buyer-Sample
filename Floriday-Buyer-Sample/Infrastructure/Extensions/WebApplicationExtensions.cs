using Microsoft.EntityFrameworkCore;

namespace Floriday_Buyer_Sample.Infrastructure.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication MigrateDbContext<TContext>(this WebApplication app, Action<TContext, IServiceProvider> seeder = null) where TContext : DbContext
        {
            return app.ExecuteAction(delegate (IServiceProvider services)
            {
                ILogger<TContext> logger = services.GetRequiredService<ILogger<TContext>>();
                TContext context = services.GetRequiredService<TContext>();
                try
                {
                    logger.LogInformation("Migrating database associated with context " + typeof(TContext).Name);
                    context.Database.Migrate();
                    if (seeder != null) seeder(context, services);
                    logger.LogInformation("Migrated database associated with context " + typeof(TContext).Name);
                }
                catch (Exception exception)
                {
                    logger.LogCritical(exception, "An error occurred while migrating the database used on context " + typeof(TContext).Name);
                    throw;
                }
            });
        }

        public static WebApplication ExecuteAction(this WebApplication app, Action<IServiceProvider> action)
        {
            using (var serviceScope = app.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                action(serviceProvider);
            }

            return app;
        }
    }
}
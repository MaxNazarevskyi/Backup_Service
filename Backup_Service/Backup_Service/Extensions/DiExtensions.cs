using Backup_Service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backup_Service.Extensions
{
    public static class DiExtensions
    {
        public static void AddSQL(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                //options.UseSqlServer(
                //        configuration.GetConnectionString("SqlDatabase"),
                //        b => b.MigrationsAssembly(typeof(MVCDbContext).Assembly.FullName))
                //    .UseLazyLoadingProxies();

                options.UseMySql(configuration.GetConnectionString("SqlDatabase"), Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.27-mysql"));
            }
            );
        }
    }
}

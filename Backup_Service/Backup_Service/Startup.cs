using Backup_Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Backup_Service.Data.Repository;
using Backup_Service.Data;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Backup_Service.Data.DataModels;
using Microsoft.AspNetCore.Identity;

namespace Backup_Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<AppDbContext>();
            services.AddScoped(typeof(IRepository<>), typeof(EFRepository<>));

            services.AddDbContextPool<AppDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("MySqlDatabase"),
                                 ServerVersion.AutoDetect(Configuration.GetConnectionString("MySqlDatabase"))));
            services.AddScoped<IArchiveService, ArchiveService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Authentication}/{action=Login}/{id?}");
            });
            dbContext.Database.EnsureCreated();
        }
    }
}

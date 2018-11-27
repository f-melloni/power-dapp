using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PowerDapp_UserArea.Database;
using PowerDapp_UserArea.Database.Entity;
using PowerDapp_UserArea.Models;
using PowerDapp_UserArea.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PowerDapp_UserArea
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IHostingEnvironment CurrentEnv { get; set; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            CurrentEnv = env;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = CurrentEnv.IsDevelopment() 
                ? Configuration.GetConnectionString("DevConnection") 
                : Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DBEntities>(options => options.UseMySql(connectionString, b => b.MigrationsAssembly("PowerDapp_UserArea")));

            services.AddIdentity<User, Role>(config => {
                config.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<DBEntities>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Dashboard}/{id?}");
            });
        }
    }
}

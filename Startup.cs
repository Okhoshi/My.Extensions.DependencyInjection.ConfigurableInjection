using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using My.Extensions.DependencyInjection.ConfigurableInjection.Implementations;

namespace My.Extensions.DependencyInjection.ConfigurableInjection
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
            services.AddConfigurableInjections(o => {
                o.UseConfigurationProvider<string, MemoryServiceConfigurationProvider.MemoryConfig, MemoryServiceConfigurationProvider>();
                o.AddAssemblyFromType<Startup>();
            });

            services.AddMvc();
            services.Configure<RazorViewEngineOptions>(o => {
                foreach (var item in o.ViewLocationFormats.Select(x => "/App" + x).ToList())
                {
                    o.ViewLocationFormats.Add(item);
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

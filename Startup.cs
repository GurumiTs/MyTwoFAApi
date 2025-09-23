using MyApi.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApi
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
            services.AddCors(options =>
            {
                options.AddPolicy("TwoFA-Cors", builder =>
                {
                    builder
                        .WithOrigins(
                            "yourdomain.com",
                            "yourdomain1.com"

                        )
                        //.SetIsOriginAllowed(origin =>
                        //{
                        //    // 只放行 HTTPS 且網域以 .example.com 結尾
                        //    if (!Uri.TryCreate(origin, UriKind.Absolute, out var u)) return false;
                        //    return u.Scheme == Uri.UriSchemeHttps &&
                        //           u.Host.EndsWith(".yourdomain.com", StringComparison.OrdinalIgnoreCase);
                        //})
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromHours(1));
                });
            });

            services.AddControllers();
            services.AddScoped<ITwoFARepository, TwoFARepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

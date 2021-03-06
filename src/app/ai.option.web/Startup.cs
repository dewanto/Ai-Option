﻿using System;
using System.IO;
using ai.option.web.Configurations;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EventFlow.Autofac.Extensions;
using EventFlow.DependencyInjection.Extensions;
using iqoption.apiservice.DependencyModule;
using iqoption.core.Extensions;
using iqoption.data;
using iqoption.data.DependencyModule;
using iqoption.data.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ai.option.web {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services) {
            var builder = new ContainerBuilder();

            //appsetting complier
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();

            services
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<AiOptionContext>(options =>
                    options
                        .UseLazyLoadingProxies()
                        .UseSqlServer(Configuration.GetConnectionString("aioptiondb")))
                .AddIqOptionIdentity()
                .AddAuthentication()
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            services.Configure<CookiePolicyOptions>(options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //logging
            var loggerFactory = new LoggerFactory()
                .AddDebug()
                .AddConsole()
                .AddAzureWebAppDiagnostics();

            services
                .AddSingleton(loggerFactory)
                .AddSingleton(loggerFactory.CreateLogger(nameof(Startup)))
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddAutoMapper()
                .AddEventFlow(o => {
                    o.UseAutofacContainerBuilder(builder);
                    o.AddEventFlowForData();
                })
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            builder.RegisterModule<DataAutofacModule>();


            builder.Populate(services);
            var container = builder.Build();


            var serviceProvider = container.Resolve<IServiceProvider>();
            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IDbSeeding seed) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            seed.SeedAsync().Wait();

            app
                .UseCors("CorsPolicy")
                .UseStaticFiles(new StaticFileOptions())
                .UseHttpsRedirection()
                .UseAuthentication()
                .UseStaticFiles()
                .UseCookiePolicy()
                .UseMvc(routes => {
                    routes.MapRoute(
                        "default",
                        "{controller=Home}/{action=Index}/{id?}");
                });
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lunar.Auth.Data;
using Lunar.Auth.Models;
using Lunar.Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.HttpOverrides;

namespace Lunar.Auth
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            // Configuration = configuration;

            var builder = new ConfigurationBuilder();

            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional:true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, MessageSender>();
            services.AddTransient<ISmsSender, MessageSender>();

            services.AddMvc();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                    };
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                })
               
                .AddAspNetIdentity<ApplicationUser>();

            
            
            if (!string.IsNullOrWhiteSpace(Configuration["Authentication:Facebook:AppId"]) 
                || !string.IsNullOrWhiteSpace(Configuration["Authentication.Facebook.AppId"]))
            {
                services.AddAuthentication()
                    .AddFacebook(opts =>
                    {
                        opts.AppId = Configuration["Authentication:Facebook:AppId"] ?? Configuration["Authentication.Facebook.AppId"];
                        opts.AppSecret = Configuration["Authentication:Facebook:AppSecret"] ?? Configuration["Authentication.Facebook.AppSecret"];
                    });
            }

            if (!string.IsNullOrWhiteSpace(Configuration["Authentication:Twitter:ConsumerKey"]) 
                || !string.IsNullOrWhiteSpace(Configuration["Authentication.Twitter.ConsumerKey"]))
            {
                services.AddAuthentication()
                    .AddTwitter(opts =>
                    {
                        opts.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"] ?? Configuration["Authentication.Twitter.ConsumerKey"];
                        opts.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"] ?? Configuration["Authentication.Twitter.ConsumerSecret"];
                    });
            }

            if (!string.IsNullOrWhiteSpace(Configuration["Authentication:Microsoft:ApplicationId"]) 
                || !string.IsNullOrWhiteSpace(Configuration["Authentication.Microsoft.ApplicationId"]))
            {
                services.AddAuthentication()
                    .AddMicrosoftAccount(opts =>
                    {
                        opts.ClientId = Configuration["Authentication:Microsoft:ApplicationId"] ?? Configuration["Authentication.Microsoft.ApplicationId"];
                        opts.ClientSecret = Configuration["Authentication:Microsoft:Password"] ?? Configuration["Authentication.Microsoft.Password"];
                    });
            }

            if (!string.IsNullOrWhiteSpace(Configuration["Authentication:AzureActiveDirectory:ApplicationId"])
                || !string.IsNullOrWhiteSpace(Configuration["Authentication.AzureActiveDirectory.ApplicationId"]))
            {

                services.AddAuthentication()
                    .AddAzureAd(opts =>
                    {
                        opts.CallbackPath = Configuration["Authentication:AzureActiveDirectory:CallbackPath"];
                        opts.Instance = Configuration["Authentication:AzureActiveDirectory:Instance"];
                        opts.ClientId = Configuration["Authentication:AzureActiveDirectory:ApplicationId"] ?? Configuration["Authentication.AzureActiveDirectory.ApplicationId"];
                        opts.TenantId = Configuration["Authentication:AzureActiveDirectory:TennantId"] ?? Configuration["Authentication.AzureActiveDirectory.TennantId"];
                        opts.Domain = Configuration["Authentication:AzureActiveDirectory:Domain"] ?? Configuration["Authentication.AzureActiveDirectory.Domain"];
                    });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeDatabase(app);

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
                
            });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            //app.UseAuthentication();
            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}

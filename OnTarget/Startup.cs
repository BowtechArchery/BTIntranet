using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Serialization;
using OnTargetLibrary.Security;

namespace OnTarget
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

         // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IHostingEnvironment env = serviceProvider.GetService<IHostingEnvironment>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services
                .AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administration", policy => policy.Requirements.Add(new IntranetSecurityGroupRequirement("Administration - Intranet")));
                options.AddPolicy("Supply Chain", policy => policy.Requirements.Add(new IntranetSecurityGroupRequirement("Supply Chain - Intranet")));
                options.AddPolicy("Sales", policy => policy.Requirements.Add(new IntranetSecurityGroupRequirement("Sales - Intranet")));
            });
            services.AddSingleton<IAuthorizationHandler, IntranetSecurityGroupHandler>();

            services.AddAuthentication(IISDefaults.AuthenticationScheme);

            // Add Kendo UI services to the services container
            services.AddKendo();

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

                app.Use(async (ctx, next) =>
                {
                    await next();

                    if (ctx.Response.StatusCode == 403 && !ctx.Response.HasStarted)
                    {
                        //Re-execute the request so the user gets the error page
                        string originalPath = ctx.Request.Path.Value;
                        ctx.Items["originalPath"] = originalPath;
                        ctx.Request.Path = "/Error/Forbidden";
                        await next();
                    }
                    else if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
                    {
                        //Re-execute the request so the user gets the error page
                        string originalPath = ctx.Request.Path.Value;
                        ctx.Items["originalPath"] = originalPath;
                        ctx.Request.Path = "/Error/PageNotFound";
                        await next();
                    }
                    else if (ctx.Response.StatusCode == 500 && !ctx.Response.HasStarted)
                    {
                        //Re-execute the request so the user gets the error page
                        string originalPath = ctx.Request.Path.Value;
                        ctx.Items["originalPath"] = originalPath;
                        ctx.Request.Path = "/Error/AppError";
                        await next();
                    }
                    else
                    {
                        app.UseExceptionHandler("/Error/Error");
                        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                        app.UseHsts();

                    }
                });

            }
           
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
         
            app.UseMvc(routes =>
            {
                //routes.MapAreaRoute(
                //    name: "AreaSupplyChain",
                //    areaName: "SupplyChain",
                //    template: "SupplyChain/{controller=SupplyChain}/{action=Index}/{id?}");

            //need route and attribute on controller: [Area("Blogs")]
             routes.MapRoute(name: "mvcAreaRoute",
                                template: "{area:exists}/{controller=Home}/{action=Index}");

            // default route for non-areas
            routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

            });
        }

    }
}

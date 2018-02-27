using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using eShop.Common;
using eShop.Data;
using eShop.Data.Contracts;
using eShop.Data.Models;
using eShop.Data.Seeding;
using eShop.Web.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using eShop.ViewModels;

namespace eShop.Web
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(this.configuration["ConnectionStrings:DefaultConnection"], b=>b.MigrationsAssembly("eShop.Data"));
                options.UseOpenIddict();
            });

            // add identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity options and password complexity here
            services.Configure<IdentityOptions>(options =>
            {
                // User settings
                options.User.RequireUniqueEmail = true;

                //    //// Password settings
                //    //options.Password.RequireDigit = true;
                //    //options.Password.RequiredLength = 8;
                //    //options.Password.RequireNonAlphanumeric = false;
                //    //options.Password.RequireUppercase = true;
                //    //options.Password.RequireLowercase = false;

                //    //// Lockout settings
                //    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                //    //options.Lockout.MaxFailedAccessAttempts = 10;

                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });



            // Register the OpenIddict services.
            services.AddOpenIddict(options =>
            {
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();
                options.AddMvcBinders();
                options.EnableTokenEndpoint("/connect/token");
                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.DisableHttpsRequirement();
             // options.UseJsonWebTokens(); //Use JWT if preferred
                options.AddSigningKey(new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(this.configuration["STSKey"])));
            });


            // Enable cors if required
            //services.AddCors();

            // Add framework services.
            services.AddMvc();

            //Todo: ***Using DataAnnotations for validation until Swashbuckle supports FluentValidation***
            //services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            //.AddJsonOptions(opts =>
            //{
            //    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //});


           // services.AddSwaggerGen(o =>
           // {
           //     o.AddSecurityDefinition("BearerAuth", new Swashbuckle.Swagger.Model.ApiKeyScheme
           //     {
           //         Name = "Authorization",
           //         Description = "Login with your bearer authentication token. e.g. Bearer <auth-token>",
           //         In = "header",
           //         Type = "apiKey"
           //     });
           // });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(GlobalConstants.ViewUserByUserIdPolicy, policy => policy.Requirements.Add(new ViewUserByIdRequirement()));

                options.AddPolicy(GlobalConstants.ViewUsersPolicy, policy => policy.RequireClaim(GlobalConstants.Permission, GlobalConstants.ViewUsers));
                                  
                options.AddPolicy(GlobalConstants.ManageUserByUserIdPolicy, policy => policy.Requirements.Add(new ManageUserByIdRequirement()));
                                  
                options.AddPolicy(GlobalConstants.ManageUsersPolicy, policy => policy.RequireClaim(GlobalConstants.Permission, GlobalConstants.ManageUsers));
                                  
                options.AddPolicy(GlobalConstants.ViewRoleByRoleNamePolicy, policy => policy.Requirements.Add(new ViewRoleByNameRequirement()));
                                  
                options.AddPolicy(GlobalConstants.ViewRolesPolicy, policy => policy.RequireClaim(GlobalConstants.Permission, GlobalConstants.ViewRoles));
                                  
                options.AddPolicy(GlobalConstants.AssignRolesPolicy, policy => policy.Requirements.Add(new AssignRolesRequirement()));
                                  
                options.AddPolicy(GlobalConstants.ManageRolesPolicy, policy => policy.RequireClaim(GlobalConstants.Permission, GlobalConstants.ManageRoles));
            });

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });


            // Repositories
            services.AddScoped<IAccountManager, AccountManager>();

            // Auth Policies
            services.AddSingleton<IAuthorizationHandler, ViewUserByIdHandler>();
            services.AddSingleton<IAuthorizationHandler, ManageUserByIdHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewRoleByNameHandler>();
            services.AddSingleton<IAuthorizationHandler, AssignRolesHandler>();

            // DB Creation and Seeding
            services.AddTransient<IDatabaseInitializerSeed, DatabaseInitializerSeed>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }




            //Configure Cors
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());


            app.UseStaticFiles();
            app.UseAuthentication();


            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = GlobalConstants.ApplicationJson;

                    var error = context.Features.Get<IExceptionHandlerFeature>();

                    if (error != null)
                    {
                        string errorMsg = JsonConvert.SerializeObject(new { error = error.Error.Message });
                        await context.Response.WriteAsync(errorMsg).ConfigureAwait(false);
                    }
                });
            });

            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}

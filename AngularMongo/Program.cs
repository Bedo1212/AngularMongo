// ---------------------------------------------------
 
 
//
 
// ---------------------------------------------------

using DAL;
using DAL.Core;
using DAL.Core.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;
using Quartz;
using QuickMongo.Authorization;
using QuickMongo.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore.Extensions;
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using static OpenIddict.Abstractions.OpenIddictConstants;
using AppPermissions = DAL.Core.ApplicationPermissions;

namespace QuickMongo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            AddServices(builder);// Add services to the container.

            var app = builder.Build();
            ConfigureRequestPipeline(app); // Configure the HTTP request pipeline.

            await SeedDatabase(app); //Seed initial database

            await app.RunAsync();
        }

        private static void AddServices(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
           
            var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDBConnection");
            var DbName = builder.Configuration.GetConnectionString("DbName");

            var client = new MongoClient(mongoConnectionString);
            var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name; //QuickMongo


            /// Mongo

          //  builder.Services.AddHttpContextAccessor();

           // builder.Services.AddSingleton<IMongoClient>(sp =>
         //   {
          //      return new MongoClient(mongoConnectionString);
         //   });

            var mongoClient = new MongoClient(mongoConnectionString);
            var database = mongoClient.GetDatabase(DbName);

            // Register MongoDB database instance
            builder.Services.AddSingleton<IMongoDatabase>(database);

            ApplicationDbContextMongo context = new ApplicationDbContextMongo(database);

            builder.Services.AddSingleton<ApplicationDbContextMongo>(context);

            // Other service configurations

            builder.Services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(DbName);
            });

            /// End Of Mongo

         
            // At the ConfigureServices section in Startup.cs
            builder.Services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole, string>(identity =>
            {
                identity.Password.RequiredLength = 8;
                // other options
            },
                mongo =>
                {
                    mongo.ConnectionString = mongoConnectionString + DbName;
                    // other options
                });

            // Configure Identity options and password complexity here
            builder.Services.Configure<IdentityOptions>(options =>
            {
                // User settings
                options.User.RequireUniqueEmail = true;

                //// Password settings
                //options.Password.RequireDigit = true;
                //options.Password.RequiredLength = 8;
                //options.Password.RequireNonAlphanumeric = false;
                //options.Password.RequireUppercase = true;
                //options.Password.RequireLowercase = false;

                //// Lockout settings
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                //options.Lockout.MaxFailedAccessAttempts = 10;

                // Configure Identity to use the same JWT claims as OpenIddict
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.ClaimsIdentity.EmailClaimType = Claims.Email;
            });

            // Configure OpenIddict periodic pruning of orphaned authorizations/tokens from the database.
            builder.Services.AddQuartz(options =>
            {
               options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });

            // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
            builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            builder.Services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseMongoDb()
                     .UseDatabase(new MongoClient(mongoConnectionString).GetDatabase(DbName)); ;

                    options.UseQuartz();
                })
                .AddServer(options =>
                {
                    options.SetTokenEndpointUris("connect/token");

                    options.AllowPasswordFlow()
                           .AllowRefreshTokenFlow();

                    options.RegisterScopes(
                        Scopes.Profile,
                        Scopes.Email,
                        Scopes.Address,
                        Scopes.Phone,
                        Scopes.Roles);

                    // https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.ViewAllUsersPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ViewUsers));
                options.AddPolicy(Policies.ManageAllUsersPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ManageUsers));

                options.AddPolicy(Policies.ViewAllRolesPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ViewRoles));
                options.AddPolicy(Policies.ViewRoleByRoleNamePolicy, policy => policy.Requirements.Add(new ViewRoleAuthorizationRequirement()));
                options.AddPolicy(Policies.ManageAllRolesPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ManageRoles));

                options.AddPolicy(Policies.AssignAllowedRolesPolicy, policy => policy.Requirements.Add(new AssignRolesAuthorizationRequirement()));
            });

            // Add cors
            builder.Services.AddCors();

            builder.Services.AddControllersWithViews();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = OidcConfiguration.ApiFriendlyName, Version = "v1" });
                c.OperationFilter<AuthorizeCheckOperationFilter>();
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri("/connect/token", UriKind.Relative)
                        }
                    }
                });
            });

            builder.Services.AddAutoMapper(typeof(Program));

            // Configurations
            builder.Services.Configure<AppSettings>(builder.Configuration);

            // Business Services
            builder.Services.AddScoped<IEmailSender, EmailSender>();

            // Repositories
            builder.Services.AddScoped<IUnitOfWork, HttpUnitOfWork>();
            builder.Services.AddScoped<IAccountManager, AccountManager>();

            // Auth Handlers
            builder.Services.AddSingleton<IAuthorizationHandler, ViewUserAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, ManageUserAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, ViewRoleAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, AssignRolesAuthorizationHandler>();

            // DB Creation and Seeding
            builder.Services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

            //File Logger
            builder.Logging.AddFile(builder.Configuration.GetSection("Logging"));

            //Email Templates
            EmailTemplates.Initialize(builder.Environment);
        }

        private static void ConfigureRequestPipeline(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "Swagger UI - QuickMongo";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{OidcConfiguration.ApiFriendlyName} V1");
                c.OAuthClientId(OidcConfiguration.SwaggerClientID);
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");

            app.Map("api/{**slug}", context =>
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            });

            app.MapFallbackToFile("index.html");
        }

        private static async Task SeedDatabase(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                    await databaseInitializer.SeedAsync();

                    await OidcConfiguration.RegisterApplicationsAsync(scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogCritical(LoggingEvents.INIT_DATABASE, ex, LoggingEvents.INIT_DATABASE.Name);

                    throw new Exception(LoggingEvents.INIT_DATABASE.Name, ex);
                }
            }
        }
    }
}
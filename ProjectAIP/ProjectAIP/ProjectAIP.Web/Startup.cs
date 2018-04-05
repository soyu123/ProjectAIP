using System;
using DataAccessLayer.Core.EntityFramework.Repositories;
using DataAccessLayer.Core.EntityFramework.UoW;
using DataAccessLayer.Core.Interfaces.Repositories;
using DataAccessLayer.Core.Interfaces.UoW;
using ProjectAIP.BLL.Entities;
using ProjectAIP.DAL.EF;
using ProjectAIP.Services.Interfaces;
using ProjectAIP.Services.Services;
using ProjectAIP.Web.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ProjectAIP.Web
{
    public class Startup
    {
        private readonly string _connectionString;
        public IHostingEnvironment HostingEnvironment { get; set; }
        public IServiceCollection Services { get; private set; }
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            if (env.IsEnvironment("Development"))
            {
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            _connectionString = Configuration["ConnectionStrings:MsSqlConnection"];
            HostingEnvironment = env;
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Framework Services
            // Add framework services.
            services.AddOptions();
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
            });

            services.AddDbContext<ApplicationDbContext<User, Role, int>>(options =>
            {
                options.UseSqlServer(_connectionString);  // SQL SERVER
                                                          // options.UseMySql(_connectionString); // My SQL
            });

            services.AddIdentity<User, Role>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.User.RequireUniqueEmail = false;

            }).AddEntityFrameworkStores<ApplicationDbContext<User, Role, int>>()
              .AddDefaultTokenProviders();
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            var corsBuilder = new CorsPolicyBuilder();
            corsBuilder.AllowAnyHeader();
            corsBuilder.WithMethods("GET", "POST", "PUT", "DELETE");
            corsBuilder.WithOrigins("*");
            corsBuilder.AllowCredentials();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowCors", corsBuilder.Build());
            });
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.KeyLengthLimit = int.MaxValue;
            });
            services.AddMvc();

            // enable JwT Token authentication for WEB API
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = "Jwt";
            //    options.DefaultChallengeScheme = "Jwt";
            //}).AddJwtBearer("Jwt", options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateAudience = false,
            //        //ValidAudience = "the audience you want to validate",
            //        ValidateIssuer = false,
            //        //ValidIssuer = "the isser you want to validate",
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtIssuerOptions:SecretKey"])),
            //        ValidateLifetime = true, //validate the expiration and not before values in the token
            //        ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
            //    };
            //});
            #endregion

            #region Our Services

            var cs = new ConnectionStringDto() { ConnectionString = _connectionString };
            services.AddSingleton(cs);

            services.AddScoped<DbContext, ApplicationDbContext<User, Role, int>>();
            services.AddScoped<DbContextOptions<ApplicationDbContext<User, Role, int>>>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IEmailSender, EmailSender>();
            var mappingConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.Mapping(services.BuildServiceProvider().GetService<UserManager<User>>());
            });

            services.AddSingleton(x => mappingConfig.CreateMapper());
            services.AddScoped<IRepository<User>, EntityFrameworkRepository<User>>();
            services.AddScoped<IRepository<Role>, EntityFrameworkRepository<Role>>();
            services.AddScoped<IRepository<IdentityUserRole<int>>, EntityFrameworkRepository<IdentityUserRole<int>>>();
            #endregion
            Services = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddConsole();
            loggerFactory.AddFile("Logs/App-{Date}.txt");

            app.UseSession();
            var dbContext = Services.BuildServiceProvider().GetRequiredService<DbContext>();
            dbContext.Database.Migrate();
            var userManager = Services.BuildServiceProvider().GetRequiredService<UserManager<User>>();
            var roleManager = Services.BuildServiceProvider().GetRequiredService<RoleManager<Role>>();
            Seed.RunSeed(dbContext, roleManager, userManager);

            app.UseAuthentication();
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

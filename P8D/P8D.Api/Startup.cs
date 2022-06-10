using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using P8D.Domain.Entities;
using P8D.Domain.Entities.Contexts;
using P8D.Infrastructure.Caching;
using P8D.Infrastructure.Common.Constants;
using P8D.Infrastructure.Mvc.Filters;
using P8D.Infrastructure.Mvc.Utilities;
using P8D.Infrastructure.Services;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P8D.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
              .SetBasePath(env.ContentRootPath)
              .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(l => l.Level == Serilog.Events.LogEventLevel.Information)
                    .WriteTo.File("logs/informations/log-information-{Date}.txt",
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(l => l.Level == Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.File("logs/warnings/log-warnings-{Date}.txt",
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(l => l.Level == Serilog.Events.LogEventLevel.Error)
                    .WriteTo.File("logs/errors/log-errors-{Date}.txt",
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(
                options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    options.Filters.Add(typeof(ValidateModelFilter));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                });
            services.AddSwaggerGenNewtonsoftSupport();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Base .Net 6 Restful API - Mediator Design Parttern",
                    Description = "Base .Net 6 Restful API - Mediator Design Parttern",
                    TermsOfService = null,
                    Contact = new OpenApiContact { Name = "DINH KHAC HOAI PHUNG", Email = "phungdkh@gmail.com", Url = null },
                });

                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            },
                            Scheme = JwtBearerDefaults.AuthenticationScheme,
                            Name = JwtBearerDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
                c.DescribeAllParametersInCamelCase();
            });

            // Entity framework
            string msSqlConnectionString = Configuration.GetValue<string>("database:msSql:connectionString");
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(
                    msSqlConnectionString,
                    options =>
                    {
                        options.EnableRetryOnFailure();
                    }));

            services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("AppSettings")["ThirdPartyRelationshipSecret"])),
                        ValidAudience = Configuration.GetSection("AppSettings")["TokenAudience"],
                        ValidIssuer = Configuration.GetSection("AppSettings")["TokenIssuer"],
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                });

            // Add Policy for each Role
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.BASIC_ACCESS,
                            policy => policy.RequireRole(RoleConstants.SYSTEM_ADMIN, RoleConstants.MEMBER_1,
                                RoleConstants.MEMBER_2));

                options.AddPolicy(Policy.ADMIN_ACCESS,
                            policy => policy.RequireRole(RoleConstants.SYSTEM_ADMIN));

            });

            services.AddCors();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            // Health checks
            services.AddHealthChecks()
                .AddSqlServer(msSqlConnectionString);

            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains(GlobalConstants.ASSEMBLY_NAME)).ToArray();
            services.AddMediatR(currentAssemblies);
            services.AddAutoMapper(currentAssemblies);

            //Register MemoryCacheManager
            services.AddScoped<ICacheManager, MemoryCacheManager>();
            services.AddTransient<IEmailSender, EmailSender>();

            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Base .Net 6 Restful API - Mediator Design Parttern");
                    c.DocumentTitle = "Base .Net 6 Restful API - Document";
                    c.DocExpansion(DocExpansion.None);
                });
            }

            app.UseCors(x => x
               .SetIsOriginAllowed(origin => true)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Auto run migrate
            db.Database.MigrateAsync().Wait();

            // Get the service  
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Init roles
            InitRoles(db, roleManager).Wait();

            // Init user
            InitializeSystemAdminUser(db, userManager, roleManager).Wait();
        }

        private static async Task InitRoles(AppDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            context.Database.EnsureCreated();

            foreach (var role in RoleConstants.GetListRoles())
            {
                if (await roleManager.FindByNameAsync(role.Name) == null)
                {
                    await roleManager.CreateAsync(new ApplicationRole(role.Name));
                }
            }
        }

        private static async Task InitializeSystemAdminUser(AppDbContext context,
                              UserManager<ApplicationUser> userManager,
                              RoleManager<ApplicationRole> roleManager)
        {
            context.Database.EnsureCreated();

            string password = "123456";

            if (await userManager.FindByNameAsync("phungdkh@gmail.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "phungdkh@gmail.com",
                    Email = "phungdkh@gmail.com",
                    PhoneNumber = "+84983260830",
                    Name = "Đinh Khắc Hoài Phụng",
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.SYSTEM_ADMIN);
                }
            }

            if (await userManager.FindByNameAsync("member_001@gmail.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "member_001@gmail.com",
                    Email = "member_001@gmail.com",
                    PhoneNumber = "+84983260830",
                    Name = "Member 001",
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.MEMBER_1);
                }
            }

            if (await userManager.FindByNameAsync("member_002@gmail.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "member_002@gmail.com",
                    Email = "member_002@gmail.com",
                    PhoneNumber = "+84983260830",
                    Name = "Member 002",
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.MEMBER_2);
                }
            }
        }
    }
}

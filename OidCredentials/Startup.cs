using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OidCredentials.Models;
using OidCredentials.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace OidCredentials
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
            services.AddCors();

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]);

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            // Register the Identity services.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseDefaultModels();

                    // Register the Entity Framework stores.
                    options.AddEntityFrameworkCoreStores<ApplicationDbContext>();
                })
                .AddServer(options =>
                {
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    options.AddMvcBinders();

                    // Enable the token endpoint (required to use the password flow).
                    options.EnableTokenEndpoint("/connect/token");

                    // Allow client applications to use the grant_type=password flow.
                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();

                    // During development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();
                });

            // Register the OpenIddict services.
            // Note: use the generic overload if you need
            // to replace the default OpenIddict entities.
            //services.AddOpenIddict(options =>
            //{
            //    // Register the Entity Framework stores.
            //    options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

            //    // Register the ASP.NET Core MVC binder used by OpenIddict.
            //    // Note: if you don't call this method, you won't be able to
            //    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
            //    options.AddMvcBinders();

            //    // Enable the token endpoint (required to use the password flow).
            //    options.EnableTokenEndpoint("/connect/token");

            //    // Allow client applications to use the grant_type=password flow.
            //    options.AllowPasswordFlow();
            //    options.AllowRefreshTokenFlow();

            //    // During development, you can disable the HTTPS requirement.
            //    options.DisableHttpsRequirement();
            //});

            //services.AddAuthentication()
            //    .AddOAuthValidation();

            // add my services
            // services.AddTransient<ISomeService, SomeServiceImpl>();

            // seed the database
            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

            // swagger
            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Test API",
                    Version = "v1"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IDatabaseInitializer databaseInitializer)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            // CORS
            // https://docs.asp.net/en/latest/security/cors.html
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            //app.UseWelcomePage();

            // seed the database
            databaseInitializer.Seed().GetAwaiter().GetResult();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test API V1");
            });
        }
    }
}

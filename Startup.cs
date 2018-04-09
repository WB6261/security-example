using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Fisher.Bookstore.Api.Data;
using Microsoft.EntityFrameworkCore;
using Fisher.Bookstore.Api.Security;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.WebSockets.Internal;

namespace Fisher.Bookstore.Api
{
    public class Startup
    {

        private const string SecretKey = "iNivDmHLpUA223sqsfhqGbMRdRj1PVkH"; // todo: get this from somewhere secure
        private readonly SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BookstoreContext>(options => options.UseNpgsql(Configuration.GetConnectionString("BookstoreConnection")));
            
            // add the identity service
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<BookstoreContext>()
                .AddDefaultTokenProviders();
            
            // configure identity service
            services.Configure<IdentityOptions>(options => 
            {
                // password settings
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                
                //user settings
                options.User.RequireUniqueEmail = true;

                //lockout settings
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
            });

            // configure jwt issuer from app settings
            var jwtIssuerOptionsAppSettings = Configuration.GetSection(nameof(JwtIssuerOptions));

            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtIssuerOptionsAppSettings[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtIssuerOptionsAppSettings[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            // configure jwt auth
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.ClaimsIssuer = jwtIssuerOptionsAppSettings[nameof(JwtIssuerOptions.Issuer)];
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuerOptionsAppSettings[nameof(JwtIssuerOptions.Issuer)],

                    ValidateAudience = true,
                    ValidAudience = Configuration["JwtIssuer"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            //TODO: configure authorization 
            
            services.AddMvc()
            .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}

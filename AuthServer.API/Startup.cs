using AuthServer.Core.Configuration;
using AuthServer.Core.GenericServices;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data;
using AuthServer.Data.Repositories;
using AuthServer.Service;
using AuthServer.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SharedLibrary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.API
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
            //..

            // DI Register
            // AddSingleton : Uygulama boyunca tek 1 instance
            // AddScoped : Ýstek baþýna 1 instance, kaç kere ayný interfacei istersek isteyelim
            // AddTransient : Ayný interface ile her karþýlaþtýðýnda yeni 1 instance
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer"), sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("AuthServer.Data");
                });
            });

            services.AddIdentity<UserApp, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false; // NonAlphanumeric > *, ? , = etc.
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


            // Options Pattern
            services.Configure<List<Client>>(Configuration.GetSection("Clients"));
            services.Configure<CustomTokenOptions>(options => Configuration.GetSection("TokenOption"));

            var tokenOption = Configuration.GetSection("TokenOption").Get<CustomTokenOptions>();

            // token geldiði zaman doðrulama kýsmý ( daðýtma ile ilgili deðil, gelen tokený doðrulama kýsmý)
            services.AddAuthentication(opt =>
            {
                // þema : bayi ve kullanýcý olarak 2 ayrý login sistemi olsaydý 2 ayrý þema verirdik. burda ona þema denir. þuan tek bir defualt þema var
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // > tokendan gelenle (yukarda) benim beklediðim (aþaðýda) eþleþtirme
            })
                // api cookie deðil de headerda token arasýn ..
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => 
            {
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidIssuer = tokenOption.Issuer, // token kimden geldi
                    ValidAudience = tokenOption.Audience[0], // token benden istek yapabilir mi? (ilki olarak kendi adresimi yazdým : "www.authserver.com")
                    IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOption.SecurityKey),

                    ValidateIssuerSigningKey = true, // imzayý doðrula
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,

                    // token ömürlerini kontrol etmek için : normalde lifetme + 5dk veriyor, farklý timezonelar arasý tolere etmek için , 5dk vermesin diye
                    // çnk tek api tek makinede tek yerde. postmande kontrol ederken beklemeyelim diye.
                    // skew >  sapma
                    ClockSkew = TimeSpan.Zero

                };
            }); 


            //..

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthServer.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthServer.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

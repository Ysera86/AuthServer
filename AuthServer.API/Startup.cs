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
            // AddScoped : �stek ba��na 1 instance, ka� kere ayn� interfacei istersek isteyelim
            // AddTransient : Ayn� interface ile her kar��la�t���nda yeni 1 instance
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

            // token geldi�i zaman do�rulama k�sm� ( da��tma ile ilgili de�il, gelen token� do�rulama k�sm�)
            services.AddAuthentication(opt =>
            {
                // �ema : bayi ve kullan�c� olarak 2 ayr� login sistemi olsayd� 2 ayr� �ema verirdik. burda ona �ema denir. �uan tek bir defualt �ema var
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // > tokendan gelenle (yukarda) benim bekledi�im (a�a��da) e�le�tirme
            })
                // api cookie de�il de headerda token aras�n ..
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => 
            {
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidIssuer = tokenOption.Issuer, // token kimden geldi
                    ValidAudience = tokenOption.Audience[0], // token benden istek yapabilir mi? (ilki olarak kendi adresimi yazd�m : "www.authserver.com")
                    IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOption.SecurityKey),

                    ValidateIssuerSigningKey = true, // imzay� do�rula
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,

                    // token �m�rlerini kontrol etmek i�in : normalde lifetme + 5dk veriyor, farkl� timezonelar aras� tolere etmek i�in , 5dk vermesin diye
                    // �nk tek api tek makinede tek yerde. postmande kontrol ederken beklemeyelim diye.
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

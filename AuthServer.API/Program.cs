using AuthServer.Core.Configuration;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data.Repositories;
using AuthServer.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Configuration;
using System.Collections.Generic;
using AuthServer.Core.Services;
using AuthServer.Service.Services;
using AuthServer.Core.GenericServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedLibrary.Extensions;
using FluentValidation.AspNetCore;
using AuthServer.API.Validations;
using SharedLibrary.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//..

// FluentValidation 
builder.Services.AddFluentValidation(opt => opt.RegisterValidatorsFromAssemblyContaining(typeof(CreateUserDtoValidator)));

// Validation hatalarýnda varsayýlan olarak FluentValidation error döndürmesin, benim oluþturduðum response dönsün
// varsayýlan dönüþ tipini ezme.
builder.Services.UseCustomValidationResponse();

// DI Register
// AddSingleton : Uygulama boyunca tek 1 instance
// AddScoped : Ýstek baþýna 1 instance, kaç kere ayný interfacei istersek isteyelim
// AddTransient : Ayný interface ile her karþýlaþtýðýnda yeni 1 instance
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("AuthServer.Data");
    });
});

builder.Services.AddIdentity<UserApp, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false; // NonAlphanumeric > *, ? , = etc.
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


// Options Pattern
builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));
builder.Services.Configure<CustomTokenOptions>(builder.Configuration.GetSection("TokenOption"));

var tokenOption = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOptions>();

#region Shared Library içinden extension

builder.Services.AddCustomTokenAuth(tokenOption);

//// token geldiði zaman doðrulama kýsmý ( daðýtma ile ilgili deðil, gelen tokený doðrulama kýsmý)
//builder.Services.AddAuthentication(opt =>
//{
//    // þema : bayi ve kullanýcý olarak 2 ayrý login sistemi olsaydý 2 ayrý þema verirdik. burda ona þema denir. þuan tek bir defualt þema var
//    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
//    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // > tokendan gelenle (yukarda) benim beklediðim (aþaðýda) eþleþtirme
//})
//    // api cookie deðil de headerda token arasýn ..
//    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
//{
//    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
//    {
//        ValidIssuer = tokenOption.Issuer, // token kimden geldi
//        ValidAudience = tokenOption.Audience[0], // token benden istek yapabilir mi? (ilki olarak kendi adresimi yazdým : "www.authserver.com")
//        IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOption.SecurityKey),

//        ValidateIssuerSigningKey = true, // imzayý doðrula
//        ValidateAudience = true,
//        ValidateIssuer = true,
//        ValidateLifetime = true,

//        // token ömürlerini kontrol etmek için : normalde lifetme + 5dk veriyor, farklý timezonelar arasý tolere etmek için , 5dk vermesin diye
//        // çnk tek api tek makinede tek yerde. postmande kontrol ederken beklemeyelim diye.
//        // skew >  sapma
//        ClockSkew = TimeSpan.Zero

//    };
//});

#endregion


//..


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    //app.UseCustomException();
}
// exception middleware
app.UseCustomException();

app.UseHttpsRedirection();
app.UseRouting();

// Authentication da olmalý. Sýralama önemli, 
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
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

// Validation hatalar�nda varsay�lan olarak FluentValidation error d�nd�rmesin, benim olu�turdu�um response d�ns�n
// varsay�lan d�n�� tipini ezme.
builder.Services.UseCustomValidationResponse();

// DI Register
// AddSingleton : Uygulama boyunca tek 1 instance
// AddScoped : �stek ba��na 1 instance, ka� kere ayn� interfacei istersek isteyelim
// AddTransient : Ayn� interface ile her kar��la�t���nda yeni 1 instance
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

#region Shared Library i�inden extension

builder.Services.AddCustomTokenAuth(tokenOption);

//// token geldi�i zaman do�rulama k�sm� ( da��tma ile ilgili de�il, gelen token� do�rulama k�sm�)
//builder.Services.AddAuthentication(opt =>
//{
//    // �ema : bayi ve kullan�c� olarak 2 ayr� login sistemi olsayd� 2 ayr� �ema verirdik. burda ona �ema denir. �uan tek bir defualt �ema var
//    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
//    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // > tokendan gelenle (yukarda) benim bekledi�im (a�a��da) e�le�tirme
//})
//    // api cookie de�il de headerda token aras�n ..
//    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
//{
//    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
//    {
//        ValidIssuer = tokenOption.Issuer, // token kimden geldi
//        ValidAudience = tokenOption.Audience[0], // token benden istek yapabilir mi? (ilki olarak kendi adresimi yazd�m : "www.authserver.com")
//        IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOption.SecurityKey),

//        ValidateIssuerSigningKey = true, // imzay� do�rula
//        ValidateAudience = true,
//        ValidateIssuer = true,
//        ValidateLifetime = true,

//        // token �m�rlerini kontrol etmek i�in : normalde lifetme + 5dk veriyor, farkl� timezonelar aras� tolere etmek i�in , 5dk vermesin diye
//        // �nk tek api tek makinede tek yerde. postmande kontrol ederken beklemeyelim diye.
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

// Authentication da olmal�. S�ralama �nemli, 
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
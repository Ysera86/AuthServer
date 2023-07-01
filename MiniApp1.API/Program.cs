using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniApp1.API.Requirement;
using SharedLibrary.Configuration;
using SharedLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//..
builder.Services.Configure<CustomTokenOptions>(builder.Configuration.GetSection("TokenOption"));
var tokenOption = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOptions>();

builder.Services.AddCustomTokenAuth(tokenOption);

// dynamic claim için policy belirlemem lazým (þartname)
builder.Services.AddScoped<IAuthorizationHandler, BirthDayRequirementHandler>();

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("MuglaPolicy", policy =>
    {
        policy.RequireClaim("city", "mugla");
        //policy.RequireClaim("city", "mugla","newyork"); // 2side olur dersem
    });

    opt.AddPolicy("AgePolicy", policy =>
    {
        policy.Requirements.Add(new BirthDayRequirement(18));
    });
});
//..

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//..
app.UseAuthentication();
//..

app.UseAuthorization();

app.MapControllers();

app.Run();
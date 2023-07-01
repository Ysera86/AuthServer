using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
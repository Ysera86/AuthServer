using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Configuration;
using SharedLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Extensions
{
    public static class CustomTokenAuth
    {
        public static void AddCustomTokenAuth(this IServiceCollection services, CustomTokenOptions tokenOption)
        {
            // token geldiği zaman doğrulama kısmı ( dağıtma ile ilgili değil, gelen tokenı doğrulama kısmı)
            services.AddAuthentication(opt =>
            {
                // şema : bayi ve kullanıcı olarak 2 ayrı login sistemi olsaydı 2 ayrı şema verirdik. burda ona şema denir. şuan tek bir defualt şema var
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // > tokendan gelenle (yukarda) benim beklediğim (aşağıda) eşleştirme
            })
                // api cookie değil de headerda token arasın ..
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
                {
                    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidIssuer = tokenOption.Issuer, // token kimden geldi
                        ValidAudience = tokenOption.Audience[0], // token benden istek yapabilir mi? (ilki olarak kendi adresimi yazdım : "www.authserver.com")
                        IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOption.SecurityKey),

                        ValidateIssuerSigningKey = true, // imzayı doğrula
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,

                        // token ömürlerini kontrol etmek için : normalde lifetme + 5dk veriyor, farklı timezonelar arası tolere etmek için , 5dk vermesin diye
                        // çnk tek api tek makinede tek yerde. postmande kontrol ederken beklemeyelim diye.
                        // skew >  sapma
                        ClockSkew = TimeSpan.Zero

                    };
                });
        }
    }
}

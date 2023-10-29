using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Student_attendence.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Student_attendence.AuthFile
{
    public class JwtAuthentication
    {
        private static string authKey = "Bearer";
        private IConfiguration _configuration;

        public JwtAuthentication(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static void JwtAuth(IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(authKey, options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = config["AppSettings:TokenIssuer"],
                    ValidAudience = config["AppSettings:TokenAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["AppSettings:SecurityKey"]))
                };
            });
            
        }

        public string GenerateJsonToken(AuthData authData)
        {
            try
            {
                var tokenClaims = new[]
                    {
                    new Claim ("UserId", Convert.ToString(authData.UserId)),
                    new Claim ("UserName", Convert.ToString(authData.UserName)),
                    new Claim ("UserStatus", Convert.ToString(authData.UserStatus)),
                    new Claim ("UserGender", Convert.ToString(authData.Gender)),
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:SecurityKey"]));

                var token = new JwtSecurityToken(
                     issuer: _configuration["AppSettings:TokenIssuer"],
                     audience: _configuration["AppSettings:TokenAudience"],
                     expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["AppSettings:TokenExpirationMinutes"])),
                     claims: tokenClaims,
                     signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                 );

                var tokenString = Convert.ToString(new JwtSecurityTokenHandler().WriteToken(token));
                return tokenString;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}

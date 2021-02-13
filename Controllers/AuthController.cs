using API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        private List<userMackDatas> appUsers = new List<userMackDatas>
        {
            new userMackDatas {  FullName = "Vaibhav Bhapkar",  UserName = "admin", Password = "1234", UserRole = "Admin" },
            new userMackDatas {  FullName = "Test User",  UserName = "user", Password = "1234", UserRole = "User" },
            new userMackDatas {  FullName = "Jutipong Subin",  UserName = "Jutipong.Su", Password = "123456", UserRole = "Admin" }
        };

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public IActionResult Login(UserLoginModel login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login);
            if (user != null)
            {
                var tokenString = GenerateJWTToken(user);
                response = Ok(new
                {
                    //token = tokenString,
                    user = user,
                });
            }
            return response;
        }

        private userMackDatas AuthenticateUser(UserLoginModel userLogin)
        {
            var user = appUsers.SingleOrDefault(x => x.UserName == userLogin.UserName && x.Password == userLogin.Password);
            return user;
        }

        private string GenerateJWTToken(userMackDatas userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new Claim("fullName", userInfo.FullName.ToString()),
                new Claim("role",userInfo.UserRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

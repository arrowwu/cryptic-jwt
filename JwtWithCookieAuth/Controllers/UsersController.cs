using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JwtWithCookieAuth.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using System.Security.Cryptography;

namespace JwtWithCookieAuth.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {
        DataAccess _dataAccess;

        public UsersController()
        {
            _dataAccess = new DataAccess();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody]LoginFormUser loginFormUser)
        {
            try
            {
                User user = new User();
                user.Email = loginFormUser.Email;
                user.Salt = GetSalt();
                user.Hash = GetHash(loginFormUser.Password+user.Salt);
                await _dataAccess.CreateUser(user);
                string location = user.Id.ToString();
                var json = new { jwt = GenerateToken(loginFormUser.Email) };
                return Created(location, json);
            }
            catch(MongoWriteException e)
            {
                return new BadRequestObjectResult("Email was taken");
            }           
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody]LoginFormUser loginFormUser)
        {
            User userReturned = _dataAccess.LoginUser(loginFormUser);
            if(userReturned == null)
            {
                return new UnauthorizedResult();
            }
            if(VerifyPassword(userReturned.Hash, userReturned.Salt, loginFormUser.Password))
            {
                string location = userReturned.Id.ToString();
                var json = new { jwt = GenerateToken(loginFormUser.Email) };
                return new ObjectResult(json);
            }
            return new UnauthorizedResult();
        }

        private string GenerateToken(string email)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };

            SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisisasecreteforauth"));
            SigningCredentials signingCredential = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            JwtHeader jwtHeader = new JwtHeader(signingCredential);
            JwtPayload jwtPayload = new JwtPayload(claims);
            JwtSecurityToken token = new JwtSecurityToken(jwtHeader, jwtPayload);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GetHash(string text)
        {
            // SHA512 is disposable by inheritance.  
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.  
                var HashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                // Get the hashed string.  
                return BitConverter.ToString(HashedBytes).Replace("-", "").ToLower();
            }
        }

        public static string GetSalt()
        {
            byte[] bytes = new byte[128 / 8];
            using (var keyGenerator = RandomNumberGenerator.Create())
            {
                keyGenerator.GetBytes(bytes);
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        private Boolean VerifyPassword(String HashedPassword, String Salt, String PasswordEntered)
        {
            if (GetHash(PasswordEntered + Salt) == HashedPassword)
            {
                return true;
            }
            return false;
        }






    }
}
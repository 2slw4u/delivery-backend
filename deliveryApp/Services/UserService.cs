using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Models.Exceptions;
using deliveryApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.Xml;
using System.Runtime.CompilerServices;

namespace deliveryApp.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public Task EditProfile(string token, UserEditModel newUserModel)
        {
            throw new NotImplementedException();
        }

        public Task<UserDto> GetProfile(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<TokenResponse> Login(LoginCredentials credentials)
        {
            if (credentials.Email == null || credentials.Password == null)
            {
                throw new BadRequest("Neither email nor password can be null");
            }
            var userIdentity = await FormIdentity(credentials.Email, credentials.Password);
            var jwtToken = new JwtSecurityToken(issuer: StandardJwtConfiguration.Issuer,
                audience: StandardJwtConfiguration.Audience, notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(StandardJwtConfiguration.Lifetime),
                signingCredentials: new SigningCredentials(StandardJwtConfiguration.GenerateSecurityKey(), SecurityAlgorithms.Sha256));
            var result = new TokenEntity()
            {
                Id = Guid.NewGuid(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                ExpirationDate = jwtToken.ValidTo
            };
            try
            {
                _context.Tokens.Add(result);
                await _context.SaveChangesAsync();
                return new TokenResponse() { Token = result.Token };
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task<Response> Logout(string token)
        {
            await ValidateToken(token);
            var result = new Response();
            try
            {
                _context.Tokens.Remove(await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync());
                await _context.SaveChangesAsync();
                result.Status = "OK";
                result.Message = "Succesfully logged out";
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
            return result;
        }

        public async Task<TokenResponse> Register(UserRegisterModel newUser)
        {
            await ValidateUserModel(newUser);
            var newUserEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                FullName = newUser.FullName,
                Password = newUser.Password,
                BirthDate = newUser.BirthDate,
                Gender = newUser.Gender,
                Phone = newUser.PhoneNumber,
                Email = newUser.Email
            };
            try
            {
                await _context.Users.AddAsync(newUserEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
            return await Login(new LoginCredentials { Password = newUser.Password, Email = newUser.Email });
        }

        private async Task ValidateToken(string token)
        {
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            if (tokenInDB == null)
            {
                throw new NotFound("The token does not exist in database");
            }
            else if (tokenInDB.ExpirationDate < DateTime.Now)
            {
                _context.Tokens.Remove(tokenInDB);
                await _context.SaveChangesAsync();
                throw new Conflict("Token is expired");
            }
        }

        private async Task<ClaimsIdentity> FormIdentity (string email, string password)
        {
            var user = await _context.Users.Where(x=> x.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new NotFound("There is no user with given email");
            }
            if (user.Password != password)
            {
                throw new Forbidden("Wrong password");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, "User");
            return claimsIdentity;
        }

        private async Task ValidateUserModel(UserRegisterModel userModel)
        {
            await ValidateEmail(userModel);
            ValidatePassword(userModel);
            ValidateGender(userModel);
            ValidateDOB(userModel);
        }

        private async Task ValidateEmail(UserRegisterModel userModel)
        {
            userModel.Email = userModel.Email.ToLower().TrimEnd();
            var emailRegex = new Regex(@"[a-zA-Z]+\w*@[a-zA-Z]+\.[a-zA-Z]+");
            if (emailRegex.Matches(userModel.Email).Count <= 0)
            {
                throw new BadRequest("Email doesn't look right");
            }
            if (await _context.Users.Where(x => userModel.Email == x.Email).FirstAsync() != null)
            {
                throw new Conflict("Your email already exists");
            }
            return;
        }

        private static void ValidatePassword(UserRegisterModel userModel)
        {
            if (userModel.Password.Length < 7)
            {
                throw new BadRequest("Password must be longer than 6 characters");
            }
            return;
        }

        private static void ValidateGender(UserRegisterModel userModel)
        {
            if (userModel.Gender != Models.Enums.Gender.Male && userModel.Gender != Models.Enums.Gender.Female)
            {
                throw new BadRequest("Gender must be either Male or Female");
            }
            return;
        }

        private static void ValidateDOB(UserRegisterModel userModel)
        {
            if (userModel.BirthDate > DateTime.Now)
            {
                throw new BadRequest("No timetravellers allowed (user hasn't been born yet)");
            }
            return;
        }
    }
}

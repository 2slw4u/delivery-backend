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
        private readonly IAddressService _addressService;

        public UserService(AppDbContext context, IAddressService addressService)
        {
            _context = context;
            _addressService = addressService;
        }

        public async Task EditProfile(string token, UserEditModel newUserModel)
        {
            await ValidateToken(token);
            var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
            var userEntity = await _context.Users.Where(x => x.Email == tokenEntity.userEmail).FirstOrDefaultAsync();
            var editedUser = new UserRegisterModel()
            {
                FullName = newUserModel.FullName,
                Password = userEntity.Password,
                Email = userEntity.Email,
                AddressId = newUserModel.AddressId,
                BirthDate = newUserModel.BirthDate.GetValueOrDefault().ToUniversalTime(),
                Gender = newUserModel.Gender,
                PhoneNumber = newUserModel.PhoneNumber
            };
            await ValidateUserModel(editedUser, true);
            _context.Remove(userEntity);
            await _context.SaveChangesAsync();
            await Register(editedUser);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDto> GetProfile(string token)
        {
            await ValidateToken(token);
            var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
            var userEntity = await _context.Users.Where(x => x.Email == tokenEntity.userEmail).FirstOrDefaultAsync();
            var result = new UserDto()
            {
                Id = userEntity.Id,
                FullName = userEntity.FullName,
                BirthDate = userEntity.BirthDate.GetValueOrDefault().ToUniversalTime(),
                Gender = userEntity.Gender,
                Address = userEntity.AddressGuid,
                Email = userEntity.Email,
                PhoneNumber = userEntity.Phone
            };
            return result;
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
                signingCredentials: new SigningCredentials(StandardJwtConfiguration.GenerateSecurityKey(), SecurityAlgorithms.HmacSha256));
            var result = new TokenEntity()
            {
                Id = Guid.NewGuid(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                ExpirationDate = jwtToken.ValidTo.ToUniversalTime(),
                userEmail = credentials.Email
            };
            _context.Tokens.Add(result);
            await _context.SaveChangesAsync();
            return new TokenResponse() { Token = result.Token };
        }

        public async Task Logout(string token)
        {
            await ValidateToken(token);
            _context.Tokens.Remove(await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync());
            await _context.SaveChangesAsync();
        }

        public async Task<TokenResponse> Register(UserRegisterModel newUser)
        {
            await ValidateUserModel(newUser, false);
            var newUserEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                FullName = newUser.FullName,
                Password = newUser.Password,
                BirthDate = newUser.BirthDate.GetValueOrDefault().ToUniversalTime(),
                Gender = newUser.Gender,
                Phone = newUser.PhoneNumber,
                Email = newUser.Email,
                AddressGuid = newUser.AddressId
            };
            _context.Users.Add(newUserEntity);
            await _context.SaveChangesAsync();
            return await Login(new LoginCredentials { Password = newUser.Password, Email = newUser.Email });
        }

        private async Task ValidateToken(string token)
        {
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            if (tokenInDB == null)
            {
                throw new Unauthorized("The token does not exist in database");
            }
            else if (tokenInDB.ExpirationDate < DateTime.Now.ToUniversalTime())
            {
                _context.Tokens.Remove(tokenInDB);
                await _context.SaveChangesAsync();
                throw new Forbidden("Token is expired");
            }
        }

        private async Task<ClaimsIdentity> FormIdentity(string email, string password)
        {
            var user = await _context.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
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

        private async Task ValidateUserModel(UserRegisterModel userModel, bool ignoreEmail)
        {
            if (!ignoreEmail)
            {
                await ValidateEmail(userModel);
            }
            ValidateGender(userModel);
            ValidateDOB(userModel);
            await _addressService.ValidateAddressGuid(userModel.AddressId);
        }

        private async Task ValidateEmail(UserRegisterModel userModel)
        {
            userModel.Email = userModel.Email.ToLower().TrimEnd();
            var emailRegex = new Regex(@"[a-zA-Z]+\w*@[a-zA-Z]+\.[a-zA-Z]+");
            if (emailRegex.Matches(userModel.Email).Count <= 0)
            {
                throw new BadRequest("Email doesn't look right");
            }
            if (await _context.Users.Where(x => userModel.Email == x.Email).FirstOrDefaultAsync() != null)
            {
                throw new Conflict("User with given email already exists");
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

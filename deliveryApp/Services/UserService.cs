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
using System.Net;

namespace deliveryApp.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IAddressService _addressService;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, IAddressService addressService, ILogger<UserService> logger)
        {
            _context = context;
            _addressService = addressService;
            _logger = logger;
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
            _logger.LogInformation($"Profile of a user with former {userEntity.Email} email has been edited. Its email is now {editedUser.Email}");
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
            _logger.LogInformation($"Profile of a user with {result.Email} email has been given out");
            return result;
        }

        public async Task<TokenResponse> Login(LoginCredentials credentials)
        {
            if (credentials.Email == null || credentials.Password == null)
            {
                _logger.LogError($"Either given email({credentials.Email}) or given password({credentials.Password}) is null");
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
            var previousTokens = await _context.Tokens.Where(x => x.userEmail == credentials.Email).ToListAsync();
            foreach (var previousToken in previousTokens)
            {
                _context.Remove(previousToken);
            }
            _context.Tokens.Add(result);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Login of a user with {credentials.Email} email has been successful");
            return new TokenResponse() { Token = result.Token };
        }

        public async Task Logout(string token)
        {
            await ValidateToken(token);
            _context.Tokens.Remove(await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync());
            _logger.LogInformation($"Logout of a user with {token} token has been successful");
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
            _logger.LogInformation($"Registration of a user with {newUser.Email} has been successful");
            return await Login(new LoginCredentials { Password = newUser.Password, Email = newUser.Email });
        }

        private async Task ValidateToken(string token)
        {
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            if (tokenInDB == null)
            {
                _logger.LogError($"Token {token} has not been found in database");
                throw new Unauthorized($"The token does not exist in database (token: {token})");
            }
            else if (tokenInDB.ExpirationDate < DateTime.Now.ToUniversalTime())
            {
                _context.Tokens.Remove(tokenInDB);
                await _context.SaveChangesAsync();
                _logger.LogError($"Token {token} has expired");
                throw new Forbidden($"Token is expired (token: {token})");
            }
            _logger.LogInformation($"Token {token} has been validated");
        }

        private async Task<ClaimsIdentity> FormIdentity(string email, string password)
        {
            var user = await _context.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogError($"User with {email} email has not been found");
                throw new NotFound($"There is no user with {email} email");
            }
            if (user.Password != password)
            {
                _logger.LogError($"User with {email} email put in incorrect password; they put in {password} although their password is not that, which is why their request is forbidden");
                throw new Forbidden($"Password {password} is wrong for the user with email {email}");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, "User");
            _logger.LogInformation($"Identity for user with {email} email has been formed");
            return claimsIdentity;
        }

        private async Task ValidateUserModel(UserRegisterModel userModel, bool ignoreEmail)
        {
            if (!ignoreEmail)
            {
                await ValidateEmail(userModel);
            }
            await ValidateGender(userModel);
            await ValidateDOB(userModel);
            await _addressService.ValidateAddressGuid(userModel.AddressId);
            _logger.LogInformation($"UserModel with {userModel.Email} email has been validated");
        }

        private async Task ValidateEmail(UserRegisterModel userModel)
        {
            userModel.Email = userModel.Email.ToLower().TrimEnd();
            var emailRegex = new Regex(@"[a-zA-Z]+\w*@[a-zA-Z]+\.[a-zA-Z]+");
            if (emailRegex.Matches(userModel.Email).Count <= 0)
            {
                throw new BadRequest($"Email {userModel.Email} doesn't look right");
            }
            if (await _context.Users.Where(x => userModel.Email == x.Email).FirstOrDefaultAsync() != null)
            {
                _logger.LogError($"User with {userModel.Email} email already exists, which is why request is bad");
                throw new Conflict($"User with {userModel.Email} email already exists");
            }
            _logger.LogInformation($"Email {userModel.Email} has been validated");
            return;
        }

        private async Task ValidateGender(UserRegisterModel userModel)
        {

            if (userModel.Gender != Models.Enums.Gender.Male && userModel.Gender != Models.Enums.Gender.Female)
            {
                _logger.LogError($"User gave the input of {userModel.Gender} although Gender can only be either 0 or 1, which is why request is bad");
                throw new BadRequest($"Gender must be either Male(0) or Female(1), no {userModel.Gender}");
            }
            _logger.LogInformation($"Gender {userModel.Gender} has been validated");
            return;
        }

        private async Task ValidateDOB(UserRegisterModel userModel)
        {
            if (userModel.BirthDate > DateTime.Now)
            {
                _logger.LogError($"{userModel.BirthDate} is higher than current date, which is why request is bad");
                throw new BadRequest("No timetravellers allowed");
            }
            _logger.LogInformation($"Date of birth {userModel.BirthDate} has been validated");
            return;
        }
    }
}

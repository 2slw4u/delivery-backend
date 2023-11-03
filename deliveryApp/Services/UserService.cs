using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        public Task<TokenResponse> Login(LoginCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public Task Logout(string token)
        {
            throw new NotImplementedException();
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
                var exception = new Exception();
                exception.Data.Add(StatusCodes.Status400BadRequest.ToString(), "Email doesn't look right");
                throw exception;
            }
            if (await _context.Users.Where(x => userModel.Email == x.Email).FirstAsync() != null)
            {
                var exception = new Exception();
                exception.Data.Add(StatusCodes.Status411LengthRequired.ToString(), "Your email already exists");
                throw exception;
            }
            return;
        }

        private static void ValidatePassword(UserRegisterModel userModel)
        {
            if (userModel.Password.Length < 7)
            {
                var exception = new Exception();
                exception.Data.Add(StatusCodes.Status400BadRequest.ToString(), "Password must be longer than 6 characters");
                throw exception;
            }
            return;
        }

        private static void ValidateGender(UserRegisterModel userModel)
        {
            if (userModel.Gender != Models.Enums.Gender.Male && userModel.Gender != Models.Enums.Gender.Female)
            {
                var exception = new Exception();
                exception.Data.Add(StatusCodes.Status400BadRequest.ToString(), "Gender must be either Male or Female");
                throw exception;
            }
            return;
        }

        private static void ValidateDOB(UserRegisterModel userModel)
        {
            if (userModel.BirthDate > DateTime.Now)
            {
                var exception = new Exception();
                exception.Data.Add(StatusCodes.Status400BadRequest.ToString(), "No timetravellers allowed (user hasn't been born yet)");
                throw exception;
            }
            return;
        }
    }
}

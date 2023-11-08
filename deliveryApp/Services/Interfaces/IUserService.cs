using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<TokenResponse> Register(UserRegisterModel newUser, Guid? preEditedGuid = null);
        Task<TokenResponse> Login(LoginCredentials credentials);
        Task Logout(string token);
        Task<UserDto> GetProfile(string token);
        Task EditProfile(string token, UserEditModel newUserModel);
    }
}

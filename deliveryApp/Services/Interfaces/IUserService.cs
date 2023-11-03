using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<TokenResponse> Register(UserRegisterModel newUser);
        Task<TokenResponse> Login(LoginCredentials credentials);
        Task<Response> Logout(string token);
        Task<UserDto> GetProfile(string token);
        Task EditProfile(string token, UserEditModel newUserModel);
    }
}

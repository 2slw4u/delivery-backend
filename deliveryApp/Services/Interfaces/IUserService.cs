using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<TokenResponse> Register(UserRegisterModel newUser, Guid? preEditedGuid = null);
        Task<TokenResponse> Login(LoginCredentials credentials);
        Task Logout(HttpContext httpContext);
        Task<UserDto> GetProfile(HttpContext httpContext);
        Task EditProfile(HttpContext httpContext, UserEditModel newUserModel);
    }
}

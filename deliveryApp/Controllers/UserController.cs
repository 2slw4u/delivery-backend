using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace deliveryApp.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<TokenResponse> Register([FromBody] UserRegisterModel newUser)
        {
            return await _userService.Register(newUser);
        }
        [HttpPost]
        [Route("login")]
        public async Task<TokenResponse> Login([FromBody] LoginCredentials credentials)
        {
            return await _userService.Login(credentials);
        }
        [HttpPost]
        [Route("logout")]
        public async Task<Response> Logout(string token)
        {
            return await _userService.Logout(token);
        }
        [HttpGet]
        [Route("profile")]
        public async Task<UserDto> GetProfile(string token)
        {
            return await _userService.GetProfile(token);
        }
    }
}

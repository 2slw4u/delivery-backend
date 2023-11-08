using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task Logout()
        {
            await _userService.Logout(HttpContext);
        }
        [HttpGet]
        [Route("profile")]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task<UserDto> GetProfile()
        {
            return await _userService.GetProfile(HttpContext);
        }
        [HttpPut]
        [Route("profile")]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task EditProfile(UserEditModel newModel)
        {
            await _userService.EditProfile(HttpContext, newModel);
        }
    }
}

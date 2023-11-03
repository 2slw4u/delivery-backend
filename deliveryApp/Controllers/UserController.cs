﻿using deliveryApp.Models.DTOs;
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
    }
}

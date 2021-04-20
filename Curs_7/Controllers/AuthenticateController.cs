using Curs_7.DTOs;
using Curs_7.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curs_7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TokenHelper _tokenHelper;

        public AuthenticateController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            TokenHelper tokenHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenHelper = tokenHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, registerDto.Role);

                return Ok(new
                {
                    Message = "User created Successfully"
                });
            }
            else
                return BadRequest("There was an error.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return NotFound();

            else
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (result.Succeeded)
                {
                    var token = await _tokenHelper.CreateAccessToken(user);
                    var refreshToken = _tokenHelper.CreateRefreshToken();

                    user.RefreshToken = refreshToken;
                    await _userManager.UpdateAsync(user);

                    return Ok(new
                    {
                        AccessToken = token,
                        RefreshToken = refreshToken
                    });
                }

                else
                    return BadRequest("Failed to login, try again");
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshDto refreshDto)
        {
            var principal = _tokenHelper.GetPrincipalFromExpiredToken(refreshDto.AccessToken);
            var username = principal.Identity.Name;

            var user = await _userManager.FindByEmailAsync(username);

            if (user.RefreshToken != refreshDto.RefreshToken)
                return BadRequest("Bad refreshToken");

            var newJwtToken = await _tokenHelper.CreateAccessToken(user);

            return Ok(new
            {
                Token = newJwtToken
            });
        }
    }
}

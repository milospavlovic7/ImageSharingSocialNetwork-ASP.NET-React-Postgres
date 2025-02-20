using AspNetCorePostgreSQLDockerApp.Models;
using AspNetCorePostgreSQLDockerApp.DTOs;
using AspNetCorePostgreSQLDockerApp.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using AspNetCorePostgreSQLDockerApp.Helpers;

namespace AspNetCorePostgreSQLDockerApp.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserServiceController(IUsersRepository usersRepository) : ControllerBase
    {
        private readonly IUsersRepository _usersRepository = usersRepository;

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid userId)
        {
            try
            {
                var user = await _usersRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture,
                    Bio = user.Bio
                };
                return Ok(userDto);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterUserDTO registerDto)
        {
            try
            {
                var existingUser = await _usersRepository.GetUserByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest("User with this email already exists.");
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    Password = HashPassword(registerDto.Password),
                    ProfilePicture = registerDto.ProfilePicture,
                    Bio = registerDto.Bio,
                    CreatedAt = DateTime.UtcNow
                };

                await _usersRepository.AddUserAsync(user);

                return CreatedAtAction(nameof(GetUserById), new { userId = user.UserId }, new { user.UserId, user.Name });
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var user = await _usersRepository.GetUserByEmailAsync(loginRequest.Email);
                if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
                {
                    return Unauthorized("Invalid email or password.");
                }

                var token = JWTHelper.GenerateJwtToken(user);
                return Ok(new { Token = token });
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{userId}")]
        [Authorize]
        public async Task<ActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDTO userDto)
        {
            try
            {
                var existingUser = await _usersRepository.GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    return NotFound("User not found.");
                }

                existingUser.Name = userDto.Name;
                existingUser.Email = userDto.Email;
                existingUser.ProfilePicture = userDto.ProfilePicture;
                existingUser.Bio = userDto.Bio;

                await _usersRepository.UpdateUserAsync(existingUser);
                return NoContent();
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{userId}/change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordDTO passwordDto)
        {
            try
            {
                var user = await _usersRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Provera stare lozinke
                if (!BCrypt.Net.BCrypt.Verify(passwordDto.OldPassword, user.Password))
                {
                    return BadRequest("Old password is incorrect.");
                }

                // Hashiranje nove lozinke i ažuriranje korisnika
                user.Password = HashPassword(passwordDto.NewPassword);
                await _usersRepository.UpdateUserAsync(user);

                return NoContent();
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{userId}")]
        [Authorize]
        public async Task<ActionResult> DeleteUser(Guid userId)
        {
            try
            {
                var user = await _usersRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                await _usersRepository.DeleteUserAsync(userId);
                return NoContent();
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
        }
    }
}
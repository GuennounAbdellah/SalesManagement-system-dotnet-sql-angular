using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend.Entities;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // api/users
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = await _userService.Authenticate(model);
            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            return Ok(response);
        }
        [HttpGet("{id}")]
        [RoleOrAdmin("Users.View")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        [HttpGet]
        [RoleOrAdmin("Users.View")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the users." });
            }

        }
        [HttpPost]
        [RoleOrAdmin("Users.Create")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest model)
        {
            if (model == null)
                return BadRequest(new { message = "User cannot be null" });
            try
            {
                var createdUser = await _userService.CreateUser(model);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the user.", error = ex.Message });
            }
        }
        [HttpPut("{id}")]
        [RoleOrAdmin("Users.Edit")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateRequest model)
        {
            if (model == null)
                return BadRequest(new { message = "User cannot be null" });
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                    return NotFound();
                var updatedUser = await _userService.UpdateUser(id, model);
                return Ok(updatedUser);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user.", error = ex.Message });
            }
        }
        [HttpDelete("{id}")] 
        [RoleOrAdmin("Users.Delete")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                    return NotFound();
                await _userService.DeleteUser(id);
                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the user.", error = ex.Message });
            }

        }
        [HttpGet("Roles")]
        [Authorize]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _userService.GetAllRoles();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the roles.", error = ex.Message });
            }
        }
    }
} 
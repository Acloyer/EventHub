using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EventHub.Models.DTOs;
using EventHub.Services;
using UserModel = EventHub.Models.User;

namespace EventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly JwtService _jwtService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<UserModel> userManager,
            JwtService jwtService,
            IUserService userService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterDto model)
        {
            try
            {
                var result = await _userService.Register(model);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Error during registration" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto model)
        {
            try
            {
                var result = await _userService.Login(model);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Error during login" });
            }
        }
    }
}

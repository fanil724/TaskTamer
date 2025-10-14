using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;
using TaskTamer_Logic.Models;

namespace TaskTamer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserService _userService;

    public AuthController(IAuthService authService,
        ILogger<AuthController> logger,
        IConfiguration configuration,
        UserService userService)
    {
        _authService = authService;
        _logger = logger;
        _configuration = configuration;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var result = await _authService.Authenticate(
            loginDto.Username,
            loginDto.Password,
            ipAddress,
            userAgent);

        if (!result.Success)
            return Unauthorized(new { result.Message });

        Response.Cookies.Append(_configuration.GetValue<string>("JWT:token") ?? "token", result.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("JWT:ExpiryHours"))
            });


        return Ok(new
        {
            result.UserId,
            result.Username,
            result.Role,
            result.Department,
            result.EmployeeId,
            result.UserType
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
    {
        var result = await _authService.Register(registrationDto);

        if (!result.Success)
            return BadRequest(new { result.Message });

        return Ok(new { result.UserId });
    }


    [HttpPost("logout/{userID}")]
    [Authorize]
    public async Task<IActionResult> Logout(int userID)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var result = await _authService.Logout(
            userID,
            ipAddress,
            userAgent);

        if (!result.Success)
            return Unauthorized(new { result.Message });


        Response.Cookies.Delete(_configuration.GetValue<string>("JWT:token") ?? "token");
        return Ok(new { message = "Successfully logged out" });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetUserProfile()
    {
        var userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var roleUser = User.FindFirst(ClaimTypes.Role)?.Value;

        var user = await _userService.GetUserIdAsync(Convert.ToInt32(userid));
        if (user.IsSuccess)
        {
            return Ok(new
            {
                userId = userid,
                username = userName,
                role = roleUser,
                department = user.Data.employeeDTO.departmentDTO.Name,
                employeeId = user.Data.employeeDTO.EmployeeID,
                userType = user.Data.employeeDTO.UserType

            });
        }

        return Unauthorized(user.Message);


    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> changePassword([FromBody] PasswordResetDto resetDto)
    {
        var result = await _authService.ChangePassword(User.Identity.Name, resetDto.OLDPassword, resetDto.NewPassword);

        if (!result.Success)
            return BadRequest(new { result.Message });

        return Ok();
    }


}
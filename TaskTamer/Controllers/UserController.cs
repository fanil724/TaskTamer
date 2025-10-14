using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;

namespace TaskTamer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }


    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UserDTO user)
    {
        var res = await _userService.Update(user);
        if (!res.IsSuccess)
        {
            return BadRequest(res.Message);
        }

        return Ok(res.Message);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var users = await _userService.GetAllAsync();

        if (!users.IsSuccess)
        {
            return NoContent();
        }
        return Ok(users.Data?.ToList());
    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _userService.GetUserIdAsync(id);
        if (user == null)
        {
            return NoContent();
        }
        return Ok(user.Data);
    }
}
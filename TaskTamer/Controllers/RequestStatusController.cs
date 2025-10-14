using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTamer_Application.Service;

namespace TaskTamer_API.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RequestStatusController:ControllerBase
{
    private readonly RequestStatusService _requestStatusService;

    public RequestStatusController(RequestStatusService requestStatusService)
    {
        _requestStatusService = requestStatusService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {


        var result = await _requestStatusService.GetAllRequestStatusAsync();
        if (result.IsSuccess)
        {

            return Ok(result.Data);
        }
        return BadRequest(result.Message);


    }
}
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;

namespace TaskTamer.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RequestController : ControllerBase
{
    private readonly RequestService _requestService;
    private readonly UserService _userService;
    private readonly IServerSentEventsService _sseService;

    public RequestController(RequestService requestService, UserService userService, IServerSentEventsService serverSent)
    {
        _requestService = requestService;
        _userService = userService;
        _sseService = serverSent;
    }

    [HttpPost]
    public async Task<IActionResult> Create(RequestDTO request)
    {

        var result = await _requestService.CreateRequestAsync(request);
        if (result.IsSuccess)
        {
            var eventdto = new EventDTO()
            {
                RequestId = result.Data,
                EventName = "create",
                EventType = "notification",
                userName = User.Identity?.Name ?? ""
            };
            var clients = _sseService.GetClients();

            foreach (var client in clients.Where(x => x.User.Identity.Name != User.Identity.Name))
            {
                await client.SendEventAsync(new ServerSentEvent
                {
                    Type = "eventmessage",
                    Data = new List<string> { JsonSerializer.Serialize(eventdto) }
                });
            }
            return Ok();

        }
        return BadRequest(result.Message);
    }

    [HttpPut]
    public async Task<IActionResult> Update(RequestDTO request)
    {
        var result = await _requestService.UpdateRequestAsync(request, Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value));
        if (result.IsSuccess)
        {
            var eventdto = new EventDTO()
            {
                RequestId = request.RequestID,
                EventName = "update",
                EventType = "notification",
                userName = User.Identity?.Name ?? ""
            };
            var clients = _sseService.GetClients();

            foreach (var client in clients.Where(x => x.User.Identity.Name != User.Identity.Name))
            {
                await client.SendEventAsync(new ServerSentEvent
                {
                    Type = "eventmessage",
                    Data = new List<string> { JsonSerializer.Serialize(eventdto) }
                });
            }
            return Ok();
        }

        return BadRequest(result.Message);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _requestService.GetAllRequestsAsync();
        if (result.IsSuccess)
        {
            var req = await filterRequest(result.Data.ToList());
            return Ok(req);
        }
        return BadRequest(result.Message);
    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _requestService.GetRequestByIdAsync(id);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        return BadRequest(result.Message);
    }

    private async Task<List<RequestDTO>> filterRequest(List<RequestDTO> requests)
    {
        var userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _userService.GetUserIdAsync(userid);
        if (!user.IsSuccess)
        {
            return requests;
        }
        var u = user.Data;

        return u.employeeDTO.UserType switch
        {
            "Employee" => requests.Where(x => (x.Executor.EmployeeID == u.employeeDTO.EmployeeID
            || x.Author.EmployeeID == u.employeeDTO.EmployeeID)
            && !x.RequestStatus.Name.Contains("Завершена")).ToList(),

            _ => requests
        };
    }
}
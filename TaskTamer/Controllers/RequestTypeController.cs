using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTamer_Application.Service;

namespace TaskTamer_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RequestTypeController : ControllerBase
    {
        private readonly RequestTypeService _requestTypeService;

        public RequestTypeController(RequestTypeService requestTypeService)
        {
            _requestTypeService = requestTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {


            var result = await _requestTypeService.GetAllRequestTypesAsync();
            if (result.IsSuccess)
            {

                return Ok(result.Data);
            }
            return BadRequest(result.Message);


        }
    }
}

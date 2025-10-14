using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTamer_Application.Service;

namespace TaskTamer_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {


            var result = await _employeeService.GetAllEmployeesAsync();
            if (result.IsSuccess)
            {

                return Ok(result.Data);
            }
            return BadRequest(result.Message);


        }


    }
}

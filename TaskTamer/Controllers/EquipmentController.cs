using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTamer_Application.Service;

namespace TaskTamer_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {

        private readonly EquipmentService _equipmentService;
        private readonly IConfiguration _configuration;
        public EquipmentController(EquipmentService equipmentService, IConfiguration configuration)
        {
            _equipmentService = equipmentService;
            _configuration = configuration;

        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _equipmentService.GetAllEquipmentAsync();
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        [HttpGet("GetVirtualFile/{id}")]
        public async Task<IActionResult> GetVirtualFile(int id)
        {
            var result = await _equipmentService.GetEquipmentByIdAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            var filepath = _configuration.GetValue<string>("FilePath");
            var filename = result.Data?.TechnicalDocumentation;
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(),filepath, filename);

            if (string.IsNullOrEmpty(fullPath))
            {
                return NotFound("Файл документации не найден");
            }

            var contentType = GetContentType(filename);

            return PhysicalFile(fullPath, contentType,filename);
        }

        private string GetContentType(string filename)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();

            var types = new Dictionary<string, string>
    {
        { ".txt", "text/plain" },
        { ".pdf", "application/pdf" },
        { ".doc", "application/vnd.ms-word" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".zip", "application/zip" },
        { ".rar", "application/vnd.rar" }
    };

            return types.TryGetValue(extension, out var contentType)
                ? contentType
                : "application/octet-stream";
        }

    }
}

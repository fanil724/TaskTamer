using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;

namespace TaskTamer_Admin.Controllers.Admin;

[Route("admin/[controller]")]
[Authorize(Roles = "Admin")]
public class EquipmentController : Controller
{
    private readonly EquipmentService _equipmentService;
    private readonly EmployeeService _employeeService;
    private readonly DepartmentService _departmentService;
    private readonly IConfiguration _configuration;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();


    public EquipmentController(EquipmentService equipmentService,
        EmployeeService employeeService,
        DepartmentService departmentService,
        IConfiguration configuration)
    {
        _equipmentService = equipmentService;
        _employeeService = employeeService;
        _departmentService = departmentService;
        _configuration = configuration;
    }
    [HttpGet]
    public async Task<IActionResult> Index(
        int employeeID, int departmentId,
        string location, string search)
    {
        var result = _equipmentService.GetAllEquipmentAsync().Result;
        if (!result.IsSuccess)
        {
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = result.Message;
            await PopulateViewBags();
            return View("~/Views/Admin/Equipment/Index.cshtml", new List<EquipmentDTO>());
        }
        var eq = result.Data;
        if (employeeID > 0)
        {
            eq = eq?.Where(x => x.ResponsibleEmployee.EmployeeID == employeeID);
        }
        if (departmentId > 0)
        {
            eq = eq?.Where(x => x.departmentDTO.DepartmentID == departmentId);
        }

        if (!string.IsNullOrEmpty(location))
        {
            eq = eq?.Where(x => x.Location.Contains(location));
        }

        if (!string.IsNullOrEmpty(search))
        {
            eq = eq?.Where(x => x.Location.Contains(search) || x.Model.Contains(search)
            || x.Name.Contains(search) || x.Type.Contains(search));
        }


        await PopulateViewBags();
        return View("~/Views/Admin/Equipment/Index.cshtml", eq);
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            if (id <= 0)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Неверный идентификатор оборудования";
                return RedirectToAction(nameof(Index));
            }

            var result = await _equipmentService.GetEquipmentByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/Equipment/Details.cshtml", result.Data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении оборудования с ID {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = "Ошибка при загрузке оборудования";
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            if (id <= 0)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Неверный идентификатор оборудования";
                return RedirectToAction(nameof(Index));
            }

            var result = await _equipmentService.GetEquipmentByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            await PopulateViewBags();
            return View("~/Views/Admin/Equipment/Edit.cshtml", result.Data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при загрузке оборудования для редактирования {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = "Ошибка при загрузке оборудования";
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EquipmentDTO equipmentDTO, IFormFile? documentationFile)
    {
        try
        {
            if (id <= 0 || equipmentDTO == null)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Неверные данные заявки";
                return RedirectToAction(nameof(Index));
            }

            if (id != equipmentDTO.EquipmentID)
            {
                ModelState.AddModelError("", "Несоответствие идентификаторов заявки");
            }

            var existingRequest = await _equipmentService.GetEquipmentByIdAsync(id);
            if (!existingRequest.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Оборудование не найдено";
                return RedirectToAction(nameof(Index));
            }

            // Обработка загруженного файла
            if (documentationFile != null && documentationFile.Length > 0)
            {
                // Проверка размера файла
                if (documentationFile.Length > 10 * 1024 * 1024) // 10MB
                {
                    ModelState.AddModelError("", "Файл слишком большой. Максимальный размер: 10MB");
                    await PopulateViewBags();
                    return View("~/Views/Admin/Equipment/Edit.cshtml", equipmentDTO);
                }

                // Проверка типа файла
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".txt" };
                var fileExtension = Path.GetExtension(documentationFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Недопустимый формат файла. Разрешены: PDF, Word, Excel, изображения");
                    await PopulateViewBags();
                    return View("~/Views/Admin/Equipment/Edit.cshtml", equipmentDTO);
                }
                equipmentDTO.TechnicalDocumentation = UploadDocumentation(documentationFile).Result;
            }
            else
            {
                equipmentDTO.TechnicalDocumentation = existingRequest.Data?.TechnicalDocumentation ?? "";
            }


            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["messageType"] = "warning";
                TempData["ErrorMessage"] = $"Ошибки валидации: {string.Join(", ", errors)}";

                await PopulateViewBags();
                return View("~/Views/Admin/Equipment/Edit.cshtml", equipmentDTO);
            }

            var updateResult = await _equipmentService.UpdateEquipmentAsync(equipmentDTO);

            if (updateResult.IsSuccess)
            {
                TempData["messageType"] = "success";
                TempData["SuccessMessage"] = "Оборудование успешно обновлено";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = updateResult.Message;

                await PopulateViewBags();
                return View("~/Views/Admin/Equipment/Edit.cshtml", equipmentDTO);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Критическая ошибка при обновлении оборудования {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = $"Произошла критическая ошибка при обновлении оборудования {ex.Message}";

            await PopulateViewBags();
            return View("~/Views/Admin/Equipment/Edit.cshtml", equipmentDTO);
        }
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            await PopulateViewBags();
            return View("~/Views/Admin/Equipment/Create.cshtml", new EquipmentDTO());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при загрузке формы для создания обрудования");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = "Ошибка при загрузке формы для создания обрудования";
            return RedirectToAction(nameof(Index));
        }
    }
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EquipmentDTO equipmentDto, IFormFile documentationFile)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBags();
                return View("~/Views/Admin/Equipment/Create.cshtml", equipmentDto);
            }

            
            if (documentationFile != null && documentationFile.Length > 0)
            {
                // Проверка размера файла
                if (documentationFile.Length > 10 * 1024 * 1024) // 10MB
                {
                    ModelState.AddModelError("", "Файл слишком большой. Максимальный размер: 10MB");
                    await PopulateViewBags();
                    return View("~/Views/Admin/Equipment/Create.cshtml", equipmentDto);
                }

                
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".txt" };
                var fileExtension = Path.GetExtension(documentationFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Недопустимый формат файла. Разрешены: PDF, Word, Excel, изображения");
                    await PopulateViewBags();
                    return View("~/Views/Admin/Equipment/Create.cshtml", equipmentDto);
                }
                equipmentDto.TechnicalDocumentation = UploadDocumentation(documentationFile).Result;
            }

            
            var result = await _equipmentService.CreateEquipmentAsync(equipmentDto);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Оборудование успешно создано";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.Message);
            await PopulateViewBags();
            return View("~/Views/Admin/Equipment/Create.cshtml", equipmentDto);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при создании оборудования");
            ModelState.AddModelError("", "Произошла ошибка при создании оборудования");
            await PopulateViewBags();
            return View("~/Views/Admin/Equipment/Create.cshtml", equipmentDto);
        }
    }

    [HttpGet("DownloadDocumentation/{equipmentId}")]
    public async Task<IActionResult> DownloadDocumentation(int equipmentId)
    {
        try
        {
            
            var equipmentResult = await _equipmentService.GetEquipmentByIdAsync(equipmentId);

            if (!equipmentResult.IsSuccess || equipmentResult.Data == null)
            {
                return NotFound("Оборудование не найдено");
            }

            var equipment = equipmentResult.Data;

            if (string.IsNullOrEmpty(equipment.TechnicalDocumentation))
            {
                return NotFound("Документация не найдена");
            }


            var fullPath = Path.Combine(_configuration.GetValue<string>("FilePath") ?? "", equipment.TechnicalDocumentation.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("Файл документации не найден");
            }

            var mimeType = GetMimeType(fullPath);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

            return File(fileBytes, mimeType, Path.GetFileName(fullPath));


        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при скачивании документации для оборудования {equipmentId}");
            return StatusCode(500, "Ошибка при скачивании документации");
        }
    }



    private async Task<string> UploadDocumentation(IFormFile documentationFile)
    {

        // Создание папки для документов если не существует
        var uploadsFolder = _configuration.GetValue<string>("FilePath") ?? Path.Combine(Environment.CurrentDirectory, "uploads", "documentation");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Генерация уникального имени файла
        var uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(documentationFile.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Сохранение файла
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await documentationFile.CopyToAsync(stream);
        }
        return uniqueFileName;
    }

    private string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();

        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".zip" => "application/zip",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private async Task PopulateViewBags()
    {
        try
        {
            var employeesResult = await _employeeService.GetAllEmployeesAsync();
            var departmentResult = await _departmentService.GetAllDepartmentsAsync();


            ViewBag.departments = new SelectList(
                departmentResult.IsSuccess
                    ? departmentResult.Data.Where(s => s.DepartmentID != null && !string.IsNullOrEmpty(s.Name))
                    : new List<RequestStatusDTO>(),
                "DepartmentID", "Name");

            ViewBag.ResponsibleEmployees = new SelectList(
                employeesResult.IsSuccess
                    ? employeesResult.Data.Where(e => e.EmployeeID != null && !string.IsNullOrEmpty(e.FullName))
                    : new List<EmployeeDTO>(),
                "EmployeeID", "FullName");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при заполнении ViewBag");
            // Устанавливаем пустые списки в случае ошибки
            ViewBag.ResponsibleEmployees = new SelectList(new List<EmployeeDTO>(), "EmployeeID", "FullName");
            ViewBag.departments = new SelectList(new List<DepartmentDTO>(), "DepartmentID", "Name");

        }
    }

}
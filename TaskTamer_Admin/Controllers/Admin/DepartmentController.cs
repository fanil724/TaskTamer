using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;

namespace TaskTamer_Admin.Controllers.Admin;

[Route("admin/[controller]")]
[Authorize(Roles = "Admin")]
public class DepartmentController : Controller
{
    private readonly DepartmentService _departmentService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public DepartmentController(DepartmentService departmentService)
    {
        _departmentService = departmentService;
    }
    [HttpGet]
    public IActionResult Index()
    {
        var result = _departmentService.GetAllDepartmentsAsync().Result;
        if (!result.IsSuccess)
        {
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = result.Message;
            return View("~/Views/Admin/Departament/Index.cshtml", new List<DepartmentDTO>());
        }
        return View("~/Views/Admin/Departament/Index.cshtml", result.Data);
    }
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            if (id <= 0)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Неверный идентификатор департамента";
                return RedirectToAction(nameof(Index));
            }

            var result = await _departmentService.GetDepartmentByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/Departament/Details.cshtml", result.Data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении департамента с ID {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = "Ошибка при загрузке департамента";
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
                TempData["ErrorMessage"] = "Неверный идентификатор департамента";
                return RedirectToAction(nameof(Index));
            }

            var result = await _departmentService.GetDepartmentByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/Departament/Edit.cshtml", result.Data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при загрузке департамента для редактирования {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = "Ошибка при загрузке департамента";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepartmentDTO departmentDTO)
    {
        try
        {
            if (id <= 0 || departmentDTO == null)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Неверные данные департамента";
                return RedirectToAction(nameof(Index));
            }

            if (id != departmentDTO.DepartmentID)
            {
                ModelState.AddModelError("", "Несоответствие идентификаторов департамента");
            }


            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["messageType"] = "warning";
                TempData["ErrorMessage"] = $"Ошибки валидации: {string.Join(", ", errors)}";
                return View("~/Views/Admin/Departament/Edit.cshtml", departmentDTO);
            }


            var existingRequest = await _departmentService.GetDepartmentByIdAsync(id);
            if (!existingRequest.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Департамент не найден";
                return RedirectToAction(nameof(Index));
            }


            var updateResult = await _departmentService.UpdateDepartmentAsync(departmentDTO);

            if (updateResult.IsSuccess)
            {
                TempData["messageType"] = "success";
                TempData["SuccessMessage"] = "Департамент успешно обновлена";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = updateResult.Message;                
                return View("~/Views/Admin/Departament/Edit.cshtml", departmentDTO);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Критическая ошибка при обновлении департамента {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = $"Произошла критическая ошибка при обновлении департамента {ex.Message}";           
            return View("~/Views/Admin/Departament/Edit.cshtml", departmentDTO);
        }

    }

        [HttpGet("Create")]
    public ActionResult Create()
    {
        var us = new DepartmentDTO();

        return View("~/Views/Admin/Departament/Create.cshtml", us);
    }




    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentDTO departmentDTO)
    {
        try
        {
            if (!ModelState.IsValid)
            {                
                return View("~/Views/Admin/Departament/Create.cshtml", departmentDTO);
            }
            var existingDepar = await _departmentService.GetDepartmentByNameAsync(departmentDTO.Name);
            if (existingDepar.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Департамент с таким именем уже есть";
                return RedirectToAction(nameof(Index));
            }

            var result = await _departmentService.CreateDepartmentAsync (departmentDTO);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                TempData["messageType"] = "warning";
                TempData["ErrorMessage"] = $"Ошибки валидации: {string.Join(", ", result.Message)}";
                return View("~/Views/Admin/Departament/Create.cshtml", departmentDTO);
            }


            var res = new RespMessage("success", result.Message);
            return RedirectToAction(nameof(Index), res);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при добавление департамента {departmentDTO.Name}");
           
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = $"Произошла критическая ошибка при обновлении департамента {ex.Message}";
            return View("~/Views/Admin/Departament/Create.cshtml", departmentDTO);
        }
    }


}
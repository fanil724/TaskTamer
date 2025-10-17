using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLog;
using TaskTamer_Admin.Models;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;

namespace TaskTamer_Admin.Controllers.Admin;

[Route("admin/[controller]")]
[Authorize(Roles = "Admin")]
public class RequestController : Controller
{
    private readonly RequestService _requestService;
    private readonly EmployeeService _employeeService;
    private readonly RequestStatusService _statusService;
    private readonly RequestTypeService _typeService;
    private readonly EquipmentService _equipmentService;
    private readonly UserService _userService;
    private readonly EmailSender _emailSender;

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public RequestController(
        RequestService requestService,
        EmployeeService employeeService,
        RequestStatusService statusService,
        RequestTypeService typeService,
        EquipmentService equipmentService,
        UserService userService,
        EmailSender emailSender
    )
    {
        _requestService = requestService;
        _employeeService = employeeService;
        _statusService = statusService;
        _typeService = typeService;
        _equipmentService = equipmentService;
        _userService = userService;
        _emailSender = emailSender;
    }


    // GET: Requests
    [HttpGet]
    public async Task<IActionResult> Index(int statusId, int priority, int executorId, string search, int period = 1)
    {
        try
        {
            var result = await _requestService.GetAllRequestsAsync();

            if (!result.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = result.Message;
                return View("~/Views/Admin/Request/Index.cshtml", new List<RequestDTO>());
            }

            var requests = result.Data;

            if (statusId > 0)
            {
                requests = requests?.Where(x => x.RequestStatus.StatusID == statusId).ToList();
            }

            if (priority > 0)
            {
                requests = requests?.Where(x => x.Priority == priority).ToList();
            }

            if (executorId > 0)
            {
                requests = requests?.Where(x => x.Executor.EmployeeID == executorId).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                requests = requests?.Where(x => x.ProblemDescription.Contains(search)).ToList();
            }

            if (period != 0)
            {
                requests = requests?.Where(x => x.CreationDate > DateTime.Now.AddMonths(-period)).ToList();
            }
            ViewBag.CurrentPeriod = period;
            

            await PopulateViewBags();
            return View("~/Views/Admin/Request/Index.cshtml", requests);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при получении списка заявок");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = "Ошибка при загрузке заявок";
            return View("~/Views/Admin/Request/Index.cshtml", new List<RequestDTO>());
        }
    }

    // GET: Requests/Details/5
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            if (id <= 0)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Неверный идентификатор заявки";
                return RedirectToAction(nameof(Index));
            }

            var result = await _requestService.GetRequestByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/Request/Detail.cshtml", result.Data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении заявки с ID {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = "Ошибка при загрузке заявки";
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
                TempData["ErrorMessage"] = "Неверный идентификатор заявки";
                return RedirectToAction(nameof(Index));
            }

            var result = await _requestService.GetRequestByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }


            await PopulateViewBags();
            return View("~/Views/Admin/Request/Edit.cshtml", result.Data);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при загрузке заявки для редактирования {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = "Ошибка при загрузке заявки";
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RequestDTO requestDto)
    {
        try
        {
            if (id <= 0 || requestDto == null)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Неверные данные заявки";
                return RedirectToAction(nameof(Index));
            }

            if (id != requestDto.RequestID)
            {
                ModelState.AddModelError("", "Несоответствие идентификаторов заявки");
            }

            // Отключаем валидацию для вложенных объектов, оставляем только ID
            ModelState.Clear();

            // Вручную добавляем валидацию только для необходимых полей
            if (string.IsNullOrEmpty(requestDto.ProblemDescription))
                ModelState.AddModelError("ProblemDescription", "Описание проблемы обязательно");

            if (requestDto.Priority < 1 || requestDto.Priority > 3)
                ModelState.AddModelError("Priority", "Приоритет должен быть от 1 до 3");

            if (requestDto.RequestStatus?.StatusID == 0)
                ModelState.AddModelError("RequestStatus.StatusID", "Статус обязателен");

            if (requestDto.Equipment?.EquipmentID == 0)
                ModelState.AddModelError("Equipment.EquipmentID", "Оборудование обязательно");

            if (requestDto.RequestType?.RequestTypeID == 0)
                ModelState.AddModelError("RequestType.RequestTypeID", "Тип заявки обязателен");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["messageType"] = "warning";
                TempData["ErrorMessage"] = $"Ошибки валидации: {string.Join(", ", errors)}";

                await PopulateViewBags();
                return View("~/Views/Admin/Request/Edit.cshtml", requestDto);
            }


            var existingRequest = await _requestService.GetRequestByIdAsync(id);
            if (!existingRequest.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Заявка не найдена";
                return RedirectToAction(nameof(Index));
            }

            var us = _userService.GetUserRoleAsync("Admin").Result;
            if (!us.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Пользователь не найден";
                return RedirectToAction(nameof(Index));
            }


            var updateResult = await _requestService.UpdateRequestAsync(requestDto, us.Data.UserID);
           
            if (updateResult.IsSuccess)
            {
                TempData["messageType"] = "success";
                TempData["SuccessMessage"] = "Заявка успешно обновлена";
                await _emailSender.SendNotificationAsync(requestDto.Executor.Email, $"Обновились данные заявки - {requestDto.RequestID}");
                await _emailSender.SendNotificationAsync(requestDto.Author.Email, $"Обновились данные заявки - {requestDto.RequestID}");

                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = updateResult.Message;

                await PopulateViewBags();
                return View("~/Views/Admin/Request/Edit.cshtml", requestDto);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Критическая ошибка при обновлении заявки {id}");
            TempData["messageType"] = "danger";
            TempData["ErrorMessage"] = $"Произошла критическая ошибка при обновлении заявки {ex.Message}";

            await PopulateViewBags();
            return View("~/Views/Admin/Request/Edit.cshtml", requestDto);
        }
    }

    private async Task PopulateViewBags()
    {
        try
        {
            var employeesResult = await _employeeService.GetAllEmployeesAsync();
            var statusesResult = await _statusService.GetAllRequestStatusAsync();
            var typesResult = await _typeService.GetAllRequestTypesAsync();
            var equipmentResult = await _equipmentService.GetAllEquipmentAsync();

            ViewBag.Statuses = new SelectList(
                statusesResult.IsSuccess
                    ? statusesResult.Data.Where(s => s.StatusID != null && !string.IsNullOrEmpty(s.Name))
                    : new List<RequestStatusDTO>(),
                "StatusID", "Name");

            ViewBag.Types = new SelectList(
                typesResult.IsSuccess
                    ? typesResult.Data.Where(t => t.RequestTypeID != null && !string.IsNullOrEmpty(t.Name))
                    : new List<RequestTypeDTO>(),
                "RequestTypeID", "Name");

            ViewBag.Equipment = equipmentResult.Data.Select(x => new ExtendedSelectListItem()
            {
                Value = x.EquipmentID.ToString(),
                Text = x.Name,
                DescriptionField = x.departmentDTO.DepartmentID.ToString(),
                DescriptionField2 = x.Location

            });

            ViewBag.Executors = employeesResult.Data.Select(e => new ExtendedSelectListItem()
            {
                Value = e.EmployeeID.ToString(),
                Text = e.FullName,
                DescriptionField = e.departmentDTO.DepartmentID.ToString(),
                DescriptionField2 = e.positionDTO.PositionID.ToString()
            });

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при заполнении ViewBag");
            // Устанавливаем пустые списки в случае ошибки
            ViewBag.Authors = new SelectList(new List<EmployeeDTO>(), "EmployeeID", "FullName");
            ViewBag.Statuses = new SelectList(new List<RequestStatusDTO>(), "StatusID", "Name");
            ViewBag.Types = new SelectList(new List<RequestTypeDTO>(), "TypeID", "Name");
            ViewBag.Equipment = new SelectList(new List<EquipmentDTO>(), "EquipmentID", "Name");
            ViewBag.Executors = new SelectList(new List<EmployeeDTO>(), "EmployeeID", "FullName");
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;

namespace TaskTamer_Admin.Controllers;
[Route("[controller]")]
[Authorize(Roles = "Manager,Admin")]
public class StatisticController : Controller
{

    private readonly RequestService _requestService;
    private readonly EmployeeService _employeeService;
    private readonly RequestStatusService _statusService;
    private readonly RequestTypeService _typeService;
    private readonly EquipmentService _equipmentService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public StatisticController(RequestService requestService,
        EmployeeService employeeService,
        RequestStatusService statusService,
        RequestTypeService typeService,
        EquipmentService equipmentService)
    {
        _requestService = requestService;
        _employeeService = employeeService;
        _statusService = statusService;
        _typeService = typeService;
        _equipmentService = equipmentService;
    }
    public async Task<IActionResult> Index(int statusId, int priority, int executorId, string search, int period = 1)
    {
        var result = _requestService.GetAllRequestsAsync().Result;
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
        var dataLabel = requests?.OrderBy(c => c.CreationDate)
            .Select(x => x.CreationDate.ToString("MMMM yyyy"))
            .Union(requests?.OrderBy(c => c.CompletionDate).Select(x => x.CompletionDate?.ToString("MMMM yyyy")))
            .Distinct().ToList();

        await PopulateViewBags();

        var data = new
        {
            labels = dataLabel,
            datasets = new[]
            {
                new
                {
                    label = "Созданные заявки",
                    data = dataLabel.Select(x=>requests.Count(c => c.CreationDate.ToString("MMMM yyyy") == x)).ToArray(),
                    backgroundColor = "rgba(54, 162, 235, 0.2)",
                    borderColor = "rgba(54, 162, 235, 1)",
                    borderWidth = 1
                },
                new
                {
                    label = "Выполненные заявки",
                    data =dataLabel.Select(x=>requests.Count(c => c.CompletionDate?.ToString("MMMM yyyy") == x)).ToArray(),
                    backgroundColor = "rgba(75, 192, 192, 0.2)",
                    borderColor = "rgba(75, 192, 192, 1)",
                    borderWidth = 1
                }

            }
        };

        TempData["dataToGrath"] = data;

        return View(requests);
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

            ViewBag.Equipment = new SelectList(
                equipmentResult.IsSuccess
                    ? equipmentResult.Data.Where(e => e.EquipmentID != null && !string.IsNullOrEmpty(e.Name))
                    : new List<EquipmentDTO>(),
                "EquipmentID", "Name");

            ViewBag.Executors = new SelectList(
                employeesResult.IsSuccess
                    ? employeesResult.Data.Where(e => e.EmployeeID != null && !string.IsNullOrEmpty(e.FullName))
                    : new List<EmployeeDTO>(),
                "EmployeeID", "FullName");
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
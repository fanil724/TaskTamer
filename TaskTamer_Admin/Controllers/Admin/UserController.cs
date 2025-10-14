using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;

namespace TaskTamer_Admin.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/[controller]")]
public class UserController : Controller
{
    private readonly UserService _userService;
    private readonly RoleService _roleService;
    private readonly DepartmentService _departmentService;
    private readonly EmployeeService _employeeService;
    private readonly PositionService _positionService;


    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public UserController(UserService userService, RoleService roleService, DepartmentService departmentService,
        EmployeeService employeeService, PositionService positionService)
    {
        _departmentService = departmentService;
        _roleService = roleService;
        _employeeService = employeeService;
        _userService = userService;
        _positionService = positionService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Index(RespMessage? resMessage)
    {
        if (resMessage != null)
        {
            ViewBag.Message = resMessage;
        }

        var us = _userService.GetAllAsync(false).Result;
        if (!us.IsSuccess) {
            ViewBag.Message = new RespMessage("danger",us.Message);
            return View("~/Views/Admin/User/Index.cshtml", new List<UserDTO>());
        }


        return View("~/Views/Admin/User/Index.cshtml", us.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("Details/{id}")]
    public IActionResult Details(int id)
    {
        var us = _userService.GetUserIdAsync(id).Result;
        if (!us.IsSuccess)
        {
            return RedirectToAction(nameof(Index),
                new RespMessage("danger", us.Message));
        }

        return View("~/Views/Admin/User/Details.cshtml", us.Data);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var us = _userService.GetUserIdAsync(id).Result;
        if (!us.IsSuccess)
        {
            return RedirectToAction(nameof(Index),
                new RespMessage("danger", us.Message));
        }
        await PopulateViewBags();
        return View("~/Views/Admin/User/Edit.cshtml", us.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserDTO model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBags();
                return View("~/Views/Admin/User/Edit.cshtml", model);
            }

            var result = await _userService.Update(model);
            if (!result.IsSuccess)
            {
                await PopulateViewBags();
                ModelState.AddModelError("", result.Message);
                return View("~/Views/Admin/User/Edit.cshtml", model);
            }


            var res = new RespMessage("success", result.Message);
            return RedirectToAction(nameof(Index), res);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при редактировании пользователя {model.Username}");
            var resMessage = new RespMessage("danger", "Произошла ошибка при обновлении данных");
            ViewBag.Message = resMessage;
            await PopulateViewBags();
            return View("~/Views/Admin/User/Edit.cshtml", model);
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var us = new UserDTO();

        await PopulateViewBags();

        return View("~/Views/Admin/User/Create.cshtml", us);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserDTO model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBags();
                return View("~/Views/Admin/User/Create.cshtml", model);
            }

            var result = await _userService.Create(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View("~/Views/Admin/User/Create.cshtml", model);
            }


            var res = new RespMessage("success", result.Message);
            return RedirectToAction(nameof(Index), res);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при добавление пользователя {model.Username}");
            var resMessage = new RespMessage("danger", "Произошла ошибка при добавление данных");
            ViewBag.Message = resMessage;
            return View("~/Views/Admin/User/Create.cshtml", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var res = await _userService.ChangeStatus(id);
        return Json(new
        {
            success = res.IsSuccess,
            isActive = res.Data.IsActive,
            message = res.Message
        });
    }


    private async Task PopulateViewBags()
    {
        var roles = _roleService.GetAllRolesAsync().Result;
        ViewBag.Roles = roles.Data.Select(x => new SelectListItem
            { Value = x.RoleID.ToString(), Text = x.Name });
        var pos = _positionService.GetAllPositionsAsync().Result;
        ViewBag.Position = pos.Data.Select(p => new SelectListItem
        {
            Value = p.PositionID.ToString(),
            Text = p.Title
        });
        var dep = _departmentService.GetAllDepartmentsAsync().Result;
        ViewBag.Depart = dep.Data.Select(x => new SelectListItem
        {
            Value = x.DepartmentID.ToString(),
            Text = x.Name
        });
    }
}

public record RespMessage(string messageType, string message);
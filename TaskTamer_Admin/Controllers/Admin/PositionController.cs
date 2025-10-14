using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Application.Service;

namespace TaskTamer_Admin.Controllers.Admin
{
    [Route("admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PositionController: Controller
    {
        private readonly PositionService _positionService;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public PositionController(PositionService positionService)
        {
            _positionService = positionService;           
        }
        [HttpGet]
        public IActionResult Index()
        {
            var result = _positionService.GetAllPositionsAsync().Result;
            if (!result.IsSuccess)
            {
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = result.Message;
                return View("~/Views/Admin/Position/Index.cshtml", new List<PositionDTO>());
            }
            return View("~/Views/Admin/Position/Index.cshtml", result.Data);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["messageType"] = "danger";
                    TempData["ErrorMessage"] = "Неверный идентификатор должности";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _positionService.GetPositionByIdAsync(id);

                if (!result.IsSuccess)
                {
                    TempData["messageType"] = "danger";
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Views/Admin/Position/Details.cshtml", result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении должности с ID {id}");
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Ошибка при загрузке должности";
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
                    TempData["ErrorMessage"] = "Неверный идентификатор должности";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _positionService.GetPositionByIdAsync(id);

                if (!result.IsSuccess)
                {
                    TempData["messageType"] = "danger";
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Views/Admin/Position/Edit.cshtml", result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при загрузке должности для редактирования {id}");
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = "Ошибка при загрузке должности";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PositionDTO positionDTO)
        {
            try
            {
                if (id <= 0 || positionDTO == null)
                {
                    TempData["messageType"] = "danger";
                    TempData["ErrorMessage"] = "Неверные данные должности";
                    return RedirectToAction(nameof(Index));
                }

                if (id != positionDTO.PositionID)
                {
                    ModelState.AddModelError("", "Несоответствие идентификаторов должности");
                }


                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    TempData["messageType"] = "warning";
                    TempData["ErrorMessage"] = $"Ошибки валидации: {string.Join(", ", errors)}";
                    return View("~/Views/Admin/Position/Edit.cshtml", positionDTO);
                }


                var existingRequest = await _positionService.GetPositionByIdAsync(id);
                if (!existingRequest.IsSuccess)
                {
                    TempData["messageType"] = "danger";
                    TempData["ErrorMessage"] = "Должность не найдена";
                    return RedirectToAction(nameof(Index));
                }


                var updateResult = await _positionService.UpdatePositionAsync(positionDTO);

                if (updateResult.IsSuccess)
                {
                    TempData["messageType"] = "success";
                    TempData["SuccessMessage"] = "Должность успешно обновлена";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["messageType"] = "danger";
                    TempData["ErrorMessage"] = updateResult.Message;
                    return View("~/Views/Admin/Position/Edit.cshtml", positionDTO);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Критическая ошибка при обновлении должности {id}");
                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = $"Произошла критическая ошибка при обновлении должности {ex.Message}";
                return View("~/Views/Admin/Position/Edit.cshtml", positionDTO);
            }

        }

        [HttpGet("Create")]
        public ActionResult Create()
        {
            var us = new PositionDTO();

            return View("~/Views/Admin/Position/Create.cshtml", us);
        }




        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PositionDTO positionDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Admin/Position/Create.cshtml", positionDTO);
                }
                var existingDepar = await _positionService.GetPositionByNameAsync(positionDTO.Title);
                if (existingDepar.IsSuccess)
                {
                    TempData["messageType"] = "danger";
                    TempData["ErrorMessage"] = "Должность с таким именем уже есть";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _positionService.CreatePositionAsync(positionDTO);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", result.Message);
                    TempData["messageType"] = "warning";
                    TempData["ErrorMessage"] = $"Ошибки валидации: {string.Join(", ", result.Message)}";
                    return View("~/Views/Admin/Position/Create.cshtml", positionDTO);
                }


                var res = new RespMessage("success", result.Message);
                return RedirectToAction(nameof(Index), res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при добавление должности {positionDTO.Title}");

                TempData["messageType"] = "danger";
                TempData["ErrorMessage"] = $"Произошла критическая ошибка при обновлении должности {ex.Message}";
                return View("~/Views/Admin/Position/Create.cshtml", positionDTO);
            }
        }

    }
}

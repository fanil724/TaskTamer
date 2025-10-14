using Microsoft.AspNetCore.Mvc;

namespace TaskTamer_Admin.Controllers;

[Route("admin/[controller]")]
public class AdminController :Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
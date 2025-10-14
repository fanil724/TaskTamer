using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace TaskTamer_Admin.Controllers;
[Route("[controller]")]
public class InfoController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
   

    public InfoController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
       
    }

    public async Task<IActionResult> Index()
    {
        List<IdentityUser> us = _userManager.GetUsersInRoleAsync("Admin").Result.ToList();
        return View(us);
    }
}
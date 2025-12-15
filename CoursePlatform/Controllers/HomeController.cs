using CoursePlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var popularCourses = await _db.Courses
            .Include(c => c.Teacher)
            .OrderByDescending(c => c.CreatedAt)
            .Take(3)
            .ToListAsync();

        return View(popularCourses);
    }
}
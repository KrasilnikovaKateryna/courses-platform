using CoursePlatform.Data;
using CoursePlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Controllers;

[Authorize]
public class CoursesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoursesController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var courses = await _db.Courses
            .Include(c => c.Teacher)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return View(courses);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet]
    public IActionResult Create() => View(new Course());

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError("Title", "Title is required");

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);

        model.TeacherId = user!.Id;
        model.CreatedAt = DateTime.UtcNow;

        _db.Courses.Add(model);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound();

        if (await CanManageCourse(course))
            return RedirectToAction(nameof(Edit), new { id });

        return View(course);
    }


    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        if (!await CanManageCourse(course))
            return RedirectToAction("AccessDenied", "Account");

        return View(course);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course model)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        if (!await CanManageCourse(course))
            return RedirectToAction("AccessDenied", "Account");

        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError("Title", "Title is required");

        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.TeacherId = course.TeacherId;
            return View(model);
        }

        course.Title = model.Title;
        course.Description = model.Description;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        if (!await CanManageCourse(course))
            return RedirectToAction("AccessDenied", "Account");

        return View(course);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        if (!await CanManageCourse(course))
            return RedirectToAction("AccessDenied", "Account");

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanManageCourse(Course course)
    {
        if (User.IsInRole("Admin")) return true;

        var user = await _userManager.GetUserAsync(User);
        return user != null && course.TeacherId == user.Id;
    }
}

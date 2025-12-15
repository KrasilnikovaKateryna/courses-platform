using CoursePlatform.Data;
using CoursePlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Controllers;

[Authorize]
public class EnrollmentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollmentsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        var user = await _userManager.GetUserAsync(User);

        var items = await _db.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c!.Teacher)
            .Where(e => e.StudentId == user!.Id)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        return View(items);
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var user = await _userManager.GetUserAsync(User);

        var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists) return NotFound();

        var already = await _db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == user!.Id);
        if (already)
        {
            TempData["EnrollMessage"] = "Already enrolled";
            return RedirectToAction("Index", "Courses");
        }

        _db.Enrollments.Add(new Enrollment
        {
            CourseId = courseId,
            StudentId = user!.Id,
            EnrolledAt = DateTime.UtcNow
        });

        try
        {
            await _db.SaveChangesAsync();
            TempData["EnrollMessage"] = "Enrolled successfully";
        }
        catch (DbUpdateException)
        {
            TempData["EnrollMessage"] = "Already enrolled";
        }

        return RedirectToAction("Index", "Courses");
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int courseId)
    {
        var user = await _userManager.GetUserAsync(User);

        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == user!.Id);

        if (enrollment == null)
        {
            TempData["EnrollMessage"] = "Not enrolled";
            return RedirectToAction(nameof(MyCourses));
        }

        _db.Enrollments.Remove(enrollment);
        await _db.SaveChangesAsync();

        TempData["EnrollMessage"] = "Unenrolled successfully";
        return RedirectToAction(nameof(MyCourses));
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet]
    public async Task<IActionResult> Students(int courseId)
    {
        var course = await _db.Courses.Include(c => c.Teacher).FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null) return NotFound();

        if (!User.IsInRole("Admin"))
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || course.TeacherId != user.Id)
                return RedirectToAction("AccessDenied", "Account");
        }

        var students = await _db.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Student!.Email)
            .ToListAsync();

        ViewBag.CourseTitle = course.Title;
        ViewBag.CourseId = courseId;
        return View(students);
    }
}

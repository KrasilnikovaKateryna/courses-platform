using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CoursePlatform.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
	[ValidateNever]
    public string TeacherId { get; set; } = "";
    public ApplicationUser? Teacher { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
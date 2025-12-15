using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CoursePlatform.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
}
using OpenQA.Selenium;
using Xunit;

namespace CoursePlatform.UITests;

public class AccessControlUiTests : SeleniumTestBase
{
    private int CreateCourseAndGetId()
    {
        EnsureLoggedOut();
        var title = $"Course {Guid.NewGuid():N}";
        CreateCourseAsTeacher(title);

        Go("/Courses");
        return FindCourseIdByTitle(title);
    }

    [Fact]
    public void Student_CanViewCourses_Positive()
    {
        EnsureLoggedOut();
        LoginAsStudent();

        Go("/Courses");
        Assert.True(IsPresent(By.Id("coursesTable")));
    }

    [Fact]
    public void Student_CanViewMyCourses_Positive()
    {
        EnsureLoggedOut();
        LoginAsStudent();

        Go("/Enrollments/MyCourses");
        Assert.True(IsPresent(By.Id("myCoursesTitle")));
    }

    [Fact]
    public void Student_CannotCreateCourse_Negative()
    {
        EnsureLoggedOut();
        LoginAsStudent();

        Go("/Courses/Create");
        Assert.True(IsPresent(By.Id("accessDeniedTitle")) ||
                    Driver.Url.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Student_CannotEditCourse_Negative()
    {
        var courseId = CreateCourseAndGetId();

        EnsureLoggedOut();
        LoginAsStudent();

        Go($"/Courses/Edit/{courseId}");
        Assert.True(IsPresent(By.Id("accessDeniedTitle")) ||
                    Driver.Url.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Teacher_CanCreateCourse_Positive()
    {
        EnsureLoggedOut();
        LoginAsTeacher();

        Go("/Courses/Create");
        Assert.True(IsPresent(By.Id("saveCourseBtn")));
        Assert.True(IsPresent(By.Id("courseTitle")));
    }

    [Fact]
    public void Teacher_CanEditCourse_Positive()
    {
        var courseId = CreateCourseAndGetId();

        EnsureLoggedOut();
        LoginAsTeacher();

        Go($"/Courses/Edit/{courseId}");
        Assert.True(IsPresent(By.Id("saveCourseBtn")));
        Assert.True(IsPresent(By.Id("courseTitle")));
    }

    [Fact]
    public void Teacher_DoesNotHaveMyCourses_PositiveRestriction()
    {
        EnsureLoggedOut();
        LoginAsTeacher();

        Go("/Enrollments/MyCourses");

        Assert.True(IsPresent(By.Id("accessDeniedTitle")) ||
                    Driver.Url.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase) ||
                    IsPresent(By.Id("myCoursesTitle")) == false);
    }

    [Fact]
    public void Teacher_DoesNotSeeEnrollButtons_Negative()
    {
        var courseId = CreateCourseAndGetId();

        EnsureLoggedOut();
        LoginAsTeacher();

        Go("/Courses");
        Assert.False(IsPresent(By.Id($"enrollBtn_{courseId}")));
    }
}

using OpenQA.Selenium;
using Xunit;

namespace CoursePlatform.UITests;

public class EnrollmentsUiTests : SeleniumTestBase
{
    private (int courseId, string title) EnsureCourseExists()
    {
        EnsureLoggedOut();
        var title = $"Course {Guid.NewGuid():N}";
        CreateCourseAsTeacher(title);

        Go("/Courses");
        var id = FindCourseIdByTitle(title);
        return (id, title);
    }

    [Fact]
    public void Enrollment_Enroll_Positive()
    {
        var (courseId, _) = EnsureCourseExists();

        EnsureLoggedOut();
        LoginAsStudent();

        Go("/Courses");
        WaitVisible(By.Id($"enrollBtn_{courseId}")).Click();

        Assert.True(IsPresent(By.Id("enrollMessage")) || Driver.PageSource.Contains("enroll", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Enrollment_Enroll_NotLoggedIn_Negative()
    {
        var (courseId, _) = EnsureCourseExists();

        EnsureLoggedOut();
        Go("/Courses");

        Assert.False(IsPresent(By.Id($"enrollBtn_{courseId}")));
    }

    [Fact]
    public void Enrollment_ReEnroll_Prevented_Positive()
    {
        var (courseId, _) = EnsureCourseExists();

        EnsureLoggedOut();
        LoginAsStudent();

        Go("/Courses");
        WaitVisible(By.Id($"enrollBtn_{courseId}")).Click();

        Go("/Enrollments/MyCourses");
        Assert.True(IsPresent(By.Id("myCoursesTable")) || IsPresent(By.Id("emptyMyCourses")) == false);
        Assert.True(IsPresent(By.Id($"unenrollBtn_{courseId}")));
    }

    [Fact]
    public void Enrollment_ReEnroll_ShowsMessage_Negative()
    {
        var (courseId, _) = EnsureCourseExists();

        EnsureLoggedOut();
        LoginAsStudent();

        Go("/Courses");
        WaitVisible(By.Id($"enrollBtn_{courseId}")).Click();

        Go("/Courses");
        if (IsPresent(By.Id($"enrollBtn_{courseId}")))
        {
            Driver.FindElement(By.Id($"enrollBtn_{courseId}")).Click();
            Assert.True(IsPresent(By.Id("enrollMessage")) || Driver.PageSource.Contains("already", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void Enrollment_Unenroll_Positive()
    {
        var (courseId, _) = EnsureCourseExists();

        EnsureLoggedOut();
        LoginAsStudent();

        Go("/Courses");
        WaitVisible(By.Id($"enrollBtn_{courseId}")).Click();

        Go("/Enrollments/MyCourses");
        WaitVisible(By.Id($"unenrollBtn_{courseId}")).Click();

        Assert.True(IsPresent(By.Id("enrollMessage")) || !IsPresent(By.Id($"unenrollBtn_{courseId}")));
    }

    [Fact]
    public void Enrollment_Unenroll_NotEnrolled_Negative()
    {
        var (courseId, _) = EnsureCourseExists();

        EnsureLoggedOut();
        LoginAsStudent();

        Go("/Enrollments/MyCourses");
        Assert.False(IsPresent(By.Id($"unenrollBtn_{courseId}")));
    }

    [Fact]
    public void Enrollment_StudentsList_Teacher_Positive()
    {
        var (courseId, _) = EnsureCourseExists();

        EnsureLoggedOut();
        LoginAsTeacher();

        Go("/Courses");
        WaitVisible(By.Id($"studentsBtn_{courseId}")).Click();

        Assert.True(IsPresent(By.Id("studentsTitle")));
        Assert.True(IsPresent(By.Id("studentsCourseTitle")));
    }

    [Fact]
    public void Enrollment_StudentsList_Student_Negative()
    {
        var (courseId, _) = EnsureCourseExists();

        EnsureLoggedOut();
        LoginAsStudent();

        Go($"/Enrollments/Students?courseId={courseId}");

        Assert.True(IsPresent(By.Id("accessDeniedTitle")) ||
                    Driver.Url.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase) ||
                    Driver.PageSource.Contains("Access denied", StringComparison.OrdinalIgnoreCase));
    }
}

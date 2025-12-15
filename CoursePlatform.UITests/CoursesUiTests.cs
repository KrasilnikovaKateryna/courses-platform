using OpenQA.Selenium;
using Xunit;

namespace CoursePlatform.UITests;

public class CoursesUiTests : SeleniumTestBase
{
    [Fact]
    public void Course_Create_Positive()
    {
        EnsureLoggedOut();
        var title = $"Course {Guid.NewGuid():N}";
        CreateCourseAsTeacher(title);

        Go("/Courses");
        WaitVisible(By.Id("coursesTable"));
        Assert.Contains(title, Driver.PageSource);
    }

    [Fact]
    public void Course_Create_EmptyTitle_Negative()
    {
        EnsureLoggedOut();
        LoginAsTeacher();

        Go("/Courses/Create");
        WaitVisible(By.Id("saveCourseBtn")).Click();

        WaitUrlContains("/Courses/Create");
        Assert.True(IsPresent(By.Id("validationSummary")) || Driver.PageSource.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Course_List_Positive()
    {
        EnsureLoggedOut();
        LoginAsTeacher();

        Go("/Courses");
        WaitVisible(By.Id("coursesTable"));
        Assert.True(IsPresent(By.Id("coursesTitle")));
    }

    [Fact]
    public void Course_Details_NotFound_Negative()
    {
        EnsureLoggedOut();
        LoginAsTeacher();

        Go("/Courses/Details/9999999");
        Assert.True(Driver.PageSource.Contains("404") ||
                    Driver.PageSource.Contains("Not Found", StringComparison.OrdinalIgnoreCase) ||
                    Driver.PageSource.Contains("не знайдено", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Course_Update_Positive()
    {
        EnsureLoggedOut();
        var title = $"Course {Guid.NewGuid():N}";
        CreateCourseAsTeacher(title);

        Go("/Courses");
        var id = FindCourseIdByTitle(title);

        Go($"/Courses/Edit/{id}");
        WaitVisible(By.Id("courseTitle")).Clear();
        var newTitle = $"Updated {Guid.NewGuid():N}";
        Driver.FindElement(By.Id("courseTitle")).SendKeys(newTitle);

        Driver.FindElement(By.Id("saveCourseBtn")).Click();
        WaitUrlContains("/Courses");

        Assert.Contains(newTitle, Driver.PageSource);
    }

    [Fact]
    public void Course_Update_EmptyTitle_Negative()
    {
        EnsureLoggedOut();
        var title = $"Course {Guid.NewGuid():N}";
        CreateCourseAsTeacher(title);

        Go("/Courses");
        var id = FindCourseIdByTitle(title);

        Go($"/Courses/Edit/{id}");
        WaitVisible(By.Id("courseTitle")).Clear();
        Driver.FindElement(By.Id("saveCourseBtn")).Click();

        WaitUrlContains($"/Courses/Edit/{id}");
        Assert.True(IsPresent(By.Id("validationSummary")) || Driver.PageSource.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Course_Delete_Positive()
    {
        EnsureLoggedOut();
        var title = $"Course {Guid.NewGuid():N}";
        CreateCourseAsTeacher(title);

        Go("/Courses");
        var id = FindCourseIdByTitle(title);

        Go($"/Courses/Delete/{id}");
        WaitVisible(By.Id("confirmDeleteBtn")).Click();

        WaitUrlContains("/Courses");
        Assert.DoesNotContain(title, Driver.PageSource);
    }

    [Fact]
    public void Course_Delete_NotFound_Negative()
    {
        EnsureLoggedOut();
        LoginAsTeacher();

        Go("/Courses/Delete/9999999");
        Assert.True(Driver.PageSource.Contains("404") ||
                    Driver.PageSource.Contains("Not Found", StringComparison.OrdinalIgnoreCase) ||
                    Driver.PageSource.Contains("не знайдено", StringComparison.OrdinalIgnoreCase));
    }
}

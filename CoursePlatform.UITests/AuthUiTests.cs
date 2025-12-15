using OpenQA.Selenium;
using Xunit;

namespace CoursePlatform.UITests;

public class AuthUiTests : SeleniumTestBase
{
    [Fact]
    public void Login_WithValidCredentials_Positive()
    {
        EnsureLoggedOut();
        LoginAsTeacher();
        Assert.True(IsPresent(By.Id("logoutBtn")));
    }

    [Fact]
    public void Login_WithInvalidCredentials_Negative()
    {
        EnsureLoggedOut();
        Login("teacher@demo.com", "wrong");
        WaitVisible(By.Id("loginError"));
        Assert.False(IsPresent(By.Id("logoutBtn")));
    }

    [Fact]
    public void ProtectedPage_WhenAuthorized_Positive()
    {
        EnsureLoggedOut();
        LoginAsTeacher();
        Go("/Courses");
        WaitUrlContains("/Courses");
        Assert.True(IsPresent(By.Id("coursesTable")));
    }

    [Fact]
    public void ProtectedPage_WhenNotAuthorized_Negative()
    {
        EnsureLoggedOut();
        Go("/Courses");
        WaitUrlContains("/Account/Login");
        Assert.True(IsPresent(By.Id("loginBtn")));
    }

    [Fact]
    public void Logout_Positive()
    {
        EnsureLoggedOut();
        LoginAsTeacher();
        Driver.FindElement(By.Id("logoutBtn")).Click();
        WaitVisible(By.Id("loginLink"));
        Assert.False(IsPresent(By.Id("logoutBtn")));
    }

    [Fact]
    public void Logout_WhenNotLoggedIn_Negative()
    {
        EnsureLoggedOut();
        Go("/");
        Assert.False(IsPresent(By.Id("logoutBtn")));
        Assert.True(IsPresent(By.Id("loginLink")));
    }

    [Fact]
    public void Login_StudentValid_Positive()
    {
        EnsureLoggedOut();
        LoginAsStudent();
        Assert.True(IsPresent(By.Id("logoutBtn")));
    }

    [Fact]
    public void Login_EmptyFields_Negative()
    {
        EnsureLoggedOut();
        Go("/Account/Login");
        Driver.FindElement(By.Id("loginBtn")).Click();

        Assert.True(IsPresent(By.Id("email")) && IsPresent(By.Id("password")));
    }
}

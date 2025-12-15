using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace CoursePlatform.UITests;

public abstract class SeleniumTestBase : IDisposable
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly string BaseUrl;

    protected SeleniumTestBase()
    {
        BaseUrl = "http://localhost:5106";

        var options = new ChromeOptions();
        options.AddArgument("--window-size=1400,900");
        options.AcceptInsecureCertificates = true;

        Driver = new ChromeDriver(options);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
    }

    protected void Go(string relativeUrl)
        => Driver.Navigate().GoToUrl($"{BaseUrl.TrimEnd('/')}{relativeUrl}");

    protected IWebElement WaitVisible(By by, int seconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200)
        };
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

        return wait.Until(d =>
        {
            var el = d.FindElement(by);
            return el.Displayed ? el : null;
        })!;
    }

    protected void WaitUrlContains(string part)
        => Wait.Until(_ => Driver.Url.Contains(part, StringComparison.OrdinalIgnoreCase));

    protected bool IsPresent(By by) => Driver.FindElements(by).Count > 0;

    protected void EnsureLoggedOut()
    {
        Go("/Account/Login");

        WaitVisible(By.Id("loginBtn"), 10);
    }



    protected void Login(string email, string password)
    {
        Go("/Account/Login");

        var emailInput = WaitVisible(By.Id("email"));
        emailInput.Clear();
        emailInput.SendKeys(email);

        var passwordInput = Driver.FindElement(By.Id("password"));
        passwordInput.Clear();
        passwordInput.SendKeys(password);

        Driver.FindElement(By.Id("loginBtn")).Click();
    }


    protected void LoginAsTeacher()
    {
        Login("teacher@demo.com", "Test@123JesusGayHrenkAtIA*bi");
        WaitVisible(By.Id("logoutBtn"));
    }

    protected void LoginAsStudent()
    {
        Login("student@demo.com", "Test@123JesusGayHrenkAtIA*bi");
        WaitVisible(By.Id("logoutBtn"));
    }
    
    protected void GoCourses()
    {
        Go("/Courses");
        WaitVisible(By.Id("coursesTitle"));
    }

    protected void CreateCourseAsTeacher(string title, string description = "Auto-generated description")
    {
        LoginAsTeacher();

        Go("/Courses/Create");

        WaitVisible(By.Id("createCourseTitle"));

        Driver.FindElement(By.Id("courseTitle")).Clear();
        Driver.FindElement(By.Id("courseTitle")).SendKeys(title);

        Driver.FindElement(By.Id("courseDescription")).Clear();
        Driver.FindElement(By.Id("courseDescription")).SendKeys(description);

        Driver.FindElement(By.Id("saveCourseBtn")).Click();

        if (IsElementPresent(By.Id("courseError")))
            throw new InvalidOperationException("Course creation failed: courseError is shown.");

        WaitVisible(By.Id("coursesTitle"), 10);
    }


    protected int FindCourseIdByTitle(string title)
    {
        GoCourses();

        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200)
        };
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

        var row = wait.Until(d =>
        {
            var rows = d.FindElements(By.CssSelector("#coursesTable tbody tr"));
            foreach (var r in rows)
            {
                var cell = r.FindElement(By.CssSelector("td.course-title"));
                if (cell.Text.Trim().Equals(title, StringComparison.Ordinal))
                    return r;
            }
            return null;
        });

        if (row == null)
            throw new InvalidOperationException($"Course row with title '{title}' not found on page.");

        var idAttr = row.GetAttribute("data-course-id");
        if (!int.TryParse(idAttr, out var courseId))
            throw new InvalidOperationException($"Invalid data-course-id='{idAttr}' for course '{title}'.");

        return courseId;
    }


    public void Dispose()
    {
        try { Driver.Quit(); } catch { }
        Driver.Dispose();
    }
    
    protected bool IsElementPresent(By by)
    {
        try
        {
            Driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

}

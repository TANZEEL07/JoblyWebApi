using JoblyWebApi.Repositories;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

public class NaukriApplyEngine
{
    private readonly string _email;
    private readonly string _password;
    private readonly int _userId;

    public NaukriApplyEngine(string email, string password, int userId)
    {
        _email = email;
        _password = password;
        _userId = userId;
    }

    public async void Run(string role, string location, string skills)
    {
        var options = new ChromeOptions();
        options.AddArgument("--window-size=1920,1080");

        using var driver = new ChromeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));

        try
        {
            // Login
            driver.Navigate().GoToUrl("https://login.naukri.com/nLogin/Login.php");
            Thread.Sleep(3000);

            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("usernameField"))).SendKeys(_email);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("passwordField"))).SendKeys(_password);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[type='submit']"))).Click();
            Console.WriteLine("✅ Logged in successfully");
            Thread.Sleep(5000);

            // Go to Homepage -> View All Jobs
            driver.Navigate().GoToUrl("https://www.naukri.com/mnjuser/homepage");
            Thread.Sleep(4000);
            var viewAll = driver.FindElements(By.CssSelector("a.view-all-link")).FirstOrDefault();
            if (viewAll != null)
            {
                viewAll.Click();
                Console.WriteLine("🔗 Clicked on 'View All'");
                Thread.Sleep(4000);
            }

            string[] tabIds = { "apply", "profile", "top_candidate", "preference", "similar_jobs" };
            int appliedCount = 0;

            foreach (var tabId in tabIds)
            {
                if (appliedCount >= 5) break;

                

                try
                {
                    var tab = driver.FindElements(By.CssSelector($"div.tab-wrapper#{tabId}")).FirstOrDefault();
                    if (tab != null)
                    {
                        tab.Click();
                        Console.WriteLine($"🟢 Tab Clicked: {tabId}");
                        Thread.Sleep(4000);

                        var jobs = driver.FindElements(By.CssSelector("article.jobTuple"));
                        Console.WriteLine($"🔍 Found {jobs.Count} jobs in '{tabId}'");
                        string originalWindow = driver.CurrentWindowHandle;

                        for (int i = 0; i < jobs.Count; i++)
                        {
                            var job = jobs[i];

                            await ApplyJob(job, driver, wait);

                            // After ApplyJob completes, you should be back to original window
                            driver.SwitchTo().Window(originalWindow);
                            Thread.Sleep(1000);  // small pause if needed
                            if (i == 5) break;
                        }

                        //await Task.WhenAll(tasks);
                    }
                }
                catch (Exception exTab)
                {
                    Console.WriteLine($"❌ Tab error [{tabId}]: {exTab.Message}");
                }
            }

            Console.WriteLine($"✅ Done: {appliedCount} jobs applied.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Main Error: " + ex.Message);
        }
        finally
        {
            driver.Quit();
        }
    }

    public async Task ApplyJob(IWebElement job, ChromeDriver driver, WebDriverWait wait)
    {
        try
        {
            bool hasCheckbox = job.FindElements(By.CssSelector(".tuple-check-box i.naukicon-ot-checkbox")).Any();
            if (!hasCheckbox)
            {
                Console.WriteLine("⚠️ Skipping job without checkbox");
                return;
            }

            var titleElem = job.FindElement(By.CssSelector("p.title"));
            string title = titleElem.Text;
            string company = job.FindElement(By.CssSelector("span.companyWrapper span[title]"))?.Text ?? "";
            string loc = job.FindElement(By.CssSelector("li.location span"))?.Text ?? "";

            titleElem.Click();
            Console.WriteLine($"🔗 Opened job: {title}");
            await Task.Delay(5000);

            string originalWindow = driver.CurrentWindowHandle;
            wait.Until(driver => driver.WindowHandles.Count > 1);

            foreach (var window in driver.WindowHandles)
            {
                if (window != originalWindow)
                {
                    driver.SwitchTo().Window(window);
                    Console.WriteLine("🪟 Switched to job tab");
                    break;
                }
            }

            wait.Until(ExpectedConditions.ElementExists(By.Id("job_header")));

            var applyButtons = driver.FindElements(By.Id("apply-button"));
            if (applyButtons.Count == 0)
            {
                Console.WriteLine("⚠️ Apply button not found, skipping.");
                return;
            }

            var applyBtn = applyButtons[0];

            // Optionally check if clickable
            if (!applyBtn.Displayed || !applyBtn.Enabled)
            {
                Console.WriteLine("⚠️ Apply button is not clickable, skipping.");
                return;
            }
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", applyBtn);
            await Task.Delay(500);

            applyBtn.Click();
            Console.WriteLine("✅ Clicked Apply");

            await Task.Delay(2000);

            new AppliedJobRepository().Save(new AppliedJob
            {
                UserId = _userId,
                JobTitle = title,
                Company = company,
                Location = loc,
                AppliedAt = DateTime.Now
            });

            Console.WriteLine($"💾 Applied to: {title}");

            driver.Close(); // Close the job tab
            driver.SwitchTo().Window(originalWindow);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error in ApplyJob: " + ex.Message);
        }
    }

}

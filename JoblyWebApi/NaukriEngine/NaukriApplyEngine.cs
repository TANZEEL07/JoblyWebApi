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

    public void Run(string role, string location, string skills)
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

                        foreach (var job in jobs)
                        {
                            if (appliedCount >= 5) break;

                            bool hasCheckbox = job.FindElements(By.CssSelector(".tuple-check-box i.naukicon-ot-checkbox")).Any();
                            if (!hasCheckbox)
                            {
                                Console.WriteLine("⚠️ Skipping job without checkbox");
                                continue;
                            }

                            try
                            {
                                var titleElem = job.FindElement(By.CssSelector("p.title"));
                                string title = titleElem.Text;
                                string company = job.FindElement(By.CssSelector("span.companyWrapper span[title]"))?.Text ?? "";
                                string loc = job.FindElement(By.CssSelector("li.location span"))?.Text ?? "";

                                titleElem.Click();
                                Console.WriteLine($"🔗 Opened job: {title}");
                                Thread.Sleep(5000);

                                try
                                {
                                    string originalWindow = driver.CurrentWindowHandle;
                                    wait.Until(driver => driver.WindowHandles.Count > 1);

                                    foreach (var window in driver.WindowHandles)
                                    {
                                        if (window != originalWindow)
                                        {
                                            driver.SwitchTo().Window(window);
                                            Console.WriteLine("🪟 Switched to new job detail window/tab");
                                            break;
                                        }
                                    }

                                    wait.Until(ExpectedConditions.ElementExists(By.Id("job_header")));

                                    // Optional: dump the DOM to inspect
                                    File.WriteAllText($"debug_dom_{DateTime.Now:HHmmss}.html", driver.PageSource);

                                    // Wait for Apply button using XPath
                                    var applyBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='apply-button']")));
                                    Console.WriteLine("✅ Found Apply button");

                                    // 🎯 Scroll it into view smoothly
                                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", applyBtn);
                                    Thread.Sleep(500);

                                    bool clicked = false;
                                    for (int i = 0; i < 2; i++)  // Try normal and JS fallback
                                    {
                                        try
                                        {
                                            applyBtn.Click();
                                            clicked = true;
                                            Console.WriteLine("✅ Apply clicked (normal)");
                                            break;
                                        }
                                        catch (Exception clickEx)
                                        {
                                            Console.WriteLine($"⚠️ Click failed: {clickEx.Message}, trying JS click...");
                                            applyBtn = driver.FindElement(By.XPath("//button[contains(@class,'apply-button')]")); // Re-locate (stale fix)
                                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", applyBtn);
                                            clicked = true;
                                            Console.WriteLine("✅ Apply clicked (JS fallback)");
                                            break;
                                        }
                                    }

                                    if (!clicked)
                                    {
                                        Console.WriteLine("❌ Failed to click Apply button.");
                                        continue;
                                    }

                                    Thread.Sleep(2000);

                                    new AppliedJobRepository().Save(new AppliedJob
                                    {
                                        UserId = _userId,
                                        JobTitle = title,
                                        Company = company,
                                        Location = loc,
                                        AppliedAt = DateTime.Now
                                    });

                                    appliedCount++;
                                    Console.WriteLine($"💾 Applied #{appliedCount}: {title}");
                                }
                                catch (Exception exApply)
                                {
                                    Console.WriteLine("❌ Apply Click Failed: " + exApply.Message);
                                    string path = Path.Combine(Directory.GetCurrentDirectory(), $"error_apply_click_{DateTime.Now:HHmmss}.png");
                                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                                    Console.WriteLine("📸 Screenshot saved: " + path);
                                }

                                try
                                {
                                    driver.Navigate().Back();
                                    Thread.Sleep(3000);
                                }
                                catch
                                {
                                    Console.WriteLine("⚠️ Failed to go back. Refreshing...");
                                    driver.Navigate().Refresh();
                                    Thread.Sleep(5000);
                                }
                            }
                            catch (Exception exJob)
                            {
                                Console.WriteLine("❌ Job processing error: " + exJob.Message);
                                try { driver.Navigate().Back(); Thread.Sleep(3000); } catch { }
                            }
                        }
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
}

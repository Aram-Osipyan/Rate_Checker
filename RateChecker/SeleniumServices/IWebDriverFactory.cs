using OpenQA.Selenium;

namespace RateChecker.SeleniumServices;
public interface IWebDriverFactory
{
    IWebDriver Get();
}
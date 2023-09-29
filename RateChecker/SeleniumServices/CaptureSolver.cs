using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using RestSharp;

namespace RateChecker.SeleniumServices;
public class CaptureSolver
{
    public async Task Solve(IWebDriver driver)
    {
        try
        {
            #region captcha_solving

            var captureDiv = driver.FindElement(By.CssSelector("div.bcap-image-area-container"));
            var takeScreenshot = ((ITakesScreenshot)captureDiv).GetScreenshot();
            var captureLabel = driver.FindElement(By.CssSelector("div#tagLabel")).Text;
            var screenshot = takeScreenshot.AsBase64EncodedString;

            var key = "e4df5792b0ba58cf3c93db834805ab68";

            var client = new RestClient("http://api.captcha.guru");
            var request = new RestRequest("in.php", Method.Post);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("textinstructions", $"binance,{captureLabel}");
            request.AddParameter("click", "geetest");
            request.AddParameter("key", key);
            request.AddParameter("method", "base64");
            request.AddParameter("body", screenshot);

            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);

            await Task.Delay(10000);
            var rt = response.Content?.Split('|')[1];

            var url = $"res.php?key={key}&id={rt}";

            var req = new RestRequest(url, Method.Get);

            var result = await client.ExecuteAsync(req);
            Console.WriteLine(result.Content);


            if (result.IsSuccessful && result.Content is not null)
            {
                // OK|coordinates:x=56,y=56;x=170,y=56
                var coordinates = result.Content.Split(':')[1].Split(';').Select(coord =>
                {
                    var xy = coord.Split(',');

                    var x = int.Parse(xy[0].Split('=')[1]);
                    var y = int.Parse(xy[1].Split('=')[1]);

                    return new { x = x, y = y };
                });
                var captureSolver = new Actions(driver);

                foreach (var coord in coordinates)
                {
                    captureSolver
                        .MoveToElement(captureDiv)
                        .MoveByOffset(-(captureDiv.Size.Width / 2), -(captureDiv.Size.Height / 2))
                        .MoveByOffset(coord.x, coord.y)
                        .Click()
                        .Build()
                        .Perform();

                    await Task.Delay(100);
                }

                var captureButton = driver.FindElement(By.CssSelector("div.bcap-verify-button"));
                captureButton.Click();
            }

            #endregion captcha_solving
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

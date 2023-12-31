﻿using Docker.DotNet.Models;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;
using RateChecker.Common;
using RateChecker.StateMachine;
using RestSharp;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace RateChecker.SeleniumServices.States;
public class CaptureSolving : State<StateMachineContext, TriggerEnum, StateEnum>
{
    public CaptureSolving(StateEnum stateEnum) : base(stateEnum)
    {
    }

    public override async Task<TriggerEnum> Perform(StateMachineContext context)
    {
        try
        {
            var driver = context.Driver;
            await Task.Delay(3000);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            var captureDiv = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.bcap-image-area-container")));
            var takeScreenshot = ((ITakesScreenshot)captureDiv).GetScreenshot();
            var captureLabel = wait.Until(ExpectedConditions.ElementToBeClickable(driver.FindElement(By.CssSelector("div#tagLabel")))).Text;
            var screenshot = takeScreenshot.AsBase64EncodedString;

            //var key = "e4df5792b0ba58cf3c93db834805ab68";
            var key = context.Input.CaptureSolverKey;
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

                    await Task.Delay(500);
                }

                var captureButton = wait.Until(ExpectedConditions.ElementToBeClickable(driver.FindElement(By.CssSelector("div.bcap-verify-button"))));
                captureButton.Click();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return TriggerEnum.CaptureNotFound;
        }


        // reload driver page and Password entering
        await Task.Delay(5000);

        return TriggerEnum.Success;
    }
}

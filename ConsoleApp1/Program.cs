using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using System.Xml.Linq;
using System;
using ConsoleApp1;
using OtpNet;
using System.Text;
using System.Net;
using Google.Protobuf;
using System;
using System.IO;
using MailKit.Net.Imap;
using MailKit;
using Org.BouncyCastle.Crypto.Prng;
using MailKit.Search;
using HtmlAgilityPack;
using System.Drawing;
using RestSharp;
using System.Text.RegularExpressions;

/*
var secretKey = "e7JT+GoUeGvk6g==";

var bytes = Convert.FromBase64String(secretKey);


//byte[] base32Bytes = Base32Encoding.ToBytes(secretKey);
var totp = new Totp(bytes);
var totpCode = totp.ComputeTotp();


*/


/*

}*/



var options = new ChromeOptions();
//options.BrowserVersion = "114.0";
var userAgent = "Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25";
options.AddAdditionalOption("selenoid:options", new Dictionary<string, object>
{
    /* How to add test badge */
    ["name"] = $"{DateTime.Now}",

    /* How to set session timeout */
    ["sessionTimeout"] = "15m",

    /* How to set timezone */
    ["env"] = new List<string>() {
                "TZ=UTC"
            },

    /* How to add "trash" button */
    ["labels"] = new Dictionary<string, object>
    {
        ["manual"] = "true"
    },
    ["enableVNC"] = true,
});

options.AddArguments(new List<string>() { "no-sandbox", "disable-gpu" });
options.AddArgument("--disable-blink-features=AutomationControlled");
options.AddArgument($"--user-agent={userAgent}");
options.AddAdditionalChromeOption("useAutomationExtension", false);

// IWebDriver driver = await Task.Run(() => new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), options.ToCapabilities(), TimeSpan.FromMinutes(2)));
IWebDriver driver = new ChromeDriver(options);

await Task.Run(() => driver.Navigate().GoToUrl("https://accounts.binance.com/en/login?"));


await Task.Delay(2000);

var cookieButton = driver.FindElement(By.CssSelector("button#onetrust-accept-btn-handler"));




cookieButton.Submit();


await Task.Delay(500);


// email entering

var emailInput = driver.FindElement(By.CssSelector("input#username"));
var email = "binancenoviy@gmail.com";


emailInput.SendKeys(email);


await Task.Delay(4000);
var nextButton = driver.FindElement(By.CssSelector("button#click_login_submit"));

nextButton.Click();


await Task.Delay(3000);


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

            await Task.Delay(1000);
        }

        var captureButton = driver.FindElement(By.CssSelector("div.bcap-verify-button"));
        captureButton.Click();
    }

    #endregion captcha_solving
}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}


// reload driver page and Password entering
await Task.Delay(5000);

driver.SwitchTo().Window(driver.CurrentWindowHandle);
var passwordInput = driver.FindElement(By.CssSelector("input[name=\"password\"]"));
var password = "28500Rama56";

passwordInput.SendKeys(password);

var loginSubmitBtn = driver.FindElement(By.CssSelector("button#click_login_submit"));
loginSubmitBtn.Click();

await Task.Delay(5000);

// authenticator entering
driver.SwitchTo().Window(driver.CurrentWindowHandle);
var authenticatorButton = driver.FindElement(By.CssSelector(".bn-mfa-overview-step-wrapper .bn-mfa-overview-step:first-child"));

authenticatorButton.Click();


var authenticatorInput = driver.FindElement(By.CssSelector("input.bn-textField-input"));

    var secretKey = "e7JT+GoUeGvk6g==";

    var bytes = Convert.FromBase64String(secretKey);


    var totp = new Totp(bytes);
    var totpCode = totp.ComputeTotp();
await Task.Delay(1000);
authenticatorInput.SendKeys(totpCode);


// email entering
await Task.Delay(1000);
var emailButton = driver.FindElement(By.CssSelector(".bn-mfa-overview-step-wrapper .bn-mfa-overview-step:nth-child(2)"));
var now = DateTime.Now;
emailButton.Click();

await Task.Delay(5000);
var emailCodeInput = driver.FindElement(By.CssSelector("input.bn-textField-input"));

/// IMAP
string messageHtml = "";
    using (var clientp = new ImapClient())
    {
        clientp.Connect("imap.gmail.com", 993, true);
        clientp.Authenticate("binancenoviy@gmail.com", "ttxo bfhg yzlu kbxm");

        var inbox = clientp.Inbox;
        inbox.Open(FolderAccess.ReadOnly);
        
        var query = SearchQuery.FromContains("Binance")
            .And(SearchQuery.SubjectContains("Подтверждение входа"))
            .And(SearchQuery.DeliveredAfter(now));
        Console.WriteLine("Total messages: {0}", inbox.Count);
        Console.WriteLine("Recent messages: {0}", inbox.Recent);

        foreach (var uid in inbox.Search(query))
        {
            var message = inbox.GetMessage(uid);
            Console.WriteLine("[match] {0}: {1}", uid, message.Subject);
            Console.WriteLine("[body] {0}: {1}", uid, message.HtmlBody);
            messageHtml = message.HtmlBody;
        }

        clientp.Disconnect(true);
    }
/// /IMAP

string pattern = @"<b>\s*(\d\d\d\d\d\d)\s*<\/b>";
var emailCode = Regex.Match(messageHtml, pattern).Groups.Values.Skip(1).First().Value;

emailCodeInput.SendKeys(emailCode);

await Task.Delay(10000);


var yesBtn = driver.FindElement(By.XPath("//button[text()=\"Yes\"]"));
yesBtn.Click();


await driver.Manage().Network.StartMonitoring();

string token = "";
string cookie = "";
var ts = new TaskCompletionSource<(string token, string cookie)>();

driver.Manage().Network.NetworkRequestSent += (obj, req) =>
{
    if (req.RequestUrl == "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search")
    {
        token = req.RequestHeaders["csrftoken"];
        cookie = req.RequestHeaders["Cookie"];

        ts.SetResult((token, cookie));
    }

};



driver.Navigate().GoToUrl("https://p2p.binance.com/ru/trade/all-payments/USDT?fiat=RUB");

await ts.Task;

Console.WriteLine(token);
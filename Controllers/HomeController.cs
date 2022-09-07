using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using shorten_url.Models;
using shorten_url.Repositories;

namespace shorten_url.Controllers;

public class HomeController : Controller
{


    private readonly ILogger<HomeController> _logger;
    private readonly IUrlRepository _repository;

    public HomeController(IUrlRepository repository, ILogger<HomeController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger;
    }


    [Route("/")]
    public async Task<IActionResult>Index()
    {
        //string sessionID = HttpContext.Session.Id;

        //var urls = await _repository.GetURLs();

        //ViewBag.urls = urls;

        ViewBag.lastLongURL = TempData["lastLongURL"];
        ViewBag.lastShortURL = TempData["lastShortURL"];



        return View();
    }

    [HttpGet("/{shortUrlCode}")]
    public async Task<IActionResult> Index(string shortUrlCode)
    {

        ViewBag.lastLongURL = TempData["lastLongURL"];
        ViewBag.lastShortURL = TempData["lastShortURL"];

        Console.WriteLine("Directing to URL with code: "  + shortUrlCode);

        try
        {
            ShortenedURL url = await _repository.GetLongURLByCode(shortUrlCode);
            if (url == null)
            {
                //ErrorViewModel model = new ErrorViewModel();
                return View("NotFoundError");
            }

            return Redirect(url.LongURL);
        }
        catch(TimeoutException te)
        {
            Console.WriteLine("Request Timed Out: " + te.Message);
            return View("TimeoutError");
        }
        catch(Exception e)
        {
            Console.WriteLine("Unknown Error Occured: " + e.Message);
            ErrorViewModel model = new ErrorViewModel();
            return View("Error");
        }

        

        
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }



    //Shorten Routes

    [HttpPost]
    public async Task<IActionResult> CreateShortenedURL(string longUrl)
    {
        //add validation to ensure URL is valid
        HttpClient client = new HttpClient();

        ViewBag.invalidURL = false;

        try
        {
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, longUrl));
            //if (response.StatusCode != HttpStatusCode.OK)
            //{
            //    //Invalid URL
            //    ViewBag.invalidURL = true;
            //    return View();
            //}
        }
        catch
        {
            //Invalid URL
            ViewBag.invalidURL = true;
            return View("Index");
        }

        

        string currentUrl = HttpContext.Request.Host.ToString();
        var scheme = HttpContext.Request.Scheme;

        ShortenedURL url;
        long counter;
        //check if url already exists in db
        try
        {
            Console.WriteLine("Checking if Url already exists.");
            url = await _repository.GetShortenedURLByLongURL(longUrl);
            if (url != null)
            {
                Console.WriteLine("URL already exists at " + url.ShortURL);
                TempData["lastLongURL"] = url.LongURL;
                TempData["lastShortURL"] = url.ShortURL;
                return RedirectToAction("Index");
            }
        }
        catch (TimeoutException te)
        {
            Console.WriteLine("Request Timed Out: " + te.Message);
            return View("TimeoutError");
        }
        catch (Exception e)
        {
            Console.WriteLine("Unknown Error Occured: " + e.Message);
            ErrorViewModel model = new ErrorViewModel();
            return View("Error");
        }


        try
        {
            Console.WriteLine("Getting Next Sequence Value from DB.");
            counter = await _repository.GetNextSequenceVal("counter");
        }
        catch (TimeoutException te)
        {
            Console.WriteLine("Request Timed Out: " + te.Message);
            return View("TimeoutError");
        }
        catch (Exception e)
        {
            Console.WriteLine("Unknown Error Occured: " + e.Message);
            ErrorViewModel model = new ErrorViewModel();
            return View("Error");
        }


        string shortCode = WebEncoders.Base64UrlEncode(BitConverter.GetBytes(counter));
        string shortUrl = scheme + "://" + currentUrl + "/" + shortCode;
        string sessionID = HttpContext.Session.Id;
        url = new ShortenedURL
        {
            ShortURL = shortUrl,
            LongURL = longUrl,
            counter = counter,
            SessionID = sessionID,
            ShortCode = shortCode
        };

        try
        {
            Console.WriteLine("Adding Url ('" + url.LongURL + "')");
            await _repository.CreateShortenedURL(url);
            Console.WriteLine("URL added at: " + url.ShortURL);
        }
        catch (TimeoutException te)
        {
            Console.WriteLine("Request Timed Out: " + te.Message);
            return View("TimeoutError");
        }
        catch (Exception e)
        {
            Console.WriteLine("Unknown Error Occured: " + e.Message);
            ErrorViewModel model = new ErrorViewModel();
            return View("Error");
        }

        //ViewBag.lastURL = url;

        TempData["lastLongURL"] = url.LongURL;
        TempData["lastShortURL"] = url.ShortURL;

        //return CreatedAtRoute("GetShortenedURL", new { id = url.Id }, url);
        //return View("Index");

        return RedirectToAction("Index");
    }
}

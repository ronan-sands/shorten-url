using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
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
    public IActionResult Index()
    {
        ViewBag.lastLongURL = TempData["lastLongURL"];
        ViewBag.lastShortURL = TempData["lastShortURL"];
        return View();
    }

    [Route("/error")]
    public IActionResult HandleError()
    {
        //catches unhandled errors
        var exceptionHandlerFeature =
            HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        return View("Error", exceptionHandlerFeature.Error);
    }


    [HttpGet("/{shortUrlCode}")]
    public async Task<IActionResult> Index(string shortUrlCode)
    {

        //Redirects user to url depending on short Url code

        Console.WriteLine("Directing to URL with code: "  + shortUrlCode);

        try
        {
            ShortenedURL url = await _repository.GetLongURLByCode(shortUrlCode);
            if (url == null)
            {
                //if no url with code found
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
            return View("Error", e);
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
        //validation to ensure URL is valid
        HttpClient client = new HttpClient();

        ViewBag.invalidURL = false;

        try
        {
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, longUrl)); //send http head request to check if url is valid
        }
        catch
        {
            //Invalid URL
            ViewBag.invalidURL = true;
            return View("Index");
        }


        var test = HttpContext.Request;
        string currentUrl = HttpContext.Request.Host.ToString();
        var scheme = HttpContext.Request.Scheme; //get http or https

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
            return View("Error", e);
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
            return View("Error", e);
        }

        //Create short url code from counter
        string shortCode = WebEncoders.Base64UrlEncode(BitConverter.GetBytes(counter));
        string shortUrl = scheme + "://" + currentUrl + "/" + shortCode;
        url = new ShortenedURL
        {
            ShortURL = shortUrl,
            LongURL = longUrl,
            counter = counter,
            ShortCode = shortCode
        };

        try
        {
            Console.WriteLine("Adding Url ('" + url.LongURL + "')");
            //add document to DB for shortened URL
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
            return View("Error", e);
        }

        //update view temp data
        TempData["lastLongURL"] = url.LongURL;
        TempData["lastShortURL"] = url.ShortURL;

        return RedirectToAction("Index");
    }
}

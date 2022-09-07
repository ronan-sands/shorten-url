using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<IActionResult>Index()
    {
        //string sessionID = HttpContext.Session.Id;

        var urls = await _repository.GetURLs();

        ViewBag.urls = urls;

        ViewBag.lastLongURL = TempData["lastLongURL"];
        ViewBag.lastShortURL = TempData["lastShortURL"];



        return View();
    }

    [HttpGet("/{shortUrlCode}")]
    public async Task<IActionResult> Index(string shortUrlCode)
    {

        ViewBag.lastLongURL = TempData["lastLongURL"];
        ViewBag.lastShortURL = TempData["lastShortURL"];

        ShortenedURL url = await _repository.GetLongURLByCode(shortUrlCode);

        if (url == null)
        {
            return View();
        }

        return Redirect(url.LongURL);
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
        //map endpoints to db
        //check if url already exists in db

        string currentUrl = HttpContext.Request.Host.ToString();
        var scheme = HttpContext.Request.Scheme;

        ShortenedURL url = await _repository.GetShortenedURLByLongURL(longUrl);
        

        if (url != null)
        {
            //ViewBag.lastURL = url;
            return RedirectToAction("Index");
        }

        var counter = await _repository.GetNextSequenceVal("counter");

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


        await _repository.CreateShortenedURL(url);

        //ViewBag.lastURL = url;

        TempData["lastLongURL"] = url.LongURL;
        TempData["lastShortURL"] = url.ShortURL;

        //return CreatedAtRoute("GetShortenedURL", new { id = url.Id }, url);
        //return View("Index");

        return RedirectToAction("Index");
    }
}

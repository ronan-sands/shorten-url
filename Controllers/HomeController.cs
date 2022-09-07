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



        return View();
    }

    [HttpGet("/{shortUrlCode}")]
    public async Task<IActionResult> Index(string shortUrlCode)
    {
        ShortenedURL url = await _repository.GetLongURLByShortURL(shortUrlCode);

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
        ShortenedURL url = await _repository.GetShortenedURLByLongURL(longUrl);

        if (url != null)
        {
            return Ok(url);
        }

        var counter = await _repository.GetNextSequenceVal("counter");

        string shortUrl = WebEncoders.Base64UrlEncode(BitConverter.GetBytes(counter));
        string sessionID = HttpContext.Session.Id;
        url = new ShortenedURL
        {
            ShortURL = shortUrl,
            LongURL = longUrl,
            counter = counter,
            SessionID = sessionID
        };


        await _repository.CreateShortenedURL(url);

        //return CreatedAtRoute("GetShortenedURL", new { id = url.Id }, url);
        return RedirectToAction("Index");
    }
}

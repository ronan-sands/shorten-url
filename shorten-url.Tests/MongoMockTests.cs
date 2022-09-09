﻿using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;
using shorten_url.Models;
using shorten_url.Data;
using shorten_url.Repositories;
using shorten_url.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace shorten_url.Tests;

public class MongoMockTests
{
    private HomeController controller;

    private Mock<IUrlRepository> _mockRepo;

    private ShortenedURL testObject;


    public MongoMockTests()
    {

        
        testObject = new ShortenedURL
        {
            ShortURL = "https://localhost:7034/AQAAAAAAAAA",
            ShortCode = "AQAAAAAAAAA",
            LongURL = "https://www.youtube.com/",
            counter = 1
        };
        _mockRepo = new Mock<IUrlRepository>();

        //Mock GetLongURLByCode output to return testObject async
        _mockRepo.Setup(x => x.GetLongURLByCode(testObject.ShortCode))
                                .Returns(Task.FromResult(testObject));

        //Create Mock Logger for controller
        var mockLogger = new Mock<ILogger<HomeController>>();
        ILogger<HomeController> logger = mockLogger.Object;

        controller = new HomeController(_mockRepo.Object, logger);

    }

    [Fact]
    public async void TestRedirectToLongURL()
    {
        RedirectResult result = (RedirectResult)await controller.Index(testObject.ShortCode);

        Assert.Equal(testObject.LongURL, result.Url);
    }

    [Fact]
    public async void TestShortenedURLCreatedCorrectly()
    {
        //Configure HttpContext to prevent exceptions
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new DefaultHttpContext();

        //Configure TempData to prevent exceptions
        Mock<ITempDataProvider> tempDataProvider = new Mock<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider.Object);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
        tempData["lastLongURL"] = "";
        tempData["lastShortURL"] = "";
        controller.TempData = tempData;

        //expected ShortenedURL to be created
        string httpScheme = controller.HttpContext.Request.Scheme;
        string httpHost = controller.HttpContext.Request.Host.ToString();
        string currentUrl = httpScheme + "://" + httpHost + "/";
        ShortenedURL expectedUrlObject = new ShortenedURL
        {
            counter = 0,
            ShortCode = "AAAAAAAAAAA",
            ShortURL = currentUrl + "AAAAAAAAAAA",
            LongURL = "https://www.youtube.com/"
        };

        var result = await controller.CreateShortenedURL(expectedUrlObject.LongURL);

        tempData = controller.TempData;

        
        Assert.True(tempData["lastLongURL"] != null && tempData["lastShortURL"] != null
                        && tempData["lastLongURL"].ToString() == expectedUrlObject.LongURL
                        && tempData["lastShortURL"].ToString() == expectedUrlObject.ShortURL);
    }



    
}

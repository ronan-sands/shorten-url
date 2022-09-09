using System;
using MongoDB.Driver;
using shorten_url.Models;

namespace shorten_url.Data
{
    public class UrlDBContext : IUrlDBContext
    {
        public UrlDBContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

            URLs = database.GetCollection<ShortenedURL>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));

            Sequences = database.GetCollection<Sequence>("Sequences");

            //Seed Data
        }

        public IMongoCollection<ShortenedURL> URLs { get; }

        public IMongoCollection<Sequence> Sequences { get; }
    }
}


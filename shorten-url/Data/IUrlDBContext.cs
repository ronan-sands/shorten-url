using System;
using MongoDB.Driver;
using shorten_url.Models;

namespace shorten_url.Data
{
    public interface IUrlDBContext
    {
        IMongoCollection<ShortenedURL> URLs { get; }

        IMongoCollection<Sequence> Sequences { get; }
    }
}


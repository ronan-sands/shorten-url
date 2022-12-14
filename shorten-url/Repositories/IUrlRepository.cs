using System;
using shorten_url.Models;

namespace shorten_url.Repositories
{
    public interface IUrlRepository
    {
        Task<IEnumerable<ShortenedURL>> GetURLs();

        Task<ShortenedURL> GetShortenedURL(string id);

        Task CreateShortenedURL(ShortenedURL url);

        Task<ShortenedURL> GetShortenedURLByLongURL(string longUrl);

        Task<ShortenedURL> GetLongURLByCode(string shortURL);

        Task<bool> UpdateShortenedURL(ShortenedURL url);

        Task<bool> DeleteShortenedURL(string id);

        Task<Sequence> GetSequence(string seqName);

        Task<long> GetNextSequenceVal(string seqName);
    }
}


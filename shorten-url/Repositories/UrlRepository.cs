using System;
using MongoDB.Driver;
using shorten_url.Data;
using shorten_url.Models;

namespace shorten_url.Repositories
{
    public class UrlRepository : IUrlRepository
    {

        private readonly IUrlDBContext _context;
        public UrlRepository(IUrlDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task CreateShortenedURL(ShortenedURL url)
        {
            await _context.URLs.InsertOneAsync(url);
        }

        public async Task<bool> DeleteShortenedURL(string id)
        {
            FilterDefinition<ShortenedURL> filter = Builders<ShortenedURL>.Filter.Eq(u => u.Id, id);

            DeleteResult deleteResult = await _context.URLs.DeleteOneAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }

        public async Task<ShortenedURL> GetShortenedURL(string id)
        {
            return await _context
                            .URLs
                            .Find(u => u.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<ShortenedURL> GetShortenedURLByLongURL(string longUrl)
        {
            return await _context
                            .URLs
                            .Find(u => u.LongURL == longUrl)
                            .FirstOrDefaultAsync();
        }

        public async Task<ShortenedURL> GetLongURLByCode(string code)
        {
            return await _context
                            .URLs
                            .Find(u => u.ShortCode == code)
                            .FirstOrDefaultAsync();
        }


        public async Task<IEnumerable<ShortenedURL>> GetURLs()
        {

            return await _context
                            .URLs
                            .Find(u => true)
                            .ToListAsync();
        }

        public async Task<bool> UpdateShortenedURL(ShortenedURL url)
        {
            var updateResult = await _context.URLs.ReplaceOneAsync(filter: u => u.Id == url.Id, replacement: url);

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<Sequence> GetSequence(string seqName)
        {
            return await _context
                            .Sequences
                            .Find(s => s.Name == seqName)
                            .FirstOrDefaultAsync();
        }

        public async Task<long> GetNextSequenceVal(string seqName)
        {
            var filter = Builders<Sequence>.Filter.Eq(a => a.Name, seqName);
            var update = Builders<Sequence>.Update.Inc(a => a.Value, 1);
            var sequence = await _context.Sequences.FindOneAndUpdateAsync(filter, update);

            return sequence.Value;
        }


    }
}


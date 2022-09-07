using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace shorten_url.Models
{
    public class ShortenedURL
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public long counter { get; set; }

        public string SessionID { get; set; }

        public string ShortCode { get; set; }

        public string ShortURL { get; set; }

        public string LongURL { get; set; }


    }
}


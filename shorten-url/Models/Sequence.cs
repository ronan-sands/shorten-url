using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace shorten_url.Models
{
    public class Sequence
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string Name { get; set; }

        public long Value { get; set; }
    }
}


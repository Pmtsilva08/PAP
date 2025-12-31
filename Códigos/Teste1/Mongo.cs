using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Teste1
{
    internal class Mongo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("user")]
        public string User { get; set; } = String.Empty;
        [BsonElement("password")]
        public string Password { get; set; } = String.Empty;
    }
}

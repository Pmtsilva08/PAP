using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Teste_Melhorado
{
    public class Account
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("user_name")]
        [BsonRequired]
        public string User { get; set; } = String.Empty;
        [BsonElement("password")]
        [BsonRequired]
        public string Password { get; set; } = String.Empty;
        [BsonElement("email")]
        [BsonRequired]
        public string Email { get; set; } = String.Empty;
        [BsonElement("notes")]
        public List<Note> Note { get; set; } = new List<Note>();
    }

    public class Note
    {
        [BsonElement("title")]
        public string Title { get; set; } = String.Empty;
        [BsonElement("content")]
        public string Content { get; set; } = String.Empty;
    }
}

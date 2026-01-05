using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projeto_PAP
{
    public class Account
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("first_name")]
        [BsonRequired]
        public string FirstName { get; set; } = string.Empty;
        [BsonElement("last_name")]
        [BsonRequired]
        public string Lastname { get; set; } = string.Empty;
        [BsonElement("email_address")]
        [BsonRequired]
        public string EmailAddress { get; set; } = string.Empty;
        [BsonElement("password")]
        [BsonRequired]
        public string Password { get; set; } = string.Empty;
        [BsonElement("list_passwords")]
        public List<Passwords> Passwords { get; set; } = new List<Passwords>();
    }

    public class Passwords
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("site")]
        [BsonRequired]
        public string Site { get; set; } = string.Empty;
        [BsonElement("email_site")]
        [BsonRequired]
        public string EmailSite { get; set; } = string.Empty;
        [BsonElement("password_site")]
        [BsonRequired]
        public string PasswordSite { get; set; } = string.Empty;
    }
}

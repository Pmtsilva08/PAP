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

        [BsonElement("totp_secret")]
        public string? TotpSecret { get; set; } = null; //o ponto de interrogação indica que a propriedade pode ser nula

        [BsonElement("require_verification_for_actions")]
        public bool RequireVerificationForActions { get; set; } = false;  // Default: desativado

        [BsonElement("security_answer_male")]
        public string? SecurityAnswerMale { get; set; }

        [BsonElement("security_answer_female")]
        public string? SecurityAnswerFemale { get; set; }

        [BsonElement("list_passwords")]
        public List<PasswordItem> PasswordItems { get; set; } = new List<PasswordItem>();
    }

    public class PasswordItem
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("site_name")]
        [BsonRequired]
        public string SiteName { get; set; } = string.Empty;

        [BsonElement("email")]
        [BsonRequired]
        public string Email { get; set; } = string.Empty;

        [BsonElement("encrypted_password")]
        [BsonRequired]
        public string EncryptedPassword { get; set; } = string.Empty;

        [BsonElement("category")]
        public string Category { get; set; } = "Other";

        [BsonElement("url")]
        public string Url { get; set; } = string.Empty;

        [BsonElement("notes")]
        public string Notes { get; set; } = string.Empty;
    }
}
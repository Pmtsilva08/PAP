using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace MongoDB_Teste
{   
    internal class Account
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("nome")]
        public string Nome { get; set; }
        [BsonElement("idade")]
        public int Idade { get; set; }
    }
}

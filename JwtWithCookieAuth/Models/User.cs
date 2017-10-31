using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JwtWithCookieAuth.Models
{

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        [BsonElement("Email")]
        [BsonRequired]
        public string Email { get; set; }

        [BsonElement("Timestamp")]
        public string Timestamp { get; set; }

        [BsonElement("Hash")]
        public string Hash { get; set; }

        [BsonElement("Salt")]
        public string Salt { get; set; }
    }

    public class LoginFormUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace JwtWithCookieAuth.Models
{

    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        [BsonElement("OrderName")]
        public String OrderName { get; set; }

        [BsonElement("Owner")]
        public User Owner { get; set; }

        [BsonElement("Timestamp")]
        public String Timestamp { get; set; }

        [BsonElement("Price")]
        public int Price { get; set; }

        [BsonElement("Category")]
        [FromQuery(Name = "category")]
        public String Category { get; set; }
    }

    public class OrderFormOrder
    {
        public String Id { get; set; }
        public String OrderName { get; set; }
        public String Category { get; set; }
        public int Price { get; set; }        
        public String Owner { get; set; }
        public String Timestamp { get; set; }
    }

}

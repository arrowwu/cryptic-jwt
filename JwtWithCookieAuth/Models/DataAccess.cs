using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace JwtWithCookieAuth.Models
{
    public class DataAccess
    {
        IMongoClient _client;
        IMongoDatabase _db;

        public DataAccess()
        {
            var MongoDatabaseName = "TradingDB"; //TradingDB  
            var MongoUsername = ""; //demouser  
            var MongoPassword = ""; //Pass@123  
            var MongoPort = "27017";  //27017  
            var MongoHost = "localhost";  //localhost  

            // Creating credentials  
            var credential = MongoCredential.CreateMongoCRCredential
                            (MongoDatabaseName,
                             MongoUsername,
                             MongoPassword);

            // Creating MongoClientSettings  
            var settings = new MongoClientSettings
            {
                //      Credentials = new[] { credential }, //commented out because local mongo does not require authentication
                Server = new MongoServerAddress(MongoHost, Convert.ToInt32(MongoPort))
            };
            _client = new MongoClient(settings);

            _db = _client.GetDatabase(MongoDatabaseName);

        }

        public IEnumerable<Order> GetOrders()
        {
            var collection = _db.GetCollection<Order>("orders");
            var filter = new BsonDocument();
            var doc = collection.Find(filter).ToListAsync();
            doc.Wait();
            return doc.Result;
        }

        public IEnumerable<Order> GetOrders(String category)
        {
            var collection = _db.GetCollection<Order>("orders");
            var filter = Builders<Order>.Filter.Eq(order => order.Category, category);
            var doc = collection.Find(filter).ToListAsync();
            doc.Wait();
            return doc.Result;
        }


        public Order GetOrder(String id)
        {
            var collection = _db.GetCollection<Order>("orders");
            var filter = Builders<Order>.Filter.Eq(order => order.Id, id);
            var doc = collection.Find(filter).SingleOrDefaultAsync();
            doc.Wait();
            return doc.Result;
        }

        public async Task Create(Order order, String OwnerEmail)
        {
            var usersCollection = _db.GetCollection<User>("users");
            var filter = Builders<User>.Filter.Eq(user => user.Email, OwnerEmail);
            var userReturned = await usersCollection.Find(filter).SingleOrDefaultAsync();
            order.Owner = userReturned;
            var ordersCollection = _db.GetCollection<Order>("orders");
            
            await ordersCollection.InsertOneAsync(order);          
        }

        public async Task CreateUser(User user)
        {         
            var collection = _db.GetCollection<User>("users");
            var options = new CreateIndexOptions() { Unique = true };
            var field = new StringFieldDefinition<User>("Email");
            var indexDefinition = new IndexKeysDefinitionBuilder<User>().Ascending(field);
            await collection.Indexes.CreateOneAsync(indexDefinition, options);
            user.Timestamp = DateTime.UtcNow.ToString();
            await collection.InsertOneAsync(user);           
        }

        public User LoginUser(LoginFormUser loginFormUser)
        {
            var collection = _db.GetCollection<User>("users");
            var filter = Builders<User>.Filter.Eq(user => user.Email, loginFormUser.Email);
            var doc = collection.Find(filter).SingleOrDefaultAsync();
            doc.Wait();
            return doc.Result;
        }

        public async Task Update(String id, Order order)
        {
            var collection = _db.GetCollection<Order>("orders");
            var filter = Builders<Order>.Filter.Eq(s => s.Id, id);
            await collection.FindOneAndReplaceAsync(filter, order);
            
        }
        public async Task Remove(String id, String emailFromJwt)
        {
            var collection = _db.GetCollection<Order>("orders");
            var filter = Builders<Order>.Filter.Eq(s => s.Id, id);
            filter = filter & Builders<Order>.Filter.Eq(s => s.Owner.Email, emailFromJwt);
            await collection.DeleteOneAsync(filter);

        }



        
    }
}

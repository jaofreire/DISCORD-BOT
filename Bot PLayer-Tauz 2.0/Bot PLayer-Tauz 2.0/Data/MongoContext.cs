using Bot_PLayer_Tauz_2._0.Data.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_PLayer_Tauz_2._0.Data
{
    public class MongoContext
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<MusicModel> _musicCollection;

        public MongoContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var mongoUrl = new MongoUrl(_configuration["MongoDb:ConnectionStrings"]);
            var mongoClient = new MongoClient(mongoUrl);

            var mongoDataBase = mongoClient.GetDatabase(_configuration["MongoDb:DataBase"]);

            _musicCollection = mongoDataBase.GetCollection<MusicModel>(_configuration["MongoDb:CollectionName"]);
        }

        public async Task<List<MusicModel>> GetAllAsync() => await _musicCollection.Find(_ => true).ToListAsync();

        public async Task CreateAsync(MusicModel newMusic) => await _musicCollection.InsertOneAsync(newMusic);
        
    }
}

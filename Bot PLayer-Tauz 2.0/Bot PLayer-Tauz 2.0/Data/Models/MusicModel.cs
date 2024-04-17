using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_PLayer_Tauz_2._0.Data.Models
{
    public class MusicModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public int Id { get; set; }
        [BsonElement("Name")]
        public string? Name { get; set; }
        [BsonElement("Url")]
        public string? Url { get; set; }
    }
}

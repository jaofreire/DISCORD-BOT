using MongoDB.Bson;


namespace Bot_PLayer_Tauz_2._0.Data.Models
{
    public class MusicModel
    {
        public ObjectId Id { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }

    }
}

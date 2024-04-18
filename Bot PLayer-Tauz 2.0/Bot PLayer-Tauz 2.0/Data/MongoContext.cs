using Bot_PLayer_Tauz_2._0.Data.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;


namespace Bot_PLayer_Tauz_2._0.Data
{
    public class MongoContext : DbContext
    {
        public MongoContext(DbContextOptions<MongoContext> options) : base(options)
        {
        }

        public DbSet<MusicModel> Musics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MusicModel>().ToCollection("Musics");
        }

    }
}

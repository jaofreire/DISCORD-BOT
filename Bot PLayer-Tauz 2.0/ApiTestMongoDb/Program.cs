using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MongoContext>(options =>
{
    options.UseMongoDB(builder.Configuration["MongoDb:ConnectionStrings"], builder.Configuration["MongoDb:DataBase"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/register", async(MongoContext _mongoContext, MusicModel model) =>
{
    try
    {
        await _mongoContext.Musics.AddAsync(model);
        await _mongoContext.SaveChangesAsync();

        return Results.Ok(model);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.Run();

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

public class MusicModel
{
    public ObjectId Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
}


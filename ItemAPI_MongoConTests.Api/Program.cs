using ItemAPI_MongoConTests.Api.Repositories;
using ItemAPI_MongoConTests.Api.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//configuracion para mongo
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>();
builder.Services.AddSingleton<IMongoClient>(serviceProvider => {
    return new MongoClient(mongoDbSettings.ConnectionString);
});

builder.Services.AddHealthChecks()
    .AddMongoDb(mongoDbSettings.ConnectionString, name: "mongodb", timeout: TimeSpan.FromSeconds(3));

builder.Services.AddSingleton<IItemsRepository, ItemRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

using System.Text.Json;
using System.Text.Json.Serialization;
using REListing.Repositories.Contracts;
using REListing.Repositories.Extensions;
using REListing.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Write enums as camelCase strings, e.g. status "active" / "pending".
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Load listings at startup so a missing or malformed data file fails fast.
app.Services.GetRequiredService<IListingRepository>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

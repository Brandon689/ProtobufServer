using Google.Protobuf;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;
using ProtobufServer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Create a content type provider and add the .proto mime type
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".proto"] = "text/plain";

app.UseStaticFiles(); // This serves from wwwroot by default

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "Protos")),
    RequestPath = "/Protos",
    ContentTypeProvider = contentTypeProvider
});


app.MapPost("/greeting", (HttpRequest request) =>
{
    using var ms = new MemoryStream();
    request.Body.CopyToAsync(ms).Wait();
    Greeting? greeting = Greeting.Parser.ParseFrom(ms.ToArray());
    Console.WriteLine(greeting.Name);
    Console.WriteLine(greeting.Content);
    var response = new Greeting
    {
        Name = greeting.Name,
        Content = $"Hello, {greeting.Name}! Your message was: {greeting.Content}"
    };

    return Results.Bytes(response.ToByteArray(), "application/x-protobuf");
});

// New GET endpoint returning a Protocol Buffer
app.MapGet("/api/greeting/{name}", (string name) =>
{
    var greeting = new Greeting
    {
        Name = name,
        Content = $"Hello, {name}! This is a protobuf response."
    };

    return Results.Bytes(greeting.ToByteArray(), "application/x-protobuf");
});

app.MapGet("/api/fruits/{owner}", (string owner) =>
{
    var fruitBasket = new FruitBasket
    {
        Owner = owner,
        TotalWeight = 500 // grams
    };
    var random = new Random();
    var fruits = new List<(string Name, string Color)>
{
    ("Apple", "Red"),
    ("Banana", "Yellow"),
    ("Grape", "Purple"),
    ("Orange", "Orange"),
    ("Strawberry", "Red"),
    ("Blueberry", "Blue"),
    ("Kiwi", "Brown"),
    ("Mango", "Yellow"),
    ("Pineapple", "Yellow"),
    ("Watermelon", "Green"),
    ("Peach", "Pink"),
    ("Pear", "Green"),
    ("Cherry", "Red"),
    ("Blackberry", "Black"),
    ("Raspberry", "Red"),
    ("Lemon", "Yellow"),
    ("Lime", "Green"),
    ("Plum", "Purple"),
    ("Pomegranate", "Red"),
    ("Apricot", "Orange"),
    ("Fig", "Purple"),
    ("Coconut", "Brown"),
    ("Papaya", "Orange"),
    ("Guava", "Green"),
    ("Dragonfruit", "Pink"),
    ("Passionfruit", "Purple"),
    ("Lychee", "White"),
    ("Starfruit", "Yellow"),
    ("Durian", "Green"),
    ("Jackfruit", "Green")
};

    // Randomly select 15-20 fruits for the basket
    int fruitCount = random.Next(15, 21);
    var selectedFruits = fruits.OrderBy(x => random.Next()).Take(fruitCount).ToList();

    foreach (var fruit in selectedFruits)
    {
        fruitBasket.Fruits.Add(new Fruit { Name = fruit.Name, Color = fruit.Color });
    }

    // Update total weight based on the number of fruits
    fruitBasket.TotalWeight = fruitCount * 100; // Assuming an average of 100 grams per fruit
    var response = new FruitResponse { Basket = fruitBasket };

    return Results.Bytes(response.ToByteArray(), "application/x-protobuf");
});

// New JSON POST endpoint
app.MapPost("/json-greeting", async (HttpRequest request) =>
{
    var jsonGreeting = await JsonSerializer.DeserializeAsync<JsonGreeting>(request.Body);
    var response = new JsonGreeting
    {
        Name = jsonGreeting.Name,
        Content = $"Hello, {jsonGreeting.Name}! Your message was: {jsonGreeting.Content}"
    };
    return Results.Json(response);
});

// New JSON GET endpoint
app.MapGet("/api/json-greeting/{name}", (string name) =>
{
    var greeting = new JsonGreeting
    {
        Name = name,
        Content = $"Hello, {name}! This is a JSON response."
    };
    return Results.Json(greeting);
});

app.MapFallbackToFile("index.html");

app.Run();

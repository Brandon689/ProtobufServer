using Google.Protobuf;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;
using ProtobufServer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

// Create a file provider for the root directory
var fileProvider = new PhysicalFileProvider(app.Environment.ContentRootPath);

// Create a content type provider and add the .proto mime type
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".proto"] = "text/plain";


// Serve files from the root directory, including message.proto
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = "",
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

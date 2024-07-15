namespace ProtobufServer;

// JSON model (identical to Protobuf Greeting)
public class JsonGreeting
{
    public string Name { get; set; }
    public string Content { get; set; }
}

public class JsonFruit
{
    public string Name { get; set; }
    public string Color { get; set; }
}

public class JsonFruitBasket
{
    public string Owner { get; set; }
    public List<JsonFruit> Fruits { get; set; }
    public int TotalWeight { get; set; }
}
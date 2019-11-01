public class Sensor 
{
    public Sensor(string id, SensorType type, string uri) 
    {
        Id = id;
        Type = type;
        Uri = uri;
    }

    public string Id { get; }

    public SensorType Type { get; }

    public string Uri { get; }
}
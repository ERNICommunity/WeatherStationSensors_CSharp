using System;

public class Sensor 
{
    public Sensor(String id, SensorType type, String uri) 
    {
        Id = id;
        Type = type;
        Uri = uri;
    }

    public string Id { get; }

    public SensorType Type { get; }

    public string Uri { get; }
}
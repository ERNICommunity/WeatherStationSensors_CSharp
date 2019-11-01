/// <summary>
/// A value read from a sensor.
/// </summary>
public readonly struct SensorValue 
{

    public SensorValue(double? value, bool valid, string unit) 
    {
        Value = value;
        Valid = valid;
        Unit = unit;
    }

    public double? Value { get; }

    public bool Valid { get; }

    public string Unit { get; }
}
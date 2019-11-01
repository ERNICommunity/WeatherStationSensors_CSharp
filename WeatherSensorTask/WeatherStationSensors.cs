using System;
using System.Collections.Generic;
using System.IO;

public class WeatherStationSensors 
{
    private readonly VendorA _vendorA;
    private readonly List<Sensor> _sensors = new List<Sensor>();

    public WeatherStationSensors(VendorA vendorA) 
    {
        _vendorA = vendorA;
    }

    public WeatherStationSensors(VendorA vendorA, VendorB vendorB) 
    {
        _vendorA = vendorA;
    }

    /// <summary>
    /// Registers a sensor.
    /// </summary>
    /// <param name="id">a unique ID to identify the sensor</param>
    /// <param name="type">the type of the sensor</param>
    /// <param name="uri">a URI specifying how to this sensor can be accessed</param>
    public void AddSensor(string id, SensorType type, string uri) 
    {
        if (!_vendorA.CanHandleUri(uri)) 
        {
            throw new ArgumentException("Cannot handle URI: " + uri);
        }
        _sensors.Add(new Sensor(id, type, uri));
    }

    /// <summary>
    /// Reads the current values for each sensor.
    /// </summary>
    /// <returns>a mapping from sensor IDs to sensor values</returns>
    public IDictionary<string, SensorValue> ReadSensorValues() 
    {
        var values = new Dictionary<string, SensorValue>();
        foreach (var sensor in _sensors) 
        {
            double? value;
            bool valid;
            try 
            {
                value = _vendorA.ReadDoubleValue(sensor.Uri);
                switch (sensor.Type) 
                {
                    case SensorType.TEMPERATURE:
                        if (value < -50.00) 
                        {
                            // underflow
                            valid = false;
                        }
                        else if (value > 150.0) 
                        {
                            // overflow
                            valid = false;
                        } 
                        else 
                        {
                            valid = true;
                        }
                        break;
                    case SensorType.WIND_SPEED:
                        valid = value >= 0.0;
                        break;
                    case SensorType.WIND_DIRECTION:
                        valid = value >= -Math.PI && value <= Math.PI;
                        break;
                    case SensorType.HUMIDITY:
                        valid = value >= 0.0 && value <= 100.0;
                        break;
                    default:
                        valid = false;
                        break;
                }
            } 
            catch (IOException) 
            {
                value = null;
                valid = false;
            }
            
            string unit;
            switch (sensor.Type) 
            {
                case SensorType.TEMPERATURE:
                    unit = "°C";
                    break;
                case SensorType.WIND_SPEED:
                    unit = "km/h";
                    break;
                case SensorType.WIND_DIRECTION:
                    unit = "";
                    break;
                case SensorType.HUMIDITY:
                    unit = "%";
                    break;
                default:
                    throw new ArgumentException("Unknown SensorType: " + sensor.Type);
            }
            values.Add(sensor.Id, new SensorValue(value, valid, unit));
        }
        return values;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

[TestFixture]
public class WeatherStationSensorsTest
{
    private DummyA _dummyA;
    private DummyB _dummyB;
    private WeatherStationSensors _weatherStationSensors;

    [SetUp]
    public void SetUp() {
        _dummyA = new DummyA();
        _dummyB = new DummyB();
        _weatherStationSensors = new WeatherStationSensors(_dummyA, _dummyB);
    }

    [Test]
    public void ValidatesUriWhenAddingSensor() {
        _weatherStationSensors.AddSensor("id1", SensorType.TEMPERATURE, "a:test");
        _weatherStationSensors.AddSensor("id2", SensorType.TEMPERATURE, "b:test");

        Assert.Throws(typeof(ArgumentException), () => _weatherStationSensors.AddSensor("id3", SensorType.TEMPERATURE, "c:test"));
    }

    [Test]
    public void TemperatureSensorVendorA() {
        CheckTemperatureSensor("a:test", _dummyA.Values);
    }

    [Test]
    public void TemperatureSensorVendorB() {
        CheckTemperatureSensor("b:test", _dummyB.Values);
    }

    private void CheckTemperatureSensor(string uri, IDictionary<string, double?> values) {
        _weatherStationSensors.AddSensor("id", SensorType.TEMPERATURE, uri);

        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", null, false, "°C");

        values[uri] = 27.3;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", 27.3, true, "°C");

        values[uri] = -274.0;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", -274.0, false, "°C");

        values[uri] = 200.2;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", 200.2, false, "°C");
    }

    [Test]
    public void WindSpeedSensorVendorA() {
        CheckWindSpeedSensor("a:test", _dummyA.Values);
    }

    [Test]
    public void WindSpeedSensorVendorB() {
        CheckWindSpeedSensor("b:test", _dummyB.Values);
    }

    private void CheckWindSpeedSensor(string uri, IDictionary<string, double?> values) {
        _weatherStationSensors.AddSensor("id", SensorType.WIND_SPEED, uri);

        values[uri] = 27.3;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", 27.3, true, "km/h");

        values[uri] = -double.Epsilon;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", -double.Epsilon, false, "km/h");
    }

    [Test]
    public void WindDirectionSensorVendorA() {
        CheckWindDirectionSensor("a:test", _dummyA.Values);
    }

    [Test]
    public void WindDirectionSensorVendorB() {
        CheckWindDirectionSensor("b:test", _dummyB.Values);
    }

    private void CheckWindDirectionSensor(string uri, IDictionary<string, double?> values)
    {
        _weatherStationSensors.AddSensor("id", SensorType.WIND_DIRECTION, uri);

        values[uri] = -Math.PI;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", -Math.PI, true, "");

        values[uri] = Math.PI;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", Math.PI, true, "");

        double justAbovePi = Math.PI + 0.0000001;
        values[uri] = justAbovePi;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", justAbovePi, false, "");
        values[uri] = -justAbovePi;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", -justAbovePi, false, "");
    }

    [Test, Description("Humidity Sensor")]
    public void HumiditySensorVendorA() {
        CheckHumiditySensor("a:test", _dummyA.Values);
    }

    [Test, Description("Humidity Sensor")]
    public void HumiditySensorVendorB() {
        CheckHumiditySensor("b:test", _dummyB.Values);
    }

    private void CheckHumiditySensor(string uri, IDictionary<string, double?> values)
    {
        _weatherStationSensors.AddSensor("id", SensorType.HUMIDITY, uri);

        values[uri] = 45.0;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", 45.0, true, "%");

        values[uri] = 100.1;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", 100.1, false, "%");

        values[uri] = -0.1;
        CheckSingleValue(_weatherStationSensors.ReadSensorValues(), "id", -0.1, false, "%");
    }

    [Test]
    public void MultipleSensors() {
        _weatherStationSensors.AddSensor("A", SensorType.TEMPERATURE, "a:test");
        _weatherStationSensors.AddSensor("B", SensorType.HUMIDITY, "b:test");

        _dummyA.Values["a:test"] = 27.3;
        _dummyB.Values["b:test"] = 56.0;
        var values = _weatherStationSensors.ReadSensorValues();
        Assert.AreEqual(2, values.Count);
        CheckValue(values, "A", 27.3, true, "°C");
        CheckValue(values, "B", 56.0, true, "%");
    }

    [Test]
    public void ErrorHandling() {
        _weatherStationSensors.AddSensor("A", SensorType.TEMPERATURE, "a:test");
        _weatherStationSensors.AddSensor("B", SensorType.HUMIDITY, "b:test");

        _dummyA.Values["a:test"] = 27.3;
        _dummyB.Values["b:test"] = 56.0;
        _dummyB.ThrowConnectError = true;

        var values = _weatherStationSensors.ReadSensorValues();
        Assert.AreEqual(2, values.Count);
        CheckValue(values, "A", 27.3, true, "°C");
        CheckValue(values, "B", null, false, "%");
    }

    private void CheckSingleValue(IDictionary<string, SensorValue> values, string id, double? number, bool valid, string unit) {
        Assert.AreEqual(1, values.Count);
        CheckValue(values, id, number, valid, unit);
    }

    private void CheckValue(IDictionary<string, SensorValue> values, string id, double? number, bool valid, string unit) {
        SensorValue value = values[id];
        Assert.AreEqual(number, value.Value);
        Assert.AreEqual(valid, value.Valid);
        Assert.AreEqual(unit, value.Unit);
    }

    class DummyA : VendorA
    {
        public IDictionary<string, double?> Values { get; }

        public DummyA() {
            Values = new Dictionary<string, double?>();
        }

        public bool CanHandleUri(string uri)
        {
            return uri.StartsWith("a:");
        }

        public double ReadDoubleValue(string uri)
        {
            double? value = null;
            if (Values.ContainsKey(uri)) {
                value = Values[uri];
            }
            if (value.HasValue) {
                return value.Value;
            } else {
                throw new IOException("error");
            }
        }
    }

    class DummyB : VendorB
    {
        public IDictionary<string, double?> Values { get; }
        public bool ThrowConnectError { get; set; }

        public DummyB() {
            Values = new Dictionary<string, double?>();
        }

        public bool AcceptsUri(string uri)
        {
            return uri.StartsWith("b:");
        }

        public VendorBConnection Connect()
        {
            if (ThrowConnectError) {
                throw new IOException("Failed to connect");
            }
            return new DummyBConnection(Values);
        }
    }

    class DummyBConnection : VendorBConnection
    {
        private readonly IDictionary<string, double?> _values;

        public DummyBConnection(IDictionary<string, double?> values) {
            _values = values;
        }

        public void Dispose()
        {
        }

        public double ReadDoubleValue(string uri)
        {
             double? value = null;
            if (_values.ContainsKey(uri)) {
                value = _values[uri];
            }
            if (value.HasValue) {
                return value.Value;
            } else {
                throw new IOException("error");
            }
        }
    }

}
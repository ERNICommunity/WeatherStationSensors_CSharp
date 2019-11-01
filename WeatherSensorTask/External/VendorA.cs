/// <summary>
/// Interface to interact with sensors from VendorA.
/// This is a third-part interface. You cannot change it!
/// </summary>
public interface VendorA 
{
    /// <summary>
    /// Returns true if the uri belongs to a sensor from VendorA.
    /// </summary>
    bool CanHandleUri(string uri);

    /// <summary>
    /// Reads the current value from the sensor identified with the given URI.
    /// </summary>
    /// <exception cref="IOException">if an error occurs</exception>
    double ReadDoubleValue(string uri);
}
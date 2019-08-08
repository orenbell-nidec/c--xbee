using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeAPI
{
    /// <summary>
    /// Generic XBee API Exception This class and its subclasses indicate conditions that an application might want to catch.
    /// </summary>
    public class XBeeException : Exception {
        public XBeeException() : base() { }
        public XBeeException(string message) : base(message) { }
    }

    /// <summary>
    /// This exception will be thrown when any problem related to the communication with the XBee device occurs.
    /// </summary>
    public class CommunicationException : XBeeException
    {
        public CommunicationException() : base() { }
        public CommunicationException(string message) : base(message) { }
    }

    /// <summary>
    /// This exception will be thrown when a response of a packet is not success or OK.
    /// </summary>
    public class ATCommandException : CommunicationException
    {
        public ATCommandException() : base() { }
        public ATCommandException(string message) : base(message) { }
    }

    /// <summary>
    /// This exception will be thrown when any problem related to the connection with the XBee device occurs.
    /// </summary>
    public class ConnectionException : XBeeException
    {
        public ConnectionException() : base() { }
        public ConnectionException(string message) : base(message) { }
    }

    /// <summary>
    /// This exception will be thrown when any problem related to the XBee device occurs
    /// </summary>
    public class XBeeDeviceException : XBeeException
    {
        public XBeeDeviceException() : base() { }
        public XBeeDeviceException(string message) : base(message) { }
    }

    /// <summary>
    /// This exception will be thrown when trying to open an interface with an invalid configuration.
    /// </summary>
    public class InvalidConfigurationException : ConnectionException
    {
        private const string DEFAULT_MESSAGE = "The configuration used to open the interface is invalid.";

        public InvalidConfigurationException(string message=DEFAULT_MESSAGE) : base(message) { }
    }

    /// <summary>
    /// This exception will be thrown if the operating mode is different than * OperatingMode.API_MODE* and* OperatingMode.API_MODE*
    /// </summary>
    public class InvalidOperatingModeException : ConnectionException
    {
        private const string DEFAULT_MESSAGE = "The operating mode of the XBee device is not supported by the library.";
        public OperatingMode.Byte OperatingMode { get; private set; }

        public InvalidOperatingModeException(string message = DEFAULT_MESSAGE) : base(message) {
            OperatingMode = XBeeAPI.OperatingMode.Byte.UNKNOWN;
        }

        public InvalidOperatingModeException(OperatingMode.Byte operatingMode)
            : base("Unsupported operating mode: " + XBeeAPI.OperatingMode.Get(operatingMode)) {
            OperatingMode = operatingMode;
        }
    }

    /// <summary>
    /// This exception will be thrown when there is an error parsing an API packet from the input stream.
    /// </summary>
    public class InvalidPacketException : CommunicationException
    {
        private const string DEFAULT_MESSAGE = "The XBee API packet is not properly formed.";

        public InvalidPacketException(string message = DEFAULT_MESSAGE) : base(message) { }
    }

    /// <summary>
    /// This exception will be thrown when the operation performed is not supported by the XBee device.
    /// </summary>
    public class OperationNotSupportedException : XBeeDeviceException
    {
        private const string DEFAULT_MESSAGE = "The requested operation is not supported by either the connection interface or the XBee device.";

        public OperationNotSupportedException(string message = DEFAULT_MESSAGE) : base(message) { }
    }

    // Since no wrapper was made for the serial class, we can use System.Timeout instead of this
    ///// <summary>
    ///// This exception will be thrown when performing synchronous operations and the configured time expires.
    ///// </summary>
    //public class TimeoutException : CommunicationException
    //{
    //    private const string DEFAULT_MESSAGE = "There was a timeout while executing the requested operation.";

    //    public TimeoutException(string message = DEFAULT_MESSAGE) : base(message) { }
    //}

    /// <summary>
    /// This exception will be thrown when receiving a transmit status different than* TransmitStatus.SUCCESS* after sending an XBee API packet.
    /// </summary>
    public class TransmitException : CommunicationException
    {
        private const string DEFAULT_MESSAGE = "There was a problem with a transmitted packet response (status not ok)";

        public TransmitException(string message = DEFAULT_MESSAGE) : base(message) { }
    }

    /// <summary>
    /// This exception will be thrown when attempting to add a packet to an XBeeQueue when the queue is full
    /// </summary>
    public class XBeeQueueFullException : XBeeException
    {
        private const string DEFAULT_MESSAGE = "The item could not be added to the XBee queue. The queue is full";

        public XBeeQueueFullException(string message = DEFAULT_MESSAGE) : base(message) { }
    }
}

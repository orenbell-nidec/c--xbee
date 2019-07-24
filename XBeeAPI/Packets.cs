using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/* See digi documentation on Api frames here:
 * https://www.digi.com/resources/documentation/Digidocs/90002002/Content/Reference/r_api_frame_format_900hp.htm
 */

namespace XBeeAPI
{
    #region AFT
    /// <summary>
    /// This class contains an enumerator, <code>Byte</code>, for all the available frame types used in any XBee protocol
    /// </summary>
    public static class ApiFrameType
    {
        /// <summary>
        /// Enumerate all possible frame types (comprehensive, but most are not implemented in this API)
        /// </summary>
        public enum Byte : byte
        {
            [Description("TX (Transmit) Request 64-bit address")]
            TX_64 = 0x00,

            [Description("TX (Transmit) Request 16-bit address")]
            TX_16 = 0x01,

            [Description("Remote AT Command Request (Wi-Fi)")]
            REMOTE_AT_COMMAND_REQUEST_WIFI = 0x07,

            [Description("AT Command")]
            AT_COMMAND = 0x08,

            [Description("AT Command Queue")]
            AT_COMMAND_QUEUE = 0x09,

            [Description("Transmit Request")]
            TRANSMIT_REQUEST = 0x10,

            [Description("Explicit Addressing Command Frame")]
            EXPLICIT_ADDRESSING = 0x11,

            [Description("Metaframe Tx")]
            METAFRAME_TX = 0x16,

            [Description("Remote AT Command Request")]
            REMOTE_AT_COMMAND_REQUEST = 0x17,

            [Description("Secured Remote AT Command")]
            SECURED_REMOTE_AT_COMMAND = 0x18,

            [Description("IPv6 Tx Request")]
            IPV6_TX_REQUEST = 0x1A,

            [Description("IPv6 Remote AT Command")]
            IPV6_REMOTE_AT_COMMAND = 0x1B,

            [Description("CoAP Tx Request")]
            COAP_TX_REQUEST = 0x1C,

            [Description("CoAP Passthru Tx Response")]
            COAP_PASSTHRU_TX_RESPONSE = 0x1D,

            [Description("CoAP Passthru Tx Request")]
            COAP_PASSTHRU_TX_REQUEST = 0x1E,

            [Description("TX SMS")]
            TX_SMS = 0x1F,

            [Description("TX IPv4")]
            TX_IPV4 = 0x20,

            [Description("Create Source Route")]
            CREATE_SOURCE_ROUTE = 0x21,

            [Description("TX Request with TLS Profile")]
            TX_REQUEST_TLS = 0x23,

            [Description("Register Joining Device")]
            REGISTER_JOINING_DEVICE = 0x24,

            [Description("Send Data Request")]
            SEND_DATA_REQUEST = 0x28,

            [Description("Device Response")]
            DEVICE_RESPONSE = 0x2A,

            [Description("User Data Relay")]
            USER_DATA_RELAY = 0x2D,

            [Description("RX (Receive) Packet 64-bit Address")]
            RX_64 = 0x80,

            [Description("RX (Receive) Packet 16-bit Address")]
            RX_16 = 0x81,

            [Description("IO Data Sample RX 64-bit Address Indicator")]
            RX_IO_64 = 0x82,

            [Description("IO Data Sample RX 16-bit Address Indicator")]
            RX_IO_16 = 0x83,

            [Description("Remote AT Command Response (Wi-Fi)")]
            REMOTE_AT_COMMAND_RESPONSE_WIFI = 0x87,

            [Description("AT Command Response")]
            AT_COMMAND_RESPONSE = 0x88,

            [Description("TX (Transmit) Status")]
            TX_STATUS = 0x89,

            [Description("Modem Status")]
            MODEM_STATUS = 0x8A,

            [Description("Transmit Status")]
            TRANSMIT_STATUS = 0x8B,

            [Description("Route Information Packet")]
            ROUTE_INFO_PACKET = 0x8D,

            [Description("Aggregate Addressing Update")]
            AGGREGATE_ADDR_UPDATE = 0x8E,

            [Description("IO Data Sample RX Indicator (Wi-Fi)")]
            IO_DATA_SAMPLE_RX_INDICATOR_WIFI = 0x8F,

            [Description("Receive Packet")]
            RECEIVE_PACKET = 0x90,

            [Description("Explicit RX Indicator")]
            EXPLICIT_RX_INDICATOR = 0x91,

            [Description("IO Data Sample RX Indicator")]
            IO_DATA_SAMPLE_RX_INDICATOR = 0x92,

            [Description("XBee Sensor Read Indicator")]
            SENSOR_READ_INDICATOR = 0x94,

            [Description("Node Identification Indicator")]
            NODE_ID_INDICATOR = 0x95,

            [Description("Remote Command Response")]
            REMOTE_AT_COMMAND_RESPONSE = 0x97,

            [Description("IPv6 Rx Response")]
            IPV6_RX_RESPONSE = 0x9A,

            [Description("IPv6 Remote AT Command Response")]
            IPV6_REMOTE_AT_COMMAND_RESPONSE = 0x9B,

            [Description("CoAP Rx Response")]
            COAP_RX_RESPONSE = 0x9C,

            [Description("CoAP Passthru Rx Request")]
            COAP_PASSTHRU_RX_REQUEST = 0x9D,

            [Description("CoAP Passthru Rx Response")]
            COAP_PASSTHRU_RX_RESPONSE = 0x9E,

            [Description("RX SMS")]
            RX_SMS = 0x9F,

            [Description("Over-the-Air Firmware Update Status")]
            OTA_FIRMWARE_UPDATE_STATUS = 0xA0,

            [Description("Route Record Indicator")]
            ROUTE_RECORD_INDICATOR = 0xA1,

            [Description("Device Authenticated Indicator")]
            DEVICE_AUTH_INDICATOR = 0xA2,

            [Description("Many-to-One Route Request Indicator")]
            ROUTE_REQUEST_INDICATOR = 0xA3,

            [Description("Register Joing Device Status")]
            REGISTER_JOINING_DEVICE_STATUS = 0xA4,

            [Description("Join Notification Status")]
            JOIN_NOTIFICATION_STATUS = 0xA5,

            [Description("Metaframe Rx")]
            METAFRAME_RX = 0xA6,

            [Description("IPv6 IO Data Sample Rx Indicator")]
            IPV6_IO_DATA_SAMPLE_RX_INDICATOR = 0xA7,

            [Description("User Data Relay Output")]
            USER_DATA_RELAY_OUTPUT = 0xAD,

            [Description("RX IPv4")]
            RX_IPV4 = 0xB0,

            [Description("Send Data Response")]
            SEND_DATA_RESPONSE = 0xB8,

            [Description("Device Request")]
            DEVICE_REQUEST = 0xB9,

            [Description("Device Response Status")]
            DEVICE_RESPONSE_STATUS = 0xBA,

            [Description("Frame Error")]
            FRAME_ERROR = 0xFE,

            [Description("Generic")]
            GENERIC = 0xFF
        }

        /// <summary>
        /// Public accessor for lookupTable
        /// </summary>
        /// <param name="code">The frame type identifying code</param>
        /// <returns>APIFrameType object with attributes Code and Description</returns>
        public static string Get(byte code)
        {
            return Get((Byte)code);
        }
        public static string Get(Byte code)
        {
            try
            {
                return ((DescriptionAttribute)typeof(Byte).GetField(code.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).First()).Description;
            }
            catch
            {
                return "invalid frame type";
            }
        }
    }
    #endregion

    #region Base
    /// <summary>
    /// All keys used in dictionaries returned by "ToDict()"
    /// </summary>
    public enum DictKeys
    {
        HEADER_BYTE,
        LENGTH,
        FRAME_TYPE,
        FRAME_ID,
        FRAME_SPEC_DATA,
        API_DATA,
        RF_DATA,
        CHECKSUM,
        COMMAND,
        PARAMETER,
        X64BIT_ADDR,
        X16BIT_ADDR,
        TRANSMIT_OPTIONS,
        RECEIVE_OPTIONS,
        BROADCAST_RADIUS,
        TRANS_R_COUNT,
        TS_STATUS,
        DS_STATUS,
        AT_CMD_STATUS,
        MODEM_STATUS,
        NUM_SAMPLES,
        DIGITAL_MASK,
        ANALOG_MASK,
        RSSI,
        SOURCE_ENDPOINT,
        DEST_ENDPOINT,
        CLUSTER_ID,
        PROFILE_ID,
        PHONE_NUMBER,
        DEST_IPV4_ADDR,
        SRC_IPV4_ADDR,
        DEST_PORT,
        SRC_PORT,
        IP_PROTOCOL,
        STATUS,
        TRANSPORT,
        FLAGS,
        REQUEST_ID,
        TARGET,
        RESERVED,
        DC_STATUS,
        FRAME_ERROR,
        PATH_LENGTH,
        PATH,
        CONTENT_TYPE_LENGTH,
        CONTENT_TYPE,
        SOURCE_EVENT,
        DATA_LENGTH,
        TIMESTAMP,
        TIMEOUT_COUNT,
        DESTINATION_ADDRESS,
        SOURCE_ADDRESS,
        RESPONDER_ADDRESS,
        RECEIVER_ADDRESS
    }

    /// <summary>
    /// This abstract class represents the basic structure of an XBee packet. Generic actions
    /// like checksum computer or packet length calculation are performed here, but payload
    /// generation should be done in derived classes depending on their type
    /// </summary>
    public abstract class XBeePacket
    {
        private static byte HASH_SEED = 23;

        public XBeePacket() { }

        /// <summary>
        /// Returns packet information as dictionary
        /// </summary>
        /// <returns>Dictionary of the packet information</returns>
        public override string ToString()
        {
            // TODO: This doesn't work recursively when there's a dictionary inside another dictionary
            Dictionary<DictKeys, object> dict = ToDict();
            var entries = dict.Select(d =>
                string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));
            return "{" + string.Join(",", entries) + "}";
        }

        /// <summary>
        /// Returns whether the given object is equal to this one
        /// </summary>
        /// <param name="obj">XBeePacket to compare</param>
        /// <returns>True if byte arrays are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is XBeePacket))
            {
                return false;
            }
            else
            {
                return Output().SequenceEqual(((XBeePacket)obj).Output());
            }
        }

        /// <summary>
        /// Returns a hash code value for the object
        /// </summary>
        /// <returns>Integer hash code value</returns>
        public override int GetHashCode()
        {
            int res = HASH_SEED;
            foreach (byte b in Output())
            {
                res = 31 * (res + b);
            }
            return res;
        }

        /// <summary>
        /// Computes the checksum value of this XBeePacket
        /// </summary>
        /// <returns>Checksum value of this XBeePacket</returns>
        public byte GetChecksum()
        {
            return (byte)(0xFF - (GetFrameSpecData().Sum(b => (byte)b) & 0xFF));
        }

        /// <summary>
        /// Returns the raw byte array of this XBeePacket, ready to be sent to the serial port
        /// </summary>
        /// <param name="escaped">Indicates if the byte array will be escaped or not</param>
        /// <returns>Raw byte array of the XBeePacket</returns>
        public byte[] Output(bool escaped = false)
        {
            byte[] frame = BuildCompleteFrameWithoutHeader(GetFrameSpecData());
            if (escaped)
            {
                frame = EscapeData(frame);
            }
            byte[] final = new byte[frame.Length + 1];
            final[0] = SpecialByte.HEADER_BYTE;
            Array.Copy(frame, 0, final, 1, frame.Length);
            return final;
        }

        /// <summary>
        /// Returns a dictionary with all the all information of the XBeePacket fields.
        /// You are responsible for type checking the dict values
        /// </summary>
        /// <returns>Dictionary with info of the XBeePacket fields</returns>
        public Dictionary<DictKeys, object> ToDict()
        {
            return new Dictionary<DictKeys, object>()
    {
        { DictKeys.HEADER_BYTE,     SpecialByte.HEADER_BYTE },
        { DictKeys.LENGTH,          GetFrameSpecData().Length },
        { DictKeys.FRAME_SPEC_DATA, GetFrameSpecData() },
        { DictKeys.CHECKSUM,        GetChecksum() }
    };
        }

        /// <summary>
        /// If you can see this in your prompt, this XBeePacket-derivced class hasn't implemented it's CreatePacket method.
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New XBeePacket derives from raw bytes</returns>
        /// <exception cref="NotImplementedException">Thrown if this method is called from base class</exception>
        public static XBeePacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            throw new NotImplementedException("CreatePacket not implemented in base class");
        }

        /// <summary>
        /// Returns the data between the length field and the checksum field as a byte array
        /// </summary>
        /// <returns>Byte array: frame data</returns>
        public abstract byte[] GetFrameSpecData();

        /// <summary>
        /// Returns frame data as dictionary
        /// </summary>
        /// <returns>Dictionary</returns>
        public abstract Dictionary<DictKeys, object> GetFrameSpecDataDict();

        /// <summary>
        /// Escapes the byte array data
        /// </summary>
        /// <param name="data">Byte array to escape</param>
        /// <returns>Escaped equivalent of byte array</returns>
        protected static byte[] EscapeData(byte[] data)
        {
            byte[] escData = new byte[data.Length * 2];
            int i = 0;
            foreach (byte b in data)
            {
                if (SpecialByte.IsSpecialByte(b))
                {
                    escData[i++] = SpecialByte.ESCAPE_BYTE;
                    escData[i++] = SpecialByte.Escape(b);
                }
                else
                {
                    escData[i++] = b;
                }
            }
            return escData.Take(i).ToArray();
        }

        /// <summary>
        /// Un-escapes the provided byte array
        /// </summary>
        /// <param name="data">The byte array to escape</param>
        /// <returns>Unescaped data</returns>
        public static byte[] UnescapeData(byte[] data)
        {
            byte[] escData = new byte[data.Length];
            bool deEscape = false;
            int i = 0;
            foreach (byte b in data)
            {
                if (b == SpecialByte.ESCAPE_BYTE)
                {
                    deEscape = true;
                }
                else
                {
                    escData[i++] = (deEscape) ? SpecialByte.Escape(b) : b;
                    deEscape = false;
                }
            }
            return escData.Take(i).ToArray();
        }

        /// <summary>
        /// Builds a complete non-escaped frame from the given frame specific data.
        /// Complete frame is: Start delimiter + length + frame specific data + checksum
        /// </summary>
        /// <param name="frameSpecData">Frame specific ata</param>
        /// <returns>Complete frame as byte array</returns>
        /// <exception cref="NullReferenceException">Argument is null</exception>
        protected byte[] BuildCompleteFrameWithoutHeader(byte[] frameSpecData)
        {
            return Utils.IntToLength((short)frameSpecData.Length)
                .Concat(frameSpecData)
                .Concat(new byte[] { GetChecksum() })
                .ToArray();
        }
    }

    /// <summary>
    /// This abstract class provides the basic structure of a API frame.<para/>
    /// </summary>
    /// <remarks>
    /// Derived classes should implement their own methods to generate the API
    /// data and frame ID in case they support it. <para/>
    /// Basic operations such as frame type retrieval are performed in this class.
    /// </remarks>
    public abstract class XBeeAPIPacket : XBeePacket
    {
        /// <summary>
        /// Constructor. Instantiates a new XBeeAPIPacket, given a frame type
        /// </summary>
        /// <param name="frameType">Frame type as a byte</param>
        public XBeeAPIPacket(ApiFrameType.Byte frameType) : base()
        {
            FrameTypeValue = frameType;
            FrameID = 0;
        }

        /// <summary>
        /// Returns the data between the length field and the checksum field as a byte array
        /// </summary>
        /// <returns>Byte array: frame data</returns>
        public override byte[] GetFrameSpecData()
        {
            byte[] data = GetAPIPacketSpecData();
            byte[] final;
            if (NeedsID())
            {
                // Add 2 bytes to beginning of data, fill in the 2nd with frame id
                final = new byte[data.Length + 2];
                Array.Copy(data, 0, final, 2, data.Length);
                final[1] = FrameID;
            }
            else
            {
                // Add a single byte to the beginning of data
                final = new byte[data.Length + 1];
                Array.Copy(data, 0, final, 1, data.Length);
            }
            // Fill in 1st byte with frame type
            final[0] = (byte)FrameTypeValue;

            return final;
        }

        /// <summary>
        /// Frame type of this class, as an ApiFrameType.Byte object. Can be cast to byte
        /// </summary>
        public ApiFrameType.Byte FrameTypeValue { get; }

        /// <summary>
        /// Returns frame data as dictionary
        /// </summary>
        /// <returns>Dictionary</returns>
        public override Dictionary<DictKeys, object> GetFrameSpecDataDict()
        {
            return new Dictionary<DictKeys, object>
            {
                { DictKeys.FRAME_TYPE,  FrameTypeValue },
                { DictKeys.FRAME_ID,    FrameID },
                { DictKeys.API_DATA,    GetAPIPacketSpecDataDict() }
            };
        }

        /// <summary>
        /// Returns whether this packet is broadcast or not
        /// </summary>
        public bool IsBroadcast { get { return false; } }

        /// <summary>
        /// The frame ID of the packet
        /// </summary>
        public byte FrameID { get; protected set; }

        /// <summary>
        /// Checks if a not escaped byte array meets conditions
        /// </summary>
        /// <param name="raw">Non-escaped byte array to check</param>
        /// <param name="minLength">Minimum length of packet</param>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly</exception>
        protected static void CheckAPIPacket(byte[] raw, int minLength = 5)
        {
            if (raw.Length < minLength)
            {
                throw new InvalidPacketException(string.Format("Bytearray must have at least {0} of complete length (header, length, frameType, checksum)", minLength));
            }

            if ((raw[0] & 0xFF) != SpecialByte.HEADER_BYTE)
            {
                throw new InvalidPacketException("Bytearray must start with the header byte (SpecialByte.HEADER_BYTE)");
            }

            int realLength = raw.Length - 4;
            int lengthField = Utils.LengthToInt(raw.Skip(1).Take(2).ToArray());

            if (realLength != lengthField)
            {
                throw new InvalidPacketException("The real length of this frame is distinct than the specified by length field (bytes 2 and 3)");
            }

            //TODO: Check if this works - if (GetCheckSum != raw.Last())
            if (0xFF - (raw.Skip(3).Reverse().Skip(1).Sum(b => (int)b) & 0xFF) != raw.Last())
            {
                throw new InvalidPacketException("Wrong checksum");
            }
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected abstract byte[] GetAPIPacketSpecData();

        /// <summary>
        /// Returns whether the packet requires a frame ID or not
        /// </summary>
        /// <returns>True if the packet needs an ID, False otherwise</returns>
        public abstract bool NeedsID();

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary or list</returns>
        protected abstract Dictionary<DictKeys, object> GetAPIPacketSpecDataDict();
    }

    /// <summary>
    /// This class represents a basic and Generic XBee packet
    /// </summary>
    public class GenericXBeePacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 5;

        /// <summary>
        /// Byte array of data between length and checksum fields
        /// </summary>
        private byte[] RfData { get; set; }

        /// <summary>
        /// Instantiates packet with provided parameters
        /// </summary>
        /// <param name="rfdata">Frame specific data to build the packet with. Lacks frame type and ID</param>
        public GenericXBeePacket(byte[] rfdata) : base(ApiFrameType.Byte.GENERIC)
        {
            this.RfData = rfdata;
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New GenericXBeePacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new GenericXBeePacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode = OperatingMode.Byte.API_MODE)
        {
            // Verify we're in the correct operating mode
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            // Do a packet check. This may throw InvalidPacketException
            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.GENERIC)
            {
                throw new InvalidPacketException(String.Format("Wrong frame type, expected: {0}. Value: {1}",
                    ApiFrameType.Get(ApiFrameType.Byte.GENERIC),
                    (byte)ApiFrameType.Byte.GENERIC));
            }

            // Return raw[4:-1]
            return new GenericXBeePacket(raw.Skip(4).Reverse().Skip(1).Reverse().ToArray());
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            return RfData;
        }

        /// <summary>
        /// Returns whether the packet requires a frame ID or not
        /// </summary>
        /// <returns>True if the packet needs an ID, False otherwise</returns>
        public override bool NeedsID()
        {
            return false;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary or list</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>
    {
        { DictKeys.RF_DATA, RfData }
    };
        }
    }

    /// <summary>
    /// Represents an unknown XBee packet
    /// </summary>
    public class UnknownXBeePacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 5;

        /// <summary>
        /// Byte array of data between length and checksum fields
        /// </summary>
        private byte[] RfData { get; set; }

        /// <summary>
        /// Instantiates UnknownXBeePacket using given frame type and frame data
        /// </summary>
        /// <param name="frameType">Type of frame</param>
        /// <param name="rfdata">Frame specific data</param>
        public UnknownXBeePacket(ApiFrameType.Byte frameType, byte[] rfdata) : base(frameType)
        {
            RfData = rfdata;
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New UnknownXBeePacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new UnknownXBeePacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            // Verify the operating mode is correct
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            // Verify the packet is valid
            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            // return UnknownXbeePacket(raw[3], raw[4:-1])
            return new UnknownXBeePacket((ApiFrameType.Byte)raw[3], raw.Skip(4).Reverse().Skip(1).Reverse().ToArray());
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            return RfData;
        }

        /// <summary>
        /// Returns whether the packet requires a frame ID or not
        /// </summary>
        /// <returns>True if the packet needs an ID, False otherwise</returns>
        public override bool NeedsID()
        {
            return false;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary or list</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>
    {
        { DictKeys.RF_DATA,     RfData }
    };
        }
    }
    #endregion

    #region Common
    /// <summary>
    /// This class represents an AT command packet. <para/>
    /// </summary>
    /// <remarks>
    /// Used to query or set module parameters on the local device.This API
    /// command applies changes after executing the command. (Changes made to
    /// module parameters take effect once changes are applied.). <para/>
    /// Command response is received as an :class:`.ATCommResponsePacket`.
    /// </remarks>
    public class ATCommPacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 6;

        /// <summary>
        /// Instantiates new ATCommPacket class with provided parameters
        /// </summary>
        /// <param name="frameID">Frame ID</param>
        /// <param name="command">2 letter command, as string</param>
        /// <param name="parameter">Value to set the command (optional for some commands)</param>
        /// <exception cref="ArgumentException">Thrown when command is not a 2 character string</exception>
        public ATCommPacket(byte frameID, string command, byte[] parameter = null) : base(ApiFrameType.Byte.AT_COMMAND)
        {
            if (command == null || command.Length != 2)
            {
                throw new ArgumentException("Invalid command " + command);
            }

            // TODO: Make sure the null terminator doesn't get included
            this.Command = command;
            this.Parameter = parameter ?? (new byte[0]);
            this.FrameID = frameID;
        }

        /// <summary>
        /// Instantiates new ATCommPacket class with provided parameters
        /// </summary>
        /// <param name="frameID">Frame ID</param>
        /// <param name="command">2 letter command as byte array</param>
        /// <param name="parameter">Value to set the command (optional for some commands)</param>
        /// <exception cref="ArgumentException">Thrown when command is not a 2 character string</exception>
        public ATCommPacket(byte frameID, byte[] command, byte[] parameter = null) : base(ApiFrameType.Byte.AT_COMMAND)
        {
            if (command == null || command.Length != 2)
            {
                throw new ArgumentException("Invalid command " + command);
            }

            // TODO: Make sure the null terminator doesn't get included
            this.Command = Utils.BytesToAscii(command);
            this.Parameter = parameter ?? (new byte[0]);
            this.FrameID = frameID;
        }

        /// <summary>
        /// AT command of the packet
        /// </summary>
        public string Command { get; private set; }
        /// <summary>
        /// Optional command parameter, some commands require it
        /// </summary>
        public byte[] Parameter { get; private set; }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New ATCommPacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new ATCommPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode));
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.AT_COMMAND)
            {
                throw new InvalidPacketException("This packet is not an AT command packet");
            }

            return new ATCommPacket(raw[4], raw.Skip(5).Take(2).ToArray(), raw.Skip(7).Reverse().Skip(1).Reverse().ToArray());
        }

        /// <summary>
        /// Returns whether the packet requires a frame ID or not
        /// </summary>
        /// <returns>True if the packet needs an ID, False otherwise</returns>
        public override bool NeedsID()
        {
            return true;
        }

        /// <summary>
        /// Returns the data between the length field and the checksum field as a byte array
        /// </summary>
        /// <returns>Byte array: frame data</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            if (Parameter != null)
            {
                return Utils.AsciiToBytes(Command).Concat(Parameter).ToArray();
            }
            else
            {
                return Utils.AsciiToBytes(Command);
            }
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary or list</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.COMMAND,     Command   },
                { DictKeys.PARAMETER,   Parameter }
            };
        }
    }

    // TODO: Implement this
    public class ATCommQueuePacket : XBeeAPIPacket
    {
        /// <summary>
        /// Not implemented
        /// </summary>
        public ATCommQueuePacket(byte frameID, string command, byte[] parameter = null) : base(ApiFrameType.Byte.AT_COMMAND_QUEUE)
        {
            throw new NotImplementedException();
        }

        public static new ATCommQueuePacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            throw new NotImplementedException("ATCommQueuePacket.CreatePacket not implemented");
        }

        public override bool NeedsID()
        {
            throw new NotImplementedException();
        }

        protected override byte[] GetAPIPacketSpecData()
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// This class represents an AT command response packet.
    /// </summary>
    /// <remarks>
    /// In response to an AT command message, the module will send an AT command
    /// response message.Some commands will send back multiple frames(for example,
    /// the ``ND`` - Node Discover command<para/>
    /// This packet is received in response of an :class:`.ATCommPacket`.
    /// Response also includes an :class:`.ATCommandStatus` object with the status
    /// of the AT command.
    /// </remarks>
    public class ATCommResponsePacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 9;

        /// <summary>
        /// Constructor. Instantiates new instance of ATCommResponsePacket
        /// </summary>
        /// <param name="frameID">Frame ID of the packet</param>
        /// <param name="command">2 letter AT command</param>
        /// <param name="response_status">Status of the AT command</param>
        /// <param name="comm_value">AT command response value. Optional</param>
        /// <exception cref="ArgumentException">Thrown when the command is not a 2 character string</exception>
        public ATCommResponsePacket(byte frameID, string command, ATCommandStatus.Byte response_status = ATCommandStatus.Byte.OK, byte[] comm_value = null)
            : base(ApiFrameType.Byte.AT_COMMAND_RESPONSE)
        {
            if (command == null || command.Length != 2)
            {
                throw new ArgumentException("Invalid command " + command);
            }

            FrameID = frameID;
            Command = command;
            ResponseStatus = response_status;
            CommValue = comm_value ?? (new byte[0]);
        }

        public string Command { get; protected set; }
        public ATCommandStatus.Byte ResponseStatus { get; protected set; }
        public byte[] CommValue { get; protected set; }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New ATCommResponsePacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new ATCommResponsePacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.AT_COMMAND_RESPONSE)
            {
                throw new InvalidPacketException("This packet is not an AT command response packet");
            }
            if (ATCommandStatus.Get(raw[7]) == null)
            {
                throw new InvalidPacketException("Invalid command status");
            }

            return new ATCommResponsePacket(raw[4], Encoding.UTF8.GetString(raw.Skip(5).Take(2).ToArray()), (ATCommandStatus.Byte)raw[7], raw.Skip(8).Reverse().Skip(1).Reverse().ToArray());
        }

        public override bool NeedsID()
        {
            return true;
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = Utils.AsciiToBytes(Command);
            if (CommValue != null)
            {
                byte[] final = new byte[ret.Length + CommValue.Length + 1];
                Array.Copy(ret, final, ret.Length);
                final[ret.Length] = (byte)ResponseStatus;
                Array.Copy(CommValue, 0, final, ret.Length + 1, CommValue.Length);
                return final;
            } else
            {
                byte[] final = new byte[ret.Length + 1];
                Array.Copy(ret, final, ret.Length);
                final[ret.Length] = (byte)ResponseStatus;
                return final;
            }
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.COMMAND,         Command },
                { DictKeys.AT_CMD_STATUS,   ResponseStatus },
                { DictKeys.RF_DATA,         CommValue }
            };
        }
    }

    public class ReceivePacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 16;

        public XBee64BitAddress X64bit_addr { get; protected set; }
        public XBee16BitAddress X16bit_addr { get; protected set; }
        public ReceiveOptions ReceiveOptions { get; protected set; }
        public byte[] Rfdata { get; protected set; }

        /// <summary>
        /// Constructor. Instantiates a new ReceivePacket class object
        /// </summary>
        /// <param name="x64bit_addr">64 bit address object</param>
        /// <param name="x16bit_addr">16 bit address object</param>
        /// <param name="receive_options">Bitfield with bit options</param>
        /// <param name="rf_data">Optional RF data</param>
        public ReceivePacket(XBee64BitAddress x64bit_addr, XBee16BitAddress x16bit_addr, ReceiveOptions receive_options, byte[] rf_data = null)
            : base(ApiFrameType.Byte.RECEIVE_PACKET)
        {
            X64bit_addr = x64bit_addr;
            X16bit_addr = x16bit_addr;
            ReceiveOptions = receive_options;
            Rfdata = rf_data ?? (new byte[0]);
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New ReceivePacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new ReceivePacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.RECEIVE_PACKET)
            {
                throw new InvalidPacketException("This packet is not a receive packet");
            }

            return new ReceivePacket(new XBee64BitAddress(raw.Skip(4).Take(8).ToArray()), new XBee16BitAddress(raw.Skip(12).Take(2).ToArray()),
                (ReceiveOptions)raw[14], raw.Skip(15).Reverse().Skip(1).Reverse().ToArray());
        }

        /// <summary>
        /// Returns whether the packet needs an ID or not
        /// </summary>
        /// <returns>False</returns>
        public override bool NeedsID()
        {
            return false;
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = new byte[11 + Rfdata.Length];

            Array.Copy(X64bit_addr.Address, ret, 8);
            Array.Copy(X16bit_addr.Address, 0, ret, 8, 2);
            Array.Copy(new byte[] { (byte)ReceiveOptions }, 0, ret, 10, 1);
            if (Rfdata != null)
            {
                Array.Copy(Rfdata, 0, ret, 11, Rfdata.Length);
            }

            return ret;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.X64BIT_ADDR,     X64bit_addr },
                { DictKeys.X16BIT_ADDR,     X16bit_addr },
                { DictKeys.RECEIVE_OPTIONS, ReceiveOptions },
                { DictKeys.RF_DATA,         Rfdata }
            };
        }
    }

    public class RemoteATCommandPacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 19;

        public XBee64BitAddress X64bit_addr { get; protected set; }
        public XBee16BitAddress X16bit_addr { get; protected set; }
        public RemoteATCmdOptions TransmitOptions { get; protected set; }
        public string Command { get; protected set; }
        public byte[] Parameter { get; protected set; }

        /// <summary>
        /// Constructor. Instantiates a new RemoteATCommandPacket object
        /// </summary>
        /// <param name="frameID">The frame ID of the packet</param>
        /// <param name="x64bit_addr">64 bit address destination address</param>
        /// <param name="x16bit_addr">16 bit destination address</param>
        /// <param name="transmit_options">Bitfield of transmit options</param>
        /// <param name="command">AT command to send</param>
        /// <param name="parameter">Optional AT command parameter</param>
        /// <exception cref="ArgumentException">Thrown when command is not a 2 character string</exception>
        public RemoteATCommandPacket(byte frameID, XBee64BitAddress x64bit_addr,
            XBee16BitAddress x16bit_addr, RemoteATCmdOptions transmit_options, string command,
            byte[] parameter = null) : base(ApiFrameType.Byte.REMOTE_AT_COMMAND_REQUEST)
        {
            if (command == null || command.Length != 2)
            {
                throw new ArgumentException("Invalid command " + command);
            }

            FrameID = frameID;
            X64bit_addr = x64bit_addr;
            X16bit_addr = x16bit_addr;
            TransmitOptions = transmit_options;
            Command = command;
            Parameter = parameter ?? (new byte[0]);
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New RemoteATCommandPacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new RemoteATCommandPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.REMOTE_AT_COMMAND_REQUEST)
            {
                throw new InvalidPacketException("This packet is not a remote AT command request packet");
            }

            return new RemoteATCommandPacket(raw[4], new XBee64BitAddress(raw.Skip(5).Take(8).ToArray()),
                new XBee16BitAddress(raw.Skip(13).Take(2).ToArray()), (RemoteATCmdOptions)raw[15],
                Encoding.ASCII.GetString(raw.Skip(16).Take(2).ToArray()), raw.Skip(18).Reverse().Skip(1).Reverse().ToArray());
        }

        /// <summary>
        /// Returns whether the packet needs an ID or not
        /// </summary>
        /// <returns>True</returns>
        public override bool NeedsID()
        {
            return true;
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = new byte[13 + Parameter.Length];

            Array.Copy(X64bit_addr.Address, ret, 8);
            Array.Copy(X16bit_addr.Address, 0, ret, 8, 2);
            ret[10] = (byte)TransmitOptions;
            Array.Copy(Utils.AsciiToBytes(Command), 0, ret, 11, 2);
            if (Parameter != null)
            {
                Array.Copy(Parameter, 0, ret, 13, Parameter.Length);
            }

            return ret;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.X64BIT_ADDR,         X64bit_addr },
                { DictKeys.X16BIT_ADDR,         X16bit_addr },
                { DictKeys.TRANSMIT_OPTIONS,    TransmitOptions },
                { DictKeys.COMMAND,             Command },
                { DictKeys.PARAMETER,           Parameter }
            };
        }
    }

    public class RemoteATCommandResponsePacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 19;

        public XBee64BitAddress X64bit_addr { get; protected set; }
        public XBee16BitAddress X16bit_addr { get; protected set; }
        public string Command { get; protected set; }
        public ATCommandStatus.Byte ResponseStatus { get; protected set; }
        public byte[] CommValue { get; protected set; }

        /// <summary>
        /// Constructor. Instantiates a new RemoteATCommandPacket object
        /// </summary>
        /// <param name="frameID">The frame ID of the packet</param>
        /// <param name="x64bit_addr">64 bit address destination address</param>
        /// <param name="x16bit_addr">16 bit destination address</param>
        /// <param name="command">AT command to send</param>
        /// <param name="response_status">The status of the AT command</param>
        /// <param name="comm_value">The AT command response value</param>
        /// <exception cref="ArgumentException">Thrown when command is not a 2 character string</exception>
        public RemoteATCommandResponsePacket(byte frameID, XBee64BitAddress x64bit_addr,
            XBee16BitAddress x16bit_addr, string command, ATCommandStatus.Byte response_status,
            byte[] comm_value = null) : base(ApiFrameType.Byte.REMOTE_AT_COMMAND_RESPONSE)
        {
            if (command == null || command.Length != 2)
            {
                throw new ArgumentException("Invalid command " + command);
            }

            FrameID = frameID;
            X64bit_addr = x64bit_addr;
            X16bit_addr = x16bit_addr;
            Command = command;
            ResponseStatus = response_status;
            CommValue = comm_value ?? (new byte[0]);
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New RemoteATCommandResponsePacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new RemoteATCommandResponsePacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.REMOTE_AT_COMMAND_RESPONSE)
            {
                throw new InvalidPacketException("This packet is not a remote AT command request packet");
            }

            return new RemoteATCommandResponsePacket(raw[4], new XBee64BitAddress(raw.Skip(5).Take(8).ToArray()),
                new XBee16BitAddress(raw.Skip(13).Take(2).ToArray()), Encoding.ASCII.GetString(raw.Skip(15).Take(2).ToArray()),
                (ATCommandStatus.Byte)raw[17], raw.Skip(18).Reverse().Skip(1).Reverse().ToArray());
        }

        /// <summary>
        /// Returns whether the packet needs an ID or not
        /// </summary>
        /// <returns>True</returns>
        public override bool NeedsID()
        {
            return true;
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = new byte[13 + CommValue.Length];

            Array.Copy(X64bit_addr.Address, ret, 8);
            Array.Copy(X16bit_addr.Address, 0, ret, 8, 2);
            Array.Copy(Utils.AsciiToBytes(Command), 0, ret, 10, 2);
            Array.Copy(new byte[] { (byte)ResponseStatus }, 0, ret, 12, 1);
            if (CommValue != null)
            {
                Array.Copy(CommValue, 0, ret, 13, CommValue.Length);
            }

            return ret;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.X64BIT_ADDR,         X64bit_addr },
                { DictKeys.X16BIT_ADDR,         X16bit_addr },
                { DictKeys.COMMAND,             Command },
                { DictKeys.AT_CMD_STATUS,       ResponseStatus },
                { DictKeys.RF_DATA,             CommValue }
            };
        }
    }

    public class TransmitPacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 18;

        public XBee64BitAddress X64bit_addr { get; protected set; }
        public XBee16BitAddress X16bit_addr { get; protected set; }
        public byte BroadcastRadius { get; protected set; }
        public TransmitOptions TransmitOptions { get; protected set; }
        public byte[] Rfdata { get; protected set; }

        /// <summary>
        /// Instantiates a new instance of TransmitPacket
        /// </summary>
        /// <param name="x64bit_addr">64 bit address object</param>
        /// <param name="x16bit_addr">16 bit address object</param>
        /// <param name="receive_options">Bitfield with bit options</param>
        /// <param name="rf_data"></param>
        public TransmitPacket(byte frameID, XBee64BitAddress x64bit_addr, XBee16BitAddress x16bit_addr,
            byte broadcast_radius, TransmitOptions transmit_options, byte[] rf_data = null)
            : base(ApiFrameType.Byte.TRANSMIT_REQUEST)
        {
            FrameID = frameID;
            X64bit_addr = x64bit_addr;
            X16bit_addr = x16bit_addr;
            BroadcastRadius = broadcast_radius;
            TransmitOptions = transmit_options;
            Rfdata = rf_data ?? (new byte[0]);
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New TransmitPacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new TransmitPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.TRANSMIT_REQUEST)
            {
                throw new InvalidPacketException("This packet is not a receive packet");
            }

            return new TransmitPacket(raw[4], new XBee64BitAddress(raw.Skip(5).Take(8).ToArray()),
                new XBee16BitAddress(raw.Skip(13).Take(2).ToArray()), raw[15], (TransmitOptions)raw[16],
                raw.Skip(17).Reverse().Skip(1).Reverse().ToArray());
        }

        /// <summary>
        /// Returns whether the packet needs an ID or not
        /// </summary>
        /// <returns>True</returns>
        public override bool NeedsID()
        {
            return true;
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = new byte[12 + Rfdata.Length];

            Array.Copy(X64bit_addr.Address, ret, 8);
            Array.Copy(X16bit_addr.Address, 0, ret, 8, 2);
            Array.Copy(new byte[] { BroadcastRadius }, 0, ret, 10, 1);
            Array.Copy(new byte[] { (byte)TransmitOptions }, 0, ret, 11, 1);
            if (Rfdata != null)
            {
                Array.Copy(Rfdata, 0, ret, 12, Rfdata.Length);
            }

            return ret;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.X64BIT_ADDR,         X64bit_addr },
                { DictKeys.X16BIT_ADDR,         X16bit_addr },
                { DictKeys.BROADCAST_RADIUS,    BroadcastRadius },
                { DictKeys.TRANSMIT_OPTIONS,    TransmitOptions },
                { DictKeys.RF_DATA,             Rfdata }
            };
        }
    }

    public class TransmitStatusPacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 11;

        public XBee16BitAddress X16bit_addr { get; protected set; }
        public byte TransmitRetryCount { get; protected set; }
        public TransmitStatus.Byte TransmitStatus { get; protected set; }
        public DiscoveryStatus.Byte DiscoveryStatus { get; protected set; }

        /// <summary>
        /// Instantiates a new instance of TransmitPacket
        /// </summary>
        /// <param name="frameID">The frame ID of the packet</param>
        /// <param name="x16bit_addr">16 bit address object</param>
        /// <param name="transmit_retry_count">The number of transmission retries that took place</param>
        /// <param name="transmit_status">Transmit status. Optional</param>
        /// <param name="discovery_status">Discovery status. Optional</param>
        public TransmitStatusPacket(byte frameID, XBee16BitAddress x16bit_addr, byte transmit_retry_count,
            TransmitStatus.Byte transmit_status = XBeeAPI.TransmitStatus.Byte.SUCCESSS,
            DiscoveryStatus.Byte discovery_status = XBeeAPI.DiscoveryStatus.Byte.NO_DISCOVERY_OVERHEAD)
            : base(ApiFrameType.Byte.TRANSMIT_STATUS)
        {
            FrameID = frameID;
            X16bit_addr = x16bit_addr;
            TransmitRetryCount = transmit_retry_count;
            TransmitStatus = transmit_status;
            DiscoveryStatus = discovery_status;
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New TransmitStatusPacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new TransmitStatusPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.TRANSMIT_STATUS)
            {
                throw new InvalidPacketException("This packet is not a receive packet");
            }

            return new TransmitStatusPacket(raw[4], new XBee16BitAddress(raw.Skip(5).Take(2).ToArray()), raw[7],
                (TransmitStatus.Byte)raw[8], (DiscoveryStatus.Byte)raw[9]);
        }

        /// <summary>
        /// Returns whether the packet needs an ID or not
        /// </summary>
        /// <returns>True</returns>
        public override bool NeedsID()
        {
            return true;
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = new byte[5];

            Array.Copy(X16bit_addr.Address, ret, 2);
            ret[2] = TransmitRetryCount;
            ret[3] = (byte)TransmitStatus;
            ret[4] = (byte)DiscoveryStatus;

            return ret;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.X16BIT_ADDR,         X16bit_addr },
                { DictKeys.TRANS_R_COUNT,       TransmitRetryCount },
                { DictKeys.TS_STATUS,           TransmitStatus },
                { DictKeys.DS_STATUS,           DiscoveryStatus }
            };
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public class ModemStatusPacket : XBeeAPIPacket
    {
        /// <summary>
        /// Not implemented
        /// </summary>
        public ModemStatusPacket(ModemStatus.Byte modem_status) : base(ApiFrameType.Byte.MODEM_STATUS)
        {
            throw new NotImplementedException();
        }

        public static new ModemStatusPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            throw new NotImplementedException("ATCommQueuePacket.CreatePacket not implemented");
        }

        public override bool NeedsID()
        {
            throw new NotImplementedException();
        }

        protected override byte[] GetAPIPacketSpecData()
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a RouteInfoPacket
    /// </summary>
    /// <remarks>
    /// This is not in the original Python XBee API
    /// </remarks>
    public class RouteInfoPacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 46;

        public byte SourceEvent { get; protected set; }
        public byte DataLength { get; protected set; }
        public int Timestamp { get; protected set; }
        public byte TimeoutCount { get; protected set; }
        public XBee64BitAddress DestinationAddress { get; protected set; }
        public XBee64BitAddress SourceAddress { get; protected set; }
        public XBee64BitAddress ResponderAddress { get; protected set; }
        public XBee64BitAddress ReceiverAddress { get; protected set; }

        /// <summary>
        /// Constructor. Instantiates a new RouteInfoPacket object
        /// </summary>
        /// <param name="source_event">Source event. 0x11=NACK, 0x12=Trace route</param>
        /// <param name="data_length">Number of bytes that follow, excluding checksum. Generally fixed, but may increase in future versions</param>
        /// <param name="timestamp">System timer value on the node when generating this packet.</param>
        /// <param name="timeout_count">The number of MAC ACK timeouts that occurred</param>
        /// <param name="dest_addr">Address of the source node, ie the node that the transmit request was directed at</param>
        /// <param name="src_addr">Address of the final destination, ie the node that sent the transmit request</param>
        /// <param name="resp_addr">Address of the node that generated this packet</param>
        /// <param name="recv_addr">Address of the next node on the path to the source node</param>
        public RouteInfoPacket(byte source_event, byte data_length, int timestamp, byte timeout_count,
            XBee64BitAddress dest_addr, XBee64BitAddress src_addr, XBee64BitAddress resp_addr, XBee64BitAddress recv_addr)
            : base(ApiFrameType.Byte.ROUTE_INFO_PACKET)
        {
            SourceEvent = source_event;
            DataLength = data_length;
            Timestamp = timestamp;
            TimeoutCount = timeout_count;
            DestinationAddress = dest_addr;
            SourceAddress = src_addr;
            ResponderAddress = recv_addr;
            ReceiverAddress = recv_addr;
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New RouteInfoPacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new RouteInfoPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.ROUTE_INFO_PACKET)
            {
                throw new InvalidPacketException("This packet is not a route info packet");
            }

            return new RouteInfoPacket(raw[4], raw[5], (short)Utils.BytesToInt(raw.Skip(6).Take(4).ToArray()), raw[10],
                new XBee64BitAddress(raw.Skip(13).Take(8).ToArray()), new XBee64BitAddress(raw.Skip(21).Take(8).ToArray()),
                new XBee64BitAddress(raw.Skip(29).Take(8).ToArray()), new XBee64BitAddress(raw.Skip(37).Take(8).ToArray()));
        }

        /// <summary>
        /// Returns whether the packet needs an ID or not
        /// </summary>
        /// <returns>False</returns>
        public override bool NeedsID()
        {
            return false;
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = new byte[41];

            ret[0] = SourceEvent;
            ret[1] = DataLength;
            Array.Copy(Utils.IntToBytes(Timestamp), 0, ret, 2, 4);
            ret[6] = TimeoutCount;
            ret[7] = ret[8] = 0;
            Array.Copy(DestinationAddress.Address, 0, ret, 9, 8);
            Array.Copy(SourceAddress.Address, 0, ret, 17, 8);
            Array.Copy(ResponderAddress.Address, 0, ret, 25, 8);
            Array.Copy(ReceiverAddress.Address, 0, ret, 33, 8);

            return ret;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.SOURCE_EVENT,        SourceEvent },
                { DictKeys.DATA_LENGTH,         DataLength },
                { DictKeys.TIMESTAMP,           Timestamp },
                { DictKeys.TIMEOUT_COUNT,       TimeoutCount },
                { DictKeys.RESERVED,            0 },
                { DictKeys.DESTINATION_ADDRESS, DestinationAddress },
                { DictKeys.SOURCE_ADDRESS,      SourceAddress },
                { DictKeys.RESPONDER_ADDRESS,   ResponderAddress },
                { DictKeys.RECEIVER_ADDRESS,    ReceiverAddress }
            };
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public class IODataSampleRxIndicatorPacket : XBeeAPIPacket
    {
        /// <summary>
        /// Not implemented
        /// </summary>
        public IODataSampleRxIndicatorPacket(XBee64BitAddress x64bit_addr, XBee16BitAddress x16bit_addr,
            byte receive_options, byte[] rf_data = null) : base(ApiFrameType.Byte.IO_DATA_SAMPLE_RX_INDICATOR)
        {
            throw new NotImplementedException();
        }

        public static new IODataSampleRxIndicatorPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            throw new NotImplementedException("ATCommQueuePacket.CreatePacket not implemented");
        }

        public override bool NeedsID()
        {
            throw new NotImplementedException();
        }

        protected override byte[] GetAPIPacketSpecData()
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            throw new NotImplementedException();
        }
    }

    public class ExplicitAddressingPacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 24;

        public XBee64BitAddress X64bit_addr { get; protected set; }
        public XBee16BitAddress X16bit_addr { get; protected set; }
        public byte SourceEndpoint { get; protected set; }
        public byte DestEndpoint { get; protected set; }
        public ushort ClusterID { get; protected set; }
        public ushort ProfileID { get; protected set; }
        public byte BroadcastRadius { get; protected set; }
        public byte TransmitOptions { get; protected set; }
        public byte[] RfData { get; protected set; }

        /// <summary>
        /// Constructor. Initiates new instance of ExplicitAddressingPacket with the provided parameters
        /// </summary>
        /// <param name="frameID">The frame ID of the packet</param>
        /// <param name="x64bit_addr">64 bit address</param>
        /// <param name="x16bit_addr">16 bit address</param>
        /// <param name="source_endpoint">Source endpoint</param>
        /// <param name="dest_endpoint">Destination endpoint</param>
        /// <param name="cluster_id">Custer ID</param>
        /// <param name="profile_id">Profile ID</param>
        /// <param name="broadcast_radius">Maximum number of hops a broadcast transmission can occur</param>
        /// <param name="transmit_options">Bitfield of supportes transmission options</param>
        /// <param name="rf_data">RF data that is sent to the destination device. Optional</param>
        public ExplicitAddressingPacket(byte frameID, XBee64BitAddress x64bit_addr, XBee16BitAddress x16bit_addr,
            byte source_endpoint, byte dest_endpoint, ushort cluster_id, ushort profile_id, byte broadcast_radius,
            byte transmit_options, byte[] rf_data = null) : base(ApiFrameType.Byte.EXPLICIT_ADDRESSING)
        {
            FrameID = frameID;
            X64bit_addr = x64bit_addr;
            X16bit_addr = x16bit_addr;
            SourceEndpoint = source_endpoint;
            DestEndpoint = dest_endpoint;
            ClusterID = cluster_id;
            ProfileID = profile_id;
            BroadcastRadius = broadcast_radius;
            TransmitOptions = transmit_options;
            RfData = rf_data ?? (new byte[0]);
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </summary>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New ExplicitAddressingPacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new ExplicitAddressingPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.EXPLICIT_ADDRESSING)
            {
                throw new InvalidPacketException("This packet is not a receive packet");
            }

            return new ExplicitAddressingPacket(raw[4], new XBee64BitAddress(raw.Skip(5).Take(8).ToArray()),
                new XBee16BitAddress(raw.Skip(13).Take(2).ToArray()), raw[15], raw[16],
                (ushort)Utils.BytesToInt(raw.Skip(17).Take(2).ToArray()), (ushort)Utils.BytesToInt(raw.Skip(19).Take(2).ToArray()),
                raw[21], raw[22], raw.Skip(23).Reverse().Skip(1).Reverse().ToArray());
        }

        /// <summary>
        /// Returns whether the packet needs an ID or not
        /// </summary>
        /// <returns>True</returns>
        public override bool NeedsID()
        {
            return true;
        }

        /// <summary>
        /// Returns the frame specific data without frame type or frame ID fields
        /// </summary>
        /// <returns>Byte array of the frame specific data without frame type or frame ID fields</returns>
        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = new byte[18 + RfData.Length];

            Array.Copy(X64bit_addr.Address, ret, 8);
            Array.Copy(X16bit_addr.Address, 0, ret, 8, 2);
            ret[10] = SourceEndpoint;
            ret[11] = DestEndpoint;
            Array.Copy(Utils.IntToBytes(ClusterID), 0, ret, 12, 2);
            Array.Copy(Utils.IntToBytes(ProfileID), 0, ret, 14, 2);
            ret[16] = BroadcastRadius;
            ret[17] = TransmitOptions;
            if (RfData != null)
            {
                Array.Copy(RfData, 0, ret, 18, RfData.Length);
            }

            return ret;
        }

        /// <summary>
        /// Similar to GetAPIPacketSpecData but returns data as dictionary
        /// </summary>
        /// <returns>Data as a dictionary</returns>
        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.X64BIT_ADDR,         X64bit_addr },
                { DictKeys.X16BIT_ADDR,         X16bit_addr },
                { DictKeys.SOURCE_ENDPOINT,     SourceEndpoint },
                { DictKeys.DEST_ENDPOINT,       DestEndpoint },
                { DictKeys.CLUSTER_ID,          ClusterID },
                { DictKeys.PROFILE_ID,          ProfileID },
                { DictKeys.BROADCAST_RADIUS,    BroadcastRadius },
                { DictKeys.TRANSMIT_OPTIONS,    TransmitOptions },
                { DictKeys.RF_DATA,             RfData }
            };
        }
    }

    /// <summary>
    /// Represents an explicit RX indicator packet.
    /// </summary>
    /// <remarks>
    /// Packet is built using the parameters of the constructor or providing a valid API payload.<para/>
    /// When the modem receives an RF packet it is sent out the UART using this message type (when ``AO= 1``).<para/>
    /// This packet is received when external devices send explicit addressing packets to this module.<para/>
    /// Among received data, some options can also be received indicating transmission parameters.
    /// </remarks>
    public class ExplicitRXIndicatorPacket : XBeeAPIPacket
    {
        private const int MIN_PACKET_LENGTH = 16;

        public XBee64BitAddress X64BitAddress { get; protected set; }
        public XBee16BitAddress X16BitAddress { get; protected set; }
        public byte SourceEndpoint { get; protected set; }
        public byte DestEndpoint { get; protected set; }
        public ushort ClusterID { get; protected set; }
        public ushort ProfileID { get; protected set; }
        public ReceiveOptions ReceiveOptions { get; protected set; }
        public byte[] RfData { get; protected set; }

        /// <summary>
        /// Not implemented
        /// </summary>
        public ExplicitRXIndicatorPacket(XBee64BitAddress x64bitAddr, XBee16BitAddress x16bitAddr, byte sourceEndpoint,
            byte destEndpoint, ushort clusterID, ushort profileID, ReceiveOptions receiveOptions, byte[] rfData)
            : base(ApiFrameType.Byte.EXPLICIT_RX_INDICATOR)
        {
            X64BitAddress = x64bitAddr;
            X16BitAddress = x16bitAddr;
            SourceEndpoint = sourceEndpoint;
            DestEndpoint = destEndpoint;
            ClusterID = clusterID;
            ProfileID = profileID;
            ReceiveOptions = receiveOptions;
            RfData = rfData;
        }

        /// <summary>
        /// Creates a full XBeePacket with the given parameters
        /// </summary>
        /// <remarks>
        /// If operatingMode is API2 (API escaped), this method de-escapes 'raw' and builds the XBeePacket.
        /// Then, you can use the method Output to get the escaped byte array or not escaped
        /// </remarks>
        /// <param name="raw">Byte array with which the frame will be packet. Must be a full packet.</param>
        /// <param name="operatingMode">The mode in which the frame was captured</param>
        /// <returns>New ExplicitAddressingPacket derives from raw bytes</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static new ExplicitRXIndicatorPacket CreatePacket(byte[] raw, OperatingMode.Byte operatingMode)
        {
            if (operatingMode != OperatingMode.Byte.ESCAPED_API_MODE
                && operatingMode != OperatingMode.Byte.API_MODE)
            {
                throw new InvalidOperatingModeException(OperatingMode.Get(operatingMode) + " is not supported");
            }

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                raw = UnescapeData(raw);

            CheckAPIPacket(raw, MIN_PACKET_LENGTH);

            if (raw[3] != (byte)ApiFrameType.Byte.EXPLICIT_RX_INDICATOR)
            {
                throw new InvalidPacketException("This packet is not a receive packet");
            }

            return new ExplicitRXIndicatorPacket(new XBee64BitAddress(raw.Skip(4).Take(8).ToArray()), new XBee16BitAddress(raw.Skip(12).Take(2).ToArray()),
                raw[14], raw[15], (ushort)Utils.BytesToInt(raw.Skip(16).Take(2).ToArray()), (ushort)Utils.BytesToInt(raw.Skip(18).Take(2).ToArray()),
                (ReceiveOptions)raw[20], raw.Skip(21).Reverse().Skip(1).Reverse().ToArray());
        }

        public override bool NeedsID()
        {
            return false;
        }

        protected override byte[] GetAPIPacketSpecData()
        {
            byte[] ret = new byte[17 + RfData.Length];

            Array.Copy(X64BitAddress.Address, ret, 8);
            Array.Copy(X16BitAddress.Address, 0, ret, 8, 2);
            ret[10] = SourceEndpoint;
            ret[11] = DestEndpoint;
            Array.Copy(Utils.IntToBytes(ClusterID), 0, ret, 12, 2);
            Array.Copy(Utils.IntToBytes(ProfileID), 0, ret, 14, 2);
            ret[16] = (byte)ReceiveOptions;
            if (RfData != null)
            {
                Array.Copy(RfData, 0, ret, 17, RfData.Length);
            }

            return ret;
        }

        protected override Dictionary<DictKeys, object> GetAPIPacketSpecDataDict()
        {
            return new Dictionary<DictKeys, object>()
            {
                { DictKeys.X64BIT_ADDR,     X64BitAddress.Address },
                { DictKeys.X16BIT_ADDR,     X16BitAddress.Address },
                { DictKeys.SOURCE_ENDPOINT, SourceEndpoint },
                { DictKeys.DEST_ENDPOINT,   DestEndpoint },
                { DictKeys.CLUSTER_ID,      ClusterID },
                { DictKeys.PROFILE_ID,      ProfileID },
                { DictKeys.RECEIVE_OPTIONS, ReceiveOptions },
                { DictKeys.RF_DATA,         RfData }
            };
        }
    }
    #endregion

    #region DeviceCloud
    #endregion

    #region Factory
    /*  This module provides functionality to build XBee packets from
        bytearray returning the appropriate XBeePacket subclass.
        All the API and API2 logic is already included so all packet reads are
        independent of the XBee operating mode.
        Two API modes are supported and both can be enabled using the ``AP``
        (API Enable) command::
            API1 - API Without Escapes
            The data frame structure is defined as follows:
              Start Delimiter          Length                   Frame Data                   Checksum
                  (Byte 1)            (Bytes 2-3)               (Bytes 4-n)                (Byte n + 1)
            +----------------+  +-------------------+  +--------------------------- +  +----------------+
            |      0x7E      |  |   MSB   |   LSB   |  |   API-specific Structure   |  |     1 Byte     |
            +----------------+  +-------------------+  +----------------------------+  +----------------+
                           MSB = Most Significant Byte, LSB = Least Significant Byte
        API2 - API With Escapes
        The data frame structure is defined as follows::
              Start Delimiter          Length                   Frame Data                   Checksum
                  (Byte 1)            (Bytes 2-3)               (Bytes 4-n)                (Byte n + 1)
            +----------------+  +-------------------+  +--------------------------- +  +----------------+
            |      0x7E      |  |   MSB   |   LSB   |  |   API-specific Structure   |  |     1 Byte     |
            +----------------+  +-------------------+  +----------------------------+  +----------------+
                                \___________________________________  _________________________________/
                                                                    \/
                                                        Characters Escaped If Needed
                           MSB = Most Significant Byte, LSB = Least Significant Byte
        When sending or receiving an API2 frame, specific data values must be
        escaped (flagged) so they do not interfere with the data frame sequencing.
        To escape an interfering data byte, the byte 0x7D is inserted before
        the byte to be escaped XOR'd with 0x20.
        The data bytes that need to be escaped:
        - ``0x7E`` - Frame Delimiter - :attr:`.SpecialByte.
        - ``0x7D`` - Escape
        - ``0x11`` - XON
        - ``0x13`` - XOFF
        The length field has a two-byte value that specifies the number of
        bytes that will be contained in the frame data field. It does not include the
        checksum field.
        The frame data  forms an API-specific structure as follows::
              Start Delimiter          Length                             Frame Data                         Checksum
                  (Byte 1)            (Bytes 2-3)                         (Bytes 4-n)                      (Byte n + 1)
            +----------------+  +-------------------+  +-------------------------------------------- +  +----------------+
            |      0x7E      |  |   MSB   |   LSB   |  |   API-specific Structure                    |  |     1 Byte     |
            +----------------+  +-------------------+  +---------------------------------------------+  +----------------+
                                                       /                                             \
                                                      /  API Identifier      Identifier specific data \
                                                      +------------------+  +--------------------------+
                                                      |       cmdID      |  |           cmdData        |
                                                      +------------------+  +--------------------------+
        The cmdID frame (API-identifier) indicates which API messages
        will be contained in the cmdData frame (Identifier-specific data).
        To unit_test data integrity, a checksum is calculated and verified on
        non-escaped data.
     */
     /// <summary>
     /// This provides two methods. One to determine the the packet type of a raw byte array,
     /// and the other to create a packet of that type from that byte array.
     /// </summary>
    public static class Factory
    {
        /// <summary>
        /// Given a raw byte array and the operating mode it was received it, returns a packet of the corresponding type.
        /// </summary>
        /// <param name="packet_bytearray">Raw byte array of the packet</param>
        /// <param name="operatingMode">Operating mode the packet was received in</param>
        /// <returns>XBeePacket object, can be cast to the type the byte array corresponds to</returns>
        /// <exception cref="InvalidOperatingModeException">Operating mode not supported</exception>
        /// <exception cref="InvalidPacketException">Thrown if the packet is not formatted correctly, or is of the wrong frame type</exception>
        public static XBeeAPIPacket BuildFrame(byte[] packet_bytearray, OperatingMode.Byte operatingMode = OperatingMode.Byte.API_MODE)
        {
            ApiFrameType.Byte frame_type = GetFrameType(packet_bytearray);

            if (frame_type == ApiFrameType.Byte.GENERIC)
                return GenericXBeePacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.AT_COMMAND)
                return ATCommPacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.AT_COMMAND_QUEUE)
                return ATCommQueuePacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.AT_COMMAND_RESPONSE)
                return ATCommResponsePacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.RECEIVE_PACKET)
                return ReceivePacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.REMOTE_AT_COMMAND_REQUEST)
                return RemoteATCommandPacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.REMOTE_AT_COMMAND_RESPONSE)
                return RemoteATCommandResponsePacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.TRANSMIT_REQUEST)
                return TransmitPacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.TRANSMIT_STATUS)
                return TransmitStatusPacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.MODEM_STATUS)
                return ModemStatusPacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.IO_DATA_SAMPLE_RX_INDICATOR)
                return IODataSampleRxIndicatorPacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.EXPLICIT_ADDRESSING)
                return ExplicitAddressingPacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.EXPLICIT_RX_INDICATOR)
                return ExplicitRXIndicatorPacket.CreatePacket(packet_bytearray, operatingMode);

            else if (frame_type == ApiFrameType.Byte.ROUTE_INFO_PACKET)
                return RouteInfoPacket.CreatePacket(packet_bytearray, operatingMode);

            else
                return UnknownXBeePacket.CreatePacket(packet_bytearray, operatingMode);
        }

        /// <summary>
        /// Returns the ApiFrameType of the provided byte array. Can be used to cast the result of BuildFrame correctly
        /// </summary>
        /// <param name="packet_bytearray">Raw byte array of packet</param>
        /// <returns>ApiFrameType of the packet</returns>
        public static ApiFrameType.Byte GetFrameType(byte[] packet_bytearray)
        {
            return (ApiFrameType.Byte)packet_bytearray[3];
        }
    }
    #endregion

    #region Network
    #endregion

    #region Raw
    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XBeeAPI
{
    #region AccessPoint
    #endregion

    #region Address
    public class XBee16BitAddress : IEnumerable
    {
        // Regex for string pattern
        public const string PATTERN = "(0[xX])?[0-9a-fA-F]{1,4}";
        public static Regex rx = new Regex(PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Constant addresses for comparison
        /// <summary>
        /// 0000
        /// </summary>
        public static XBee16BitAddress COORDINATOR_ADDRESS = new XBee16BitAddress("0000");
        /// <summary>
        /// FFFF
        /// </summary>
        public static XBee16BitAddress BROADCAST_ADDRESS = new XBee16BitAddress("FFFF");
        /// <summary>
        /// FFFE
        /// </summary>
        public static XBee16BitAddress UNKNOWN_ADDRESS = new XBee16BitAddress("FFFE");

        /// <summary>
        /// The 2 byte array that is the actual address
        /// </summary>
        public byte[] Address { get; private set; }

        /// <summary>
        /// Class constructor. Instantiates a new XBee16BitAddress object with the provided parameters.
        /// </summary>
        /// <param name="address">2 byte array. If 1 byte is provided, it is assumed to be lsb and a zero byte is prepended to it</param>
        /// <exception cref="ArgumentException">Argument is the wrong number of bytes</exception>
        /// <exception cref="NullReferenceException">Thrown if address is null</exception>
        public XBee16BitAddress(params byte[] address)
        {
            if (address.Length < 1)
                throw new ArgumentException("Address must contain at least 1 byte");
            if (address.Length > 2)
                throw new ArgumentException("Address can't contain more than 2 bytes");

            if (address.Length == 1)
                Address = new byte[] { 0 }.Concat(address).ToArray();
            else
                Address = address;
        }

        /// <summary>
        /// Class constructor. Instantiates a new XBee16BitAddress object with the provided parameters.
        /// </summary>
        /// <param name="address">Hex string. Represents 1-2 bytes</param>
        /// <exception cref="ArgumentException">Argument is formatted incorrectly</exception>
        /// <exception cref="Exception">This shouldn't happen</exception>
        public XBee16BitAddress(string address)
        {
            if (address.Length < 1)
                throw new ArgumentException("Address must contain at least 1 digit");
            if (!rx.IsMatch(address))
                throw new ArgumentException("Address not formatted validly");

            byte[] conv = Utils.HexStringToBytes(address);
            if (conv.Length == 1)
            {
                Address = new byte[2] { 0, conv[0] };
            } else if (conv.Length == 2) {
                Address = conv;
            } else
            {
                throw new Exception("Something weird happened initializing XBee16BitAddress. Check your regex string, idk");
            }
        }

        /// <summary>
        /// Return IEnumerable of this instance's byte address
        /// </summary>
        /// <returns>IEnumerable of this instance's byte address</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Address.GetEnumerator();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// Compares this address to another
        /// </summary>
        /// <param name="other">Another 16 bit address</param>
        /// <returns>True if arrays are equal byte-for-byte, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(XBee16BitAddress))
            {
                return false;
            } else
            {
                return Address.SequenceEqual(((XBee16BitAddress)obj).Address);
            }
        }

        public override int GetHashCode()
        {
            return Utils.BytesToInt(Address).GetHashCode();
        }
    }

    public class XBee64BitAddress : IEnumerable
    {
        public const string DEVICE_ID_SEPARATOR = "-";
        public const string DEVICE_ID_MAC_SEPARATOR = "FF";
        public const string PATTERN = "(0[xX])?[0-9a-fA-F]{1,16}";
        public static Regex rx = new Regex(PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 00 00 00 00 00 00 00 00
        /// </summary>
        public static XBee64BitAddress COORDINATOR_ADDRESS = new XBee64BitAddress("0000");
        /// <summary>
        /// 00 00 00 00 00 00 FF FF
        /// </summary>
        public static XBee64BitAddress BROADCAST_ADDRESS = new XBee64BitAddress("FFFF");
        /// <summary>
        /// FF FF FF FF FF FF FF FF
        /// </summary>
        public static XBee64BitAddress UNKNOWN_ADDRESS = new XBee64BitAddress("FFFFFFFFFFFFFFFF");

        /// <summary>
        /// The 2 byte array that is the actual address
        /// </summary>
        public byte[] Address { get; private set; }

        /// <summary>
        /// Class constructor. Instantiates a new XBee64BitAddress object with the provided parameters.
        /// </summary>
        /// <param name="address">Hex string. Represents up to 8 bytes</param>
        /// <exception cref="ArgumentException">Argument is formatted incorrectly</exception>
        public XBee64BitAddress(string address)
        {
            if (address.Length < 1)
                throw new ArgumentException("Address must contain at least 1 byte");
            if (!rx.IsMatch(address))
                throw new ArgumentException("Address not formatted validly");

            Address = new byte[8];
            byte[] conv = Utils.HexStringToBytes(address);
            Array.Clear(Address, 0, 8 - conv.Length);
            Array.Copy(conv, 0, Address, 8 - conv.Length, conv.Length);
        }

        /// <summary>
        /// Class constructor. Instantiates a new XBee16BitAddress object with the provided parameters.
        /// </summary>
        /// <param name="hsb">High significant byte</param>
        /// <param name="lsb">Low significant byte</param>
        /// <exception cref="ArgumentException">Argument is the wrong number of bytes</exception>
        public XBee64BitAddress(params byte[] address)
        {
            if (address.Length < 1)
                throw new ArgumentException("Address must contain at least 1 byte");
            if (address.Length > 8)
                throw new ArgumentException("Address can't contain more than 8 bytes");

            // Prepend zeros if address isn't 8 bytes
            byte[] final = new byte[8];
            Array.Clear(final, 0, 8 - address.Length);
            Array.Copy(final, 0, final, 8 - address.Length, address.Length);

            Address = address;
        }

        /// <summary>
        /// Return IEnumerable of this instance's byte address
        /// </summary>
        /// <returns>IEnumerable of this instance's byte address</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Address.GetEnumerator();
        }

        /// <summary>
        /// Compares this address to another
        /// </summary>
        /// <param name="other">Another 16 bit address</param>
        /// <returns>True if arrays are equal byte-for-byte, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(XBee64BitAddress))
            {
                return false;
            }
            else
            {
                return Address.SequenceEqual(((XBee64BitAddress)obj).Address);
            }
        }

        /// <summary>
        /// Returns a hex string representing the address
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Utils.BytesToHexString(Address);
        }

        public override int GetHashCode()
        {
            return Utils.BytesToInt(Address).GetHashCode();
        }
    }

    public class XBeeIMEIAddress
    {
        // This class represents an IMEI address used by cellular devices.
        // Not implemented.
    }
    #endregion

    #region ATComm
    public static class ATStringCommand
    {
        public static string NI = "NI";
        public static string KY = "KY";
        public static string NK = "NK";
        public static string ZU = "ZU";
        public static string ZV = "ZV";
        public static string CC = "CC";
    }

    public static class SpecialByte
    {
        public static byte ESCAPE_BYTE = 0x7D;
        public static byte HEADER_BYTE = 0x7E;
        public static byte XON_BYTE = 0x11;
        public static byte XOFF_BYTE = 0x13;

        private static List<byte> specialBytes = (from prop in typeof(SpecialByte).GetFields() select (byte)prop.GetValue(null)).ToList();

        /// <summary>
        /// Escapes or de-escapes the byte by performing an XOR operation with 0x20 value
        /// </summary>
        /// <param name="value">Value to escape</param>
        /// <returns>Integer escaped</returns>
        public static byte Escape(byte value)
        {
            return (byte)(value ^ 0x20);
        }

        public static bool IsSpecialByte(byte value)
        {
            return specialBytes.Contains(value);
        }
    }

    public class ATCommand
    {
        /// <summary>
        /// Instantiates an ATCommand object
        /// </summary>
        /// <param name="command">2 letter AT command</param>
        /// <param name="parameter">Command parameter</param>
        /// <exception cref="ArgumentException">Thrown if command string isn't 2 character string</exception>
        public ATCommand(string command, string parameter = null)
        {
            if (command == null || command.Length != 2) throw new ArgumentException("Command string must be 2 letters");
            this.Command = Encoding.GetEncoding("UTF-8").GetBytes(command);
            this.Parameter = (parameter == null) ? new byte[0] : Encoding.GetEncoding("UTF-8").GetBytes(parameter);
        }
        /// <summary>
        /// Instantiates an ATCommand object
        /// </summary>
        /// <param name="command">2 letter AT command</param>
        /// <param name="parameter">Command parameter</param>
        /// <exception cref="ArgumentException">Thrown if command string isn't 2 character string</exception>
        public ATCommand(string command, byte[] parameter)
        {
            if (command == null || command.Length != 2) throw new ArgumentException("Command string must be 2 letters");
            this.Command = Encoding.GetEncoding("UTF-8").GetBytes(command);
            this.Parameter = parameter ?? new byte[0];
        }
        /// <summary>
        /// Instantiates an ATCommand object
        /// </summary>
        /// <param name="command">2 letter AT command</param>
        /// <param name="parameter">Command parameter</param>
        /// <exception cref="ArgumentException">Thrown if command string isn't 2 character string</exception>
        public ATCommand(byte[] command, string parameter = null)
        {
            if (command == null || command.Length != 2) throw new ArgumentException("Command string must be 2 letters");
            this.Command = command;
            this.Parameter = (parameter == null) ? new byte[0] : Encoding.GetEncoding("UTF-8").GetBytes(parameter);
        }
        /// <summary>
        /// Instantiates an ATCommand object
        /// </summary>
        /// <param name="command">2 letter AT command</param>
        /// <param name="parameter">Command parameter</param>
        /// <exception cref="ArgumentException">Thrown if command string isn't 2 character string</exception>
        public ATCommand(byte[] command, byte[] parameter)
        {
            if (command == null || command.Length != 2) throw new ArgumentException("Command string must be 2 letters");
            this.Command = command;
            this.Parameter = parameter ?? new byte[0];
        }

        public override string ToString()
        {
            return "Command: " + Command + "\nParameter: " + Parameter;
        }

        public int Length
        {
            get
            {
                return Command.Length + Parameter.Length;
            }
        }

        public byte[] Command { get; }

        public byte[] Parameter { get; set; }
    }

    public class ATCommandResponse
    {
        public ATCommandResponse(ATCommand command, byte[] response = null, ATCommandStatus.Byte status = ATCommandStatus.Byte.OK)
        {
            this.Command = command;
            this.Response = response;
            this.Status = status;
        }

        public ATCommand Command { get; }

        public byte[] Response { get; }

        public ATCommandStatus.Byte Status { get; }
    }
    #endregion

    #region HW
    public static class HardwareVersion
    {
        public enum Byte : byte
        {
            [Description("X09-009")]
            X09_009 = 0x01,

            [Description("X09-019")]
            X09_019 = 0x02,

            [Description("XH9-009")]
            XH9_009 = 0x03,

            [Description("XH9-019")]
            XH9_019 = 0x04,

            [Description("X24-009")]
            X24_009 = 0x05,

            [Description("X24-019")]
            X24_019 = 0x06,

            [Description("X09-001")]
            X09_001 = 0x07,

            [Description("XH9-001")]
            XH9_001 = 0x08,

            [Description("X08-004")]
            X08_004 = 0x09,

            [Description("XC09-009")]
            XC09_009 = 0x0A,

            [Description("XC09-038")]
            XC09_038 = 0x0B,

            [Description("X24-038")]
            X24_038 = 0x0C,

            [Description("X09-009-TX")]
            X09_009_TX = 0x0D,

            [Description("X09-019-TX")]
            X09_019_TX = 0x0E,

            [Description("XH9-009-TX")]
            XH9_009_TX = 0x0F,

            [Description("XH9-019-TX")]
            XH9_019_TX = 0x10,

            [Description("X09-001-TX")]
            X09_001_TX = 0x11,

            [Description("XH9-001-TX")]
            XH9_001_TX = 0x12,

            [Description("XT09B-xxx (Attenuator version)")]
            XT09B_XXX = 0x13,

            [Description("XT09-xxx")]
            XT09_XXX = 0x14,

            [Description("XC08-009")]
            XC08_009 = 0x15,

            [Description("XC08-038")]
            XC08_038 = 0x16,

            [Description("XB24-Axx-xx")]
            XB24_AXX_XX = 0x17,

            [Description("XBP24-Axx-xx")]
            XBP24_AXX_XX = 0x18,

            [Description("XB24-BxIx-xxx and XB24-Z7xx-xxx")]
            XB24_BXIX_XXX = 0x19,

            [Description("XBP24-BxIx-xxx and XBP24-Z7xx-xxx")]
            XBP24_BXIX_XXX = 0x1A,

            [Description("XBP09-DxIx-xxx Digi Mesh")]
            XBP09_DXIX_XXX = 0x1B,

            [Description("XBP09-XCxx-xxx: S3 XSC Compatibility")]
            XBP09_XCXX_XXX = 0x1C,

            [Description("XBP08-Dxx-xxx 868MHz")]
            XBP08_DXXX_XXX = 0x1D,

            [Description("XBP24B: Low cost ZB PRO and PLUS S2B")]
            XBP24B = 0x1E,

            [Description("XB24-WF: XBee 802.11 (Redpine module)")]
            XB24_WF = 0x1F,

            [Description("??????: M-Bus module made by Amber")]
            AMBER_MBUS = 0x20,

            [Description("XBP24C: XBee PRO SMT Ember 357 S2C PRO")]
            XBP24C = 0x21,

            [Description("XB24C: XBee SMT Ember 357 S2C")]
            XB24C = 0x22,

            [Description("XSC_GEN3: XBP9 XSC 24 dBm")]
            XSC_GEN3 = 0x23,

            [Description("SDR_868_GEN3: XB8 12 dBm")]
            SRD_868_GEN3 = 0x24,

            [Description("Abandonated")]
            ABANDONATED = 0x25,

            [Description("900LP (SMT): 900LP on 'S8 HW'")]
            SMT_900LP = 0x26,

            [Description("WiFi Atheros (TH-DIP) XB2S-WF")]
            WIFI_ATHEROS = 0x27,

            [Description("WiFi Atheros (SMT) XB2B-WF")]
            SMT_WIFI_ATHEROS = 0x28,

            [Description("475LP (SMT): Beta 475MHz")]
            SMT_475LP = 0x29,

            [Description("XBee-Cell (TH): XBee Cellular")]
            XBEE_CELL_TH = 0x2A,

            [Description("XLR Module")]
            XLR_MODULE = 0x2B,

            [Description("XB900HP (New Zealand): XB9 NZ HW/SW")]
            XB900HP_NZ = 0x2C,

            [Description("XBP24C (TH-DIP): XBee PRO DIP")]
            XBP24C_TH_DIP = 0x2D,

            [Description("XB24C (TH-DIP): XBee DIP")]
            XB24C_TH_DIP = 0x2E,

            [Description("XLR Baseboard")]
            XLR_BASEBOARD = 0x2F,

            [Description("XBee PRO SMT")]
            XBP24C_S2C_SMT = 0x30,

            [Description("SX Pro")]
            SX_PRO = 0x31,

            [Description("XBP24D: S2D SMT PRO")]
            S2D_SMT_PRO = 0x32,

            [Description("XB24D: S2D SMT Reg")]
            S2D_SMT_REG = 0x33,

            [Description("XBP24D: S2D TH PRO")]
            S2D_TH_PRO = 0x34,

            [Description("XB24D: S2D TH Reg")]
            S2D_TH_REG = 0x35,

            [Description("SX")]
            SX = 0x3E,

            [Description("XTR")]
            XTR = 0x3F,

            [Description("XBee Cellular Cat 1 LTE Verizon")]
            CELLULAR_CAT1_LTE_VERIZON = 0x40,

            [Description("XBEE3")]
            XBEE3 = 0x41,

            [Description("XBEE3 SMT")]
            XBEE3_SMT = 0x42,

            [Description("XBEE3 TH")]
            XBEE3_TH = 0x43,

            [Description("XBee Cellular 3G")]
            CELLULAR_3G = 0x44,

            [Description("XB8X")]
            XB8X = 0x45,

            [Description("XBee Cellular LTE-M Verizon")]
            CELLULAR_LTE_VERIZON = 0x46,

            [Description("XBee Cellular LTE-M AT&T")]
            CELLULAR_LTE_ATT = 0x47,

            [Description("XBee Cellular NBIoT Europe")]
            CELLULAR_NBIOT_EUROPE = 0x48,

            [Description("XBee Cellular 3 Cat 1 LTE AT&T")]
            CELLULAR_3_CAT1_LTE_ATT = 0x49,

            [Description("XBee Cellular 3 LTE-M Verizon")]
            CELLULAR_3_LTE_M_VERIZON = 0x4A,

            [Description("XBee Cellular 3 LTE-M AT&T")]
            CELLULAR_3_LTE_M_ATT = 0x4B
        }

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
                return null;
            }
        }
    }
    #endregion

    #region Message
    /// <summary>
    /// Represents an XBee message, which is formed by a RemoteXBeeDevice (the sender) and some data as a byte array
    /// </summary>
    public class XBeeMessage
    {
        /// <summary>
        /// Data of the message
        /// </summary>
        public byte[] Data { get; }
        /// <summary>
        /// Device that sent the message
        /// </summary>
        public RemoteXBeeDevice RemoteDevice { get; }
        /// <summary>
        /// True if message is broadcast, false if unicast
        /// </summary>
        public bool Broadcast { get; }
        /// <summary>
        /// Timestamp of when the message was sent
        /// </summary>
        public double Timestamp { get; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="data">The data sent by the remote device</param>
        /// <param name="remoteDevice">The sender of the data</param>
        /// <param name="timestamp">Instant of t ime when the message was received</param>
        /// <param name="broadcast">Flag indicating whether the message is broadcast (true) or not (false)</param>
        public XBeeMessage(byte[] data, RemoteXBeeDevice remoteDevice, double timestamp, bool broadcast = false)
        {
            Data = data;
            RemoteDevice = remoteDevice;
            Timestamp = timestamp;
            Broadcast = broadcast;
        }

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                { "Data: ",         Data },
                { "Sender: ",       RemoteDevice.Address64bit },
                { "Broadcast: ",    Broadcast },
                { "Received at: ",  Timestamp }
            };
        }
    }

    public class ExplicitXBeeMessage : XBeeMessage
    {
        public ExplicitXBeeMessage(byte[] data, RemoteXBeeDevice remoteDevice, double timestamp,
            byte sourceEndpoint, byte destEndpoint, ushort clusterID, ushort profileID, bool broadcast)
            : base(data, remoteDevice, timestamp, broadcast)
        {
            throw new NotImplementedException("ExplicitXBeeMessage not implemented");
        }
    }

    public class IPMessage
    {

    }

    public class SMSMessage
    {

    }
    #endregion

    #region Mode
    public static class OperatingMode
    {
        public enum Byte : byte
        {
            [Description("AT Mode")]
            AT_MODE             = 0,

            [Description("API Mode")]
            API_MODE            = 1,

            [Description("API mode with escaped characters")]
            ESCAPED_API_MODE    = 2,

            [Description("Unknown")]
            UNKNOWN             = 99
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

    public static class APIOutputMode
    {
        public enum Byte : byte
        {
            [Description("Native")]
            NATIVE                  = 0x00,

            [Description("Explicit")]
            EXPLICIT                = 0x01,

            [Description("Explicit with ZDO passthru")]
            EXPLICIT_ZDO_PASSTHRU   = 0x03,
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

    public static class IPAddressingMode
    {
        public enum Byte : byte
        {
            [Description("DHCP")]
            DHCP = 0x00,

            [Description("Static")]
            STATIC = 0x01,
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

    #region Options
    /// <summary>
    /// Bitfield of all possible options that have been set while receiving an XBee Packet
    /// </summary>
    public enum  ReceiveOptions : byte
    {
        /// <summary>
        /// No special receive options
        /// </summary>
        NONE = 0x00,
        /// <summary>
        /// Packet was acknowledged
        /// </summary>
        /// <remarks>Not valid for WiFi protocol</remarks>
        PACKET_ACKNOWLEDGED = 0x01,
        /// <summary>
        /// Packet was a broadcast packet
        /// </summary>
        /// <remarks>Not valid for WiFi protocol</remarks>
        BROADCAST_PACKET = 0x02,
        /// <summary>
        /// Packet encrypted with APS encryption
        /// </summary>
        /// <remarks>Only valid for ZigBee XBee protocol</remarks>
        APS_ENCRYPTED = 0x20,
        /// <summary>
        /// Packet was sent from an end device
        /// </summary>
        /// <remarks>Only valid for ZigBee XBee protocol</remarks>
        SENT_FROM_END_DEVICE = 0x40
    }

    /// <summary>
    /// Bitfield of all possible options that have been set while receiving an XBee Packet
    /// </summary>
    public enum TransmitOptions : byte
    {
        /// <summary>
        /// No special transmit options.
        /// </summary>
        NONE = 0x00,
        /// <summary>
        /// Disables acknowledgments on all unicasts
        /// </summary>
        /// <remarks>Only valid for DigiMesh, 802.15.4 and Point-to-multipoint protocols.</remarks>
        DISABLE_ACK = 0x01,
        /// <summary>
        /// Disables the retries and router repair in the frame
        /// </summary>
        /// <remarks>Only valid for ZigBee protocol.</remarks>
        DISABLE_RETRIES_AND_REPAIR = 0x01,
        /// <summary>
        /// Doesn't attempt Route Discovery
        /// </summary>
        /// <remarks>Only valid for DigiMesh protocol.</remarks>
        DONT_ATTEMPT_RD = 0x02,
        /// <summary>
        /// Sends packet with broadcast PAN ID. Packet will be sent to all 
        /// devices in the same channel ignoring the PAN ID
        /// </summary>
        /// <remarks>Cannot be combined with other options. Only valid for 802.15.4 XBee protocol</remarks>
        USE_BROADCAST_PAN_ID = 0x04,
        /// <summary>
        /// Enables unicast NACK messages
        /// </summary>
        /// <remarks>NACK message is enabled on the packet. Only valid for DigiMesh 868/900 protocol</remarks>
        ENABLE_UNICAST_NACK = 0x04,
        /// <summary>
        /// Enables unicast trace route messages
        /// </summary>
        /// <remarks>Only valid for DigiMesh 868/900 protocol</remarks>
        ENABLE_UNICAST_TRACE_ROUTE = 0x04,
        /// <summary>
        /// Enables multicast transmission request.
        /// </summary>
        /// <remarks>Only valid for ZigBee protocol.</remarks>
        ENABLE_MULTICAST = 0x08,
        /// <summary>
        /// Enables APS encryption, only if EE=1
        /// </summary>
        /// <remarks>Enabling APS encryption decreases the maximum number of RF payload bytes by 4 (below the value reported by NP.<para/>
        /// Only valid for ZigBee XBee protocol.</remarks>
        ENABLE_APS_ENCRYPTION = 0x20,
        /// <summary>
        /// Uses the extended transmission timeout
        /// </summary>
        /// <remarks>Setting the extended timeout bit causes the stack to set extended transmission timeout for the destination address.<para/>
        /// Only valid for ZigBee XBee protocol.</remarks>
        USE_EXTENDED_TIMEOUT = 0x40,
        /// <summary>
        /// Transmission is performed using point-to-multipoint mode.
        /// </summary>
        /// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
        POINT_MULTIPOINT_MODE = 0x40,
        /// <summary>
        /// Transmission is performed using repeater mode
        /// </summary>
        /// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
        REPEATER_MODE = 0x80,
        /// <summary>
        /// Transmission is performed using Digimesh mode
        /// </summary>
        /// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
        DIGIMESH_MODE = 0xC0
    }

    /// <summary>
    /// Bitfield of all possible options that have been set while transmitting a remote AT command.
    /// </summary>
    public enum RemoteATCmdOptions : byte
    {
        /// <summary>
        /// No special transmit options
        /// </summary>
        NONE = 0x00,
        /// <summary>
        /// Disables ACK
        /// </summary>
        DISABLE_ACK = 0x01,
        /// <summary>
        /// Applies changes in the remote device
        /// </summary>
        /// <remarks>If this option is not set, AC command must be sent before changes will take effect.</remarks>
        APPLY_CHANGES = 0x02,
        /// <summary>
        /// Uses the extended transmission timeout
        /// </summary>
        /// <remarks>Setting the extended timeout bit causes the stack to set the extended transmission timeout for the destination address.<para/>
        /// Only valid for ZigBee XBee protocol.</remarks>
        EXTENDED_TIMEOUT = 0x40
    }

    /// <summary>
    /// Enumerates the different options for SendDataRequestPacket
    /// </summary>
    public static class SendDataRequestOptions
    {
        public enum Byte : byte
        {
            [Description("Overwrite")]
            OVERWRITE = 0x00,

            [Description("Archive")]
            ARCHIVE = 0x01,

            [Description("Append")]
            APPEND = 0x02,

            [Description("Transient data (do not store)")]
            TRANSIENT = 0x03,
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

    /// <summary>
    /// Enumerates the different options used in the discovery process
    /// </summary>
    public static class DiscoveryOptions
    {
        public enum Byte : byte
        {
            /// <summary>
            /// Append device type identifier (DD) to the discovery response.
            /// </summary>
            /// <remarks>Valid for the following protocols:
            ///     * DigiMesh
            ///     * Point-to-multipoint (Digi Point)
            ///     * ZigBee
            /// </remarks>
            [Description("Append device type identifier (DD)")]
            APPEND_DD = 0x01,

            /// <summary>
            /// Local device sends response frame when discovery is issued.
            /// </summary>
            /// <remarks>Valid for the following protocols:
            ///     * DigiMesh
            ///     * Point-to-multipoint (Digi Point)
            ///     * ZigBee
            ///     * 802.15.4
            /// </remarks>
            [Description("Local device sends response frame")]
            DISCOVER_MYSELF = 0x02,

            /// <summary>
            /// Append RSSI of the last hop to the discovery response.
            /// </summary>
            /// <remarks>Valid for the following protocols:
            ///     * DigiMesh
            ///     * Point-to-multipoint (Digi Point)
            /// </remarks>
            [Description("Append RSSI (of the last hop)")]
            APPEND_RSSI = 0x04
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

        public static Byte CalculateDiscoveryValue(XBeeProtocol.Byte protocol, Byte options)
        {
            Byte value = 0;
            if (protocol == XBeeProtocol.Byte.ZIGBEE || protocol == XBeeProtocol.Byte.ZNET)
            {
                // TODO: Test this
                value = (options & (~Byte.APPEND_RSSI));
            } else if (protocol == XBeeProtocol.Byte.DIGI_MESH || protocol == XBeeProtocol.Byte.DIGI_POINT
                || protocol == XBeeProtocol.Byte.XLR || protocol == XBeeProtocol.Byte.XLR_DM)
            {
                value = options;
            } else if ((Byte.DISCOVER_MYSELF & options) != 0)
            {
                value = (Byte)1;  // This is different for 802.15.4
            }

            return value;
        }
    }
    #endregion

    #region Protocol
    public static class XBeeProtocol
    {
        public enum Byte : byte
        {
            [Description("ZigBee")]
            ZIGBEE = 0,

            [Description("802.15.4")]
            RAW_802_15_4 = 1,

            [Description("Wi-Fi")]
            XBEE_WIFI = 2,

            [Description("DigiMesh")]
            DIGI_MESH = 3,

            [Description("XCite")]
            XCITE = 4,

            [Description("XTend (Legacy)")]
            XTEND = 5,

            [Description("XTend (DigiMesh)")]
            XTEND_DM = 6,

            [Description("Smart Energy")]
            SMART_ENERGY = 7,

            [Description("Point-to-multipoint")]
            DIGI_POINT = 8,

            [Description("ZNet 2.5")]
            ZNET = 9,

            [Description("XSC")]
            XC = 10,

            [Description("XLR")]
            XLR = 11,

            [Description("XLR")]
            XLR_DM = 12,

            [Description("XBee SX")]
            SX = 13,

            [Description("XLR Module")]
            XLR_MODULE = 14,

            [Description("Cellular")]
            CELLULAR = 15,

            [Description("Cellular NB-IoT")]
            CELLULAR_NBIOT = 16,

            [Description("Unknown")]
            UNKNOWN = 99
        }

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
                return null;
            }
        }

        public static Byte DetermineProtocol(HardwareVersion.Byte hardwareVersion, byte[] firmwareVersion)
        {
            string fwVersion = Utils.BytesToHexString(firmwareVersion);

            if (fwVersion.Length == 0 || (byte)hardwareVersion < 0x09 || HardwareVersion.Get(hardwareVersion).Length == 0)
            {
                return Byte.UNKNOWN;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XC09_009 || hardwareVersion == HardwareVersion.Byte.XC09_038)
            {
                return Byte.XCITE;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XT09_XXX || hardwareVersion == HardwareVersion.Byte.XT09B_XXX)
            {
                if ((fwVersion.Length == 4 && (fwVersion[0] == '8')) || (fwVersion.Length == 5 && fwVersion[1] == '8'))
                {
                    return Byte.XTEND_DM;
                }
                return Byte.XTEND;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XB24_AXX_XX || hardwareVersion == HardwareVersion.Byte.XBP24_AXX_XX)
            {
                if (fwVersion.Length == 4 && (fwVersion[0] == '8'))
                {
                    return Byte.DIGI_MESH;
                }
                return Byte.RAW_802_15_4;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XB24_BXIX_XXX || hardwareVersion == HardwareVersion.Byte.XBP24_BXIX_XXX)
            {
                if ((fwVersion.Length == 4 && fwVersion[0] == '1' && fwVersion.EndsWith("20"))
                    || (fwVersion.Length == 4 && fwVersion[0] == '2'))
                {
                    return Byte.ZIGBEE;
                }
                else if (fwVersion.Length == 4 && fwVersion[0] == '3')
                {
                    return Byte.SMART_ENERGY;
                }
                else
                {
                    return Byte.ZNET;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.XBP09_DXIX_XXX)
            {
                if (fwVersion.Length == 4 && fwVersion[0] == '8' ||
                    (fwVersion.Length == 4 && fwVersion[1] == '8') ||
                    (fwVersion.Length == 5 && fwVersion[1] == '8'))
                {
                    return Byte.DIGI_MESH;
                }
                else
                {
                    return Byte.DIGI_POINT;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.XBP09_XCXX_XXX)
            {
                return Byte.XC;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XBP08_DXXX_XXX)
            {
                return Byte.DIGI_POINT;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XBP24B)
            {
                if (fwVersion.Length == 4 && fwVersion[0] == '3')
                {
                    return Byte.SMART_ENERGY;
                }
                else
                {
                    return Byte.ZIGBEE;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.XB24_WF
              || hardwareVersion == HardwareVersion.Byte.WIFI_ATHEROS
              || hardwareVersion == HardwareVersion.Byte.SMT_WIFI_ATHEROS)
            {
                return Byte.XBEE_WIFI;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XBP24C
              || hardwareVersion == HardwareVersion.Byte.XB24C)
            {
                if ((fwVersion.Length == 4 && fwVersion[0] == '5')
                    || fwVersion[0] == '6')
                {
                    return Byte.SMART_ENERGY;
                }
                else if (fwVersion[0] == '2')
                {
                    return Byte.RAW_802_15_4;
                }
                else if (fwVersion[0] == '9')
                {
                    return Byte.DIGI_MESH;
                }
                else
                {
                    return Byte.ZIGBEE;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.XSC_GEN3
              || hardwareVersion == HardwareVersion.Byte.SRD_868_GEN3)
            {
                if (fwVersion.Length == 4 && fwVersion[0] == '8')
                {
                    return Byte.DIGI_MESH;
                }
                else if (fwVersion.Length == 4 && fwVersion[0] == '1')
                {
                    return Byte.DIGI_POINT;
                }
                else
                {
                    return Byte.XC;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.XBEE_CELL_TH)
            {
                return Byte.UNKNOWN;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XLR_MODULE)
            {
                if (fwVersion[0] == '1')
                {
                    return Byte.XLR;
                }
                else
                {
                    return Byte.XLR_MODULE;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.XLR_BASEBOARD)
            {
                if (fwVersion[0] == '1')
                {
                    return Byte.XLR;
                }
                else
                {
                    return Byte.XLR_MODULE;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.XB900HP_NZ)
            {
                return Byte.DIGI_POINT;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XBP24C_TH_DIP
              || hardwareVersion == HardwareVersion.Byte.XB24C_TH_DIP
              || hardwareVersion == HardwareVersion.Byte.XBP24C_S2C_SMT)
            {
                if (fwVersion.Length == 4 && (fwVersion[0] == '5' || fwVersion[0] == '6'))
                {
                    return Byte.SMART_ENERGY;
                }
                else if (fwVersion[0] == '2')
                {
                    return Byte.RAW_802_15_4;
                }
                else if (fwVersion[0] == '9')
                {
                    return Byte.DIGI_MESH;
                }
                else
                {
                    return Byte.ZIGBEE;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.SX_PRO
             || hardwareVersion == HardwareVersion.Byte.SX
             || hardwareVersion == HardwareVersion.Byte.XTR)
            {
                if (fwVersion[0] == '2')
                {
                    return Byte.XTEND;
                }
                else if (fwVersion[0] == '8')
                {
                    return Byte.XTEND_DM;
                }
                else
                {
                    return Byte.DIGI_MESH;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.S2D_SMT_PRO
             || hardwareVersion == HardwareVersion.Byte.S2D_SMT_REG
             || hardwareVersion == HardwareVersion.Byte.S2D_TH_PRO
             || hardwareVersion == HardwareVersion.Byte.S2D_TH_REG)
            {
                return Byte.ZIGBEE;
            }
            else if (hardwareVersion == HardwareVersion.Byte.CELLULAR_CAT1_LTE_VERIZON
             || hardwareVersion == HardwareVersion.Byte.CELLULAR_3G
             || hardwareVersion == HardwareVersion.Byte.CELLULAR_LTE_ATT
             || hardwareVersion == HardwareVersion.Byte.CELLULAR_LTE_VERIZON
             || hardwareVersion == HardwareVersion.Byte.CELLULAR_3_CAT1_LTE_ATT
             || hardwareVersion == HardwareVersion.Byte.CELLULAR_3_LTE_M_VERIZON
             || hardwareVersion == HardwareVersion.Byte.CELLULAR_3_LTE_M_ATT)
            {
                return Byte.CELLULAR;
            }
            else if (hardwareVersion == HardwareVersion.Byte.CELLULAR_NBIOT_EUROPE)
            {
                return Byte.CELLULAR_NBIOT;
            }
            else if (hardwareVersion == HardwareVersion.Byte.XBEE3
             || hardwareVersion == HardwareVersion.Byte.XBEE3_SMT
             || hardwareVersion == HardwareVersion.Byte.XBEE3_TH)
            {
                if (fwVersion[0] == '2')
                {
                    return Byte.RAW_802_15_4;
                }
                else if (fwVersion[0] == '3')
                {
                    return Byte.DIGI_MESH;
                }
                else
                {
                    return Byte.ZIGBEE;
                }
            }
            else if (hardwareVersion == HardwareVersion.Byte.XB8X)
            {
                return Byte.DIGI_MESH;
            }
            else
            {
                return Byte.ZIGBEE;
            }
        }
    }
    #endregion

                #region Status
        public static class ATCommandStatus
    {
        public enum Byte : byte
        {
            [Description("Status OK")]
            OK                  = 0,

            [Description("Status Error")]
            ERROR               = 1,

            [Description("Invalid command")]
            INVALID_COMMAND     = 2,

            [Description("Invalid parameter")]
            INVALID_PARAMETER   = 3,

            [Description("TX failure")]
            TX_FAILURE          = 4,

            [Description("Unknown status")]
            UNKNOWN              = 5
        }

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
                return null;
            }
        }
    }

    public static class DiscoveryStatus
    {
        public enum Byte : byte
        {
            [Description("No discovery overhead")]
            NO_DISCOVERY_OVERHEAD       = 0x00,

            [Description("Address discovery")]
            ADDRESS_DISCOVERY           = 0x01,

            [Description("Route discovery")]
            ROUTE_DISCOVERY             = 0x02,

            [Description("Address and route")]
            ADDRESS_AND_ROUTE           = 0x03,

            [Description("Extended timeout discovery")]
            EXTENDED_TIMEOUT_DISCOVERY  = 0x40,

            [Description("Unkown")]
            UNKNOWN                     = 0xFF
        }

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
                return null;
            }
        }
    }

    public static class TransmitStatus
    {
        public enum Byte : byte
        {
            [Description("Success.")]
            SUCCESSS                = 0x00,

            [Description("No acknowledgement received.")]
            NO_ACK                  = 0x01,

            [Description("CCA failure.")]
            CCA_FAILURE             = 0x02,

            [Description("Transmission purged, it was attempted before stack was up.")]
            PURGED                  = 0x03,

            [Description("Physical error occurred on the interface with the WiFi transceiver.")]
            WIFI_PHYSICAL_ERROR     = 0x04,

            [Description("Invalid destination endpoint.")]
            INVALID_DESTINATION     = 0x15,

            [Description("No buffers.")]
            NO_BUFFERS = 0x18,

            [Description("Network ACK Failure.")]
            NETWORK_ACK_FAILURE = 0x21,

            [Description("Not joined to network.")]
            NOT_JOINED_NETWORK = 0x22,

            [Description("Self-addressed.")]
            SELF_ADDRESSED = 0x23,

            [Description("Address not found.")]
            ADDRESS_NOT_FOUND = 0x24,

            [Description("Route not found.")]
            ROUTE_NOT_FOUND = 0x25,

            [Description("Broadcast source failed to hear a neighbor relay the message.")]
            BROADCAST_FAILED = 0x26,

            [Description("Invalid binding table index.")]
            INVALID_BINDING_TABLE_INDEX = 0x2B,

            [Description("Invalid endpoint")]
            INVALID_ENDPOINT = 0x2C,

            [Description("Attempted broadcast with APS transmission.")]
            BROADCAST_ERROR_APS = 0x2D,

            [Description("Attempted broadcast with APS transmission, but EE=0.")]
            BROADCAST_ERROR_APS_EE0 = 0x2E,

            [Description("A software error occurred.")]
            SOFTWARE_ERROR = 0x31,

            [Description("Resource error lack of free buffers, timers, etc.")]
            RESOURCE_ERROR = 0x32,

            [Description("Data payload too large.")]
            PAYLOAD_TOO_LARGE = 0x74,

            [Description("Indirect message unrequested")]
            INDIRECT_MESSAGE_UNREQUESTED = 0x75,

            [Description("Attempt to create a client socket failed.")]
            SOCKET_CREATION_FAILED = 0x76,

            [Description("TCP connection to given IP address and port doesn't exist. Source port is non-zero so that a new connection is not attempted.")]
            IP_PORT_NOT_EXIST = 0x77,

            [Description("Source port on a UDP transmission doesn't match a listening port on the transmitting module.")]
            UDP_SRC_PORT_NOT_MATCH_LISTENING_PORT = 0x78,

            [Description("Key not authorized.")]
            KEY_NOT_AUTHORIZED = 0xBB,

            [Description("Unknown.")]
            UNKNOWN = 0xFF
        }

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
                return null;
            }
        }
    }

    /// <summary>
    /// Lists different modem status events. Intended to be used with the ModemStatusPacket
    /// </summary>
    public static class ModemStatus
    {
        public enum Byte : byte
        {
            [Description("Device was reset")]
            HARDWARE_RESET = 0x00,

            [Description("Watchdog timer was reset")]
            WATCHDOG_TIMER_RESET = 0x01,

            [Description("Device joined to network")]
            JOINED_NETWORK = 0x02,

            [Description("Device disassociated")]
            DISASSOCIATED = 0x03,

            [Description("Configuration error/synchronization lost")]
            ERROR_SYNCHRONIZATION_LOST = 0x04,

            [Description("Coordinator realignment")]
            COORDINATOR_REALIGNMENT = 0x05,

            [Description("The coordinator started")]
            COORDINATOR_STARTED = 0x06,

            [Description("Network security key was updated")]
            NETWORK_SECURITY_KEY_UPDATED = 0x07,

            [Description("Network Woke Up")]
            NETWORK_WOKE_UP = 0x0B,

            [Description("Network Went To Sleep")]
            NETWORK_WENT_TO_SLEEP = 0x0C,

            [Description("Voltage supply limit exceeded")]
            VOLTAGE_SUPPLY_LIMIT_EXCEEDED = 0x0D,

            [Description("Remote Manager connected")]
            REMOTE_MANAGER_CONNECTED = 0x0E,

            [Description("Remote Manager disconnected")]
            REMOTE_MANAGER_DISCONNECTED = 0x0F,

            [Description("Modem configuration changed while joining")]
            MODEM_CONFIG_CHANGED_WHILE_JOINING = 0x11,

            [Description("Stack error")]
            ERROR_STACK = 0x80,

            [Description("Send/join command issued without connecting from AP")]
            ERROR_AP_NOT_CONNECTED = 0x82,

            [Description("Access point not found")]
            ERROR_AP_NOT_FOUND = 0x83,

            [Description("PSK not configured")]
            ERROR_PSK_NOT_CONFIGURED = 0x84,

            [Description("SSID not found")]
            ERROR_SSID_NOT_FOUND = 0x87,

            [Description("Failed to join with security enabled")]
            ERROR_FAILED_JOIN_SECURITY = 0x88,

            [Description("Invalid channel")]
            ERROR_INVALID_CHANNEL = 0x8A,

            [Description("Failed to join access point")]
            ERROR_FAILED_JOIN_AP = 0x8E,

            [Description("UNKNOWN")]
            UNKNOWN = 0xFF
        }

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
                return null;
            }
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public static class PowerLevel
    {
        public enum Byte : byte
        {
        }

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
                return null;
            }
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public static class AssociationIndicationStatus
    {
        public enum Byte : byte
        {
        }

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
                return null;
            }
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public static class CellularAssociationIndicationStatus
    {
        public enum Byte : byte
        {
        }

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
                return null;
            }
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public static class DeviceCloudStatus
    {
        public enum Byte : byte
        {
        }

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
                return null;
            }
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public static class FrameError
    {
        public enum Byte : byte
        {
        }

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
                return null;
            }
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public static class WiFiAssociationIndicationStatus
    {
        public enum Byte : byte
        {
        }

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
                return null;
            }
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public static class NetworkDiscoveryStatus
    {
        public enum Byte : byte
        {
            [Description("Success")]
            SUCCESS             = 0x00,

            [Description("Read timeout error")]
            ERROR_READ_TIMEOUT  = 0x01
        }

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
                return null;
            }
        }
    }
    #endregion
}

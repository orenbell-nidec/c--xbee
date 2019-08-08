using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XBeeAPI
{
    public static class Reader
    {
        /// <summary>
        /// This event is fired when an XBee receives any packet, independent of its frame type.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="packet">The received packet</param>
        public delegate void PacketReceivedHandler(object sender, XBeeAPIPacket packet);
        /// <summary>
        /// This event is fired when an XBee receives data.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="message">A message containing the data received</param>
        public delegate void DataReceivedHandler(object sender, XBeeMessage message);
        /// <summary>
        /// This event is fired when a XBee receives a modem status packet.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="status">Modem status received</param>
        public delegate void ModemStatusReceivedHandler(object sender, ModemStatus.Byte status);
        /// <summary>
        /// This event is fired when a XBee receives an IO packet.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="sample">Received IO Sample</param>
        /// <param name="remote">Remote XBee device that sent the packet</param>
        /// <param name="time">Time packet was received</param>
        public delegate void IOSampleReceivedHandler(object sender, object sample, RemoteXBeeDevice remote, int time); // TODO: Change type of 2nd arg to IOSample once the io.py file is translated
        /// <summary>
        /// This event is fired when an XBee discovers another remote XBee during a discovering operation.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="remote">Remote device that was discovered</param>
        public delegate void DeviceDiscoveredHandler(object sender, RemoteXBeeDevice remote);
        /// <summary>
        /// This event is fired when the discovery process finishes, either successfully or due to an error.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="status">Network discovery status</param>
        public delegate void DiscoveryProcessFinishedHandler(object sender, NetworkDiscoveryStatus.Byte status);
        /// <summary>
        /// This event is fired when an XBee receives an explicit data packet.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="message">Message containing data received, the sender, the time, and the explicit data message parameters</param>
        public delegate void ExplicitDataReceivedHandler(object sender, ExplicitXBeeMessage message);
        /// <summary>
        /// This event is fired when an XBee receives IP data.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="message">Message containing the IP address the message belongs to, source and dest ports, IP protocol, and the content of the message</param>
        public delegate void IPDataReceivedHandler(object sender, IPMessage message);
        /// <summary>
        /// This event is fired when an XBee receives an SMS.
        /// </summary>
        /// <param name="sender">Object that sent triggered the event</param>
        /// <param name="message">Message containing the phone number that sent the message and the content of the message</param>
        public delegate void SMSReceivedHandler(object sender, SMSMessage message);
    }

    public class PacketListener
    {
        private static int DEFAULT_QUEUE_MAX_SIZE = 40;
        private static string LOG_PATTERN = "{0:<6s} {1:<12s} {2:<10s} {3:<18s} {4:<50s}";
        protected static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Thread thread;

        protected XBeeDevice xbeeDevice;
        protected SerialPort comPort;
        protected bool stop;
        protected int queueMaxSize;

        #region Event Handlers
        /// <summary>
        /// Maximum number of simultaneous callbacks
        /// </summary>
        public static int MAX_PARALLEL_CALLBACKS = 50;

        /// <summary>
        /// This event is fired when needing to process an API packet
        /// </summary>
        protected event Reader.PacketReceivedHandler PacketReceivedAPI;
        /// <summary>
        /// This event is fired when an XBee receives any packet, independent of its frame type.
        /// </summary>
        public event Reader.PacketReceivedHandler PacketReceived;
        /// <summary>
        /// This event is fired when an XBee receives data.
        /// </summary>
        public event Reader.DataReceivedHandler DataReceived;
        /// <summary>
        /// This event is fired when a XBee receives a modem status packet.
        /// </summary>
        public event Reader.ModemStatusReceivedHandler ModemStatusReceived;
        /// <summary>
        /// This event is fired when a XBee receives an IO packet.
        /// </summary>
        public event Reader.IOSampleReceivedHandler IOSampleReceived;
        /// <summary>
        /// This event is fired when an XBee receives an explicit data packet.
        /// </summary>
        public event Reader.ExplicitDataReceivedHandler ExplicitDataReceived;
        /// <summary>
        /// This event is fired when an XBee receives IP data.
        /// </summary>
        public event Reader.IPDataReceivedHandler IPDataReceived;
        /// <summary>
        /// This event is fired when an XBee receives an SMS.
        /// </summary>
        public event Reader.SMSReceivedHandler SMSReceived;

        /// <summary>
        /// Programmatic trigger for PacketReceived event
        /// </summary>
        /// <param name="packet">The received packet</param>
        protected virtual void OnPacketReceivedAPI(XBeeAPIPacket packet)
        {
            // This could be consolidated with the PacketReceived event, but the python implementation
            // has a separate one, so oh well
            PacketReceivedAPI?.Invoke(this, packet);
        }
        /// <summary>
        /// Programmatic trigger for PacketReceived event
        /// </summary>
        /// <param name="packet">The received packet</param>
        protected virtual void OnPacketReceived(XBeeAPIPacket packet)
        {
            PacketReceived?.Invoke(this, packet);
        }
        /// <summary>
        /// Programmatic trigger for DataReceived event
        /// </summary>
        /// <param name="message">A message containing the data received</param>
        protected virtual void OnDataReceived(XBeeMessage message)
        {
            DataReceived?.Invoke(this, message);
        }
        /// <summary>
        /// Programmatic trigger for ModemStatusReceived event
        /// </summary>
        /// <param name="status">Modem status received</param>
        protected virtual void OnModemStatusReceived(ModemStatus.Byte status)
        {
            ModemStatusReceived?.Invoke(this, status);
        }
        /// <summary>
        /// Programmatic trigger for IOSampleReceived event
        /// </summary>
        /// <param name="sample">Received IO Sample</param>
        /// <param name="remote">Remote XBee device that sent the packet</param>
        /// <param name="time">Time packet was received</param>
        protected virtual void OnIOSampleReceived(object sample, RemoteXBeeDevice remote, int time)
        {
            IOSampleReceived?.Invoke(this, sample, remote, time);
        }
        /// <summary>
        /// Programmatic trigger for ExplicitDataReceived event
        /// </summary>
        /// <param name="message">Message containing data received, the sender, the time, and the explicit data message parameters</param>
        protected virtual void OnExplicitDataReceived(ExplicitXBeeMessage message)
        {
            ExplicitDataReceived?.Invoke(this, message);
        }
        /// <summary>
        /// Programmatic trigger for IPDataReceived event
        /// </summary>
        /// <param name="message">Message containing the IP address the message belongs to, source and dest ports, IP protocol, and the content of the message</param>
        protected virtual void OnIPDataReceived(IPMessage message)
        {
            IPDataReceived?.Invoke(this, message);
        }
        /// <summary>
        /// Programmatic trigger for SMSReceived event
        /// </summary>
        /// <param name="message">Message containing the phone number that sent the message and the content of the message</param>
        protected virtual void OnSMSReceived(SMSMessage message)
        {
            SMSReceived?.Invoke(this, message);
        }
        #endregion

        public PacketListener(SerialPort serialPort, XBeeDevice xbeeDevice, int queueMaxSize = -1) { 
            foreach(Reader.PacketReceivedHandler handler in xbeeDevice.GetXBeeDeviceCallbacks())
            {
                // Get list of internal callbacks from device, and subscribe them all to PacketReceivedAPI
                PacketReceivedAPI += handler;
            }
            this.xbeeDevice = xbeeDevice;
            this.comPort = serialPort;
            this.stop = true;
            this.queueMaxSize = (queueMaxSize < 0) ? DEFAULT_QUEUE_MAX_SIZE : queueMaxSize;
            XBeeQueue = new XBeeQueue(this.queueMaxSize);
            DataXBeeQueue = new XBeeQueue(this.queueMaxSize);
            ExplicitXBeeQueue = new XBeeQueue(this.queueMaxSize);
            IPXBeeQueue = new XBeeQueue(this.queueMaxSize);
        }

        /// <summary>
        /// Starts the thread loop
        /// </summary>
        /// <exception cref="OutOfMemoryException">Thrown if there's not enough memory to start a new thread</exception>
        public void Start()
        {
            thread = new Thread(new ThreadStart(this.Run));
            thread.Start();
        }

        /// <summary>
        /// Function for PacketListener to loop in
        /// </summary>
        private void Run()
        {
            try
            {
                stop = false;
                while (!stop)
                {
                    byte[] rawPacket = TryReadPacket(xbeeDevice.OpMode);

                    if (rawPacket != null)
                    {
                        // If the protocol is 802.15.4, check if the packet is valid. If not, skip it
                        if (xbeeDevice.Protocol == XBeeProtocol.Byte.RAW_802_15_4 && !CheckPacket802_15_4(rawPacket))
                        {
                            continue;
                        }

                        // Build the packet
                        XBeeAPIPacket readPacket;
                        try
                        {
                            readPacket = (XBeeAPIPacket)Factory.BuildFrame(rawPacket, xbeeDevice.OpMode);
                        } catch (InvalidPacketException e)
                        {
                            Log.Error(String.Format("Error processing packet {0} : {1}", Utils.BytesToHexString(rawPacket, true), e.Message));
                            continue;
                        }

                        Log.Debug(String.Format(XBeeDevice.LOG_PATTERN, xbeeDevice.ComPort.PortName, "RECEIVED", OperatingMode.Get(xbeeDevice.OpMode), Utils.BytesToHexString(rawPacket, true)));

                        // Add the packet to the queue
                        AddPacketQueue(readPacket);

                       // If the packet has information about a remote device, extract it and update the remote device
                       RemoteXBeeDevice remote = TryAddRemoteDevice(readPacket);

                        // Execute callbacks to process API packets
                        OnPacketReceivedAPI(readPacket);

                        ExecuteUserCallbacks(readPacket, remote);
                    }
                }
            } catch (Exception e)
            {
                if (!stop)
                {
                    Log.Error(e);
                    Console.Write(e.StackTrace);
                }
            } finally
            {
                if (!stop)
                {
                    stop = true;
                    if (comPort.IsOpen)
                    {
                        comPort.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Stops listening
        /// </summary>
        public void Stop()
        {
            stop = true;
        }

        /// <summary>
        /// Calls Join() on the internal thread
        /// </summary>
        /// <exception cref="ThreadStateException">Attempted to join the thread when it wasn't started</exception>
        /// <exception cref="ThreadInterruptedException">Thread was interrupted while waiting</exception>
        public void Join()
        {
            thread.Join();
        }

        /// <summary>
        /// Returns whether this instance is running or not
        /// </summary>
        public bool IsRunning { get { return !stop; } }
        /// <summary>
        /// Returns the packets queue
        /// </summary>
        public XBeeQueue XBeeQueue { get; }
        /// <summary>
        /// Returns the data packets queue
        /// </summary>
        public XBeeQueue DataXBeeQueue { get; }
        /// <summary>
        /// Returns the explicit packets queue
        /// </summary>
        public XBeeQueue ExplicitXBeeQueue { get; }
        /// <summary>
        /// Returns the IP packets queue
        /// </summary>
        public XBeeQueue IPXBeeQueue { get; }

        /// <summary>
        /// Executes callbacks corresponding to the received packet.
        /// </summary>
        /// <param name="xbeePacket">The received packet.</param>
        /// <param name="remote">The XBee device that sent the packet.</param>
        protected void ExecuteUserCallbacks(XBeeAPIPacket xbeePacket, RemoteXBeeDevice remote = null)
        {
            OnPacketReceived(xbeePacket);

            // If the packet is a Receive Packet, add it to the data queue
            if (xbeePacket.FrameTypeValue == ApiFrameType.Byte.RECEIVE_PACKET)
            {
                byte[] data = ((ReceivePacket)xbeePacket).Rfdata;
                bool isBroadcast = ((ReceivePacket)xbeePacket).ReceiveOptions == ReceiveOptions.BROADCAST_PACKET;
                OnDataReceived(new XBeeMessage(data, remote, Utils.TimeNow(), isBroadcast));
                Log.Info(String.Format(LOG_PATTERN, xbeeDevice.ComPort.PortName, "RECEIVED",
                    "DATA", remote?.Address64bit, Utils.BytesToHexString(((ReceivePacket)xbeePacket).Rfdata, true)));
            } else if (xbeePacket.FrameTypeValue == ApiFrameType.Byte.RX_64 || xbeePacket.FrameTypeValue == ApiFrameType.Byte.RX_16)
            {
                throw new NotImplementedException("Callback does not support RX_64 or RX_16 packets yet");
            } else if (xbeePacket.FrameTypeValue == ApiFrameType.Byte.MODEM_STATUS)
            {
                throw new NotImplementedException("Callback does not support Modem Status packets yet");
            }
            else if (xbeePacket.FrameTypeValue == ApiFrameType.Byte.RX_IO_16 || xbeePacket.FrameTypeValue == ApiFrameType.Byte.RX_IO_64
                || xbeePacket.FrameTypeValue == ApiFrameType.Byte.IO_DATA_SAMPLE_RX_INDICATOR)
            {
                throw new NotImplementedException("Callback does not support RX IO 16, RX IO 64, or IO data sample RX indicator packets yet");
            } else if (xbeePacket.FrameTypeValue == ApiFrameType.Byte.EXPLICIT_RX_INDICATOR)
            {
                throw new NotImplementedException("Callback does not support Explicit RX indicator packets yet");
            } else if (xbeePacket.FrameTypeValue == ApiFrameType.Byte.RX_IPV4)
            {
                throw new NotImplementedException("Callback does not support RX IPv4 packets yet");
            } else if (xbeePacket.FrameTypeValue == ApiFrameType.Byte.RX_SMS)
            {
                throw new NotImplementedException("Callback does not support RX SMS packets yet");
            }
        }

        /// <summary>
        /// Returns the next byte in bytearray format.
        /// </summary>
        /// <remarks>
        /// If the operating mode is ESCAPED_API_MODE, the bytearray could contain 2 bytes.
        /// If in escaped API mode and the byte that was read was the escape byte,
        /// it will also read the next byte.
        /// </remarks>
        /// <param name="operatingMode">The operating mode in which the byte should be read</param>
        /// <returns>The read byte or bytes as an array</returns>
        /// <exception cref="InvalidOperationException">COM port isn't open</exception>
        protected byte[] ReadNextByte(OperatingMode.Byte operatingMode)
        {
            byte[] readData = new byte[2];
            readData[0] = (byte)comPort.ReadByte();

            if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE && readData[0] == SpecialByte.ESCAPE_BYTE)
            {
                readData[1] = (byte)comPort.ReadByte();
                return readData;
            } else
            {
                return new byte[]{ readData[0] };
            }
        }

        // TODO: Verify this works with escaped packets
        /// <summary>
        /// Reads the next packet. Starts to read when finds the start delimiter. The last byte read is the checksum.
        /// </summary>
        /// <remarks>
        /// If there is something in the COM buffer after the start delimiter, this method discards it.<para/>
        /// If the method can't read a complete and correct packet, it will return null.
        /// </remarks>
        /// <param name="operatingMode">The operating mode in which the packet should be read</param>
        /// <returns>The read packet as a byte array if a packet is read, null otherwise.</returns>
        /// <exception cref="InvalidOperationException">COM port isn't open</exception>
        protected byte[] TryReadPacket(OperatingMode.Byte operatingMode = OperatingMode.Byte.API_MODE)
        {
            try
            {
                byte headerByte = (byte)comPort.ReadByte();
                if (headerByte != SpecialByte.HEADER_BYTE)
                    return null;

                byte[] tempArray = ReadNextByte(operatingMode);
                tempArray = tempArray.Concat(ReadNextByte(operatingMode)).ToArray();

                int length;
                if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                {
                    length = Utils.LengthToInt(XBeeAPIPacket.UnescapeData(tempArray));
                } else
                {
                    length = Utils.LengthToInt(tempArray);
                }

                // Define byte array: header byte + packet length + data + checksum.
                // Assume worst case scenario: the last 2 fields have escaped every single byte
                byte[] xbeePacket = new byte[1 + 2 * (tempArray.Length + length + 1)];

                xbeePacket[0] = headerByte;
                Array.Copy(tempArray, 0, xbeePacket, 1, tempArray.Length);
                int j = 1 + tempArray.Length;
                for(int i=0; i <= length; i++)
                {
                    tempArray = ReadNextByte(operatingMode);
                    Array.Copy(tempArray, 0, xbeePacket, j, tempArray.Length);
                    j += tempArray.Length;
                }

                if (operatingMode == OperatingMode.Byte.ESCAPED_API_MODE)
                {
                    return XBeeAPIPacket.UnescapeData(xbeePacket.Take(j).ToArray());
                } else
                {
                    return xbeePacket.Take(j).ToArray();
                }
            } catch (TimeoutException err)
            {
                return null;
            }
        }

        /// <summary>
        /// Scrapes the 64bit and 16bit addresses from the provided packet and returns a remote XBee device from those
        /// </summary>
        /// <param name="packet">The API packet coming from (or possibly, going to) the remote device</param>
        /// <returns>RemoteXBeeDevice object</returns>
        protected RemoteXBeeDevice CreateRemoteDeviceFromPacket(XBeeAPIPacket packet)
        {
            Tuple<XBee64BitAddress, XBee16BitAddress> addresses = GetRemoteDeviceDataFromPacket(packet);
            return new RemoteXBeeDevice(xbeeDevice, addresses.Item1, addresses.Item2);
        }

        /// <summary>
        /// Returns the 64bit and 16bit addresses of a packet.
        /// </summary>
        /// <remarks>
        /// Note this has different functionality than python, as it can accept any of 7 packets.
        /// The original python API only accepts 4 kinds.
        /// It is also somewhat redundant to Devices.XBeeNetwork.GetDataForRemote, although that takes a raw byte array instead of a packet
        /// </remarks>
        /// <param name="packet">The API packet to scrape addresses from</param>
        /// <returns>A pair of 64bit and 16bit addresses</returns>
        protected static Tuple<XBee64BitAddress, XBee16BitAddress> GetRemoteDeviceDataFromPacket(XBeeAPIPacket packet)
        {
            XBee64BitAddress x64bitAddr = null;
            XBee16BitAddress x16bitAddr = null;
            if (((Dictionary<DictKeys, object>)packet.GetFrameSpecDataDict()[DictKeys.API_DATA]).ContainsKey(DictKeys.X64BIT_ADDR)) {
                x64bitAddr = (XBee64BitAddress)((Dictionary<DictKeys, object>)packet.GetFrameSpecDataDict()[DictKeys.API_DATA])[DictKeys.X64BIT_ADDR];
            }
            if (((Dictionary<DictKeys, object>)packet.GetFrameSpecDataDict()[DictKeys.API_DATA]).ContainsKey(DictKeys.X16BIT_ADDR)) {
                x16bitAddr = (XBee16BitAddress)((Dictionary<DictKeys, object>)packet.GetFrameSpecDataDict()[DictKeys.API_DATA])[DictKeys.X16BIT_ADDR];
            }

            return new Tuple<XBee64BitAddress, XBee16BitAddress>(x64bitAddr, x16bitAddr);
        }

        /// <summary>
        /// Check a received byte array and retursn false if it's an RX64 IO packet with an invalid payload
        /// </summary>
        /// <remarks>If the current XBee's protocol is 802.15.4 and the user sends many 'ND' commands, the device could return an RX 64 IO packet with an invalid payload(length<5).<para/>
        /// In this case the packet must be discarded, or an exception must be raised.
        /// </remarks>
        /// <param name="rawData"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Always thrown, since 802.15.4 functionality is not implemented</exception>
        protected static bool CheckPacket802_15_4(byte[] rawData)
        {
            throw new NotImplementedException("This method not implemented");
            //return !(rawData[3] == (byte)ApiFrameType.Byte.RX_IO_16 && rawData.Length - 5 < IOSample.MinIOSamplePayload());
        }

        /// <summary>
        /// This scrapes the provided packet for information about a remote node, creates an object for that node and attempts to add it to the network.
        /// </summary>
        /// <param name="packet">API packet, ideally one sent from a remote node</param>
        /// <returns>The remote xbee device</returns>
        protected RemoteXBeeDevice TryAddRemoteDevice(XBeeAPIPacket packet)
        {
            RemoteXBeeDevice remote = null;
            Tuple<XBee64BitAddress, XBee16BitAddress> addresses = GetRemoteDeviceDataFromPacket(packet);
            if (addresses.Item1 != null || addresses.Item2 != null)
            {
                remote = xbeeDevice.Network.AddIfNotExist(addresses.Item1, addresses.Item2);
            }
            return remote;
        }

        /// <summary>
        /// Checks if an explicit data packet in 'special'
        /// </summary>
        /// <remarks>
        /// 'Special' means that this XBee has its API Output Mode distinct than Native (it's expecting
        /// explicit data packets), but some device has sent it a non-explicit data packet (TransmitRequest f.e.).
        /// In this case, this XBee will receive a explicit data packet with the following values:
        ///    1. Source endpoint = 0xE8
        ///    2. Destination endpoint = 0xE8
        ///    3. Cluster ID = 0x0011
        ///    4. Profile ID = 0xC105
        /// </remarks>
        /// <param name="packet"></param>
        /// <returns></returns>
        protected bool IsSpecialExplicitPacket(ExplicitAddressingPacket packet)
        {
            return (packet.SourceEndpoint == 0xE8 && packet.DestEndpoint == 0xE8 && packet.ClusterID == 0x0011 && packet.ProfileID == 0xC105);
        }
        /// <summary>
        /// Checks if an explicit data packet in 'special'
        /// </summary>
        /// <remarks>
        /// 'Special' means that this XBee has its API Output Mode distinct than Native (it's expecting
        /// explicit data packets), but some device has sent it a non-explicit data packet (TransmitRequest f.e.).
        /// In this case, this XBee will receive a explicit data packet with the following values:
        ///    1. Source endpoint = 0xE8
        ///    2. Destination endpoint = 0xE8
        ///    3. Cluster ID = 0x0011
        ///    4. Profile ID = 0xC105
        /// </remarks>
        /// <param name="packet"></param>
        /// <returns></returns>
        protected bool IsSpecialExplicitPacket(ExplicitRXIndicatorPacket packet)
        {
            return (packet.SourceEndpoint == 0xE8 && packet.DestEndpoint == 0xE8 && packet.ClusterID == 0x0011 && packet.ProfileID == 0xC105);
        }

        /// <summary>
        /// Creates a 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Thrown if device uses 802.15.4 protocol, since that functionality isn't implemented</exception>
        protected XBeeAPIPacket ExplToNoExpl(ExplicitRXIndicatorPacket packet)
        {
            XBee64BitAddress x64addr = packet.X64BitAddress;
            XBee16BitAddress x16addr = packet.X16BitAddress;
            XBeeAPIPacket newPacket;
            if (xbeeDevice.Protocol == XBeeProtocol.Byte.RAW_802_15_4)
            {
                throw new NotImplementedException("This method does not support 802.15.4 protocol yet");
            } else
            {
                newPacket = new ReceivePacket(packet.X64BitAddress, packet.X16BitAddress, packet.ReceiveOptions, packet.RfData);
            }

            return newPacket;
        }

        // TODO: Double check this once XBeeQueue is implemented
        /// <summary>
        /// Adds a packet to the queue. If the queue is full, the head is removed to make room
        /// </summary>
        /// <param name="packet">The packet to be added</param>
        /// <exception cref="NotImplementedException">Thrown if packet is explicit, since that functionality isn't implemented</exception>
        protected void AddPacketQueue(XBeeAPIPacket packet)
        {
            // Data packets
            if (packet.FrameTypeValue == ApiFrameType.Byte.RECEIVE_PACKET || packet.FrameTypeValue == ApiFrameType.Byte.RX_64
                || packet.FrameTypeValue == ApiFrameType.Byte.RX_16)
            {
                // BUG: Race condition in the original python implementation (it has been fixed here). There was
                //      no lock between making room in the queue and adding a new element. If another packet gets
                //      added in between these two operations, the latter will throw an unhandled "XBeeQueueFullException"
                DataXBeeQueue.PutNoWait(packet, true);
            }
            else if (packet.FrameTypeValue == ApiFrameType.Byte.EXPLICIT_RX_INDICATOR)
            {
                ExplicitXBeeQueue.PutNoWait(packet, true);
                if (IsSpecialExplicitPacket((ExplicitRXIndicatorPacket)packet))
                {
                    AddPacketQueue(ExplToNoExpl((ExplicitRXIndicatorPacket)packet));
                }
            } else if (packet.FrameTypeValue == ApiFrameType.Byte.RX_IPV4)
            {
                IPXBeeQueue.PutNoWait(packet, true);
            } else
            {
                XBeeQueue.PutNoWait(packet, true);
            }
        }

        /// <summary>
        /// Converts an explicit packet to an explicit message
        /// </summary>
        /// <param name="remote">The remote XBee device that sent the packet</param>
        /// <param name="broadcast">Flag indicating whether the message is broadcast (default is false)</param>
        /// <param name="packet">The packet to be converted</param>
        /// <returns>The explicit message genereated from the provided parameters</returns>
        protected static ExplicitXBeeMessage ExplToMessage(RemoteXBeeDevice remote, bool broadcast, ExplicitAddressingPacket packet) {
            return new ExplicitXBeeMessage(packet.RfData, remote, Utils.TimeNow(), packet.SourceEndpoint, packet.DestEndpoint,
                packet.ClusterID, packet.ProfileID, broadcast);
        }
        /// <summary>
        /// Converts an explicit packet to an explicit message
        /// </summary>
        /// <param name="remote">The remote XBee device that sent the packet</param>
        /// <param name="broadcast">Flag indicating whether the message is broadcast (default is false)</param>
        /// <param name="packet">The packet to be converted</param>
        /// <returns>The explicit message genereated from the provided parameters</returns>
        protected static ExplicitXBeeMessage ExplToMessage(RemoteXBeeDevice remote, bool broadcast, ExplicitRXIndicatorPacket packet)
        {
            return new ExplicitXBeeMessage(packet.RfData, remote, Utils.TimeNow(), packet.SourceEndpoint, packet.DestEndpoint,
                packet.ClusterID, packet.ProfileID, broadcast);
        }
    }

    /// <summary>
    /// Represents a XBee queue
    /// </summary>
    public class XBeeQueue
    {
        private readonly object mutex;
        private LinkedList<XBeeAPIPacket> list;

        /// <summary>
        /// The max number of elements in this queue
        /// </summary>
        public int Capacity
        {
            get;
        }

        /// <summary>
        /// This number of elements in the queue
        /// </summary>
        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        /// <summary>
        /// Indicates whether the list has reached capacity
        /// </summary>
        public bool Full
        {
            get
            {
                return Count >= Capacity;
            }
        }

        /// <summary>
        /// Constructor. Instantiates a new XBeeQueue
        /// </summary>
        /// <param name="maxsize">Capacity of queue</param>
        public XBeeQueue(int maxsize = 10) : base() {
            mutex = new object();
            Capacity = maxsize;
            list = new LinkedList<XBeeAPIPacket>();
        }

        /// <summary>
        /// Returns the first element of the queue if there is some element ready
        /// </summary>
        /// <param name="block">Suppposed to be a flag to block, but it's not even used</param>
        /// <param name="timeout">Time, in seconds, to wait for the queue to return something</param>
        /// <returns>A packet if there's any available before the timeout. Null otherwise</returns>
        /// <exception cref="TimeoutException">Thrown if timeout expires before a element is found</exception>
        public XBeeAPIPacket Get(bool block = true, int timeout = 0)
        {
            XBeeAPIPacket packet = null;

            if (timeout <= 0)
            {
                // Lock down access to the queue
                lock (mutex)
                {
                    // Attempt to remove and return the first packet, but give up at the slightest sign of trouble
                    try
                    {
                        packet = list.First.Value;
                        list.RemoveFirst();
                    }
                    catch (Exception)
                    {
                        packet = null;
                    }
                }
                return packet;
            } else
            {
                // Call the above non-waiting block of code multiple times until we get a matching packet or the timeout expires
                packet = Get(block, 0);
                DateTime deadline = DateTime.Now + new TimeSpan(0, 0, seconds: timeout);
                while (packet == null && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                    packet = Get(block, 0);
                }
                if (packet == null)
                {
                    throw new TimeoutException();
                }
                return packet;
            }
        }

        /// <summary>
        /// Returns the first element of the queue that had been sent by the remote device
        /// </summary>
        /// <param name="remote">Remote device that we're looking for a packet from</param>
        /// <param name="timeout">Time to wait if there's no packet immediately available</param>
        /// <returns>Packet from remote device, if any is available in allotted time. Otherwise, null</returns>
        /// <exception cref="NotImplementedException">Thrown if an RX packet is found in the queue. RX packets aren't supported yet</exception>
        /// <exception cref="TimeoutException">Thrown if timeout expires before a element is found</exception>
        public XBeeAPIPacket GetByRemote(RemoteXBeeDevice remote, int timeout = 0)
        {
            if (timeout <= 0)
            {
                // Lock down the queue
                lock (mutex)
                {
                    // Scan through the list for a packet matching the remote device
                    var node = list.First;
                    while (node != null)
                    {
                        if (RemoteDeviceMatch(node.Value, remote))
                        {
                            list.Remove(node);
                            return node.Value;
                        }
                        node = node.Next;
                    }
                }
                return null;
            } else
            {
                // Call the above non-waiting block of code multiple times until we get a matching packet or the timeout expires
                XBeeAPIPacket packet = GetByRemote(remote, 0);
                DateTime deadline = DateTime.Now + new TimeSpan(0, 0, seconds: timeout);
                while (packet == null && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                    packet = GetByRemote(remote, 0);
                }
                if (packet == null)
                {
                    throw new TimeoutException();
                }
                return packet;
            }
        }

        // NOTE: GetByIP not translated

        /// <summary>
        /// Returns the first packet from the queue whose frame ID matches the provided one
        /// </summary>
        /// <param name="frameID">Frame ID to search for</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <returns>Packet if any is available. Otherwise, null</returns>
        /// <exception cref="TimeoutException">Thrown if timeout is larger than 0 and expires</exception>
        public XBeeAPIPacket GetByID(int frameID, int timeout=0)
        {
            if (timeout <= 0)
            {
                
                // Lock down the queue
                lock (mutex)
                {
                    // Scan through the list for a packet with the matching frame ID
                    var node = list.First;
                    while (node != null)
                    {
                        if (node.Value.NeedsID() && node.Value.FrameID == frameID)
                        {
                            list.Remove(node);
                            return node.Value;
                        }
                        node = node.Next;
                    }
                }
                return null;
            }
            else
            {
                XBeeAPIPacket packet = GetByID(frameID, 0);
                DateTime deadline = DateTime.Now + new TimeSpan(0, 0, seconds: timeout);
                while (packet == null && DateTime.Now < deadline)
                {
                    Thread.Sleep(100);
                    packet = GetByID(frameID, 0);
                }
                if (packet == null)
                {
                    throw new TimeoutException();
                }
                return packet;
            }
        }

        /// <summary>
        /// Clears the entire queue
        /// </summary>
        public void Flush()
        {
            lock(mutex)
            {
                list.Clear();
            }
        }

        /// <summary>
        /// Adds an element to the end of the queue
        /// </summary>
        /// <param name="packet">Packet to add</param>
        /// <param name="block">Whether or not to wait for a spot to open, should the queue be full</param>
        /// <param name="timeout">How long to wait, if queue is full</param>
        /// <param name="makeRoom">If true, queue will remove the oldest packet to make room for this one, after waiting the timeout</param>
        /// <exception cref="XBeeQueueFullException">Thrown if the queue is full and makeRoom is set to false</exception>
        public void Put(XBeeAPIPacket packet, bool block=false, int timeout=0, bool makeRoom=false)
        {
            if (!block)
            {
                lock (mutex)
                {
                    if (!Full)
                    {
                        list.AddLast(packet);
                    } else if (makeRoom)
                    {
                        // NOTE: The original python implementation did not have this makeRoom variable,
                        //       but the original python implementation also had a bug where it never emptied
                        //       its Queues and attempted to make room anyways in a thread-unsafe way
                        list.RemoveFirst();
                        list.AddLast(packet);
                    }
                    else
                    {
                        throw new XBeeQueueFullException();
                    }
                }
            } else
            {
                bool added = false;
                DateTime deadline = DateTime.Now + new TimeSpan(0, 0, seconds: timeout);
                while (DateTime.Now < deadline && !added)
                {
                    try
                    {
                        Put(packet, false);
                    } catch (XBeeQueueFullException)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    added = true;
                }
                if (!added)
                {
                    if (makeRoom)
                    {
                        Put(packet, makeRoom: true);
                    }
                    else
                    {
                        throw new XBeeQueueFullException();
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to add a packet to the queue, but does not wait around if it is full
        /// </summary>
        /// <param name="packet">Packet to add to queue</param>
        /// <param name="makeRoom">If true, queue will remove the oldest packet to make room for this one</param>
        /// <exception cref="XBeeQueueFullException">Thrown if the queue is full and makeRoom is set to false</exception>
        public void PutNoWait(XBeeAPIPacket packet, bool makeRoom=false)
        {
            Put(packet, makeRoom:makeRoom);
        }

        /// <summary>
        /// Returns whether or not the source address of the provided packet matches the given remote device.
        /// </summary>
        /// <param name="packet">The packet to get the address to compare</param>
        /// <param name="remoteDevice">The remote device to get the address to compare</param>
        /// <returns>True is the remote device matches, false otherwise</returns>
        /// <exception cref="NotImplementedException">Thrown if the provided packets are RX, which aren't supported</exception>
        protected static bool RemoteDeviceMatch(XBeeAPIPacket packet, RemoteXBeeDevice remoteDevice)
        {
            if (packet.FrameTypeValue == ApiFrameType.Byte.RECEIVE_PACKET)
            {
                return (((ReceivePacket)packet).X64bit_addr.Equals(remoteDevice.Address64bit))
                        || (((ReceivePacket)packet).X16bit_addr.Equals(remoteDevice.Address16bit))
                            && ((remoteDevice.Protocol == XBeeProtocol.Byte.RAW_802_15_4
                            || remoteDevice.Protocol == XBeeProtocol.Byte.DIGI_POINT
                            || remoteDevice.Protocol == XBeeProtocol.Byte.ZIGBEE));
            }
            else if (packet.FrameTypeValue == ApiFrameType.Byte.REMOTE_AT_COMMAND_RESPONSE)
            {
                return (((RemoteATCommandPacket)packet).X64bit_addr.Equals(remoteDevice.Address64bit))
                        || (((ReceivePacket)packet).X16bit_addr.Equals(remoteDevice.Address16bit))
                            && ((remoteDevice.Protocol == XBeeProtocol.Byte.RAW_802_15_4
                            || remoteDevice.Protocol == XBeeProtocol.Byte.DIGI_POINT
                            || remoteDevice.Protocol == XBeeProtocol.Byte.ZIGBEE));
            }
            else if (packet.FrameTypeValue == ApiFrameType.Byte.RX_16
                || packet.FrameTypeValue == ApiFrameType.Byte.RX_64
                || packet.FrameTypeValue == ApiFrameType.Byte.RX_IO_16
                || packet.FrameTypeValue == ApiFrameType.Byte.RX_IO_64)
            {
                throw new NotImplementedException("This method does not support RX packets");
            }
            else if (packet.FrameTypeValue == ApiFrameType.Byte.EXPLICIT_RX_INDICATOR)
            {
                return ((ExplicitRXIndicatorPacket)packet).X64BitAddress.Equals(remoteDevice.Address64bit);
            } else
            {
                return false;
            }
        }
        
        // NOTE: IPAddrMatch is not implemented
    }
}

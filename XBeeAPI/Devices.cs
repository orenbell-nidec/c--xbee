using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace XBeeAPI
{
    /// <summary>
    /// This class provides common functionality for all XBee devices
    /// </summary>
    public abstract class AbstractXBeeDevice
    {
        /// <summary>
        /// The default timeout for all synchronous opeartions, in seconds
        /// </summary>
        public const int DEFAULT_TIMEOUT_SYNC_OPERATIONS = 60;
        /// <summary>
        /// Pattern used to log packet events
        /// </summary>
        public const string LOG_PATTERN = "{0:<6} {1:<12} {2:<20} {3:<50}";
        
        /// <summary>
        /// Indicates whether this device is open or not
        /// </summary>
        protected bool IsOpen;
        /// <summary>
        /// The operating mode of this device (only relevant for local devices) 
        /// </summary>
        protected OperatingMode.Byte operatingMode;
        /// <summary>
        /// The local device to access this device through (only relevant for remote devices)
        /// </summary>
        protected XBeeDevice localXbeeDevice;
        /// <summary>
        /// Flag indicating if an IO packet was received
        /// </summary>
        protected bool ioPacketReceived;
        /// <summary>
        /// The timeout for this device
        /// </summary>
        protected int timeout;
        protected XBeeAPIPacket ioPacketPayload;
        protected XBee16BitAddress addr16bit;

        protected PacketListener packetListener;
        protected readonly object genericLock;

        public SerialPort ComPort { get; }

        /// <summary>
        /// Constructor. Instantiates a new AbstractXBeeDevice object with the provided parameters
        /// </summary>
        /// <param name="localXbeeDevice">Only neccesary if XBee device being initialized is remote. This local device will behave as a connection interface</param>
        /// <param name="serialPort">Onyl neccessary if the XBee device is local. Used to talk to the XBee device</param>
        /// <param name="syncOpsTimeout">The timeout (in seconds) that will be applied for all synchronous operations</param>
        public AbstractXBeeDevice(XBeeDevice localXbeeDevice = null,
            SerialPort serialPort = null, int syncOpsTimeout = DEFAULT_TIMEOUT_SYNC_OPERATIONS)
        {
            CurrentFrameID = 0x00;
            addr16bit = null;
            Address64bit = null;
            ApplyChangesFlag = true;
            IsOpen = false;
            operatingMode = OperatingMode.Byte.UNKNOWN;
            this.localXbeeDevice = localXbeeDevice;
            ComPort = serialPort;
            SyncOpsTimeout = syncOpsTimeout;
            ioPacketReceived = false;
            ioPacketPayload = null;
            HardwareVersion = 0;
            FirmwareVersion = null;
            Protocol = XBeeProtocol.Byte.UNKNOWN;
            NodeID = null;
            packetListener = null;
            Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        /// Compares two AbstractXBeeDevice classes
        /// </summary>
        /// <param name="obj">Device to compare to</param>
        /// <returns>If both devices have a defined 64 bit address, return true if they match, false otherwise.
        /// Failed a defined 64 bit address, compare 16bit addresses instead. If those aren't defined either, return false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !typeof(AbstractXBeeDevice).IsAssignableFrom(obj.GetType()))
            {
                return false;
            }
            else
            {
                if (this.Address64bit != null && ((AbstractXBeeDevice)obj).Address64bit != null)
                {
                    return (this.Address64bit.Equals(((AbstractXBeeDevice)obj).Address64bit));
                }
                if (this.Address16bit != null && ((AbstractXBeeDevice)obj).Address16bit != null)
                {
                    return (this.Address16bit.Equals(((AbstractXBeeDevice)obj).Address16bit));
                }
                return false;
            }
        }

        /// <summary>
        /// Updates the current device reference with the data provided for the given device.
        /// </summary>
        /// <param name="device">XBee device to get the data from</param>
        public void UpdateDeviceDataFrom(AbstractXBeeDevice device)
        {
            if (device.NodeID != null && device.NodeID.Length > 0)
            {
                this.NodeID = device.NodeID;
            }
        }

        /// <summary>
        /// Returns the value of the provided parameter via an AT command
        /// </summary>
        /// <param name="parameter">Parameter to get</param>
        /// <returns>Byte array of the parameter value</returns>
        /// <exception cref="OperationNotSupportedException">Thrown if the value returned is null. That means something real weird happened</exception>
        /// <exception cref="ArgumentException">Thrown in the parameter is not a 2 character string</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if checking the AT command response failed</exception>
        public virtual byte[] GetParameter(string parameter)
        {
            byte[] value = SendParameter(parameter);

            if (value == null)
            {
                throw new OperationNotSupportedException(String.Format("Could not get the {0} value.", parameter));
            }

            return value;
        }

        /// <summary>
        /// Sets the value of a parameter via an AT Command.
        /// </summary>
        /// <param name="parameter">Parameter to set</param>
        /// <param name="value">Value of the parameter</param>
        /// <remarks>
        /// If you send parameter to a local XBee device, all changes
        /// will be applied automatically, except for non-volatile memory,
        /// in which case you will need to execute the parameter "WR" via
        /// ExecuteCommand method, or applyChanges method.<para/>
        /// If you are sending parameters to a remote XBee device,
        /// the changes will be not applied automatically, unless the "apply_changes"
        /// flag is activated.<para/>
        /// You can set this flag via the method enableApplyChanges.<para/>
        /// This flag only works for volatile memory, if you want to save
        /// changed parameters in non-volatile memory, even for remote devices,
        /// you must execute "WR" command by one of the 2 ways mentioned above
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the parameter is not a 2 character string, or if the value is null. Or if value does not meet the required length a given parameter dictates</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        public virtual void SetParameter(string parameter, byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentException("Value of the parameter cannot be null");
            }

            SendParameter(parameter, value);

            // Refresh cahched parameters if this method modifies some of them
            RefreshIfCached(parameter, value);
        }

        /// <summary>
        /// Executes the provided command. DISCOURAGED.
        /// </summary>
        /// <remarks>
        /// For local nodes, please use format: radio.SendPacket(new ATCommPacket(radio.GetNextFrameID(), "ND"));
        /// For remote nodes, please use format: radio.LocalXBeeDevice.SendPacket(new RemoteATCommandPacket(r.LocalXBeeDevice.GetNextFrameID(), radio.Address64bit, r.Address16bit, RemoteATCmdOptions.APPLY_CHANGES, "FN"));
        /// </remarks>
        /// <param name="parameter">AT command to execute</param>
        /// <exception cref="ArgumentException">Thrown in the parameter is not a 2 character string</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        public void ExecuteCommand(string parameter)
        {
            SendParameter(parameter, null);
        }

        /// <summary>
        /// Sends the given AT parameter to this XBee device with an optional
        /// argument or value and returns the response(likely the value) of that
        /// parameter in a byte array format.
        /// </summary>
        /// <param name="parameter">The name of the AT command to be executed</param>
        /// <param name="parameterValue">Value of the parameter to set (if any)</param>
        /// <returns>Byte array containing response</returns>
        /// <exception cref="ArgumentException">Thrown in the parameter is not a 2 character string</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        protected byte[] SendParameter(string parameter, byte[] parameterValue = null)
        {
            if (parameter == null)
            {
                throw new ArgumentException("Parameter cannot be null");
            } else if (parameter.Length != 2)
            {
                throw new ArgumentException("Parameter must contain exactly 2 characters");
            }

            // Send an at command
            ATCommand atCommand = new ATCommand(parameter, parameterValue);
            ATCommandResponse response = SendATCommand(atCommand);
            CheckATCmdResponseIsValid(response);

            return response.Response;
        }

        /// <summary>
        /// Checks if the provided ATCommandResponse is valid.
        /// </summary>
        /// <param name="response">The AT command response to check</param>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        protected void CheckATCmdResponseIsValid(ATCommandResponse response)
        {
            if (response == null)
            {
                throw new ATCommandException();
            } else if (response.Status != ATCommandStatus.Byte.OK)
            {
                throw new ATCommandException(ATCommandStatus.Get(response.Status));
            }
        }

        // TODO: Test this function. There's likely errors in the way types are handled with the AT command and response packets
        /// <summary>
        /// Sends the given AT command and aits for answer or until the configured receive timeout expires
        /// </summary>
        /// <param name="command">AT command to be sent</param>
        /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        protected ATCommandResponse SendATCommand(ATCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("AT command cannot be null");
            }

            OperatingMode.Byte operatingMode = GetOperatingMode();
            if (operatingMode != OperatingMode.Byte.API_MODE && operatingMode != OperatingMode.Byte.ESCAPED_API_MODE)
            {
                throw new InvalidOperatingModeException(operatingMode);
            }

            ATCommandResponse response = null;
            if (IsRemote())
            {
                RemoteATCmdOptions remoteATCmdOpts = RemoteATCmdOptions.NONE;

                if (ApplyChangesFlag)
                {
                    remoteATCmdOpts |= RemoteATCmdOptions.APPLY_CHANGES;
                }

                XBee16BitAddress remote16bitAddr = Address16bit;
                if (remote16bitAddr == null)
                {
                    remote16bitAddr = XBee16BitAddress.UNKNOWN_ADDRESS;
                }

                RemoteATCommandPacket packet = new RemoteATCommandPacket(GetNextFrameID(), Address64bit,
                    remote16bitAddr, remoteATCmdOpts, Utils.BytesToAscii(command.Command), command.Parameter);

                RemoteATCommandResponsePacket answerPacket = (RemoteATCommandResponsePacket)localXbeeDevice.SendPacket(packet, true);
                response = new ATCommandResponse(command, answerPacket.CommValue, answerPacket.ResponseStatus);
            } else
            {
                XBeeAPIPacket packet;
                if (ApplyChangesFlag)
                {
                    packet = new ATCommPacket(GetNextFrameID(), command.Command, command.Parameter);
                } else
                {
                    // TODO: ATCommQueuePackets not implemented yet
                    packet = new ATCommQueuePacket(GetNextFrameID(), Utils.BytesToAscii(command.Command), command.Parameter);
                }

                // BUG: If the serial port is in echo mode, the reply will be the original packet and throw a casting exception
                //      The original python implementation use SendPacketSyncAndGetResponse here, but that has the more damaging
                //      bug of never retrieving responses from the packet queues until the overflow. However, I think the queues
                //      have since been modified to automatically discard old entries if full, so returning to SendPacketSyncAndGetResponse
                //      may be needed.
                ATCommResponsePacket answerPacket = (ATCommResponsePacket)SendPacket(packet, true);
                response = new ATCommandResponse(command, answerPacket.CommValue, answerPacket.ResponseStatus);
            }

            return response;
        }

        /// <summary>
        /// Applies changes via AC command
        /// </summary>
        /// <exception cref="ArgumentException">Thrown in the parameter is not a 2 character string</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        public void ApplyChanges()
        {
            ExecuteCommand("AC");
        }

        /// <summary>
        /// Writes configurable parameters values to non-volatile memory so modifications persist through reset.
        /// </summary>
        /// <remarks>
        /// Parameters values remain in this device's memory until overwritten by
        /// subsequent use of this method.<para/>
        /// If changes are made without writing them to non-volatile memory, the
        /// module reverts back to previously saved parameters the next time the
        /// module is powered-on.<para/>
        /// Writing the parameter modifications does not mean those values are
        /// immediately applied, this depends on the status of the 'apply
        /// configuration changes' option. Use method isApplyConfigurationChangesEnabled
        /// to get its status and enableApplyConfigurationChanges to enable/disable the
        /// option.If it is disabled, method applyChanges can be used in order to
        /// manually apply the changes.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown in the parameter is not a 2 character string</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        public void WriteChanges()
        {
            ExecuteCommand("WR");
        }

        /// <summary>
        /// Performs a software reset on this XBee device and blocks until the process is completed
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Updates all instance parameters reading them from the XBee device
        /// </summary>
        /// <exception cref="XBeeException">Thrown if remote and the local device's serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Thrown if not operating in API mode</exception>
        /// <exception cref="OperationNotSupportedException">Thrown if the value returned is null. That means something real weird happened</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if checking the AT command response failed</exception>
        public void ReadDeviceInfo()
        {
            if (IsRemote())
            {
                if (!localXbeeDevice.ComPort.IsOpen)
                {
                    throw new XBeeException("Local XBee device's serial port is closed");
                }
            } else
            {
                if (operatingMode != OperatingMode.Byte.API_MODE && operatingMode != OperatingMode.Byte.ESCAPED_API_MODE)
                {
                    throw new InvalidOperatingModeException("Not supported operating mode: " + OperatingMode.Get(operatingMode));
                }

                if (!ComPort.IsOpen)
                {
                    throw new XBeeException("XBee device's serial port is closed");
                }

                // Set hardware version, firmware version, and deduce protocol used
                HardwareVersion = (XBeeAPI.HardwareVersion.Byte)GetParameter("HV")[0];
                FirmwareVersion = GetParameter("VR");
                XBeeProtocol.Byte origProtocol = Protocol;
                Protocol = XBeeProtocol.DetermineProtocol(HardwareVersion, FirmwareVersion);
                if (origProtocol != XBeeProtocol.Byte.UNKNOWN && origProtocol != Protocol)
                {
                    throw new XBeeException(String.Format("Error reading device information:" +
                        "Your device seems to be {0} and NOT {1}. Check if you are using the approrpiate device class",
                        XBeeProtocol.Get(Protocol), XBeeProtocol.Get(origProtocol)));
                }

                byte[] sh = GetParameter("SH");
                byte[] sl = GetParameter("SL");
                Address64bit = new XBee64BitAddress(sh.Concat(sl).ToArray());
                NodeID = Utils.BytesToAscii(GetParameter("NI"));
                if (Protocol == XBeeProtocol.Byte.ZIGBEE || Protocol == XBeeProtocol.Byte.RAW_802_15_4
                    || Protocol == XBeeProtocol.Byte.XTEND || Protocol == XBeeProtocol.Byte.SMART_ENERGY
                    || Protocol == XBeeProtocol.Byte.ZNET)
                {
                    byte[] r = GetParameter("MY");
                    addr16bit = new XBee16BitAddress(r);
                }
            }
        }

        /// <summary>
        /// Node identifier (NI)
        /// </summary>
        public String NodeID { get; set; }
        /// <summary>
        /// The hardware version of the device
        /// </summary>
        public HardwareVersion.Byte HardwareVersion { get; protected set; }
        /// <summary>
        /// The firmware version of the device
        /// </summary>
        public byte[] FirmwareVersion { get; protected set; }
        /// <summary>
        /// The current protocol of the device
        /// </summary>
        public XBeeProtocol.Byte Protocol { get; protected set; }
        /// <summary>
        /// The 16-bit adddress of the device
        /// </summary>
        /// <exception cref="OperationNotSupportedException">Thrown if device is not using 802.15.4 protocol</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        public XBee16BitAddress Address16bit
        {
            get
            {
                return addr16bit;
            }
            protected set
            {
                if (Protocol != XBeeProtocol.Byte.RAW_802_15_4)
                {
                    throw new OperationNotSupportedException("16-bit address can only be set in 802.15.4 protocol");
                }
                SetParameter("MY", value.Address);
                addr16bit = value;
            }
        }
        /// <summary>
        /// The 64-bit address of the device
        /// </summary>
        public XBee64BitAddress Address64bit { get; protected set; }
        /// <summary>
        /// The last used frame ID
        /// </summary>
        public byte CurrentFrameID { get; protected set; }
        /// <summary>
        /// The apply changes flag
        /// </summary>
        protected bool ApplyChangesFlag { get; set; }

        /// <summary>
        /// Determines whether the XBee device is remote or not
        /// </summary>
        /// <returns>True if device is remote, false if local</returns>
        public abstract bool IsRemote();

        /// <summary>
        /// The serial port read timeout, in seconds
        /// </summary>
        /// <exception cref="System.IO.IOException">Thrown when port is in invalid state</exception>
        public int SyncOpsTimeout
        {
            set
            {
                timeout = value;
                if (IsRemote())
                {
                    localXbeeDevice.ComPort.ReadTimeout = value;
                } else
                {
                    ComPort.ReadTimeout = value;
                }
            }
            get
            {
                return timeout;
            }
        }
        /// <summary>
        /// 64-bit address of the device data will be reported to
        /// </summary>
        /// <exception cref="OperationNotSupportedException">Thrown if the value returned is null. That means something real weird happened</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if checking the AT command response failed</exception>
        public XBee64BitAddress DestAddress
        {
            get
            {
                byte[] dh = GetParameter("DH");
                byte[] dl = GetParameter("DL");
                return new XBee64BitAddress(dh.Concat(dl).ToArray());
            }
            set
            {
                bool applyChanges = false;
                lock (genericLock)
                {
                    try
                    {
                        applyChanges = ApplyChangesFlag;
                        ApplyChangesFlag = false;
                        SetParameter("DH", value.Address.Take(4).ToArray());
                        SetParameter("DL", value.Address.Skip(4).ToArray());
                    } catch (TimeoutException err)
                    {
                        throw err;
                    }
                    catch (InvalidOperatingModeException err)
                    {
                        throw err;
                    }
                    catch (ATCommandException err)
                    {
                        throw err;
                    }
                    catch (XBeeException err)
                    {
                        throw err;
                    } finally
                    {
                        if (applyChanges)
                        {
                            ApplyChangesFlag = true;
                            ApplyChanges();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Operating PAN ID of the XBee Device
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if setting the value to null</exception>
        /// <exception cref="OperationNotSupportedException">Thrown if the value returned is null. That means something real weird happened</exception>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if checking the AT command response failed</exception>
        public byte[] PanID
        {
            get
            {
                if (Protocol == XBeeProtocol.Byte.ZIGBEE)
                {
                    return GetParameter("OP");
                } else
                {
                    return GetParameter("ID");
                }
            }
            set
            {
                SetParameter("ID", value);
            }
        }

        // TODO: Go to line 693 of devices.py and resume translating here. I decided to skip some methods

        /// <summary>
        /// Refreshes the proper cached parameter
        /// </summary>
        /// <param name="parameter">Parameter to refresh</param>
        /// <param name="value">New value of the parameter</param>
        /// <exception cref="NullReferenceException">Thrown if value is null</exception>
        /// <exception cref="ArgumentException">Thrown if parameter is "MY" and value is not 2 bytes</exception>
        protected void RefreshIfCached(string parameter, byte[] value)
        {
            if (parameter.CompareTo("NI") == 0)
            {
                NodeID = Utils.BytesToAscii(value);
            } else if (parameter.CompareTo("MY") == 0)
            {
                addr16bit = new XBee16BitAddress(value);
            } else if (parameter.CompareTo("AP") == 0)
            {
                operatingMode = (OperatingMode.Byte)value[0];
            }
        }

        /// <summary>
        /// Returns the next frame ID of the XBee device
        /// </summary>
        /// <returns></returns>
        protected byte GetNextFrameID()
        {
            if (CurrentFrameID == 0xFF)
                CurrentFrameID = 1;
            else
                CurrentFrameID++;
            return CurrentFrameID;
        }

        /// <summary>
        /// Returns the Operating mode (AT, API or API escaped) of this XBee device
        /// for a local device, and the operating mode of the local device used as
        /// communication interface for a remote device.
        /// </summary>
        /// <returns>The operating mode of the local XBee device</returns>
        protected OperatingMode.Byte GetOperatingMode()
        {
            if (IsRemote())
            {
                return localXbeeDevice.GetOperatingMode();
            } else
            {
                return operatingMode;
            }
        }

        /// <summary>
        /// Used to check the operating mode and COM port's state before a sending operation
        /// </summary>
        /// <remarks>
        /// In the Python API, this was implemented as a decorator, but decorators in C# are a
        /// little more involved, so just tack this at the head of a relevant function
        /// </remarks>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        protected void BeforeSendMethod()
        {
            if (!ComPort.IsOpen)
            {
                throw new XBeeException("XBee device's serial port closed");
            }
            if (operatingMode != OperatingMode.Byte.API_MODE && operatingMode != OperatingMode.Byte.ESCAPED_API_MODE)
            {
                throw new InvalidOperatingModeException("Not supported oeprating mode: " + OperatingMode.Get(operatingMode));
            }
        }

        /// <summary>
        /// Used to check if a sending operationg is successful after it is completed
        /// </summary>
        /// <param name="response">The TransmitStatusPacket generated as a reply to the send command</param>
        /// <returns>The same TransmitStatusPacket passed as an argument</returns>
        /// <exception cref="XBeeException">Transmit status was unsuccessful</exception>
        protected TransmitStatusPacket AfterSendMethod(TransmitStatusPacket response)
        {
            if (response.TransmitStatus != TransmitStatus.Byte.SUCCESSS)
            {
                throw new XBeeException("Transmit status: " + TransmitStatus.Get(response.TransmitStatus));
            }
            return response;
        }
        
        /// <summary>
        /// Reads packets until there is one packet found with the provided frame ID
        /// </summary>
        /// <param name="frame_id">Frame ID to use</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Thrown if queue does not return matching packet in time</exception>
        protected XBeeAPIPacket GetPacketByID(byte frame_id)
        {
            XBeeQueue queue = packetListener.XBeeQueue;
            XBeeAPIPacket packet = queue.GetByID(frame_id, XBeeDevice.TIMEOUT_READ_PACKET);
            return packet;
        }

        // TODO: Test this. I'm not sure C# handles enumeration the way python does
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xbee_packet"></param>
        /// <returns></returns>
        protected static bool IsAPIPacket(XBeeAPIPacket xbee_packet)
        {
            ApiFrameType.Byte aft = xbee_packet.FrameTypeValue;
            return (ApiFrameType.Get(aft) != null);
        }

        /// <summary>
        /// Perform all operating needed for synchronous operation when the packet listener is online
        /// </summary>
        /// <remarks>
        /// Steps for synchronous operation:
        ///     1) Put sync_packet to null to discard the last synchronous packet read
        ///     2) Refresh sync_packet to be used by the thread in charge of the synchronous read
        ///     3) Tells the packet listener that this device is waiting for a packet with a determined frame ID
        ///     4) Sends the packet to send
        ///     5) Waits the configured timeout for synchronous operations
        ///     6) Returns all attributes to a consistent state
        ///         a) sync_packet to null
        ///         b) Notify the listener that we are no logner waiting for any packet
        ///     7) Returns the received packet if it has arrived
        /// </remarks>
        /// <param name="packetToSend">Packet to send</param>
        /// <returns>The response packet</returns>
        protected XBeeAPIPacket SendPacketSyncAndGetResponse(XBeeAPIPacket packetToSend)
        {
            // BUG: This method is redundant to just calling SendPacket(packetToSend, true)
            List<XBeeAPIPacket> responseList = new List<XBeeAPIPacket>();
            Semaphore lockVar = new Semaphore(0, 1);

            Reader.PacketReceivedHandler callback = (object sender, XBeeAPIPacket receivedPacket) =>
            {
                if (receivedPacket.NeedsID() && receivedPacket.FrameID == packetToSend.FrameID)
                {
                    if (!typeof(XBeeAPIPacket).IsAssignableFrom(packetToSend.GetType())
                    || !typeof(XBeeAPIPacket).IsAssignableFrom(receivedPacket.GetType()))
                    {
                        return;
                    }

                    // If the packet sent is an AT command, verify that the received one an AT command response and the command matches in both packets
                    if (packetToSend.FrameTypeValue == ApiFrameType.Byte.AT_COMMAND)
                    {
                        if (receivedPacket.FrameTypeValue != ApiFrameType.Byte.AT_COMMAND_RESPONSE)
                        {
                            return;
                        }
                        if (((ATCommPacket)packetToSend).Command != ((ATCommResponsePacket)receivedPacket).Command)
                        {
                            return;
                        }
                    }

                    // If the packet sent is a remote AT command, verify the received one is a remote AT command response and the command matches in both packets
                    if (packetToSend.FrameTypeValue == ApiFrameType.Byte.REMOTE_AT_COMMAND_REQUEST)
                    {
                        if (receivedPacket.FrameTypeValue != ApiFrameType.Byte.REMOTE_AT_COMMAND_RESPONSE)
                        {
                            return;
                        }
                        if (((RemoteATCommandPacket)packetToSend).Command != ((RemoteATCommandResponsePacket)receivedPacket).Command)
                        {
                            return;
                        }
                    }

                    // Verify the sent packet is not the received one. This can happen when echo mode is enabled.
                    if (!packetToSend.Equals(receivedPacket))
                    {
                        // BUG: This also gets added to packetListener's queues, but is never removed from them.
                        //      The queues will eventually fill and even though packetListener pops elements out
                        //      to make room for new ones, it doesn't do so in a thread safe way. This also means
                        //      the method HasPackets is now useless as a means to determine if there's unprocessed
                        //      packets. See Reader.AddPacketQueue for more on this issue.
                        //      This bug exists in the original python implementation.
                        //      Proposed solution if this becomes an issue: Replace every instance of SendPacketSyncAndGetResponse
                        //      with SendPacket(packetToSend, true) and add an argument to XBeeQueue.Put that will
                        //      permit the automatic deletion of old packets whenever the queue is full, and do so
                        //      within the thread lock
                        responseList.Add(receivedPacket);

                        // Signal to the code below (in the try block) that a packet was received
                        lockVar.Release();
                    }
                }
            };

            // Subscribe the above callback to the packet received handler
            packetListener.PacketReceived += callback;

            try
            {
                SendPacket(packetToSend);

                lockVar.WaitOne(timeout * 1000);  // Block here until a packet is received

                if (responseList.Count <= 0)
                {
                    throw new TimeoutException("Response was not recieved in the configured timeout");
                }
                return responseList[0];
            } finally
            {
                // Unsubscribe the callback from the packet received handler
                packetListener.PacketReceived -= callback;
            }
        }

        /// <summary>
        /// Allows you to add an external callback to be run whenever an API packet is received
        /// </summary>
        /// <remarks>
        /// This is not in the original python implementation, so idfk how XCTU is able to intercept API packet responses
        /// </remarks>
        /// <param name="callback">The callback function to run</param>
        public void AddPacketReceivedCallback(Reader.PacketReceivedHandler callback)
        {
            packetListener.PacketReceived += callback;
        }
        /// <summary>
        /// Remove an external callback from the PacketReceived event
        /// </summary>
        /// <remarks>
        /// This is not in the original python implementation, so idfk how XCTU is able to intercept API packet responses
        /// </remarks>
        /// <param name="callback">The callback function to remove</param>
        public void DelPacketReceivedCallback(Reader.PacketReceivedHandler callback)
        {
            packetListener.PacketReceived -= callback;
        }

        /// <summary>
        /// Sends a packet to the device and waits for a response
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="sync">Decides whether to wait for response</param>
        /// <returns>The response packet if sync is true. Null if sync is false</returns>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        protected XBeeAPIPacket SendPacket(XBeeAPIPacket packet, bool sync=false)
        {
            if (!packetListener.IsRunning)
            {
                throw new XBeeException("Packet listener is not running");
            }

            bool escape = operatingMode == OperatingMode.Byte.ESCAPED_API_MODE;
            byte[] output = packet.Output(escape);
            ComPort.Write(output, 0, output.Length);
            Log.Info(String.Format(LOG_PATTERN, ComPort.PortName, "SENT", OperatingMode.Get(operatingMode), Utils.BytesToHexString(output, true)));
            return (sync) ? GetPacketByID(packet.FrameID) : null;
        }

        public log4net.ILog Log { get; private set; }
    }

    /// <summary>
    /// Refers to a local XBee device, ie one being talked to directly via serial
    /// </summary>
    public class XBeeDevice : AbstractXBeeDevice
    {
        protected const double TIMEOUT_BEFORE_COMMAND_MODE = 1.2;
        protected const double TIMEOUT_ENTER_COMMAND_MODE = 1.5;
        protected const int TIMEOUT_RESET = 5;
        public const int TIMEOUT_READ_PACKET = 3;
        protected const char COMMMAND_MODE_CHAR = '+';
        protected const string COMMAND_MODE_OK = "OK\r";

        /// <summary>
        /// XBeeNetwork associated with this local device
        /// </summary>
        public XBeeNetwork Network { get; protected set; }

        protected XBeeQueue PacketQueue { get; set; }
        protected XBeeQueue DataQueue { get; set; }
        protected XBeeQueue ExplicitQueue { get; set; }
        protected bool ModemStatusReceived { get; set; }

        /// <summary>
        /// Operating mode of the local device (transparent, AT mode, API)
        /// </summary>
        public OperatingMode.Byte OpMode
        {
            get { return operatingMode; }
        }
        
        /// <summary>
        /// Constructor. Instantiates new XBeeDevice with provided parameters for serial port
        /// </summary>
        /// <param name="port">String representing COM port to use</param>
        /// <param name="baudrate">Baud rate of the serial port</param>
        /// <param name="databits">COM port bitsize, either 7 or 8 (8 is default)</param>
        /// <param name="stopBits">COM port stop bits, default is 1</param>
        /// <param name="parity">COM port parity settings, default is none</param>
        /// <param name="flowControl">COM port flow control. Default is none</param>
        /// <param name="syncOpsTimeout">COM port read timeout</param>
        /// <exception cref="System.IO.IOException">Thrown if COM port is in unstable state</exception>
        public XBeeDevice(string port, int baudrate, int databits = 8, StopBits stopBits = StopBits.One,
            Parity parity = Parity.None, Handshake flowControl = Handshake.None,
            int syncOpsTimeout = DEFAULT_TIMEOUT_SYNC_OPERATIONS )
            : base(serialPort:new SerialPort(port, baudrate, parity, databits, stopBits))
        {
            ComPort.Handshake = flowControl;
            ComPort.ReadTimeout = syncOpsTimeout * 1000;
            Network = new XBeeNetwork(this);
            PacketQueue = null;
            DataQueue = null;
            ExplicitQueue = null;
            ModemStatusReceived = false;
        }

        /// <summary>
        /// Opens the communication with the XBee device and loads some information about it
        /// </summary>
        /// <exception cref="XBeeException">Thrown if device is already open, or if packet listener spontaneously shut down while checking operating mode</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when you do not have the permissions to open specified COM port</exception>
        /// <exception cref="System.IO.IOException">The specified COM port is in an unexpected state</exception>
        /// <exception cref="OutOfMemoryException">There is not enough memory to start the packet listener thread</exception>
        /// <exception cref="OperationNotSupportedException">Thrown if, while determining operating mode, and AT command got a null response. That means something real weird happened</exception>
        /// <exception cref="ATCommandException">Thrown if checking the AT command response failed while determining oeprating mode</exception>
        /// <exception cref="ThreadInterruptedException">Packet listener thread was waiting while attempting to join while determining oeprating mode</exception>
        /// <exception cref="InvalidOperatingModeException">Thrown if operating mode is either AT command or undetermined</exception>
        /// <exception cref="TimeoutException">COM port timed out while trying to write</exception>
        public virtual void Open()
        {
            if (IsOpen)
            {
                throw new XBeeException("XBee device already open");
            }

            // Open the serial port
            ComPort.Open();
            Log.Info(String.Format("%s port opened", ComPort.PortName));

            // Initialize packet listener
            packetListener = new PacketListener(ComPort, this);
            PacketQueue = packetListener.XBeeQueue;
            DataQueue = packetListener.DataXBeeQueue;
            ExplicitQueue = packetListener.ExplicitXBeeQueue;
            packetListener.Start();

            // Determine operating mode of XBee device
            operatingMode = DetermineOperatingMode();
            if (operatingMode == OperatingMode.Byte.UNKNOWN)
            {
                Close();
                throw new InvalidOperatingModeException("Could not determine operating mode");
            }
            if (operatingMode == OperatingMode.Byte.AT_MODE)
            {
                Close();
                throw new InvalidOperatingModeException(operatingMode);
            }
        }

        /// <summary>
        /// Closes the communication with this XBee device
        /// </summary>
        /// <exception cref="System.IO.IOException">COM port was in an invalid state</exception>
        public void Close()
        {
            if (Network != null)
            {
                Network.StopDiscoveryProcess();
            }

            if (packetListener != null)
            {
                packetListener.Stop();
            }

            if (ComPort != null && ComPort.IsOpen)
            {
                ComPort.Close();
                Log.Info(String.Format("%s port closed", ComPort));
            }

            IsOpen = false;
        }

        /// <summary>
        /// Returns the value of the provided parameter via an AT command
        /// </summary>
        /// <param name="parameter">Parameter to get</param>
        /// <returns>Byte array of the parameter value</returns>
        /// <exception cref="OperationNotSupportedException">Thrown if the value returned is null. That means something real weird happened</exception>
        /// <exception cref="ArgumentException">Thrown in the parameter is not a 2 character string</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running or serial port is closed</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if checking the AT command response failed</exception>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        public override byte[] GetParameter(string parameter)
        {
            BeforeSendMethod();
            return base.GetParameter(parameter);
        }

        /// <summary>
        /// Sets the value of a parameter via an AT Command.
        /// </summary>
        /// <param name="parameter">Parameter to set</param>
        /// <param name="value">Value of the parameter</param>
        /// <exception cref="XBeeException">Serial port is closed or packet listener isn't running</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentException">Thrown in the parameter is not a 2 character string</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not repoErt success</exception>
        public override void SetParameter(string parameter, byte[] value)
        {
            BeforeSendMethod();
            base.SetParameter(parameter, value);
        }

        /// <summary>
        /// Sends data to a remote device corresponding to the given 64/16 bit address. Blocking
        /// </summary>
        /// <param name="x64addr">64 bit address of the device to receive the data</param>
        /// <param name="x16addr">16 bit address of the device that will receive the data</param>
        /// <param name="data">Data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>The response/status packet</returns>
        /// <exception cref="XBeeException">Serial port is closed, packet listener isn't running, or receive status is unsuccessful</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentNullException">Thrown if x64addr, x16addr, or data is null</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        public TransmitStatusPacket SendData64_16(XBee64BitAddress x64addr, XBee16BitAddress x16addr, byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            BeforeSendMethod();

            if (x64addr == null)
            {
                throw new ArgumentNullException("64-bit address cannot be null");
            }
            if (x16addr == null)
            {
                throw new ArgumentNullException("16-bit address cannot be null");
            }
            if (data == null)
            {
                throw new ArgumentNullException("Data cannot be null");
            }

            // Build the transmit packet and send. Check the response and return
            TransmitPacket packet = new TransmitPacket(GetNextFrameID(), x64addr, x16addr, 0, transmitOptions, data);
            TransmitStatusPacket status = (TransmitStatusPacket)SendPacket(packet, true);
            return AfterSendMethod(status);
        }

        /// <summary>
        /// Sends data to a remote device corresponding to the given 64-bit address and returns the packet response. Blocking
        /// </summary>
        /// <remarks>
        /// This method blocks while is waits for a response
        /// </remarks>
        /// <param name="x64addr">64-bit address of the device that will receive the data</param>
        /// <param name="data">The raw data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>The response packet</returns>
        /// <exception cref="XBeeException">Serial port is closed or transmit status reports unsuccessful</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentNullException">Thrown if x64addr or data is null</exception>
        /// <exception cref="NotImplementedException">Thrown if device uses 802.15.4 firmware, as that functionality is not supported here</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        public XBeeAPIPacket SendData64(XBee64BitAddress x64addr, byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            BeforeSendMethod();

            if (x64addr == null)
            {
                throw new ArgumentNullException("64-bit address cannot be null");
            }
            if (data == null)
            {
                throw new ArgumentNullException("Data cannot be null");
            }

            TransmitPacket packet;
            if (Protocol == XBeeProtocol.Byte.RAW_802_15_4)
            {
                // NOTE: Even if it was, it would require making and returning a TX64Packet, so anyone who uses this function would be burdened with more type-checking
                throw new NotImplementedException("RAW 802.15.4 is not implemented yet");
            } else
            {
                packet = new TransmitPacket(GetNextFrameID(), x64addr, XBee16BitAddress.UNKNOWN_ADDRESS, 0, transmitOptions, data);
            }

            TransmitStatusPacket status = (TransmitStatusPacket)SendPacket(packet, true);
            return AfterSendMethod(status);
        }

        /// <summary>
        /// Sends data to a remote device corresponding to the 16-bit address. Blocking
        /// </summary>
        /// <param name="x16addr">The 16 bit address to send to</param>
        /// <param name="data">Raw data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>The response/status packet</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentNullException">Thrown if arguments are null</exception>
        /// <exception cref="NotImplementedException">Always thrown even if above errors are avoided, because this API doesn't support 802.15.4 yet</exception>
        public XBeeAPIPacket SendData16(XBee16BitAddress x16addr, byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            BeforeSendMethod();

            if (x16addr == null)
            {
                throw new ArgumentNullException("64-bit address cannot be null");
            }
            if (data == null)
            {
                throw new ArgumentNullException("Data cannot be null");
            }

            throw new NotImplementedException("RAW 802.15.4 is not implemented yet");
        }

        /// <summary>
        /// Sends data to a remote device synchronously
        /// </summary>
        /// <param name="remote">Remote device the send data to</param>
        /// <param name="data">Raw data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>The response/status packet</returns>
        /// <exception cref="ArgumentNullException">Thrown if provided remote device or its addresses are null</exception>
        /// <exception cref="XBeeException">Serial port is closed, packet listener isn't running, or transmit status is unsuccessful</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        public XBeeAPIPacket SendData(RemoteXBeeDevice remote, byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            if (remote == null)
            {
                throw new ArgumentNullException("Remote XBee device cannot be null");
            }
            
            if (Protocol == XBeeProtocol.Byte.ZIGBEE || Protocol == XBeeProtocol.Byte.DIGI_POINT)
            {
                if (remote.Address64bit != null && remote.Address16bit != null)
                {
                    return SendData64_16(remote.Address64bit, remote.Address16bit, data, transmitOptions);
                } else if (remote.Address64bit != null)
                {
                    return SendData64(remote.Address64bit, data, transmitOptions);
                } else
                {
                    return SendData64_16(XBee64BitAddress.UNKNOWN_ADDRESS, remote.Address16bit, data, transmitOptions);
                }
            } else if (Protocol == XBeeProtocol.Byte.RAW_802_15_4)
            {
                if (remote.Address64bit != null)
                {
                    return SendData64(remote.Address64bit, data, transmitOptions);
                } else
                {
                    return SendData16(remote.Address16bit, data, transmitOptions);
                }
            } else
            {
                return SendData64(remote.Address64bit, data, transmitOptions);
            }
        }

        /// <summary>
        /// This method sends data to a remote device corresponding to the 64/16 bit address. Non-blocking
        /// </summary>
        /// <param name="x64addr">64bit address to send to</param>
        /// <param name="x16addr">16bit address to send to</param>
        /// <param name="data">Raw data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <exception cref="XBeeException">Thrown if com port is closed, or packet listener isn't running</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentNullException">Thrown when arguments are null</exception>
        /// <exception cref="TimeoutException">Thrown if the COM port times out when writing</exception>
        public void SendDataAsync64_16(XBee64BitAddress x64addr, XBee16BitAddress x16addr, byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            BeforeSendMethod();

            if (x64addr == null)
            {
                throw new ArgumentNullException("64-bit address cannot be null");
            }
            if (x16addr == null)
            {
                throw new ArgumentNullException("16-bit address cannot be null");
            }
            if (data == null)
            {
                throw new ArgumentNullException("Data cannot be null");
            }

            // Build the transmit packet and send. Check the response and return
            TransmitPacket packet = new TransmitPacket(GetNextFrameID(), x64addr, x16addr, 0, transmitOptions, data);
            SendPacket(packet);
        }

        /// <summary>
        /// Sends data to a remote device given by the 64bit address. Non-blocking
        /// </summary>
        /// <param name="x64addr">64bit address to send data to</param>
        /// <param name="data">Raw data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <exception cref="XBeeException">Serial port is closed or packet listener isn't running</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="NotImplementedException">Thrown if local device uses 802.15.4 firmware, which is unsupported</exception>
        /// <exception cref="ArgumentNullException">Thrown when arguments are null</exception>
        /// <exception cref="TimeoutException">Thrown if the COM port times out when writing</exception>
        public void SendDataAsync64(XBee64BitAddress x64addr, byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            BeforeSendMethod();

            if (x64addr == null)
            {
                throw new ArgumentNullException("64-bit address cannot be null");
            }
            if (data == null)
            {
                throw new ArgumentNullException("Data cannot be null");
            }

            TransmitPacket packet;
            if (Protocol == XBeeProtocol.Byte.RAW_802_15_4)
            {
                // NOTE: Even if it was, it would require making and returning a TX64Packet, so anyone who uses this function would be burdened with more type-checking
                throw new NotImplementedException("RAW 802.15.4 is not implemented yet");
            }
            else
            {
                packet = new TransmitPacket(GetNextFrameID(), x64addr, XBee16BitAddress.UNKNOWN_ADDRESS, 0, transmitOptions, data);
            }
            
            SendPacket(packet);
        }

        /// <summary>
        /// Sends data to the device given by the 16bit address
        /// </summary>
        /// <param name="x16addr">16bit address to send to</param>
        /// <param name="data">Raw data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <exception cref="XBeeException">Serial port is closed or packet listener isn't running</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentNullException">Thrown when arguments are null</exception>
        /// <exception cref="NotImplementedException">Even if all other exceptions are avoided, this is guaranteed to be thrown, since 802.15.4 support is not added yet</exception>
        public void SendDataAsync16(XBee16BitAddress x16addr, byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            BeforeSendMethod();

            if (x16addr == null)
            {
                throw new ArgumentNullException("64-bit address cannot be null");
            }
            if (data == null)
            {
                throw new ArgumentNullException("Data cannot be null");
            }

            throw new NotImplementedException("RAW 802.15.4 is not implemented yet");
        }

        /// <summary>
        /// Sends data to a remote device. Non-blocking
        /// </summary>
        /// <param name="remote">Remote device to send to</param>
        /// <param name="data">Raw data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <exception cref="ArgumentNullException">Thrown if remote or its addresses are null</exception>
        /// <exception cref="XBeeException">Thrown if com port is closed, or packet listener isn't running</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="TimeoutException">Thrown if writing to the COM port times out when writing</exception>
        /// <exception cref="NotImplementedException">Thrown if attempting to make use of 802.15.4 functionality, which is unsupported</exception>
        public void SendDataAsync(RemoteXBeeDevice remote, byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            if (remote == null)
            {
                throw new ArgumentNullException("Remote XBee device cannot be null");
            }

            if (Protocol == XBeeProtocol.Byte.ZIGBEE || Protocol == XBeeProtocol.Byte.DIGI_POINT)
            {
                if (remote.Address64bit != null && remote.Address16bit != null)
                {
                    SendDataAsync64_16(remote.Address64bit, remote.Address16bit, data, transmitOptions);
                }
                else if (remote.Address64bit != null)
                {
                    SendDataAsync64(remote.Address64bit, data, transmitOptions);
                }
                else
                {
                    SendDataAsync64_16(XBee64BitAddress.UNKNOWN_ADDRESS, remote.Address16bit, data, transmitOptions);
                }
            }
            else if (Protocol == XBeeProtocol.Byte.RAW_802_15_4)
            {
                if (remote.Address64bit != null)
                {
                    SendDataAsync64(remote.Address64bit, data, transmitOptions);
                }
                else
                {
                    SendDataAsync16(remote.Address16bit, data, transmitOptions);
                }
            }
            else
            {
                SendDataAsync64(remote.Address64bit, data, transmitOptions);
            }
        }

        /// <summary>
        /// Sends the provided data to all the XBee nodes in the network
        /// </summary>
        /// <param name="data">Byte array to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>XBeeAPIPacket of the response</returns>
        /// <exception cref="XBeeException">Serial port is closed or transmit status reports unsuccessful</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="NotImplementedException">Thrown if device uses 802.15.4 firmware, as that functionality is not supported here</exception>
        public XBeeAPIPacket SendDataBroadcast(byte[] data, TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            return SendData64(XBee64BitAddress.BROADCAST_ADDRESS, data, transmitOptions);
        }

        /// <summary>
        /// Reads new data received by this XBee device
        /// </summary>
        /// <param name="timeout">Read timeout in seconds</param>
        /// <returns>The read message or null if no data was received</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentException">Thrown if a negative timeout value is passed</exception>
        /// <exception cref="NotImplementedException">Thrown if an RX packet is found in the data or explicit queues. RX packets aren't supported yet</exception>
        /// <exception cref="TimeoutException">Thrown if timeout expires before a element is found in the queues</exception>
        public XBeeMessage ReadData(int timeout = 0)
        {
            return ReadDataPacket(null, timeout, false);
        }

        /// <summary>
        /// Reads new data received from the given remote device
        /// </summary>
        /// <param name="remote">Remote device that sent the data</param>
        /// <param name="timeout">Read timeout in seconds</param>
        /// <returns>The read message sent by the remote device or null if no data was received</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentException">Thrown if a negative timeout value is passed</exception>
        /// <exception cref="NotImplementedException">Thrown if an RX packet is found in the data or explicit queues. RX packets aren't supported yet</exception>
        /// <exception cref="TimeoutException">Thrown if timeout expires before a element is found in the queues</exception>
        public XBeeMessage ReadDataFrom(RemoteXBeeDevice remote, int timeout = 0)
        {
            return ReadDataPacket(remote, timeout, false);
        }

        /// <summary>
        /// Return whether the XBee device's queue has packets or not (does not include explicit packets)
        /// </summary>
        /// <returns>True is this device has packets, false otherwise</returns>
        public bool HasPackets()
        {
            return PacketQueue.Count != 0;
        }

        /// <summary>
        /// Returns whether the XBee's device's queue has explicit packets or not.
        /// </summary>
        /// <returns>True if this device has explicit packets, false otherwise</returns>
        public bool HasExplicitPackets()
        {
            return ExplicitQueue.Count != 0;
        }

        /// <summary>
        /// Flushes the packet queues
        /// </summary>
        public void FlushQueues()
        {
            PacketQueue.Flush();
            DataQueue.Flush();
            ExplicitQueue.Flush();
        }

        /// <summary>
        /// Performs a software reset on this XBee device and blocks until the process is completed.
        /// </summary>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running or if invalid modem status is received</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        public override void Reset()
        {
            // Send reset command
            ATCommandResponse response = SendATCommand(new ATCommand("FR"));

            // Check if AT command response is valid
            CheckATCmdResponseIsValid(response);

            Semaphore lockVar = new Semaphore(0, 1);
            bool modemStatusReceived = false;

            Reader.ModemStatusReceivedHandler callback = (object sender, ModemStatus.Byte status) =>
            {
                if (status == ModemStatus.Byte.HARDWARE_RESET || status == ModemStatus.Byte.WATCHDOG_TIMER_RESET)
                {
                    modemStatusReceived = true;
                    lockVar.Release();
                }
            };

            packetListener.ModemStatusReceived += callback;
            lockVar.WaitOne(TIMEOUT_RESET);
            packetListener.ModemStatusReceived -= callback;

            if (!modemStatusReceived)
            {
                throw new XBeeException("Invalid modem status");
            }
        }

        /// <summary>
        /// Returns a list of internal callbacks for received packets
        /// </summary>
        /// <returns>List of PacketReceivedHandler's</returns>
        public List<Reader.PacketReceivedHandler> GetXBeeDeviceCallbacks()
        {
            return Network.GetDiscoveryCallbacks();
        }

        /// <summary>
        /// Indicates whether this device is remote
        /// </summary>
        /// <returns>True if communicating with this device via another device. False if controlling this device from the COM port</returns>
        public override bool IsRemote()
        {
            return false;
        }

        /// <summary>
        /// Sends data in an explicit packet
        /// </summary>
        /// <param name="remote">Remote device to send data to</param>
        /// <param name="data">Data to send</param>
        /// <param name="srcEndpoint">Source endpoint of transmission</param>
        /// <param name="destEndpoint">Destination endpoint of transmission</param>
        /// <param name="clusterID">Cluster ID of transmission</param>
        /// <param name="profileID">Profile ID of transmission</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>Response packet</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        protected XBeeAPIPacket SendExplData(RemoteXBeeDevice remote, byte[] data, byte srcEndpoint, byte destEndpoint,
            ushort clusterID, ushort profileID, TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            BeforeSendMethod();
            return SendPacket(BuildExplDataPacket(remote, data, srcEndpoint, destEndpoint,
                clusterID, profileID, false, transmitOptions), true);
        }

        /// <summary>
        /// Sends data in an explicit packet
        /// </summary>
        /// <param name="remote">Remote device to send data to</param>
        /// <param name="data">Data to send</param>
        /// <param name="srcEndpoint">Source endpoint of transmission</param>
        /// <param name="destEndpoint">Destination endpoint of transmission</param>
        /// <param name="clusterID">Cluster ID of transmission</param>
        /// <param name="profileID">Profile ID of transmission</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>null pointer</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="TimeoutException">Thrown if the COM port times out when writing</exception>
        protected XBeeAPIPacket SendExplDataAsync(RemoteXBeeDevice remote, byte[] data, byte srcEndpoint, byte destEndpoint,
            ushort clusterID, ushort profileID, TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            BeforeSendMethod();
            return SendPacket(BuildExplDataPacket(remote, data, srcEndpoint, destEndpoint,
                clusterID, profileID, false, transmitOptions));
        }
        /// <summary>
        /// Sends the provided data to all nodes of the network
        /// </summary>
        /// <param name="data">Raw data to send</param>
        /// <param name="srcEndpoint">Source endpoint of transmission</param>
        /// <param name="destEndpoint">Destination endpoint of transmission</param>
        /// <param name="clusterID">Cluster ID of transmission</param>
        /// <param name="profileID">Profile ID of transmission</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>Response packet</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        protected XBeeAPIPacket SendExplDataBroadcast(byte[] data, byte srcEndpoint, byte destEndpoint,
            ushort clusterID, ushort profileID, TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            return SendPacket(BuildExplDataPacket(null, data, srcEndpoint, destEndpoint,
                clusterID, profileID, true, transmitOptions), true);
        }

        /// <summary>
        /// Reads new explicit data received by this device
        /// </summary>
        /// <param name="timeout">Time to wait for an available message</param>
        /// <returns>Message containing the received data</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentException">Thrown if a negative timeout value is passed</exception>
        /// <exception cref="NotImplementedException">Thrown if an RX packet is found in the data or explicit queues. RX packets aren't supported yet</exception>
        /// <exception cref="TimeoutException">Thrown if timeout expires before a element is found in the queues</exception>
        protected ExplicitXBeeMessage ReadExplData(int timeout = 0)
        {
            return (ExplicitXBeeMessage)ReadDataPacket(null, timeout, true);
        }

        /// <summary>
        /// Reads new explicit data received by this device from a specified remote
        /// </summary>
        /// <param name="remote">Remote device to find a packet from</param>
        /// <param name="timeout">Time to wait for an available message</param>
        /// <returns>Message containing the received data</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentException">Thrown if a negative timeout value is passed</exception>
        /// <exception cref="NotImplementedException">Thrown if an RX packet is found in the data or explicit queues. RX packets aren't supported yet. Always thrown if expl is set to true</exception>
        /// <exception cref="TimeoutException">Thrown if timeout expires before a element is found in the queues</exception>
        /// <exception cref="NotImplementedException">Always thrown since ExplicitXBeeMessages haven't been implemented yet</exception>
        protected ExplicitXBeeMessage ReadExplDataFrom(RemoteXBeeDevice remote, int timeout = 0)
        {
            return (ExplicitXBeeMessage)ReadDataPacket(remote, timeout, true);
        }

        /// <summary>
        /// Reads a a new data packet received by this XBee device during the provided timeout
        /// </summary>
        /// <param name="remote">Remote device to get a packet from. If null, reads packet from any device</param>
        /// <param name="timeout">The time, in seconds, to wait for a packet</param>
        /// <param name="expl">If true, read an explicit packet. If false, read a standard packet</param>
        /// <returns>The XBee message received by this device</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentException">Thrown if a negative timeout value is passed</exception>
        /// <exception cref="NotImplementedException">Thrown if an RX packet is found in the data or explicit queues. RX packets aren't supported yet. Always thrown if expl is set to true</exception>
        /// <exception cref="TimeoutException">Thrown if timeout expires before a element is found in the queues</exception>
        protected XBeeMessage ReadDataPacket(RemoteXBeeDevice remote, int timeout, bool expl)
        {
            BeforeSendMethod();

            if (timeout < 0)
            {
                throw new ArgumentException("Read timeout must be 0 or greater");
            }

            XBeeAPIPacket packet;
            if (!expl)
            {
                if (remote == null)
                {
                    packet = DataQueue.Get(timeout: timeout);
                }
                else
                {
                    packet = DataQueue.GetByRemote(remote, timeout);
                }
            } else
            {
                if (remote == null)
                {
                    packet = ExplicitQueue.Get(timeout: timeout);
                } else
                {
                    packet = ExplicitQueue.GetByRemote(remote, timeout);
                }
            }

            if (packet == null)
            {
                return null;
            }

            ApiFrameType.Byte frameType = packet.FrameTypeValue;
            if (frameType == ApiFrameType.Byte.RECEIVE_PACKET || frameType == ApiFrameType.Byte.RX_16 || frameType == ApiFrameType.Byte.RX_64)
            {
                return BuildXBeeMessage(packet, false);
            } else if (frameType == ApiFrameType.Byte.EXPLICIT_RX_INDICATOR)
            {
                return BuildXBeeMessage(packet, true);
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Attemps to put this device in AT command mode
        /// </summary>
        /// <returns>True if the device has entered AT command mode, false otherwise</returns>
        /// <exception cref="InvalidOperatingModeException">Thrown if device not in transparent mode</exception>
        /// <exception cref="ThreadInterruptedException">Packet listener thread was waiting while attempting to join</exception>
        /// <exception cref="System.IO.IOException">COM port is in an invalid state while attempting to clear RX buffer</exception>
        /// <exception cref="InvalidOperationException">COM port was closed</exception>
        /// <exception cref="TimeoutException">COM port timed out while trying to write</exception>
        protected bool EnterATCommandMode()
        {
            if (OpMode != OperatingMode.Byte.AT_MODE)
            {
                throw new InvalidOperatingModeException("Invalid mode. Command mode can be only accessed while in AT mode");
            }

            // Disable packet listener
            if (packetListener != null && packetListener.IsRunning)
            {
                packetListener.Stop();
                packetListener.Join();
            }
            ComPort.DiscardInBuffer();

            // Wait at least 1 sec to enter command mode
            Thread.Sleep((int)(TIMEOUT_BEFORE_COMMAND_MODE*1000));

            // Send command mode sequence
            byte[] b = new byte[] { (byte)COMMMAND_MODE_CHAR };
            ComPort.Write(b, 0, b.Length);
            ComPort.Write(b, 0, b.Length);
            ComPort.Write(b, 0, b.Length);

            // Wait some time to let the module generate a response
            Thread.Sleep((int)(TIMEOUT_ENTER_COMMAND_MODE*1000));

            // Read data from the device (should be 'OK\r')
            string data = ComPort.ReadExisting();

            return data != null && data.Contains(COMMAND_MODE_OK);
        }

        /// <summary>
        /// Deduces the operating mode of the local device
        /// </summary>
        /// <returns>Enumerator indicating AT or API modes</returns>
        /// <exception cref="OperationNotSupportedException">Thrown if the value returned is null. That means something real weird happened</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running or the serial port is closed</exception>
        /// <exception cref="ATCommandException">Thrown if checking the AT command response failed</exception>
        /// <exception cref="ThreadInterruptedException">Packet listener thread was waiting while attempting to join</exception>
        /// <exception cref="System.IO.IOException">COM port is in an invalid state while attempting to clear RX buffer</exception>
        /// <exception cref="TimeoutException">COM port timed out while trying to write</exception>
        protected OperatingMode.Byte DetermineOperatingMode()
        {
            try {
                // Query for the operating mode. Will get one byte back.
                operatingMode = OperatingMode.Byte.API_MODE;
                byte[] response = GetParameter("AP");
                return (OperatingMode.Byte)response[0];
            } catch (TimeoutException)
            {
                // If we're in AT mode, the radio won't response to an API packet
                operatingMode = OperatingMode.Byte.AT_MODE;
                try
                {
                    if (EnterATCommandMode())
                    {
                        return OperatingMode.Byte.AT_MODE;
                    }
                } catch (TimeoutException err2)
                {
                    Log.Error(err2);
                }
            }

            return OperatingMode.Byte.UNKNOWN;
        }

        /// <summary>
        /// Sends packet to the device and (optionally) awaits response
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="sync">If true, wait for response. If false, do not</param>
        /// <returns>Response/status packet</returns>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        public new XBeeAPIPacket SendPacket(XBeeAPIPacket packet, bool sync = false)
        {
            // This improves the visibility of AbstratXBeeDevice.SendPacket, to mimic 
            return base.SendPacket(packet, sync);
        }

        /// <summary>
        /// Builds and returns the XBee message corresponding to the provided packet
        /// </summary>
        /// <param name="packet">The packet to convert to an XBee message</param>
        /// <param name="expl">Tru is packet is explicit, false otherwise</param>
        /// <returns>XBee message corresponding to the packet given</returns>
        /// <exception cref="NotImplementedException">Thrown if expl is set to true. ExplicitXBeeMessages haven't been implemented yet</exception>
        protected XBeeMessage BuildXBeeMessage(XBeeAPIPacket packet, bool expl=false)
        {
            XBee64BitAddress x64addr = null;
            XBee16BitAddress x16addr = null;
            if (packet.FrameTypeValue == ApiFrameType.Byte.RECEIVE_PACKET)
            {
                x64addr = ((ReceivePacket)packet).X64bit_addr;
                x16addr = ((ReceivePacket)packet).X16bit_addr;
            } else if (packet.FrameTypeValue == ApiFrameType.Byte.EXPLICIT_ADDRESSING)
            {
                x64addr = ((ExplicitAddressingPacket)packet).X64bit_addr;
                x16addr = ((ExplicitAddressingPacket)packet).X16bit_addr;
            }
            RemoteXBeeDevice remote = new RemoteXBeeDevice(this, x64addr, x16addr);

            if (expl)
            {
                ExplicitAddressingPacket ep = (ExplicitAddressingPacket)packet;
                return new ExplicitXBeeMessage(ep.RfData, remote, Utils.TimeNow(),
                    ep.SourceEndpoint, ep.DestEndpoint, ep.ClusterID, ep.ProfileID, ep.IsBroadcast);
            } else
            {
                ReceivePacket rp = (ReceivePacket)packet;
                return new XBeeMessage(rp.Rfdata, remote, Utils.TimeNow(), rp.IsBroadcast);
            }
        }

        /// <summary>
        /// Builds a returns an explicit data packet
        /// </summary>
        /// <param name="remote">Remote to send data to</param>
        /// <param name="data">Raw data to send</param>
        /// <param name="srcEndpoint">Source endpoint of transmission</param>
        /// <param name="destEndpoint">Destination endpoint of transmission</param>
        /// <param name="clusterID">Cluster ID of transmission</param>
        /// <param name="profileID">Profile ID of transmission</param>
        /// <param name="broadcast">Flag indicating whether the packet should be broadcast or not</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <exception cref="NotImplementedException">Always returns this, since explicit packets are not implemented yet</exception>
        /// <returns></returns>
        protected ExplicitAddressingPacket BuildExplDataPacket(RemoteXBeeDevice remote, byte[] data, byte srcEndpoint, byte destEndpoint,
            ushort clusterID, ushort profileID, bool broadcast=false, TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            throw new NotImplementedException("ExplicitAddressingPacket not implemented");
        }

        public new byte GetNextFrameID()
        {
            return base.GetNextFrameID();
        }

    }

    /// <summary>
    /// Represents Wifi Device. Not implemented
    /// </summary>
    public class Raw802Device : XBeeDevice
    {
        /// <summary>
        /// Constructor for Raw802Device
        /// </summary>
        /// <param name="port">COM port device is on</param>
        /// <param name="baudrate">Baudrate of the com port</param>
        /// <exception cref="System.IO.IOException">Thrown if COM port is in unstable state</exception>
        /// <exception cref="NotImplementedException">Always thrown, as 802.15.4 fumctionality isn't implemented</exception>
        public Raw802Device(string port, int baudrate) : base(port, baudrate)
        {
            throw new NotImplementedException("Raw802Device not implemented");
        }
    }

    /// <summary>
    /// DISCOURAGED. Represents a DigiMesh XBee device. The original python implementation was designed for a weakly typed language, and I've found making use
    /// of this subclass is impractical when it comes to typecasting
    /// </summary>
    public class DigiMeshDevice : XBeeDevice
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="port">COM port to connect to</param>
        /// <param name="baudrate">Baud rate of COM port</param>
        /// <exception cref="System.IO.IOException">Thrown if COM port is in unstable state</exception>
        public DigiMeshDevice(string port, int baudrate, int databits = 8, StopBits stopBits = StopBits.One,
            Parity parity = Parity.None, Handshake flowControl = Handshake.None,
            int syncOpsTimeout = DEFAULT_TIMEOUT_SYNC_OPERATIONS)
            : base(port, baudrate, databits, stopBits, parity, flowControl)
        {
            Protocol = XBeeProtocol.Byte.DIGI_MESH;
            Network = new DigiMeshNetwork(this);
        }

        /// <summary>
        /// Opens the communication channel with the device and loads some information about it
        /// </summary>
        /// <exception cref="XBeeException">Thrown if this device isn't Digimesh (so never thrown). Inherited function from the original weakly-typed python implementation</exception>
        public override void Open()
        {
            base.Open();
            if (Protocol != XBeeProtocol.Byte.DIGI_MESH)
            {
                throw new XBeeException("Invalid protocol");
            }
        }
        
        public new DigiMeshNetwork Network
        {
            get
            {
                if (base.Network == null)
                {
                    base.Network = new DigiMeshNetwork(this);
                }
                return (DigiMeshNetwork)base.Network;
            }
            protected set
            {
                base.Network = value;
            }
        }
    }

    public class DigiPointDevice : XBeeDevice
    {
        public DigiPointDevice(string port, int baudrate) : base(port, baudrate)
        {
            throw new NotImplementedException("DigiPointDevice not implemented");
        }
    }

    public class ZigBeeDevice : XBeeDevice
    {
        public ZigBeeDevice(string port, int baudrate) : base(port, baudrate)
        {
            throw new NotImplementedException("ZigBeeDevice not implemented");
        }
    }

    public class IPDevice : XBeeDevice
    {
        public IPDevice(string port, int baudrate) : base(port, baudrate)
        {
            throw new NotImplementedException("IPDevice not implemented");
        }
    }

    public class CellularDevice : XBeeDevice
    {
        public CellularDevice(string port, int baudrate) : base(port, baudrate)
        {
            throw new NotImplementedException("CellularDevice not implemented");
        }
    }

    public class LPWANDevice : XBeeDevice
    {
        public LPWANDevice(string port, int baudrate) : base(port, baudrate)
        {
            throw new NotImplementedException("LPWANDevice not implemented");
        }
    }

    public class NBIoTDevice : XBeeDevice
    {
        public NBIoTDevice(string port, int baudrate) : base(port, baudrate)
        {
            throw new NotImplementedException("NBIoTDevice not implemented");
        }
    }

    public class WiFiDevice : XBeeDevice
    {
        public WiFiDevice(string port, int baudrate) : base(port, baudrate)
        {
            throw new NotImplementedException("WiFiDevice not implemented");
        }
    }

    /// <summary>
    /// Represents a remote device. Found in the XBeeNetwork of a local XBeeDevice
    /// </summary>
    public class RemoteXBeeDevice : AbstractXBeeDevice
    {
        /// <summary>
        /// Constructor. Instantiates new RemoteXBeeDevice
        /// </summary>
        /// <param name="localDevice">The local device used to communicate with the remote</param>
        /// <param name="x64bitAddr">64 bit address of the remote device</param>
        /// <param name="x16bitAddr">16 bit address of the remote device</param>
        /// <param name="nodeID">The node identifier of the remote device, optional</param>
        public RemoteXBeeDevice(XBeeDevice localDevice, XBee64BitAddress x64bitAddr=null, XBee16BitAddress x16bitAddr=null, string nodeID=null)
            : base(localDevice, localDevice.ComPort)
        {
            if (x64bitAddr == null)
            {
                x64bitAddr = XBee64BitAddress.UNKNOWN_ADDRESS;
            }
            if (x16bitAddr == null)
            {
                x16bitAddr = XBee16BitAddress.UNKNOWN_ADDRESS;
            }
            addr16bit = x16bitAddr;
            Address64bit = x64bitAddr;
            NodeID = nodeID;
        }

        /// <summary>
        /// Determines whether the XBee device is remote or not
        /// </summary>
        /// <returns>Returns true</returns>
        public override bool IsRemote()
        {
            return true;
        }

        /// <summary>
        /// Performs a software reset on this XBee device and blocks until the process is completed
        /// </summary>
        /// <exception cref="InvalidOperatingModeException">Throw if not operating in API mode</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running</exception>
        /// <exception cref="InvalidOperationException">Thrown if the COM port isn't open</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        public override void Reset()
        {
            ATCommandResponse response = null;
            try
            {
                response = SendATCommand(new ATCommand("FR"));
            } catch (TimeoutException te)
            {
                if (localXbeeDevice.Protocol == XBeeProtocol.Byte.RAW_802_15_4)
                {
                    return;
                } else
                {
                    throw te;
                }
            }

            CheckATCmdResponseIsValid(response);
        }

        /// <summary>
        /// Sends data to this remote device synchronously
        /// </summary>
        /// <remarks>
        /// Not in the original Python XBee API
        /// </remarks>
        /// <param name="data">Raw data to send</param>
        /// <param name="transmitOptions">Bitfield of transmit options</param>
        /// <returns>The response/status packet</returns>
        /// <exception cref="ArgumentNullException">Thrown if this device's Address16bit or Address64bit addresses are null</exception>
        /// <exception cref="XBeeException">Serial port is closed, packet listener isn't running, or transmit status is unsuccessful</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        public XBeeAPIPacket SendData(byte[] data,
            TransmitOptions transmitOptions = TransmitOptions.NONE)
        {
            return localXbeeDevice.SendData(this, data, transmitOptions);
        }

        /// <summary>
        /// Reads data from this remote device
        /// </summary>
        /// <remarks>
        /// Not in the original Python XBee API
        /// </remarks>
        /// <param name="timeout">Time to wait, in seconds</param>
        /// <returns>Next XBeeMessage sent by this remote</returns>
        /// <exception cref="XBeeException">Serial port is closed</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="ArgumentException">Thrown if a negative timeout value is passed</exception>
        /// <exception cref="NotImplementedException">Thrown if an RX packet is found in the data or explicit queues. RX packets aren't supported yet</exception>
        /// <exception cref="TimeoutException">Thrown if timeout expires before a element is found in the queues</exception>
        public XBeeMessage ReadData(int timeout = 0)
        {
            return localXbeeDevice.ReadDataFrom(this, timeout);
        }

        /// <summary>
        /// Gets or sets the local XBeeDevice this remote device communicates through
        /// </summary>
        public XBeeDevice LocalXBeeDevice
        {
            get { return localXbeeDevice; }
            set { localXbeeDevice = value; }
        }

        /// <summary>
        /// Returns string that represents current device
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string nodeID = (NodeID == null) ? "" : NodeID;
            return String.Format("{0} - {1}", Address64bit, nodeID);
        }
    }

    public class RemoteRaw802Device : RemoteXBeeDevice
    {
        /// <summary>
        /// Constructor. Instantiates new RemoteRaw802Device
        /// </summary>
        /// <param name="localDevice">The local device used to communicate with the remote</param>
        /// <param name="x64bitAddr">64 bit address of the remote device</param>
        /// <param name="x16bitAddr">16 bit address of the remote device</param>
        /// <param name="nodeID">The node identifier of the remote device, optional</param>
        public RemoteRaw802Device(XBeeDevice localDevice, XBee64BitAddress x64bitAddr = null, XBee16BitAddress x16bitAddr = null, string nodeID = null)
            : base(localDevice, x64bitAddr, x16bitAddr, nodeID)
        {
            throw new NotImplementedException("RemoteRaw802Device is not implemented");
        }
    }

    /// <summary>
    /// Represents a remote DigiMesh XBee device
    /// </summary>
    public class RemoteDigiMeshDevice : RemoteXBeeDevice
    {
        /// <summary>
        /// Constructor. Instantiates new RemoteRaw802Device
        /// </summary>
        /// <param name="localDevice">The local device used to communicate with the remote</param>
        /// <param name="x64bitAddr">64 bit address of the remote device</param>
        /// <param name="x16bitAddr">16 bit address of the remote device</param>
        /// <param name="nodeID">The node identifier of the remote device, optional</param>
        public RemoteDigiMeshDevice(XBeeDevice localDevice, XBee64BitAddress x64bitAddr = null, string nodeID = null)
            : base(localDevice, x64bitAddr, null, nodeID)
        {
            if (localDevice.Protocol != XBeeProtocol.Byte.DIGI_MESH)
            {
                throw new XBeeException("Invalid protocol");
            }

            Protocol = XBeeProtocol.Byte.DIGI_MESH;
        }
    }

    public class RemoteDigiPointDevice : RemoteXBeeDevice
    {
        /// <summary>
        /// Constructor. Instantiates new RemoteRaw802Device
        /// </summary>
        /// <param name="localDevice">The local device used to communicate with the remote</param>
        /// <param name="x64bitAddr">64 bit address of the remote device</param>
        /// <param name="x16bitAddr">16 bit address of the remote device</param>
        /// <param name="nodeID">The node identifier of the remote device, optional</param>
        public RemoteDigiPointDevice(XBeeDevice localDevice, XBee64BitAddress x64bitAddr = null, string nodeID = null)
            : base(localDevice, x64bitAddr, null, nodeID)
        {
            if (localDevice.Protocol != XBeeProtocol.Byte.DIGI_POINT)
            {
                throw new XBeeException("Invalid protocol");
            }

            Protocol = XBeeProtocol.Byte.DIGI_POINT;
        }
    }

    public class RemoteZigBeeDevice : RemoteXBeeDevice
    {
        /// <summary>
        /// Constructor. Instantiates new RemoteRaw802Device
        /// </summary>
        /// <param name="localDevice">The local device used to communicate with the remote</param>
        /// <param name="x64bitAddr">64 bit address of the remote device</param>
        /// <param name="x16bitAddr">16 bit address of the remote device</param>
        /// <param name="nodeID">The node identifier of the remote device, optional</param>
        public RemoteZigBeeDevice(XBeeDevice localDevice, XBee64BitAddress x64bitAddr = null, XBee16BitAddress x16bitAddr = null, string nodeID = null)
            : base(localDevice, x64bitAddr, x16bitAddr, nodeID)
        {

            throw new NotImplementedException("RemoteZigBeeDevice is not implemented");
        }
    }

    /// <summary>
    /// Allows the discovery of remote devices in the same network as the local one and stores them
    /// </summary>
    public class XBeeNetwork : IEnumerable<RemoteXBeeDevice>
    {
        /// <summary>
        /// Flag that indicates a "discovery process finish" packet
        /// </summary>
        public const int ND_PACKET_FINISH = 0x01;

        /// <summary>
        /// Flag that indicates a discovery process packet with info about a remote XBee device. 
        /// </summary>
        public const int ND_PACKET_REMOTE = 0x02;

        /// <summary>
        /// Default timeout for discovering process
        /// </summary>
        protected const int DEFAULT_DISCOVERY_TIMEOUT = 20;

        /// <summary>
        /// Correction values for the digimesh timeout
        /// </summary>
        protected const int DIGI_MESH_TIMEOUT_CORRECTION = 3;
        /// <summary>
        /// Correction values for the digimesh timeout with sleep support
        /// </summary>
        protected const double DIGI_MESH_SLEEP_TIMEOUT_CORRECTION = 0.1;
        /// <summary>
        /// Correction values for the digipoint timeout
        /// </summary>
        protected const int DIGI_POINT_TIMEOUT_CORRECTION = 8;

        /// <summary>
        /// AT command for node discovery
        /// </summary>
        protected const string NODE_DISCOVERY_COMMAND = "ND";

        /// <summary>
        /// FN command for node discovery
        /// </summary>
        protected const string FIND_NEIGHBOR_COMMAND = "FN";

        /// <summary>
        /// The local XBeeDevice this network is being accessed through
        /// </summary>
        protected XBeeDevice XbeeDevice { get; set; }
        /// <summary>
        /// List of remote devices in the network
        /// </summary>
        protected LinkedList<RemoteXBeeDevice> DeviceList { get; set; }
        /// <summary>
        /// Redundant list of remtoe devices in the network
        /// </summary>
        protected LinkedList<RemoteXBeeDevice> LastSearchDevList { get; set; }
        /// <summary>
        /// Lock object so this XBeeNetwork can be handled from multiple threads
        /// </summary>
        protected readonly object genericLock;
        /// <summary>
        /// Boolean to indicate whether the network is actively looking for devices. If false, DeviceDiscovery callbacks should do nothing
        /// </summary>
        public bool Discovering { get; set; }
        /// <summary>
        /// Event handler that's triggered whenever a new device is detected
        /// </summary>
        protected event Reader.DeviceDiscoveredHandler DeviceDiscovered;
        /// <summary>
        /// Event handler that's triggered when device discovery is finished
        /// </summary>
        protected event Reader.DiscoveryProcessFinishedHandler DiscoveryProcessFinished;
        /// <summary>
        /// Separate thread that runs the device discovery process
        /// </summary>
        protected Thread DiscoveryThread { get; set; }
        /// <summary>
        /// Used to store the ID of a device the discovery process is searching for
        /// </summary>
        protected string SoughtDeviceID { get; set; }
        /// <summary>
        /// Reference to a remote matching the SoughtDeviceID
        /// </summary>
        protected RemoteXBeeDevice DiscoveredDevice { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">The local device to interact with</param>
        /// <exception cref="ArgumentNullException">Thrown if local device specified is null</exception>
        public XBeeNetwork(XBeeDevice device)
        {
            XbeeDevice = device ?? throw new ArgumentNullException("Local device cannot be null");
            DeviceList = new LinkedList<RemoteXBeeDevice>();
            LastSearchDevList = new LinkedList<RemoteXBeeDevice>();
            genericLock = new object();
            Discovering = false;
            DiscoveryThread = null;
            SoughtDeviceID = null;
            DiscoveredDevice = null;
        }

        /// <summary>
        /// Programmatic trigger for DeviceDiscovered event
        /// </summary>
        /// <param name="remote">Remote device that was discovered</param>
        public virtual void OnDeviceDiscovered(RemoteXBeeDevice remote)
        {
            DeviceDiscovered?.Invoke(this, remote);
        }
        /// <summary>
        /// Programmatic trigger for DiscoveryProcessFinished event
        /// </summary>
        /// <param name="status">Network discovery status</param>
        public virtual void OnDiscoveryProcessFinished(NetworkDiscoveryStatus.Byte status)
        {
            DiscoveryProcessFinished?.Invoke(this, status);
        }

        /// <summary>
        /// Starts the discovery process. Non-blocking
        /// </summary>
        /// <remarks>
        /// The discovery process will be running until the configured
        /// timeout expires or, in case of 802.15.4, until the 'end' packet
        /// is read.<para/>
        /// It may be that, after the timeout expires, there are devices
        /// that continue sending discovery packets to this XBee device.In this
        /// case, these devices will not be added to the network.
        /// </remarks>
        /// <exception cref="OutOfMemoryException">Thrown if there's not enough memory to start a new thread</exception>
        public void StartDiscoveryProcess()
        {
            lock(genericLock)
            {
                if (Discovering) return;
            }

            DiscoveryThread = new Thread(new ThreadStart(DiscoverDevicesAndNotifyCallbacks));
            Discovering = true;
            DiscoveryThread.Start();
        }

        /// <summary>
        /// Stops the discovery process if it is running
        /// </summary>
        /// <remarks>
        /// Note that DigiMesh/DigiPoint devices are blocked until the discovery
        /// time configured(NT parameter) has elapsed, so if you try to get/set
        /// any parameter during the discovery process you will receive a timeout
        /// exception.
        /// </remarks>
        public void StopDiscoveryProcess()
        {
            if (Discovering)
            {
                lock (genericLock)
                {
                    Discovering = false;
                }
            }
        }

        /// <summary>
        /// Discovers and reports the first remote device that matches the supplied node ID
        /// </summary>
        /// <param name="nodeID">Node ID to search for</param>
        /// <returns>The remote device found</returns>
        /// <exception cref="NotImplementedException">Always thrown, since this method isn't implemented yet</exception>
        public RemoteXBeeDevice DiscoverDevice(string nodeID)
        {
            // NOTE: Not to be confused with hte protected method DiscoverDevices(string) below
            throw new NotImplementedException("DiscoverDevice is not implemented yet");
        }

        /// <summary>
        /// Discovers and reports any remote that matches the list of node IDs
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns>List of matching remotes</returns>
        /// <exception cref="NotImplementedException">Always thrown, since this method isn't implemented yet</exception>
        public List<RemoteXBeeDevice> DiscoverDevices(List<string> nodeID)
        {
            // NOTE: Not to be confused with the protected method DiscoverDevices(string) below
            throw new NotImplementedException("DiscoverDevices is not implemented yet");
        }

        /// <summary>
        /// Subscribes the provided callback to the DiscoveryProcessFinished event
        /// </summary>
        /// <param name="callback">Callback function to run</param>
        public void AddDiscoveryProcessFinishedCallback(Reader.DiscoveryProcessFinishedHandler callback)
        {
            DiscoveryProcessFinished += callback;
        }
        /// <summary>
        /// Unsubscribes the provided callback to the DiscoveryProcessFinished event
        /// </summary>
        /// <param name="callback">Callback function to remove</param>
        public void DelDiscoveryProcessFinishedCallback(Reader.DiscoveryProcessFinishedHandler callback)
        {
            DiscoveryProcessFinished -= callback;
        }
        /// <summary>
        /// Unsubscribes the provided callback to the DeviceDiscovered event
        /// </summary>
        /// <param name="callback">Callback function to run</param>
        public void AddDeviceDiscoveredCallback(Reader.DeviceDiscoveredHandler callback)
        {
            DeviceDiscovered += callback;
        }
        /// <summary>
        /// Unsubscribes the provided callback to the DeviceDiscovered event
        /// </summary>
        /// <param name="callback">Callback function to remove</param>
        public void DelDeviceDiscoveredCallback(Reader.DeviceDiscoveredHandler callback)
        {
            DeviceDiscovered -= callback;
        }

        /// <summary>
        /// Removes all remote devices from the network
        /// </summary>
        public void Clear()
        {
            lock(genericLock)
            {
                DeviceList.Clear();
            }
        }

        /// <summary>
        /// Returns the network discovery process options
        /// </summary>
        /// <returns>The parameter value from sending the NO command. Should be single byte that can convert to DiscoveryOptions.Byte</returns>
        /// <exception cref="OperationNotSupportedException">Thrown if the value returned is null. That means something real weird happened</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running or serial port is closed</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if checking the AT command response failed</exception>
        public byte[] GetDiscoveryOptions()
        {
            return XbeeDevice.GetParameter("NO");
        }

        /// <summary>
        /// Configures the discovery options with the NO command
        /// </summary>
        /// <param name="options">New discovery options</param>
        /// <exception cref="XBeeException">Serial port is closed or packet listener isn't running</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        /// <exception cref="ATCommandException">Thrown if the response is null or does not report success</exception>
        public void SetDiscoveryOptions(DiscoveryOptions.Byte options)
        {
            options = DiscoveryOptions.CalculateDiscoveryValue(XbeeDevice.Protocol, options);
            XbeeDevice.SetParameter("NO", new byte[] { (byte)options });
        }

        /// <summary>
        /// Returns the network discovery timeout
        /// </summary>
        /// <returns>Network discovery timeout in seconds</returns>
        /// <exception cref="NotImplementedException">Always thrown, since this method is not implemented yet</exception>
        public float GetDiscoveryTimeout()
        {
            throw new NotImplementedException("GetDiscoveryTimeout is not implemented yet");
        }

        /// <summary>
        /// Sets the network discovery timeout
        /// </summary>
        /// <param name="timeout">New network discovery timeout, in seonds</param>
        /// <exception cref="NotImplementedException">Always thrown, since this method is not implemented yet</exception>
        public void SetDiscoveryTimeout(float timeout)
        {
            throw new NotImplementedException("SetDiscoveryTimeout is not implemented yet");
        }

        /// <summary>
        /// Returns the remote device already contained in the network whose 64bit address matches the one provided
        /// </summary>
        /// <param name="x64bitAddr">The 64bit address to search for</param>
        /// <returns>The RemoteXBeeDevice whose address matches, or null if none is found</returns>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null</exception>
        public RemoteXBeeDevice GetDeviceBy64(XBee64BitAddress x64bitAddr)
        {
            if (x64bitAddr == null)
            {
                throw new ArgumentNullException("Value of the address cannot be null");
            }

            lock(genericLock)
            {
                try
                {
                    return this.First((RemoteXBeeDevice r) =>
                    {
                        return x64bitAddr.Equals(r.Address64bit);
                    });
                } catch (InvalidOperationException e)
                {
                    // There was no match in the array
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the remote device already contained in the network whose 16bit address matches the one provided
        /// </summary>
        /// <param name="x16bitAddr">The 16bit address to search for</param>
        /// <returns>The RemoteXBeeDevice whose address matches, or null if none is found</returns>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null</exception>
        public RemoteXBeeDevice GetDeviceBy16(XBee16BitAddress x16bitAddr)
        {
            if (x16bitAddr == null)
            {
                throw new ArgumentNullException("Value of the address cannot be None");
            }

            lock (genericLock)
            {
                try
                {
                    return this.First((RemoteXBeeDevice r) =>
                    {
                        return x16bitAddr.Equals(r.Address16bit);
                    });
                }
                catch (InvalidOperationException)
                {
                    // There was no match in the array
                    return null;
                }
        }
        }

        /// <summary>
        /// Returns the remote device already contained in the network whose 16bit address matches the one provided
        /// </summary>
        /// <param name="nodeID">The node ID to search for</param>
        /// <returns>The RemoteXBeeDevice whose node ID matches, or null if none is found</returns>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null</exception>
        public RemoteXBeeDevice GetDeviceByNodeID(string nodeID)
        {
            if (nodeID == null)
            {
                throw new ArgumentNullException("Value of the address cannot be None");
            }

            lock (genericLock)
            {
                try
                {
                    return this.First((RemoteXBeeDevice r) =>
                    {
                        return nodeID.CompareTo(r.NodeID) == 0;
                    });
                }
                catch (InvalidOperationException e)
                {
                    // There was no match in the array
                    return null;
                }
            }
        }
        
        /// <summary>
        /// Adds a device with the provided parameters if it does not exist in the current network, updates it's parameters if it does exist
        /// </summary>
        /// <param name="x64bitAddr">Remote device's 64bit address</param>
        /// <param name="x16bitAddr">Remote device's 16bit address</param>
        /// <param name="nodeID">Node identifier of the device</param>
        /// <returns>Remote device with the updated parameters</returns>
        public RemoteXBeeDevice AddIfNotExist(XBee64BitAddress x64bitAddr=null, XBee16BitAddress x16bitAddr=null, string nodeID="")
        {
            RemoteXBeeDevice remote = new RemoteXBeeDevice(XbeeDevice, x64bitAddr, x16bitAddr, nodeID);
            return AddRemote(remote);
        }

        /// <summary>
        /// Adds the provided remote device to the network if it is not contained yet
        /// </summary>
        /// <param name="remote">Remote device to add</param>
        /// <returns>The provided device with updated parameters</returns>
        public RemoteXBeeDevice AddRemote(RemoteXBeeDevice remote)
        {
            lock(genericLock)
            {
                foreach(RemoteXBeeDevice dev in DeviceList)
                {
                    if (dev.Equals(remote))
                    {
                        dev.UpdateDeviceDataFrom(remote);
                        return dev;
                    }
                }
                DeviceList.AddLast(remote);
                return remote;
            }
        }

        /// <summary>
        /// Adds a list of remote devices to the network
        /// </summary>
        /// <param name="remotes">List of remote devices</param>
        public void AddRemotes(List<RemoteXBeeDevice> remotes)
        {
            foreach(RemoteXBeeDevice r in remotes)
            {
                AddRemote(r);
            }
        }

        /// <summary>
        /// Removes the provided remote device from the network
        /// </summary>
        /// <param name="remote"></param>
        public void RemoveDevice(RemoteXBeeDevice remote)
        {
            DeviceList.Remove(remote);
        }

        /// <summary>
        /// Returns the API callbacks that are used in the device discovery process
        /// </summary>
        /// <returns></returns>
        public List<Reader.PacketReceivedHandler> GetDiscoveryCallbacks()
        {
            // Callback for generic devices discovery process
            Reader.PacketReceivedHandler DiscoveryGenCallback = (object sender, XBeeAPIPacket packet) =>
            {
                // BUG: Type checking for ATCommResponsePacket has been implemented here (so no bug). But in the
                //      original python, if you receive a non-ATcommand packet in discovery mode, it would throw an error
                if (!Discovering || packet.FrameTypeValue != ApiFrameType.Byte.AT_COMMAND_RESPONSE)
                {
                    return;
                }
                
                // Check so see if this packet is a response to the ND command
                int ndID = CheckNDPacket((ATCommResponsePacket)packet);
                if (ndID == ND_PACKET_FINISH)
                {
                    lock (genericLock)
                    {
                        Discovering = ((ATCommResponsePacket)packet).ResponseStatus != ATCommandStatus.Byte.OK;
                    }
                } else if (ndID == ND_PACKET_REMOTE)
                {
                    // If the packet is a non-finishing response to the ND command, it holds info about a remote device
                    RemoteXBeeDevice remote = CreateRemote(((ATCommResponsePacket)packet).CommValue);

                    // If remote was created successfully and it is not in the device list, add it and notify callbacks
                    if (remote != null)
                    {
                        if (!DeviceList.Contains(remote))
                        {
                            lock (genericLock)
                            {
                                DeviceList.AddLast(remote);
                            }
                        }

                        // Always add the device to the last discovered devices list
                        LastSearchDevList.AddLast(remote);
                        OnDeviceDiscovered(remote);
                    }
                }
            };

            // Callback is used for discovering specific device ops
            Reader.PacketReceivedHandler DiscoverySpecCallback = (object sender, XBeeAPIPacket packet) =>
            {
                // If not searching for device, exit
                if (SoughtDeviceID == null)
                {
                    return;
                }

                // Check the packet
                if (packet.FrameTypeValue != ApiFrameType.Byte.AT_COMMAND_RESPONSE) return;
                int ndID = CheckNDPacket((ATCommResponsePacket)packet);
                if (ndID == ND_PACKET_FINISH)
                {
                    // If it's an ND finish signal, stop wait for packets
                    if (((ATCommResponsePacket)packet).ResponseStatus == ATCommandStatus.Byte.OK)
                    {
                        lock (genericLock)
                        {
                            SoughtDeviceID = null;
                        }
                    }
                } else if (ndID == ND_PACKET_REMOTE)
                {
                    // If it's not a finsh signal, it contains info about a remote device
                    RemoteXBeeDevice remote = CreateRemote(((ATCommResponsePacket)packet).CommValue);

                    // If it's the sought device, put it in the proper variable
                    if (SoughtDeviceID.CompareTo(remote.NodeID) == 0)
                    {
                        lock (genericLock)
                        {
                            DiscoveredDevice = remote;
                            SoughtDeviceID = null;
                        }
                    }
                }

            };

            return new List<Reader.PacketReceivedHandler>() {
                DiscoveryGenCallback,
                DiscoverySpecCallback
            };
        }

        /// <summary>
        /// Checks to see if the AT comand response packet is in response to an ND command
        /// </summary>
        /// <param name="packet">The ATCommResponsePacket to check</param>
        /// <returns>ND_PACKET_FINISH if the packet has no parameter value. ND_PACKET_REMOTE if it does</returns>
        protected static int CheckNDPacket(ATCommResponsePacket packet)
        {
            if (packet.Command.CompareTo(NODE_DISCOVERY_COMMAND) == 0)
            {
                if (packet.CommValue == null || packet.CommValue.Length == 0)
                {
                    return ND_PACKET_FINISH;
                } else
                {
                    return ND_PACKET_REMOTE;
                }
            } else
            {
                return -1;
            }
        }

        /// <summary>
        /// Performs a discovery operation, blocks until finished
        /// </summary>
        protected void DiscoverDevicesAndNotifyCallbacks()
        {
            DiscoverDevices();
            OnDiscoveryProcessFinished(NetworkDiscoveryStatus.Byte.SUCCESS);    //TODO: After discovery, this complains of being null
        }

        /// <summary>
        /// Discovers and reports the first remote device that matches the supplied identifier. Blocking
        /// </summary>
        /// <param name="nodeID">Node ID to search for (null to search for all)</param>
        protected void DiscoverDevices(string nodeID=null)
        {
            try
            {
                double initTime = Utils.TimeNow();

                // Not neccessary to calculate timeout for 802 devices
                bool is802Compatible = Is802Compatible();
                double timeout = 0;
                if (!is802Compatible)
                {
                    timeout = CalculateTimeout();
                }

                // Send "ND" async
                XbeeDevice.SendPacket(new ATCommPacket(XbeeDevice.GetNextFrameID(), "ND", (nodeID == null) ? null : Utils.AsciiToBytes(nodeID)), false);

                if (!is802Compatible)
                {
                    // If device is not 802.15.4, wait until timeout expires
                    while (Discovering || SoughtDeviceID != null)
                    {
                        if ((Utils.TimeNow() - initTime) > timeout) {
                            lock(genericLock)
                            {
                                Discovering = false;
                            }
                            break;
                        }
                        Thread.Sleep(100);
                    }
                } else
                {
                    // If device 802.15.4, wait until the 'end' message to arrive
                    while (Discovering || SoughtDeviceID != null)
                    {
                        Thread.Sleep(100);
                    }
                }
            } catch (Exception e)
            {
                XbeeDevice.Log.Error(e);
            } finally
            {
                lock(genericLock)
                {
                    Discovering = false;
                }
            }
        }

        /// <summary>
        /// Checks if the device performing the node discovery is a legacy 802.15.4 device or a S1B device working in compatibility mode
        /// </summary>
        /// <returns>True if the local device is a legacy 802.15.4 device or S1B in compatibility mode. False otherwise</returns>
        /// <exception cref="OperationNotSupportedException">Thrown if the value returned is null. That means something real weird happened</exception>
        /// <exception cref="InvalidOperatingModeException">Operating mode of local device is not supported</exception>
        /// <exception cref="XBeeException">Thrown if packet listener isn't running or serial port is closed</exception>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        protected bool Is802Compatible()
        {
            if (XbeeDevice.Protocol != XBeeProtocol.Byte.RAW_802_15_4)
            {
                return false;
            }
            byte[] param = null;
            try
            {
                param = XbeeDevice.GetParameter("C8");
            } catch (ATCommandException) { }
            if (param == null || (param[0] & 0x2) == 2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines the discovery timeout
        /// </summary>
        /// <remarks>
        /// Gets timeout information from the device and applies the proper
        /// corrections to it.<para/>
        /// If the timeout cannot be determined getting it from the device, this
        /// method returns the default timeout for discovery operations.
        /// </remarks>
        /// <returns>Discovery timeout in seconds</returns>
        /// <exception cref="TimeoutException">Thrown if reply packet is not found in time, or (more rarely) if the COM port times out when writing</exception>
        protected double CalculateTimeout()
        {
            double discoveryTimeout = -1;

            // Read the max discovery timeout (N?)
            try
            {
                discoveryTimeout = Utils.BytesToInt(XbeeDevice.GetParameter("N?")) / 1000;
            } catch (XBeeException) { }

            // If N? does not exist, read the NT parameter
            if (discoveryTimeout == -1)
            {
                // Read the device timeout (NT)
                try
                {
                    discoveryTimeout = Utils.BytesToInt(XbeeDevice.GetParameter("NT")) / 10;
                } catch (XBeeException xe) {
                    discoveryTimeout = DEFAULT_DISCOVERY_TIMEOUT;
                    XbeeDevice.Log.Error(xe);
                    OnDiscoveryProcessFinished(NetworkDiscoveryStatus.Byte.ERROR_READ_TIMEOUT);
                }

                // In digimesh/digipoint, the network discovery timeout is NT + network propagation time
                if (XbeeDevice.Protocol == XBeeProtocol.Byte.DIGI_MESH)
                {
                    discoveryTimeout += DIGI_MESH_TIMEOUT_CORRECTION;
                } else if (XbeeDevice.Protocol == XBeeProtocol.Byte.DIGI_POINT)
                {
                    discoveryTimeout += DIGI_POINT_TIMEOUT_CORRECTION;
                }
            }

            if (XbeeDevice.Protocol == XBeeProtocol.Byte.DIGI_MESH)
            {
                // If the module has sleep support, wait another discovery cycle
                try
                {
                    if (Utils.BytesToInt(XbeeDevice.GetParameter("SM")) == 7)
                    {
                        discoveryTimeout += discoveryTimeout + (discoveryTimeout * DIGI_MESH_SLEEP_TIMEOUT_CORRECTION);
                    }
                } catch (XBeeException xe)
                {
                    XbeeDevice.Log.Error(xe);
                }
            }

            return discoveryTimeout;
        }

        /// <summary>
        /// Creates and returns a RemoteXBeeDevice from the provided data
        /// </summary>
        /// <param name="discoveryData"></param>
        /// <returns>Newly created remote</returns>
        protected RemoteXBeeDevice CreateRemote(byte[] discoveryData)
        {
            if (discoveryData == null)
            {
                return null;
            }

            XBeeProtocol.Byte p = XbeeDevice.Protocol;
            Tuple<XBee16BitAddress, XBee64BitAddress, string> devData = GetDataForRemote(discoveryData);

            if (p == XBeeProtocol.Byte.ZIGBEE)
            {
                return new RemoteZigBeeDevice(XbeeDevice, devData.Item2, devData.Item1, devData.Item3);
            } else if (p == XBeeProtocol.Byte.DIGI_MESH)
            {
                return new RemoteDigiMeshDevice(XbeeDevice, devData.Item2, devData.Item3);
            } else if (p == XBeeProtocol.Byte.DIGI_POINT)
            {
                return new RemoteDigiPointDevice(XbeeDevice, devData.Item2, devData.Item3);
            } else if (p == XBeeProtocol.Byte.RAW_802_15_4)
            {
                return new RemoteRaw802Device(XbeeDevice, devData.Item2, devData.Item1, devData.Item3);
            } else
            {
                return new RemoteXBeeDevice(XbeeDevice, devData.Item2, devData.Item1, devData.Item3);
            }
        }

        /// <summary>
        /// Extracts the 16 bit address, 64 bit address, and ndoe identifier from the raw byte array
        /// </summary>
        /// <remarks>
        /// This functionality is somewhat redundant to Reader.PacketListener.GetRemoteDeviceDataFromPacket, although that takes a packet instead of a raw byte array
        /// </remarks>
        /// <param name="data">A portion of the payload extracted from an AT Command Response packet</param>
        /// <returns>Tuple of 16 bit address, 64 bit address, and the node identifier</returns>
        protected Tuple<XBee16BitAddress, XBee64BitAddress, string> GetDataForRemote(byte[] data)
        {
            int i;
            if (XbeeDevice.Protocol == XBeeProtocol.Byte.RAW_802_15_4)
            {
                i = 11;
            } else
            {
                i = 10;
            }
            while (data[i] != 0x00) i++;
            string nodeID = Utils.BytesToAscii(data.Skip(10).Take(i - 10).ToArray());
            return new Tuple<XBee16BitAddress, XBee64BitAddress, string>(new XBee16BitAddress(data.Take(2).ToArray()),
                new XBee64BitAddress(data.Skip(2).Take(8).ToArray()), nodeID);
        }

        /// <summary>
        /// Enumerator for the network. Iterates over remote devices in the networks
        /// </summary>
        /// <returns>Enumerator of remote devices in the network</returns>
        public IEnumerator<RemoteXBeeDevice> GetEnumerator()
        {
            return DeviceList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return DeviceList.GetEnumerator();
        }
    }

    public class ZigBeeNetwork : XBeeNetwork
    {
        public ZigBeeNetwork(XBeeDevice device) : base(device) { }
    }

    public class Raw802Network : XBeeNetwork
    {
        public Raw802Network(XBeeDevice device) : base(device) { }
    }

    public class DigiMeshNetwork : XBeeNetwork
    {
        public DigiMeshNetwork(XBeeDevice device) : base(device) { }
    }

    public class DigiPointNetwork : XBeeNetwork
    {
        public DigiPointNetwork(XBeeDevice device) : base(device) { }
    }
}

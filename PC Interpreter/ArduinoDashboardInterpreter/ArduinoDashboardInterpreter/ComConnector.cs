using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter
{
    public class ComConnector
    {
        SerialPort port;

        public bool Connect(string portName)
        {
            try
            {
                Disconnect();
                port = new SerialPort(portName, 9600, Parity.Odd, 7, StopBits.One);
                port.Open();
                return port.IsOpen;
            }catch { return false; }
        }

        public bool Disconnect()
        {
            if (IsConnected())
            {
                try
                {
                    port.Close();
                    return !port.IsOpen;
                } catch { return false; }
            }
            return true;
        }

        public bool IsConnected()
        {
            return port != null && port.IsOpen;
        }

        public static string[] GetPortList()
        {
            return SerialPort.GetPortNames();
        }

        public SerialPort GetSerialPortObject()
        {
            return port;
        }
    }
}

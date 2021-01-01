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
        const string VALUE_SEPARATOR = "@";
        const string LED_UPDATE_COMMAND = "LED";
        const string BACKLIGHT_UPDATE_COMMAND = "BKL";
        const string GAUGE_UPDATE_COMMAND = "GAU";
        const string REG_A_UPDATE_COMMAND = "REA";
        const string REG_B_UPDATE_COMMAND = "REB";
        const string REG_C_UPDATE_COMMAND = "REC";
        const string LCD_CLEAR_COMMAND = "CLS";
        const string LCD_PRINT_COMMAND = "PRT";
        const string LCD_UPDATE_COMMAND = "UPD";
        const string BEEP_COMMAND = "BEP";

        SerialPort port;

        public delegate void DataSendedDelegate(string content);
        public event DataSendedDelegate DataSended;

        #region "PORT"

        public bool Connect(string portName)
        {
            try
            {
                Disconnect();
                port = new SerialPort(portName, 9600);
                port.Open();
                return port.IsOpen;
            }
            catch { return false; }
        }

        public bool Disconnect()
        {
            if (IsConnected())
            {
                try
                {
                    port.Close();
                    return !port.IsOpen;
                }
                catch { return false; }
            }
            return true;
        }

        public bool IsConnected() => port != null && port.IsOpen;

        public static string[] GetPortList() => SerialPort.GetPortNames();

        public SerialPort GetSerialPortObject() => port;
        #endregion

        #region "CONNECTION"

        private bool SendData(string content)
        {
            if(IsConnected())
            {
                port.Write(content + "\n");
                DataSended?.Invoke(content);
                return true;
            }
            return false;
        }

        public bool SendLedUpdate(ArduinoController.LedState[] ledConfig)
        {
            string content = LED_UPDATE_COMMAND + VALUE_SEPARATOR;
            foreach (ArduinoController.LedState led in ledConfig) content += (int)led;
            return SendData(content);
        }

        public bool SendBacklightUpdate(ArduinoController.LedState[] backlightConfig)
        {
            string content = BACKLIGHT_UPDATE_COMMAND + VALUE_SEPARATOR;
            foreach (ArduinoController.LedState bl in backlightConfig) content += (int)bl;
            return SendData(content);
        }

        public bool SendGaugeUpdate(Double[] gaugeConfig)
        {
            string content = GAUGE_UPDATE_COMMAND + VALUE_SEPARATOR;
            foreach (Double gauge in gaugeConfig) content += (int)gauge + VALUE_SEPARATOR;
            content = content.Substring(0, content.Length - 1);
            return SendData(content);
        }

        public bool SendRegistryUpdate(ArduinoController.RegistryType type, string[] values)
        {
            string content = "";
            switch(type)
            {
                case ArduinoController.RegistryType.RegistryA: content = REG_A_UPDATE_COMMAND; break;
                case ArduinoController.RegistryType.RegistryB: content = REG_B_UPDATE_COMMAND; break;
                case ArduinoController.RegistryType.RegistryC: content = REG_C_UPDATE_COMMAND; break;
            }
            content += VALUE_SEPARATOR;
            foreach (string value in values) content += value + VALUE_SEPARATOR;
            content = content.Substring(0, content.Length - 1);
            return SendData(content);
        }

        public bool SendClearLcdCommand() => SendData(LCD_CLEAR_COMMAND);

        public bool SendPrintLcdCommand(ScreenController.ScreenType screen) => SendData(LCD_PRINT_COMMAND + VALUE_SEPARATOR + (int)screen);

        public bool SendUpdateLcdCommand() => SendData(LCD_UPDATE_COMMAND);

        public bool SendBeepCommand() => SendData(BEEP_COMMAND);
        #endregion
    }
}

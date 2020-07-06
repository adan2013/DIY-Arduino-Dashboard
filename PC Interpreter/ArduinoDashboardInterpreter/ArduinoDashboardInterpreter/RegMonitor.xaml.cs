using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArduinoDashboardInterpreter
{
    /// <summary>
    /// Interaction logic for RegMonitor.xaml
    /// </summary>
    public partial class RegMonitor : Window
    {
        ArduinoController arduino;

        public RegMonitor(ArduinoController arduino)
        {
            InitializeComponent();
            this.arduino = arduino;
            this.arduino.LcdDataChanged += Arduino_LcdDataChanged;
            Arduino_LcdDataChanged(0, new string[5], new string[5], new string[5]);
        }

        private void Arduino_LcdDataChanged(ScreenController.ScreenType id, string[] regA, string[] regB, string[] regC)
        {
            MonitorConsole.Text = "SCREEN ID: " + (int)id + "\n" + PrintRegistry("A", regA) + "\n" + PrintRegistry("B", regB) + "\n" + PrintRegistry("C", regC);
        }

        private string PrintRegistry(string letter, string[] values)
        {
            string result = "";
            for(int i = 0; i < values.Length; i++)
            {
                result += "\n" + letter + i + ": " + values[i];
            }
            return result;
        }
    }
}

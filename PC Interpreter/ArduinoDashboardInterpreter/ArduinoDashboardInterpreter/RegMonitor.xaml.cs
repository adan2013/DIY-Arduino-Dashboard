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
        public RegMonitor()
        {
            InitializeComponent();
            MonitorConsole.Text = "A1:\nA2:\nA3:\nA4:\nA5:\n\nB1:\nB2:\nB3:\nB4:\nB5:\n\nC1:\nC2:\nC3:\nC4:\nC5:";
        }
    }
}

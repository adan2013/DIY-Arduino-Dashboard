using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Forms;

namespace ArduinoDashboardInterpreter
{
    /// <summary>
    /// Interaction logic for WaitForKey.xaml
    /// </summary>
    public partial class WaitForKey : Window
    {
        public Keys DetectedKey;

        public WaitForKey()
        {
            InitializeComponent();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                DialogResult = false;
            }
            else
            {
                DetectedKey = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
                DialogResult = true;
            }
        }
    }
}

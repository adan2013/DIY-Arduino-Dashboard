﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
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
    /// Interaction logic for ComMonitor.xaml
    /// </summary>
    public partial class ComMonitor : Window
    {
        ComConnector connector;

        public ComMonitor(ComConnector connector)
        {
            InitializeComponent();
            this.connector = connector;
            this.connector.GetSerialPortObject().DataReceived += ComMonitor_DataReceived;
        }

        private void ComMonitor_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort port = (SerialPort)sender;
            string incoming = port.ReadExisting();
            MonitorConsole.Text += incoming + "\n";
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            MonitorConsole.Text = "";
        }

        private void MonitorConsole_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((bool)AutoscrollChkBox.IsChecked) MonitorConsole.ScrollToEnd();
        }
    }
}
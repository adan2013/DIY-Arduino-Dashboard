using ArduinoDashboardInterpreter.ProgramLoops;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArduinoDashboardInterpreter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        ComConnector serial;
        ArduinoController arduino;
        Settings settings;

        ComMonitor ComMonitorWindow;
        RegMonitor RegMonitorWindow;

        ProgramLoop program;

        public enum ProgramType
        {
            Home,
            Manual,
            Test,
            Telemetry
        }

        public MainWindow()
        {
            InitializeComponent();

            serial = new ComConnector();
            RefreshPortList();
            arduino = new ArduinoController();
            arduino.LedStateChanged += LedButtonColor;
            arduino.BacklightStateChanged += BacklightButtonColor;
            arduino.GaugePositionChanged += GaugeSliderUpdate;
            LoadSettingsFromFile();
            SetNewProgram(ProgramType.Manual);
        }

        private void LoadSettingsFromFile()
        {
            string settingsPath = System.AppDomain.CurrentDomain.BaseDirectory + "settings.adi";
            if (System.IO.File.Exists(settingsPath))
            {
                settings = Serialization.DeserializeObject(settingsPath);
                if (settings is null) settings = new Settings();
                settings.StartListening();
            }
            else
            {
                settings = new Settings();
                settings.StartListening();
            }
            settings.ShortcutsUpdated += UpdateShortcutButtons;
            settings.KeyPressed += ShortcutPressed;
            settings.ShortcutsUpdated += SaveSettingsToFile;
            settings.OptionsUpdated += SaveSettingsToFile;
            settings.OptionsUpdated += UpdateOptionButtons;
            UpdateOptionButtons();
            UpdateShortcutButtons();
        }

        private void SaveSettingsToFile()
        {
            string settingsPath = System.AppDomain.CurrentDomain.BaseDirectory + "settings.adi";
            Serialization.SerializeObject(ref settings, settingsPath);
        }
        
        #region "LED"

        private void LedButtonColor(ArduinoController.LedType id, ArduinoController.LedState state)
        {
            Brush color = state == ArduinoController.LedState.Off ? Brushes.White : ArduinoController.GetLedColor(id);
            switch ((int)id)
            {
                case 0: Led0.Background = color; break;
                case 1: Led1.Background = color; break;
                case 2: Led2.Background = color; break;
                case 3: Led3.Background = color; break;
                case 4: Led4.Background = color; break;
                case 5: Led5.Background = color; break;
                case 6: Led6.Background = color; break;
                case 7: Led7.Background = color; break;
                case 8: Led8.Background = color; break;
                case 9: Led9.Background = color; break;
                case 10: Led10.Background = color; break;
                case 11: Led11.Background = color; break;
                case 12: Led12.Background = color; break;
                case 13: Led13.Background = color; break;
                case 14: Led14.Background = color; break;
                case 15: Led15.Background = color; break;
            }
        }

        private void LedButton_Click(object sender, RoutedEventArgs e)
        {
            ArduinoController.LedType id = (ArduinoController.LedType)int.Parse(((Button)sender).Name.Substring(3));
            ArduinoController.LedState state = arduino.GetLedState(id);
            if (state == ArduinoController.LedState.Off)
            {
                state = ArduinoController.LedState.On;
            }
            else
            {
                state = ArduinoController.LedState.Off;
            }
            arduino.SetLedState(id, state);
        }
        #endregion

        #region "BACKLIGHT"

        private void BacklightButtonColor(ArduinoController.BacklightType id, ArduinoController.LedState state)
        {
            Brush color = state == ArduinoController.LedState.Off ? Brushes.White : Brushes.LightBlue;
            switch (id)
            {
                case ArduinoController.BacklightType.WhiteBig: BacklightWB.Background = color; break;
                case ArduinoController.BacklightType.WhiteSmall: BacklightWS.Background = color; break;
                case ArduinoController.BacklightType.RedBig: BacklightRB.Background = color; break;
                case ArduinoController.BacklightType.RedSmall: BacklightRS.Background = color; break;
                case ArduinoController.BacklightType.LcdLed: LcdLed.Background = color; break;
            }
        }

        private void BacklightButton_Click(object sender, RoutedEventArgs e)
        {
            ArduinoController.BacklightType id = 0;
            switch (((Button)sender).Name)
            {
                case "BacklightWB": id = ArduinoController.BacklightType.WhiteBig; break;
                case "BacklightWS": id = ArduinoController.BacklightType.WhiteSmall; break;
                case "BacklightRB": id = ArduinoController.BacklightType.RedBig; break;
                case "BacklightRS": id = ArduinoController.BacklightType.RedSmall; break;
                case "LcdLed": id = ArduinoController.BacklightType.LcdLed; break;
            }
            ArduinoController.LedState state = arduino.GetBacklightState(id);
            if (state == ArduinoController.LedState.Off)
            {
                state = ArduinoController.LedState.On;
            }
            else
            {
                state = ArduinoController.LedState.Off;
            }
            arduino.SetBacklightState(id, state);
        }
        #endregion

        #region "GAUGE"

        bool ignoreSliderUpdate = false;

        private void Gauge_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!ignoreSliderUpdate)
            {
                ArduinoController.GaugeType id = 0;
                switch (((Slider)sender).Name.Substring(5))
                {
                    case "A": id = ArduinoController.GaugeType.Speed; break;
                    case "B": id = ArduinoController.GaugeType.Fuel; break;
                    case "C": id = ArduinoController.GaugeType.Air; break;
                    case "D": id = ArduinoController.GaugeType.Engine; break;
                }
                arduino.SetGaugePosition(id, e.NewValue);
            }
        }

        private void GaugeSliderUpdate(ArduinoController.GaugeType id, double value)
        {
            ignoreSliderUpdate = true;
            switch (id)
            {
                case ArduinoController.GaugeType.Speed: GaugeA.Value = value; break;
                case ArduinoController.GaugeType.Fuel: GaugeB.Value = value; break;
                case ArduinoController.GaugeType.Air: GaugeC.Value = value; break;
                case ArduinoController.GaugeType.Engine: GaugeD.Value = value; break;
            }
            ignoreSliderUpdate = false;
        }

        private void GaugeHome_Click(object sender, RoutedEventArgs e)
        {
            ignoreSliderUpdate = true;
            arduino.SetGaugePosition(ArduinoController.GaugeType.Speed, 0);
            arduino.SetGaugePosition(ArduinoController.GaugeType.Fuel, 0);
            arduino.SetGaugePosition(ArduinoController.GaugeType.Air, 0);
            arduino.SetGaugePosition(ArduinoController.GaugeType.Engine, 0);
            ignoreSliderUpdate = false;
        }

        private void GaugeMax_Click(object sender, RoutedEventArgs e)
        {
            ignoreSliderUpdate = true;
            arduino.SetGaugePosition(ArduinoController.GaugeType.Speed, 100);
            arduino.SetGaugePosition(ArduinoController.GaugeType.Fuel, 100);
            arduino.SetGaugePosition(ArduinoController.GaugeType.Air, 100);
            arduino.SetGaugePosition(ArduinoController.GaugeType.Engine, 100);
            ignoreSliderUpdate = false;
        }
        #endregion

        #region "PROGRAM"

        public void SetNewProgram(ProgramType newProgram)
        {
            switch(newProgram)
            {
                case ProgramType.Home: program = new HomeLoop(); break;
                case ProgramType.Manual: program = new ManualLoop(); break;
                case ProgramType.Test: program = new TestLoop(); break;
                case ProgramType.Telemetry: program = new TelemetryLoop(); break;
            }
            Brush offColor = Brushes.White;
            Brush onColor = Brushes.LightBlue;
            ProgHome.Background = offColor;
            ProgManual.Background = offColor;
            ProgTest.Background = offColor;
            ProgTelemetry.Background = offColor;
            if (program is HomeLoop) ProgHome.Background = onColor;
            if (program is ManualLoop) ProgManual.Background = onColor;
            if (program is TestLoop) ProgTest.Background = onColor;
            if (program is TelemetryLoop) ProgTelemetry.Background = onColor;
        }

        private void ProgramButton_Click(object sender, RoutedEventArgs e)
        {
            switch(((Button)sender).Name)
            {
                case "ProgHome": SetNewProgram(ProgramType.Home); break;
                case "ProgManual": SetNewProgram(ProgramType.Manual); break;
                case "ProgTest": SetNewProgram(ProgramType.Test); break;
                case "ProgTelemetry": SetNewProgram(ProgramType.Telemetry); break;
            }
        }
        #endregion

        #region "OPTIONS"
        
        private void UpdateOptionButtons()
        {
            Brush offColor = Brushes.White;
            Brush onColor = Brushes.LightBlue;
            Option0.Background = settings.GetOptionValue((Settings.OptionType)0) ? onColor : offColor;
            Option1.Background = settings.GetOptionValue((Settings.OptionType)1) ? onColor : offColor;
            Option2.Background = settings.GetOptionValue((Settings.OptionType)2) ? onColor : offColor;
            Option3.Background = settings.GetOptionValue((Settings.OptionType)3) ? onColor : offColor;
            Option4.Background = settings.GetOptionValue((Settings.OptionType)4) ? onColor : offColor;
            Option5.Background = settings.GetOptionValue((Settings.OptionType)5) ? onColor : offColor;
            Option6.Background = settings.GetOptionValue((Settings.OptionType)6) ? onColor : offColor;
            Option7.Background = settings.GetOptionValue((Settings.OptionType)7) ? onColor : offColor;
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            int optionId = int.Parse(((Button)sender).Name.Substring(6));
            settings.ToggleOption((Settings.OptionType)optionId);
        }
        #endregion

        #region "SHORTCUTS"

        private void UpdateShortcutButtons()
        {
            KeybDiffLock.Content = "DIFF LOCK (" + settings.GetShortcutString(KeyboardShortcut.TargetType.DiffLock) + ")";
            KeybLeft.Content = "LEFT (" + settings.GetShortcutString(KeyboardShortcut.TargetType.Left) + ")";
            KeybOk.Content = "OK (" + settings.GetShortcutString(KeyboardShortcut.TargetType.Ok) + ")";
            KeybRight.Content = "RIGHT (" + settings.GetShortcutString(KeyboardShortcut.TargetType.Right) + ")";
        }

        private void ShortcutPressed(KeyboardShortcut shortcut)
        {
            switch(shortcut.Target)
            {
                case KeyboardShortcut.TargetType.DiffLock:
                    new System.Threading.Thread(() => Dispatcher.Invoke(new Action(() => settings.ToggleOption(Settings.OptionType.DiffLock)))).Start();
                    break;
                case KeyboardShortcut.TargetType.Left:
                    arduino.Screen.LeftButton();
                    break;
                case KeyboardShortcut.TargetType.Ok:
                    arduino.Screen.OkButton();
                    break;
                case KeyboardShortcut.TargetType.Right:
                    arduino.Screen.RightButton();
                    break;
            }
        }

        private void KeyboardShortcut_Click(object sender, RoutedEventArgs e)
        {
            KeyboardShortcut.TargetType target = 0;
            switch(((Button)sender).Name)
            {
                case "KeybDiffLock": target = KeyboardShortcut.TargetType.DiffLock; break;
                case "KeybLeft": target = KeyboardShortcut.TargetType.Left; break;
                case "KeybOk": target = KeyboardShortcut.TargetType.Ok; break;
                case "KeybRight": target = KeyboardShortcut.TargetType.Right; break;
            }
            WaitForKey modalWindow = new WaitForKey();
            modalWindow.Owner = this;
            if((bool)modalWindow.ShowDialog())
            {
                settings.AddShortcut(new KeyboardShortcut(target, modalWindow.DetectedKey, 0));
            }
            else
            {
                if(settings.GetShortcutString(target) != "none")
                {
                    if (MessageBox.Show("Delete this shortcut?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        settings.DeleteShortcut(target);
                    }
                }
            }

        }
        #endregion

        #region "CONNECTION"

        private void ConnScan_Click(object sender, RoutedEventArgs e)
        {
            RefreshPortList();
        }

        private void ConnConnect_Click(object sender, RoutedEventArgs e)
        {
            if(ConnPortList.SelectedItem != null)
            {
                string port = ConnPortList.SelectedItem.ToString();
                if(serial.Connect(port))
                {
                    ComMonitorWindow?.Close();
                }
                else
                {
                    MessageBox.Show("Connection error!");
                }
            }
            else
            {
                MessageBox.Show("Select serial port name first!");
            }
        }

        private void ConnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (!serial.Disconnect()) MessageBox.Show("Disconnection error!");
        }

        private void ConnComMonitor_Click(object sender, RoutedEventArgs e)
        {
            if(serial != null && serial.IsConnected())
            {
                ComMonitorWindow?.Close();
                ComMonitorWindow = new ComMonitor(serial);
                ComMonitorWindow.Show();
            }
            else
            {
                MessageBox.Show("Connection closed!");
            }
        }

        private void ConnRegMonitor_Click(object sender, RoutedEventArgs e)
        {
            RegMonitorWindow?.Close();
            RegMonitorWindow = new RegMonitor();
            RegMonitorWindow.Show();
        }

        private void RefreshPortList()
        {
            ConnPortList.Items.Clear();
            string[] list = ComConnector.GetPortList();
            foreach (string item in list) ConnPortList.Items.Add(item);
            if (ConnPortList.Items.Count == 1) ConnPortList.SelectedIndex = 0;
        }
        #endregion

        #region "SCREEN"

        private void LcdNavLeft_Click(object sender, RoutedEventArgs e)
        {
            arduino.Screen.LeftButton();
        }

        private void LcdNavOk_Click(object sender, RoutedEventArgs e)
        {
            arduino.Screen.OkButton();
        }

        private void LcdNavRight_Click(object sender, RoutedEventArgs e)
        {
            arduino.Screen.RightButton();
        }
        #endregion
    }
}

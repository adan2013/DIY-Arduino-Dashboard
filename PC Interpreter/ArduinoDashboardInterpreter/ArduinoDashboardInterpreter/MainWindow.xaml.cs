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
        ArduinoController arduino;
        Settings settings;

        public MainWindow()
        {
            InitializeComponent();

            arduino = new ArduinoController();
            arduino.LedStateChanged += LedButtonColor;
            arduino.BacklightStateChanged += BacklightButtonColor;
            arduino.GaugePositionChanged += GaugeSliderUpdate;
            arduino.ProgramChanged += ProgramButtonColor;

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
                Serialization.SerializeObject(ref settings, settingsPath);
            }
            settings.ShortcutsUpdated += UpdateShortcutButtons;
            settings.ShortcutsUpdated += SaveSettingsToFile;
            settings.KeyPressed += ShortcutPressed;
            //settings.AddShortcut(new KeyboardShortcut(KeyboardShortcut.TargetType.Left, System.Windows.Forms.Keys.F, KeyModifiers.Shift));
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

        private void ProgramButtonColor(ArduinoController.ProgramType newProgram)
        {
            Brush offColor = Brushes.White;
            Brush onColor = Brushes.LightBlue;
            ProgHome.Background = offColor;
            ProgManual.Background = offColor;
            ProgTest.Background = offColor;
            ProgTelemetry.Background = offColor;
            switch (newProgram)
            {
                case ArduinoController.ProgramType.Home: ProgHome.Background = onColor; break;
                case ArduinoController.ProgramType.Manual: ProgManual.Background = onColor; break;
                case ArduinoController.ProgramType.Test: ProgTest.Background = onColor; break;
                case ArduinoController.ProgramType.Telemetry: ProgTelemetry.Background = onColor; break;
            }
        }

        private void ProgramButton_Click(object sender, RoutedEventArgs e)
        {
            switch(((Button)sender).Name)
            {
                case "ProgHome": arduino.SetNewProgram(ArduinoController.ProgramType.Home); break;
                case "ProgManual": arduino.SetNewProgram(ArduinoController.ProgramType.Manual); break;
                case "ProgTest": arduino.SetNewProgram(ArduinoController.ProgramType.Test); break;
                case "ProgTelemetry": arduino.SetNewProgram(ArduinoController.ProgramType.Telemetry); break;
            }
        }
        #endregion

        #region "OPTIONS"
        
        private void OptionDiffLockSwitch_Click(object sender, RoutedEventArgs e)
        {
            arduino.OptionDiffLock = !arduino.OptionDiffLock;
            ((Button)sender).Background = arduino.OptionDiffLock ? Brushes.LightBlue : Brushes.White;
        }

        private void OptionSound_Click(object sender, RoutedEventArgs e)
        {
            arduino.OptionSound = !arduino.OptionSound;
            ((Button)sender).Background = arduino.OptionSound ? Brushes.LightBlue : Brushes.White;
        }

        private void OptionKeyboard_Click(object sender, RoutedEventArgs e)
        {
            arduino.OptionKeyboard = !arduino.OptionKeyboard;
            ((Button)sender).Background = arduino.OptionKeyboard ? Brushes.LightBlue : Brushes.White;
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
            MessageBox.Show(shortcut.ShortcutKey.ToString() + " " + shortcut.ShortcutModifiers.ToString());
        }
        #endregion
    }
}

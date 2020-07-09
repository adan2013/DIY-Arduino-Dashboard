using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ArduinoDashboardInterpreter
{
    public class ArduinoController
    {
        LedState[] LedController = new LedState[16];
        public delegate void LedStateChangedDelegate();
        public event LedStateChangedDelegate LedStateChanged;

        LedState[] BacklightController = new LedState[5];
        public delegate void BacklightStateChangedDelegate();
        public event BacklightStateChangedDelegate BacklightStateChanged;

        Double[] GaugePositions = new double[4];
        public delegate void GaugePositionChangedDelegate();
        public event GaugePositionChangedDelegate GaugePositionChanged;

        public ScreenController Screen;
        string[] RegistryA;
        string[] RegistryB;
        string[] RegistryC;
        public delegate void LcdDataChangedDelegate(ScreenController.ScreenType id, string[] regA, string[] regB, string[] regC);
        public event LcdDataChangedDelegate LcdDataChanged;

        public bool LedModified = false;
        public bool BacklightModified = false;
        public bool GaugeModified = false;
        public bool RegistryAModified = false;
        public bool RegistryBModified = false;
        public bool RegistryCModified = false;
        public bool ScreenIdModified = false;

        public ArduinoController()
        {
            SetDefaultLcdState();
            Screen = new ScreenController(this);
            Screen.ScreenIdChanged += Screen_ScreenIdChanged;
        }

        public void MarkChangesAsUpdated()
        {
            LedModified = false;
            BacklightModified = false;
            GaugeModified = false;
            RegistryAModified = false;
            RegistryBModified = false;
            RegistryCModified = false;
            ScreenIdModified = false;
        }

        #region "LED"

        public enum LedType
        {
            CheckEngine,
            GlowPlug,
            LowBeam,
            HighBeam,
            Fuel,
            LiftAxle,
            DiffLock,
            LeftIndicator,
            RightIndicator,
            CruiseControl,
            Rest,
            WarningBrake,
            ParkingBrake,
            Battery,
            Oil,
            Retarder
        }

        public enum LedState
        {
            Off,
            On,
            Blink
        }

        public static Brush GetLedColor(LedType id)
        {
            switch(id)
            {
                case LedType.LowBeam:
                case LedType.LeftIndicator:
                case LedType.RightIndicator:
                case LedType.CruiseControl:
                case LedType.Retarder:
                    return Brushes.LightGreen;
                case LedType.CheckEngine:
                case LedType.GlowPlug:
                case LedType.Fuel:
                case LedType.LiftAxle:
                case LedType.DiffLock:
                    return Brushes.Yellow;
                case LedType.Rest:
                case LedType.WarningBrake:
                case LedType.ParkingBrake:
                case LedType.Battery:
                case LedType.Oil:
                    return Brushes.Red;
                case LedType.HighBeam:
                    return Brushes.DodgerBlue;
                default:
                    return Brushes.Pink;
            }
        }

        public LedState GetLedState(LedType id) => LedController[(int)id];

        public LedState[] GetLedFullState() => LedController;

        public void SetDefaultLedState()
        {
            LedController = new LedState[16];
            LedModified = true;
            LedStateChanged?.Invoke();
        }

        public bool SetLedState(LedType id, LedState state)
        {
            LedState currentState = LedController[(int)id];
            LedController[(int)id] = state;
            if(currentState != state)
            {
                LedModified = true;
                LedStateChanged?.Invoke();
                return true;
            }
            return false;
        }
        #endregion

        #region "BACKLIGHT"

        public enum BacklightType
        {
            WhiteBig,
            WhiteSmall,
            RedBig,
            RedSmall,
            LcdLed
        }

        public LedState GetBacklightState(BacklightType id) => BacklightController[(int)id];

        public LedState[] GetBacklightFullState() => BacklightController;

        public void SetDefaultBacklightState()
        {
            BacklightController = new LedState[5];
            BacklightModified = true;
            BacklightStateChanged?.Invoke();
        }

        public bool SetBacklightState(BacklightType id, LedState state)
        {
            LedState currentState = BacklightController[(int)id];
            BacklightController[(int)id] = state;
            if(currentState != state)
            {
                BacklightModified = true;
                BacklightStateChanged?.Invoke();
                return true;
            }
            return false;
        }
        #endregion

        #region "GAUGE"

        public enum GaugeType
        {
            Speed,
            Fuel,
            Air,
            Engine
        }

        public Double GetGaugeState(GaugeType id) => GaugePositions[(int)id];

        public Double[] GetGaugeFullState() => GaugePositions;

        public void SetDefaultGaugePosition()
        {
            GaugePositions = new double[4];
            GaugeModified = true;
            GaugePositionChanged?.Invoke();
        }

        public bool SetGaugePosition(GaugeType id, Double value)
        {
            Double currentValue = GaugePositions[(int)id];
            value = Math.Round(value);
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            GaugePositions[(int)id] = value;
            if(currentValue != value)
            {
                GaugeModified = true;
                GaugePositionChanged?.Invoke();
                return true;
            }
            return false;
        }
        #endregion

        #region "SCREEN"

        public enum RegistryType
        {
            RegistryA,
            RegistryB,
            RegistryC
        }

        public bool ChangeRegistryValue(RegistryType reg, int cellID, string value)
        {
            if(cellID >= 0 && cellID < 5)
            {
                string current = "";
                switch(reg)
                {
                    case RegistryType.RegistryA:
                        current = RegistryA[cellID];
                        RegistryA[cellID] = value;
                        break;
                    case RegistryType.RegistryB:
                        current = RegistryB[cellID];
                        RegistryB[cellID] = value;
                        break;
                    case RegistryType.RegistryC:
                        current = RegistryC[cellID];
                        RegistryC[cellID] = value;
                        break;
                }
                if(current != value)
                {
                    switch(reg)
                    {
                        case RegistryType.RegistryA: RegistryAModified = true; break;
                        case RegistryType.RegistryB: RegistryBModified = true; break;
                        case RegistryType.RegistryC: RegistryCModified = true; break;
                    }
                    LcdDataChanged?.Invoke(Screen.ScreenId, RegistryA, RegistryB, RegistryC);
                    return true;
                }
            }
            return false;
        }

        public string[] GetRegistryA() => RegistryA;

        public string[] GetRegistryB() => RegistryB;

        public string[] GetRegistryC() => RegistryC;

        private void Screen_ScreenIdChanged(ScreenController.ScreenType newScreen)
        {
            ScreenIdModified = true;
        }

        public void SetDefaultLcdState()
        {
            RegistryA = new string[5];
            RegistryB = new string[5];
            RegistryC = new string[5];
            RegistryAModified = true;
            RegistryBModified = true;
            RegistryCModified = true;
            ScreenIdModified = true;
        }
        #endregion
    }
}

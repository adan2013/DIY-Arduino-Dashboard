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

        int[] GaugePositions = new int[4];
        public delegate void GaugePositionChangedDelegate();
        public event GaugePositionChangedDelegate GaugePositionChanged;

        public ScreenController Screen;
        string[] RegistryA = new string[5];
        string[] RegistryB = new string[5];
        string[] RegistryC = new string[5];
        public delegate void LcdDataChangedDelegate(ScreenController.ScreenType id, string[] regA, string[] regB, string[] regC);
        public event LcdDataChangedDelegate LcdDataChanged;

        public bool LedModified = false;
        public bool BacklightModified = false;
        public bool GaugeModified = false;
        public bool[] RegistryAModified = new bool[5];
        public bool[] RegistryBModified = new bool[5];
        public bool[] RegistryCModified = new bool[5];
        public bool ScreenIdModified = false;

        public ArduinoController(Settings settingsManager)
        {
            Screen = new ScreenController(this, settingsManager);
            Screen.ScreenIdChanged += Screen_ScreenIdChanged;
            SetDefaultLcdState(true);
        }

        public void MarkChangesAsUpdated()
        {
            LedModified = false;
            BacklightModified = false;
            GaugeModified = false;
            RegistryAModified = new bool[5];
            RegistryBModified = new bool[5];
            RegistryCModified = new bool[5];
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
            LeftBlinker,
            RightBlinker,
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
                case LedType.LeftBlinker:
                case LedType.RightBlinker:
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

        public void SetDefaultLedState(bool forceUpdate)
        {
            if(!forceUpdate)
            {
                bool isClear = true;
                foreach(LedState led in LedController) if (led != LedState.Off) isClear = false;
                if (isClear) return;
            }
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

        public void SetDefaultBacklightState(bool forceUpdate)
        {
            if (!forceUpdate)
            {
                bool isClear = true;
                foreach (LedState bl in BacklightController) if (bl != LedState.Off) isClear = false;
                if (isClear) return;
            }
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

        public bool SetBacklightState(bool gauges, bool lcd)
        {
            bool result = false;
            result = SetBacklightState(BacklightType.WhiteBig, gauges ? LedState.On : LedState.Off) || result;
            result = SetBacklightState(BacklightType.WhiteSmall, gauges ? LedState.On : LedState.Off) || result;
            result = SetBacklightState(BacklightType.RedBig, gauges ? LedState.On : LedState.Off) || result;
            result = SetBacklightState(BacklightType.RedSmall, gauges ? LedState.On : LedState.Off) || result;
            result = SetBacklightState(BacklightType.LcdLed, lcd ? LedState.On : LedState.Off) || result;
            return result;
        }

        public bool SetBacklightState(bool whiteBig, bool whiteSmall, bool redBig, bool redSmall, bool lcdLed)
        {
            bool result = false;
            result = SetBacklightState(BacklightType.WhiteBig, whiteBig ? LedState.On : LedState.Off) || result;
            result = SetBacklightState(BacklightType.WhiteSmall, whiteSmall ? LedState.On : LedState.Off) || result;
            result = SetBacklightState(BacklightType.RedBig, redBig ? LedState.On : LedState.Off) || result;
            result = SetBacklightState(BacklightType.RedSmall, redSmall ? LedState.On : LedState.Off) || result;
            result = SetBacklightState(BacklightType.LcdLed, lcdLed ? LedState.On : LedState.Off) || result;
            return result;
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

        public int[] GetGaugeFullState() => GaugePositions;

        public void SetDefaultGaugePosition(bool forceUpdate)
        {
            if (!forceUpdate)
            {
                bool isClear = true;
                foreach (int gauge in GaugePositions) if (gauge != 0) isClear = false;
                if (isClear) return;
            }
            GaugePositions = new int[4];
            GaugeModified = true;
            GaugePositionChanged?.Invoke();
        }

        public bool SetGaugePosition(GaugeType id, float value)
        {
            int currentValue = GaugePositions[(int)id];
            if (value < 0) value = 0;
            if (value > 1000) value = 1000;
            GaugePositions[(int)id] = (int)value;
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
                        case RegistryType.RegistryA: RegistryAModified[cellID] = true; break;
                        case RegistryType.RegistryB: RegistryBModified[cellID] = true; break;
                        case RegistryType.RegistryC: RegistryCModified[cellID] = true; break;
                    }
                    LcdDataChanged?.Invoke(Screen.ScreenId, RegistryA, RegistryB, RegistryC);
                    return true;
                }
            }
            return false;
        }

        public bool RegistryAIsModified()
        {
            for (int i = 0; i < 5; i++) if (RegistryAModified[i]) return true;
            return false;
        }

        public bool RegistryBIsModified()
        {
            for (int i = 0; i < 5; i++) if (RegistryBModified[i]) return true;
            return false;
        }

        public bool RegistryCIsModified()
        {
            for (int i = 0; i < 5; i++) if (RegistryCModified[i]) return true;
            return false;
        }

        public string[] GetRegistryA() => RegistryA;

        public string[] GetRegistryB() => RegistryB;

        public string[] GetRegistryC() => RegistryC;

        private void Screen_ScreenIdChanged(ScreenController.ScreenType newScreen)
        {
            ScreenIdModified = true;
            LcdDataChanged?.Invoke(Screen.ScreenId, RegistryA, RegistryB, RegistryC);
        }

        public void SetDefaultLcdState(bool forceUpdate)
        {
            if (!forceUpdate)
            {
                bool isClear = true;
                foreach (string regValue in RegistryA) if (regValue != "") isClear = false;
                foreach (string regValue in RegistryB) if (regValue != "") isClear = false;
                foreach (string regValue in RegistryC) if (regValue != "") isClear = false;
                if (isClear) return;
            }
            RegistryA = new string[5];
            RegistryB = new string[5];
            RegistryC = new string[5];
            for (int i = 0; i < 5; i++)
            {
                RegistryAModified[i] = true;
                RegistryBModified[i] = true;
                RegistryCModified[i] = true;
            }
            Screen.SwitchScreen(ScreenController.ScreenType.ClearBlack);
        }
        #endregion
    }
}

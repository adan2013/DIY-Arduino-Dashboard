using ArduinoDashboardInterpreter.ScreenControllers;
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
        public delegate void LedStateChangedDelegate(LedType id, LedState state);
        public event LedStateChangedDelegate LedStateChanged;

        LedState[] BacklightController = new LedState[5];
        public delegate void BacklightStateChangedDelegate(BacklightType id, LedState state);
        public event BacklightStateChangedDelegate BacklightStateChanged;

        Double[] GaugePositions = new double[4];
        public delegate void GaugePositionChangedDelegate(GaugeType id, Double value);
        public event GaugePositionChangedDelegate GaugePositionChanged;

        public ScreenController Screen;
        ScreenController.ScreenType CurrentScreen;
        string[] RegistryA = new string[5];
        string[] RegistryB = new string[5];
        string[] RegistryC = new string[5];
        public delegate void LcdDataChangedDelegate(ScreenController.ScreenType id, string[] regA, string[] regB, string[] regC);
        public event LcdDataChangedDelegate LcdDataChanged;

        public bool LedModified = false;
        public bool BacklightModified = false;
        public bool GaugeModified = false;
        public bool RegistryAModified = false;
        public bool RegistryBModified = false;
        public bool RegistryCModified = false;
        public bool ScreenTypeModified = false;

        public ArduinoController()
        {
            Screen = new ClearBlackController(this);
            CurrentScreen = ScreenController.ScreenType.ClearBlack;
        }

        public void MarkChangesAsUpdated()
        {
            LedModified = false;
            BacklightModified = false;
            GaugeModified = false;
            RegistryAModified = false;
            RegistryBModified = false;
            RegistryCModified = false;
            ScreenTypeModified = false;
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

        public bool SetLedState(LedType id, LedState state)
        {
            LedState currentState = LedController[(int)id];
            LedController[(int)id] = state;
            if(currentState != state)
            {
                LedModified = true;
                LedStateChanged?.Invoke(id, state);
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

        public bool SetBacklightState(BacklightType id, LedState state)
        {
            LedState currentState = BacklightController[(int)id];
            BacklightController[(int)id] = state;
            if(currentState != state)
            {
                BacklightModified = true;
                BacklightStateChanged?.Invoke(id, state);
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

        public Double GetGaugePosition(GaugeType id) => GaugePositions[(int)id];

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
                GaugePositionChanged?.Invoke(id, value);
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
                    LcdDataChanged?.Invoke(CurrentScreen, RegistryA, RegistryB, RegistryC);
                    return true;
                }
            }
            return false;
        }

        public string[] GetRegisterA() => RegistryA;

        public string[] GetRegisterB() => RegistryB;

        public string[] GetRegisterC() => RegistryC;

        public bool SwitchScreen(ScreenController.ScreenType newScreen)
        {
            ScreenController.ScreenType current = CurrentScreen;
            switch(newScreen)
            {
                case ScreenController.ScreenType.ClearBlack: Screen = new ClearBlackController(this); break;
            }
            CurrentScreen = newScreen;
            if(current != newScreen)
            {
                ScreenTypeModified = true;
                LcdDataChanged?.Invoke(CurrentScreen, RegistryA, RegistryB, RegistryC);
                return true;
            }
            return false;
        }

        public ScreenController.ScreenType GetCurrentScreenType() => CurrentScreen;
        #endregion
    }
}

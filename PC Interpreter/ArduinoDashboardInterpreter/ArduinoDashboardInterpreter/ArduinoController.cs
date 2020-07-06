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
    class ArduinoController
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

        ProgramType CurrentProgram = ProgramType.Manual;
        public delegate void ProgramChangedDelegate(ProgramType newProgram);
        public event ProgramChangedDelegate ProgramChanged;

        public ScreenController Screen;
        ScreenController.ScreenType CurrentScreen;
        string[] RegistryA = new string[5];
        string[] RegistryB = new string[5];
        string[] RegistryC = new string[5];

        public ArduinoController()
        {
            Screen = new ClearBlackController(this);
            CurrentScreen = ScreenController.ScreenType.ClearBlack;
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

        public enum BacklightType
        {
            WhiteBig,
            WhiteSmall,
            RedBig,
            RedSmall,
            LcdLed
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

        public void SetLedState(LedType id, LedState state)
        {
            LedState currentState = LedController[(int)id];
            LedController[(int)id] = state;
            if (currentState != state) LedStateChanged?.Invoke(id, state);
        }
        #endregion

        #region "BACKLIGHT"

        public LedState GetBacklightState(BacklightType id) => BacklightController[(int)id];

        public void SetBacklightState(BacklightType id, LedState state)
        {
            LedState currentState = BacklightController[(int)id];
            BacklightController[(int)id] = state;
            if (currentState != state) BacklightStateChanged?.Invoke(id, state);
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

        public void SetGaugePosition(GaugeType id, Double value)
        {
            Double currentValue = GaugePositions[(int)id];
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            GaugePositions[(int)id] = value;
            if (currentValue != value) GaugePositionChanged?.Invoke(id, value);
        }
        #endregion

        #region "PROGRAM"

        public enum ProgramType
        {
            Home,
            Manual,
            Test,
            Telemetry
        }

        public ProgramType GetCurrentProgram()
        {
            return CurrentProgram;
        }

        public void SetNewProgram(ProgramType newProgram)
        {
            ProgramType oldProgram = CurrentProgram;
            CurrentProgram = newProgram;
            if (oldProgram != CurrentProgram) ProgramChanged?.Invoke(newProgram);
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
                return current != value;
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
            return current != newScreen;
        }

        public ScreenController.ScreenType GetCurrentScreenType() => CurrentScreen;
        #endregion
    }
}

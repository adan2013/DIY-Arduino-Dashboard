using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter.ProgramLoops
{
    class TestLoop : ProgramLoop
    {

        public enum TestStep
        {
            Initial,
            Gauges,
            GaugesReturn,
            Gauge1,
            Gauge2,
            Gauge3,
            Gauge4,
            GaugesReturn2,
            LedRings,
            LedBlinkers,
            LedsOff,
            BacklightWB,
            BacklightWS,
            BacklightRB,
            BacklightRS,
            BacklightLCD,
            LcdRed,
            LcdGreen,
            LcdBlue,
            LcdWhite,
            Reset
        }

        TestStep step = TestStep.Initial;
        bool stepExecuted = false;
        DateTime breakpoint = DateTime.Now;

        public TestLoop(ComConnector serial, ArduinoController arduino, Settings settings) : base(serial, arduino, settings) { }

        private void NextStep(bool setBreakpoint = true)
        {
            step = step == TestStep.Reset ? TestStep.Gauges : (TestStep)((int)step + 1);
            stepExecuted = false;
            if (setBreakpoint) SetBreakpoint();
        }

        private void SetBreakpoint() => breakpoint = DateTime.Now;

        private bool CheckBreakpoint(int milliseconds) => (DateTime.Now - breakpoint).TotalMilliseconds >= milliseconds;

        public override void Start(ComConnector serial, ArduinoController arduino, Settings settings)
        {
            //RESET
            arduino.SetDefaultLedState(true);
            arduino.SetDefaultBacklightState(true);
            arduino.SetDefaultGaugePosition(true);
            arduino.SetDefaultLcdState(true);
            //SENDING
            serial.SendLedUpdate(arduino.GetLedFullState());
            serial.SendBacklightUpdate(arduino.GetBacklightFullState());
            serial.SendGaugeUpdate(arduino.GetGaugeFullState());
            serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryA, arduino.GetRegistryA());
            serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryB, arduino.GetRegistryB());
            serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryC, arduino.GetRegistryC());
            serial.SendPrintLcdCommand(arduino.Screen.ScreenId);
            arduino.MarkChangesAsUpdated();
            base.Start(serial, arduino, settings);
        }

        private void SetLcdTestColor(ArduinoController arduino, int number)
        {
            arduino.Screen.testColor = number;
            arduino.Screen.SwitchScreen(ScreenController.ScreenType.Testing);
            arduino.Screen.Loop();
        }

        public override void Loop(ComConnector serial, ArduinoController arduino, Settings settings)
        {
            if (!stepExecuted)
            {
                switch (step)
                {
                    case TestStep.Gauges:
                        for(int g = 0; g < 4; g++) arduino.SetGaugePosition((ArduinoController.GaugeType)g, 1000);
                        break;
                    case TestStep.GaugesReturn:
                        for (int g = 0; g < 4; g++) arduino.SetGaugePosition((ArduinoController.GaugeType)g, 0);
                        break;
                    case TestStep.Gauge1:
                        arduino.SetGaugePosition(ArduinoController.GaugeType.Speed, 1000);
                        break;
                    case TestStep.Gauge2:
                        arduino.SetGaugePosition(ArduinoController.GaugeType.Fuel, 1000);
                        break;
                    case TestStep.Gauge3:
                        arduino.SetGaugePosition(ArduinoController.GaugeType.Air, 1000);
                        break;
                    case TestStep.Gauge4:
                        arduino.SetGaugePosition(ArduinoController.GaugeType.Engine, 1000);
                        break;
                    case TestStep.GaugesReturn2:
                        for (int g = 0; g < 4; g++) arduino.SetGaugePosition((ArduinoController.GaugeType)g, 0);
                        break;
                    case TestStep.LedRings:
                        for (int l = 0; l < 7; l++) arduino.SetLedState((ArduinoController.LedType)l, ArduinoController.LedState.On);
                        for (int l = 9; l < 16; l++) arduino.SetLedState((ArduinoController.LedType)l, ArduinoController.LedState.On);
                        break;
                    case TestStep.LedBlinkers:
                        for (int l = 0; l < 16; l++) arduino.SetLedState((ArduinoController.LedType)l, ArduinoController.LedState.Off);
                        arduino.SetLedState(ArduinoController.LedType.LeftBlinker, ArduinoController.LedState.Blink);
                        arduino.SetLedState(ArduinoController.LedType.RightBlinker, ArduinoController.LedState.Blink);
                        break;
                    case TestStep.LedsOff:
                        for (int l = 0; l < 16; l++) arduino.SetLedState((ArduinoController.LedType)l, ArduinoController.LedState.Off);
                        break;
                    case TestStep.BacklightWB:
                        arduino.SetBacklightState(ArduinoController.BacklightType.WhiteBig, ArduinoController.LedState.On);
                        break;
                    case TestStep.BacklightWS:
                        arduino.SetBacklightState(ArduinoController.BacklightType.WhiteSmall, ArduinoController.LedState.On);
                        break;
                    case TestStep.BacklightRB:
                        arduino.SetBacklightState(ArduinoController.BacklightType.RedBig, ArduinoController.LedState.On);
                        break;
                    case TestStep.BacklightRS:
                        arduino.SetBacklightState(ArduinoController.BacklightType.RedSmall, ArduinoController.LedState.On);
                        break;
                    case TestStep.BacklightLCD:
                        arduino.SetBacklightState(ArduinoController.BacklightType.LcdLed, ArduinoController.LedState.On);
                        break;
                    case TestStep.LcdRed:
                        SetLcdTestColor(arduino, 0);
                        break;
                    case TestStep.LcdGreen:
                        SetLcdTestColor(arduino, 1);
                        break;
                    case TestStep.LcdBlue:
                        SetLcdTestColor(arduino, 2);
                        break;
                    case TestStep.LcdWhite:
                        SetLcdTestColor(arduino, 3);
                        break;
                    case TestStep.Reset:
                        arduino.SetDefaultBacklightState(false);
                        arduino.SetDefaultLcdState(false);
                        break;

                }
                stepExecuted = true;
            }
            switch (step)
            {
                case TestStep.Initial: if (CheckBreakpoint(1200)) NextStep(); break;
                case TestStep.Gauges:
                case TestStep.GaugesReturn:
                case TestStep.GaugesReturn2: if (CheckBreakpoint(1600)) NextStep(); break;
                case TestStep.Gauge1:
                case TestStep.Gauge2:
                case TestStep.Gauge3: if (CheckBreakpoint(1000)) NextStep(); break;
                case TestStep.Gauge4: if (CheckBreakpoint(2000)) NextStep(); break;
                case TestStep.LedRings: if (CheckBreakpoint(1400)) NextStep(); break;
                case TestStep.LedBlinkers: if (CheckBreakpoint(2800)) NextStep(); break;
                case TestStep.LedsOff: if (CheckBreakpoint(400)) NextStep(); break;
                case TestStep.BacklightWB:
                case TestStep.BacklightWS:
                case TestStep.BacklightRB:
                case TestStep.BacklightRS:
                case TestStep.BacklightLCD: if (CheckBreakpoint(1400)) NextStep(); break;
                case TestStep.LcdRed:
                case TestStep.LcdGreen:
                case TestStep.LcdBlue:
                case TestStep.LcdWhite: if (CheckBreakpoint(900)) NextStep(); break;
                case TestStep.Reset: if (CheckBreakpoint(1500)) NextStep(); break;
            }
            if (arduino.LedModified) serial.SendLedUpdate(arduino.GetLedFullState());
            if (arduino.BacklightModified) serial.SendBacklightUpdate(arduino.GetBacklightFullState());
            if (arduino.GaugeModified) serial.SendGaugeUpdate(arduino.GetGaugeFullState());
            if (arduino.RegistryAIsModified()) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryA, arduino.GetRegistryA());
            if (arduino.RegistryBIsModified()) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryB, arduino.GetRegistryB());
            if (arduino.RegistryCIsModified()) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryC, arduino.GetRegistryC());
            if (arduino.ScreenIdModified)
            {
                serial.SendPrintLcdCommand(arduino.Screen.ScreenId);
            }
            else
            {
                if (arduino.RegistryAIsModified() || arduino.RegistryBIsModified() || arduino.RegistryCIsModified()) serial.SendUpdateLcdCommand();
            }
            arduino.MarkChangesAsUpdated();
            base.Loop(serial, arduino, settings);
        }
    }
}

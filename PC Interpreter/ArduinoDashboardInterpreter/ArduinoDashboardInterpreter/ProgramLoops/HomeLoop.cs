﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter.ProgramLoops
{
    class HomeLoop : ProgramLoop
    {

        public HomeLoop(ComConnector serial, ArduinoController arduino, Settings settings) : base(serial, arduino, settings) { }

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
    }
}

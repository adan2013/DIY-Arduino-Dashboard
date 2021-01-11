using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter.ProgramLoops
{
    class ManualLoop : ProgramLoop
    {

        public ManualLoop(ComConnector serial, ArduinoController arduino, Settings settings) : base(serial, arduino, settings) { }

        public override void Loop(ComConnector serial, ArduinoController arduino, Settings settings)
        {
            //LCD
            arduino.Screen.Loop();
            //SENDING
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

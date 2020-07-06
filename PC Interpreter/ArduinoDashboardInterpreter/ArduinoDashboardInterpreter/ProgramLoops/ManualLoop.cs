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
            //SENDING
            if (arduino.LedModified) serial.SendLedUpdate(arduino.GetLedFullState());
            if (arduino.BacklightModified) serial.SendBacklightUpdate(arduino.GetBacklightFullState());
            if (arduino.GaugeModified) serial.SendGaugeUpdate(arduino.GetGaugeFullState());
            if (arduino.RegistryAModified) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryA, arduino.GetRegistryA());
            if (arduino.RegistryBModified) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryB, arduino.GetRegistryB());
            if (arduino.RegistryCModified) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryC, arduino.GetRegistryC());
            if (arduino.ScreenTypeModified) serial.SendPrintLcdCommand(arduino.GetCurrentScreenType());
            if (arduino.RegistryAModified || arduino.RegistryBModified || arduino.RegistryCModified) serial.SendUpdateLcdCommand();
            arduino.MarkChangesAsUpdated();
            base.Loop(serial, arduino, settings);
        }
    }
}

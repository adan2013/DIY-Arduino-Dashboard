using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter.ProgramLoops
{
    class TelemetryLoop : ProgramLoop
    {

        TelemetryReceiver telemetry;

        public TelemetryLoop(ComConnector serial, ArduinoController arduino, Settings settings) : base(serial, arduino, settings)
        {
            telemetry = new TelemetryReceiver();
        }

        public void ShutDownTelemetryServer() => telemetry?.ShutDownTelemetryServer();
    }
}

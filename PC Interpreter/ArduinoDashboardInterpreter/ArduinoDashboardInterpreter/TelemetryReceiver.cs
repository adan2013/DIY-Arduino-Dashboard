using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCSSdkClient;
using SCSSdkClient.Object;

namespace ArduinoDashboardInterpreter
{
    class TelemetryReceiver
    {
        SCSSdkTelemetry telemetry;

        public TelemetryReceiver()
        {
            telemetry = new SCSSdkTelemetry();
            telemetry.Data += Telemetry_Data;
            if(telemetry.Error != null) System.Windows.MessageBox.Show(telemetry.Error.Message);
        }

        public void ShutDownTelemetryServer()
        {
            telemetry.pause();
            telemetry.Dispose();
        }

        private void Telemetry_Data(SCSTelemetry data, bool newTimestamp)
        {
            if (!newTimestamp) return;
            
        }
    }
}

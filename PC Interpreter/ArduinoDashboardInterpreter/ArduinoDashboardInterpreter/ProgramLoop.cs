using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter
{
    class ProgramLoop
    {
        public ProgramLoop(ComConnector serial, ArduinoController arduino, Settings settings)
        {
            Start(serial, arduino, settings);
        }

        virtual public void Start(ComConnector serial, ArduinoController arduino, Settings settings) { }

        virtual public void Loop(ComConnector serial, ArduinoController arduino, Settings settings) { }
    }
}

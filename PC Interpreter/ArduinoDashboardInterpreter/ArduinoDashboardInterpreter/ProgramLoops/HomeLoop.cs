using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter.ProgramLoops
{
    class HomeLoop : ProgramLoop
    {

        public HomeLoop(ComConnector serial, ArduinoController arduino, Settings settings) : base(serial, arduino, settings) { }
}
}

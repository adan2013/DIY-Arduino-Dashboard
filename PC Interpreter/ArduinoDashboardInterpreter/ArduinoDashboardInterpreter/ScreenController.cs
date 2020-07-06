using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter
{
    class ScreenController
    {
        public enum ScreenType
        {
            ClearBlack,

        }

        protected ArduinoController arduino;

        public ScreenController(ArduinoController arduino)
        {
            this.arduino = arduino;
            Start();
        }

        virtual public void Start() { }

        virtual public void Loop() { }
        
        virtual public void LeftButton() { }

        virtual public void OkButton() { }

        virtual public void RightButton() { }
    }
}

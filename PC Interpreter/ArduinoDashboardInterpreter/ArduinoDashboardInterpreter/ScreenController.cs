using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter
{
    public class ScreenController
    {
        public enum ScreenType
        {
            ClearBlack,
            Testing,
            InitialImage,
            Assistant,
            Navigation,
            Job,
            Engine,
            Fuel,
            Truck,
            Trailer,
            Menu,
            Settings,
            CompConfiguration,
            Acceleration
        }

        public delegate void ScreenIdChangedDelegate(ScreenType newScreen);
        public event ScreenIdChangedDelegate ScreenIdChanged;

        public ScreenType? PrevScreenId { get; private set; }
        public ScreenType ScreenId { get; private set; }
        ArduinoController arduino;

        //REGISTER A
        public string gear = "";
        public string ecoshift = "OK";
        public string clock = "12:00";
        public int ccSpeed = 0;

        //REGISTER B
        public int testColor = 0;
        public int initImageType = 0;
        public int assistantType1 = 0;
        public string assistantValue1 = "";
        public int assistantType2 = 0;
        public string assistantValue2 = "";
        public string navTime = "0 min";
        public string navDistance = "0 km";
        public string restTime = "0 min";
        public string jobDeliveryTime = "0 min";
        public string jobSource = "";
        public string jobDestination = "";
        public string airPressure = "";
        public string oilTemperature = "";
        public string oilPressure = "";
        public string waterTemperature = "";
        public string battery = "0.0 V";
        public string fuelLeft = "0 L";
        public string fuelCapacity = "0 L";
        public string fuelAvgConsumption = "0 L/100km";
        public string fuelRange = "0 km";
        public string adblue = "0";
        public string damageEngine = "0%";
        public string damageTransmission = "0%";
        public string damageCabin = "0%";
        public string damageChassis = "0%";
        public string damageWheels = "0%";
        public string trailerDamage = "0%";
        public string trailerLiftAxle = "0";
        public string trailerName = "";
        public string trailerMass = "0 kg";
        public string trailerAttached = "0";
        public int menuScreenPart = 0;
        public int menuCursorPosition = 0;
        public string menuCheckboxs = "";
        public int accelerationState = 0;
        public int currentSpeed = 0;
        public int accelerationTargetSpeed = 0;
        public string accelerationTimer = "00:00";

        //REGISTER C
        public int alertId = 0;
        public int retarderCurrent = 0;
        public int retarderMax = 0;
        public int odometer = 0;
        public int speedLimit = 0;
        
        public ScreenController(ArduinoController arduino)
        {
            this.arduino = arduino;
            SwitchScreen(ScreenType.ClearBlack);
        }

        public bool SwitchScreen(ScreenType newScreen)
        {
            if(ScreenId != newScreen)
            {
                ScreenId = newScreen;
                ScreenIdChanged?.Invoke(newScreen);
                Start();
                return true;
            }
            return false;
        }

        public bool RestorePrevScreen()
        {
            if (PrevScreenId is null) return false;
            bool response = SwitchScreen((ScreenType)PrevScreenId);
            PrevScreenId = null;
            return response;
        }

        public bool SaveCurrentScreen()
        {
            if(PrevScreenId != ScreenId)
            {
                PrevScreenId = ScreenId;
                return true;
            }
            return false;
        }

        public void Start()
        {
            switch (ScreenId)
            {

            }
        }

        public void Loop()
        {
            switch (ScreenId)
            {

            }
        }
        
        public void LeftButton()
        {
            switch(ScreenId)
            {
                
            }
        }

        public void OkButton()
        {
            switch (ScreenId)
            {

            }
        }

        public void RightButton()
        {
            switch (ScreenId)
            {

            }
        }
    }
}

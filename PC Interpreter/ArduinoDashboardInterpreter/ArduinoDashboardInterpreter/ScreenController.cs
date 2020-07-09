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
            CompConfigurator,
            Acceleration
        }

        #region "ENUMS"

        public enum AssistantValueType
        {
            Off,
            NavTime,
            NavDistance,
            RestTime,
            JobDeliveryTime,
            AirPressure,
            OilTemperature,
            OilPressure,
            WaterTemperature,
            Battery,
            FuelLeft,
            FuelAvg,
            FuelRange,
            CurrentSpeed
        }

        public enum InitialImageType
        {
            ScsSoftware,
            Redark,
            Daf,
            Man,
            Mercedes,
            Renault,
            Scania,
            Volvo
        }

        public enum AssistantAlertType
        {
            Normal
        }

        public enum CompactAlertType
        {
            Off
        }
        #endregion

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
        public InitialImageType initImageType = 0;
        public AssistantAlertType assistantAlertId = AssistantAlertType.Normal;
        public AssistantValueType assistantType1 = AssistantValueType.Off;
        public string assistantValue1 = "";
        public AssistantValueType assistantType2 = AssistantValueType.Off;
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
        public CompactAlertType alertId = CompactAlertType.Off;
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
            //switch (ScreenId)
            //{
                
            //}
            Loop();
        }

        public void Loop()
        {
            switch (ScreenId)
            {
                case ScreenType.Testing:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, testColor.ToString());
                    break;
                case ScreenType.InitialImage:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, initImageType.ToString());
                    break;
                case ScreenType.Assistant:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, ((int)assistantAlertId).ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, ((int)assistantType1).ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, assistantValue1);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, ((int)assistantType2).ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 4, assistantValue2);
                    break;
                case ScreenType.Navigation:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, navTime);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, navDistance);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, restTime);
                    break;
                case ScreenType.Job:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, jobDeliveryTime);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, jobSource);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, jobDestination);
                    break;
                case ScreenType.Engine:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, airPressure);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, oilTemperature);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, oilPressure);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, waterTemperature);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 4, battery);
                    break;
                case ScreenType.Fuel:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, fuelLeft);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, fuelCapacity);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, fuelAvgConsumption);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, fuelRange);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 4, adblue);
                    break;
                case ScreenType.Truck:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, damageEngine);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, damageTransmission);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, damageCabin);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, damageChassis);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 4, damageWheels);
                    break;
                case ScreenType.Trailer:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, trailerDamage);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, trailerLiftAxle);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, trailerName);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, trailerMass);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 4, trailerAttached);
                    break;
                case ScreenType.Menu:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, menuScreenPart.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, menuCursorPosition.ToString());
                    break;
                case ScreenType.Settings:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, menuScreenPart.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, menuCursorPosition.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, menuCheckboxs);
                    break;
                case ScreenType.CompConfigurator:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, menuCursorPosition.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, ((int)initImageType).ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, ((int)assistantType1).ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, ((int)assistantType2).ToString());
                    break;
                case ScreenType.Acceleration:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, accelerationState.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, currentSpeed.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, accelerationTargetSpeed.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, accelerationTimer);
                    break;
            }
        }
        
        public void LeftButton()
        {
            switch(ScreenId)
            {
                case ScreenType.Assistant: SwitchScreen(ScreenType.Trailer); break;
                case ScreenType.Navigation: SwitchScreen(ScreenType.Assistant); break;
                case ScreenType.Job: SwitchScreen(ScreenType.Navigation); break;
                case ScreenType.Engine: SwitchScreen(ScreenType.Job); break;
                case ScreenType.Fuel: SwitchScreen(ScreenType.Engine); break;
                case ScreenType.Truck: SwitchScreen(ScreenType.Fuel); break;
                case ScreenType.Trailer: SwitchScreen(ScreenType.Truck); break;
            }
        }

        public void OkButton()
        {
            switch (ScreenId)
            {
                case ScreenType.Assistant:
                case ScreenType.Navigation:
                case ScreenType.Job:
                case ScreenType.Engine:
                case ScreenType.Fuel:
                case ScreenType.Truck:
                case ScreenType.Trailer:
                    SaveCurrentScreen();
                    SwitchScreen(ScreenType.Menu);
                    break;
            }
        }

        public void RightButton()
        {
            switch (ScreenId)
            {
                case ScreenType.Assistant: SwitchScreen(ScreenType.Navigation); break;
                case ScreenType.Navigation: SwitchScreen(ScreenType.Job); break;
                case ScreenType.Job: SwitchScreen(ScreenType.Engine); break;
                case ScreenType.Engine: SwitchScreen(ScreenType.Fuel); break;
                case ScreenType.Fuel: SwitchScreen(ScreenType.Truck); break;
                case ScreenType.Truck: SwitchScreen(ScreenType.Trailer); break;
                case ScreenType.Trailer: SwitchScreen(ScreenType.Assistant); break;
            }
        }

        public string GetAssistantValueByType(AssistantValueType type)
        {
            switch (type)
            {
                case AssistantValueType.NavTime: return navTime;
                case AssistantValueType.NavDistance: return navDistance;
                case AssistantValueType.RestTime: return restTime;
                case AssistantValueType.JobDeliveryTime: return jobDeliveryTime;
                case AssistantValueType.AirPressure: return airPressure;
                case AssistantValueType.OilTemperature: return oilTemperature;
                case AssistantValueType.OilPressure: return oilPressure;
                case AssistantValueType.WaterTemperature: return waterTemperature;
                case AssistantValueType.Battery: return battery;
                case AssistantValueType.FuelLeft: return fuelLeft;
                case AssistantValueType.FuelAvg: return fuelAvgConsumption;
                case AssistantValueType.FuelRange: return fuelRange;
                case AssistantValueType.CurrentSpeed: return currentSpeed.ToString();
                default: return "";
            }
        }
    }
}

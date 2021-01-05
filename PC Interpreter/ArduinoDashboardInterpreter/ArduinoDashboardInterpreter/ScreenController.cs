using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoDashboardInterpreter
{
    public class ScreenController
    {
        #region "ENUMS"

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
            MainMenu,
            SettingsMenu,
            Customization,
            Acceleration,
            Information
        }

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

        public enum AlertType
        {
            Off = 0,
            BrakeLowPressure = 31
        }

        public enum MenuType
        {
            MainMenu,
            SettingsMenu
        }

        public enum MainMenuItems
        {
            Back,
            Settings,
            Customization,
            Acceleration,
            Information
        }

        public enum SettingsMenuItems
        {
            Back,
            Sound,
            Clock24h,
            RealTimeClock,
            EcoShift,
            SpeedLimitWarning
        }

        public enum CustomizationMenuItems
        {
            InitImage,
            AssistantType1,
            AssistantType2,
            AssistantType3,
            AssistantType4
        }

        public enum AccelerationScreenState
        {
            SpeedSelection,
            WaitingForStop,
            WaitingForStart,
            SpeedMeasurement,
            TimeDisplay
        }
        #endregion

        public delegate void ScreenIdChangedDelegate(ScreenType newScreen);
        public event ScreenIdChangedDelegate ScreenIdChanged;

        public ScreenType? PrevScreenId { get; private set; }
        public ScreenType ScreenId { get; private set; }
        ArduinoController arduino;
        Settings settings;

        //REGISTER A
        public string gear = "";
        public string ecoShift = "OK";
        public string clock = "12:00";
        public int ccSpeed = 0;

        //REGISTER B
        public int testColor = 0;
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
        public string damageEngine = "0";
        public string damageTransmission = "0";
        public string damageCabin = "0";
        public string damageChassis = "0";
        public string damageWheels = "0";
        public string trailerDamage = "0%";
        public string trailerLiftAxle = "0";
        public string trailerName = "";
        public string trailerMass = "0 kg";
        public string trailerAttached = "0";
        public int menuCursorPosition = 0;
        public string menuCurrentValue = "";
        public int currentSpeed = 0;
        public int accelerationTargetSpeed = 0;
        public Stopwatch accelerationTimer;
        public string accelerationTimerText = "00:00";

        //REGISTER C
        public AlertType alertId = AlertType.Off;
        public int retarderCurrent = 0;
        public int retarderMax = 0;
        public int odometer = 0;
        public int speedLimit = 0;
        
        public ScreenController(ArduinoController arduino, Settings settings)
        {
            this.arduino = arduino;
            this.settings = settings;
            SwitchScreen(ScreenType.ClearBlack);
        }

        public bool SwitchScreen(ScreenType newScreen, int initialCursorPosition = 0)
        {
            if(ScreenId != newScreen)
            {
                menuCursorPosition = initialCursorPosition;
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
                case ScreenType.Acceleration:
                    accelerationTargetSpeed = 80;
                    accelerationTimer = new Stopwatch();
                    break;
            }
            Loop();
        }

        public void Loop()
        {
            if((int)ScreenId > 2)
            {
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryA, 0, gear);
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryA, 1, ecoShift);
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryA, 2, clock);
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryA, 3, ccSpeed.ToString());
            }
            switch (ScreenId)
            {
                case ScreenType.Testing:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, testColor.ToString());
                    break;
                case ScreenType.InitialImage:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, ((int)settings.GetInitialImage()).ToString());
                    break;
                case ScreenType.Assistant:
                    String types
                        = settings.GetAssistantType1().ToString("X")
                        + settings.GetAssistantType2().ToString("X")
                        + settings.GetAssistantType3().ToString("X")
                        + settings.GetAssistantType4().ToString("X");
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, types);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, GetAssistantValueByType(settings.GetAssistantType1()));
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, GetAssistantValueByType(settings.GetAssistantType2()));
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, GetAssistantValueByType(settings.GetAssistantType3()));
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 4, GetAssistantValueByType(settings.GetAssistantType4()));
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
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, damageCabin);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, damageChassis);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, damageEngine);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, damageTransmission);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 4, damageWheels);
                    break;
                case ScreenType.Trailer:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, trailerDamage);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, trailerLiftAxle);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, trailerName);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, trailerMass);
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 4, trailerAttached);
                    break;
                case ScreenType.MainMenu:
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, menuCursorPosition.ToString());
                    break;
                case ScreenType.SettingsMenu:
                    switch((SettingsMenuItems)menuCursorPosition)
                    {
                        case SettingsMenuItems.Back: menuCurrentValue = ""; break;
                        case SettingsMenuItems.Sound: menuCurrentValue = settings.GetOptionValue(Settings.OptionType.Sound) ? "Yes" : "No"; break;
                        case SettingsMenuItems.Clock24h: menuCurrentValue = settings.GetOptionValue(Settings.OptionType.Clock24h) ? "Yes" : "No"; break;
                        case SettingsMenuItems.RealTimeClock: menuCurrentValue = settings.GetOptionValue(Settings.OptionType.RealTimeClock) ? "Yes" : "No"; break;
                        case SettingsMenuItems.EcoShift: menuCurrentValue = settings.GetOptionValue(Settings.OptionType.EcoShift) ? "Yes" : "No"; break;
                        case SettingsMenuItems.SpeedLimitWarning: menuCurrentValue = settings.GetOptionValue(Settings.OptionType.SpeedLimitWarning) ? "Yes" : "No"; break;
                    }
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, menuCursorPosition.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, menuCurrentValue);
                    break;
                case ScreenType.Customization:
                    switch ((CustomizationMenuItems)menuCursorPosition)
                    {
                        case CustomizationMenuItems.InitImage: menuCurrentValue = settings.GetInitialImageName(); break;
                        case CustomizationMenuItems.AssistantType1: menuCurrentValue = settings.GetAssistantTypeName(settings.GetAssistantType1()); break;
                        case CustomizationMenuItems.AssistantType2: menuCurrentValue = settings.GetAssistantTypeName(settings.GetAssistantType2()); break;
                        case CustomizationMenuItems.AssistantType3: menuCurrentValue = settings.GetAssistantTypeName(settings.GetAssistantType3()); break;
                        case CustomizationMenuItems.AssistantType4: menuCurrentValue = settings.GetAssistantTypeName(settings.GetAssistantType4()); break;

                    }
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, menuCursorPosition.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, menuCurrentValue);
                    break;
                case ScreenType.Acceleration:
                    switch ((AccelerationScreenState)menuCursorPosition)
                    {
                        case AccelerationScreenState.WaitingForStop:
                            if (currentSpeed == 0) menuCursorPosition = (int)AccelerationScreenState.WaitingForStart;
                            break;
                        case AccelerationScreenState.WaitingForStart:
                            if (currentSpeed > 0)
                            {
                                menuCursorPosition = (int)AccelerationScreenState.SpeedMeasurement;
                                accelerationTimer.Start();
                            }
                            break;
                        case AccelerationScreenState.SpeedMeasurement:
                            if(accelerationTargetSpeed <= currentSpeed)
                            {
                                menuCursorPosition = (int)AccelerationScreenState.TimeDisplay;
                                accelerationTimer.Stop();
                            }
                            TimeSpan timeDiff = accelerationTimer.Elapsed;
                            accelerationTimerText = timeDiff.Minutes + ":" + timeDiff.Seconds + ":" + timeDiff.Milliseconds;
                            break;
                    }
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 0, menuCursorPosition.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 1, currentSpeed.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 2, accelerationTargetSpeed.ToString());
                    arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryB, 3, accelerationTimerText);
                    break;
            }
            if ((int)ScreenId > 2 && (int)ScreenId < 10)
            {
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryC, 0, ((int)alertId).ToString());
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryC, 1, retarderCurrent.ToString());
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryC, 2, retarderMax.ToString());
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryC, 3, odometer.ToString());
                arduino.ChangeRegistryValue(ArduinoController.RegistryType.RegistryC, 4, speedLimit.ToString());
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
                case ScreenType.MainMenu: MoveMenuCursor(-1, MenuType.MainMenu); break;
                case ScreenType.SettingsMenu: MoveMenuCursor(-1, MenuType.SettingsMenu); break;
                case ScreenType.Customization:
                    switch((CustomizationMenuItems)menuCursorPosition)
                    {
                        case CustomizationMenuItems.InitImage: settings.SwitchInitialImage(-1); break;
                        case CustomizationMenuItems.AssistantType1: settings.SwitchAssistantType(1, -1); break;
                        case CustomizationMenuItems.AssistantType2: settings.SwitchAssistantType(2, -1); break;
                        case CustomizationMenuItems.AssistantType3: settings.SwitchAssistantType(3, -1); break;
                        case CustomizationMenuItems.AssistantType4: settings.SwitchAssistantType(4, -1); break;
                    }
                    break;
                case ScreenType.Acceleration:
                    if((AccelerationScreenState)menuCursorPosition == AccelerationScreenState.SpeedSelection)
                    {
                        accelerationTargetSpeed -= 5;
                        if (accelerationTargetSpeed < 40) accelerationTargetSpeed = 40;
                    }
                    break;
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
                    SwitchScreen(ScreenType.MainMenu);
                    break;
                case ScreenType.MainMenu:
                    switch((MainMenuItems)menuCursorPosition)
                    {
                        case MainMenuItems.Back: RestorePrevScreen(); break;
                        case MainMenuItems.Settings: SwitchScreen(ScreenType.SettingsMenu); break;
                        case MainMenuItems.Customization: SwitchScreen(ScreenType.Customization); break;
                        case MainMenuItems.Acceleration: SwitchScreen(ScreenType.Acceleration); break;
                    }
                    break;
                case ScreenType.SettingsMenu:
                    switch((SettingsMenuItems)menuCursorPosition)
                    {
                        case SettingsMenuItems.Back: SwitchScreen(ScreenType.MainMenu, (int)MainMenuItems.Settings); break;
                        case SettingsMenuItems.Sound: settings.ToggleOption(Settings.OptionType.Sound); break;
                        case SettingsMenuItems.Clock24h: settings.ToggleOption(Settings.OptionType.Clock24h); break;
                        case SettingsMenuItems.RealTimeClock: settings.ToggleOption(Settings.OptionType.RealTimeClock); break;
                        case SettingsMenuItems.EcoShift: settings.ToggleOption(Settings.OptionType.EcoShift); break;
                        case SettingsMenuItems.SpeedLimitWarning: settings.ToggleOption(Settings.OptionType.SpeedLimitWarning); break;
                    }
                    break;
                case ScreenType.Customization:
                    switch((CustomizationMenuItems)menuCursorPosition)
                    {
                        case CustomizationMenuItems.InitImage: menuCursorPosition = (int)CustomizationMenuItems.AssistantType1; break;
                        case CustomizationMenuItems.AssistantType1: menuCursorPosition = (int)CustomizationMenuItems.AssistantType2; break;
                        case CustomizationMenuItems.AssistantType2: menuCursorPosition = (int)CustomizationMenuItems.AssistantType3; break;
                        case CustomizationMenuItems.AssistantType3: menuCursorPosition = (int)CustomizationMenuItems.AssistantType4; break;
                        case CustomizationMenuItems.AssistantType4: SwitchScreen(ScreenType.MainMenu, (int)MainMenuItems.Customization); break;
                    }
                    break;
                case ScreenType.Acceleration:
                    if ((AccelerationScreenState)menuCursorPosition == AccelerationScreenState.SpeedSelection)
                    {
                        menuCursorPosition = (int)(currentSpeed == 0 ? AccelerationScreenState.WaitingForStart : AccelerationScreenState.WaitingForStop);
                    }
                    else
                    {
                        SwitchScreen(ScreenType.MainMenu, (int)MainMenuItems.Acceleration);
                    }
                    break;
                case ScreenType.Information:
                    SwitchScreen(ScreenType.MainMenu, (int)MainMenuItems.Information);
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
                case ScreenType.MainMenu: MoveMenuCursor(1, MenuType.MainMenu); break;
                case ScreenType.SettingsMenu: MoveMenuCursor(1, MenuType.SettingsMenu); break;
                case ScreenType.Customization:
                    switch ((CustomizationMenuItems)menuCursorPosition)
                    {
                        case CustomizationMenuItems.InitImage: settings.SwitchInitialImage(1); break;
                        case CustomizationMenuItems.AssistantType1: settings.SwitchAssistantType(1, 1); break;
                        case CustomizationMenuItems.AssistantType2: settings.SwitchAssistantType(2, 1); break;
                        case CustomizationMenuItems.AssistantType3: settings.SwitchAssistantType(3, 1); break;
                        case CustomizationMenuItems.AssistantType4: settings.SwitchAssistantType(4, 1); break;
                    }
                    break;
                case ScreenType.Acceleration:
                    if ((AccelerationScreenState)menuCursorPosition == AccelerationScreenState.SpeedSelection)
                    {
                        accelerationTargetSpeed += 5;
                        if (accelerationTargetSpeed > 140) accelerationTargetSpeed = 140;
                    }
                    break;
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

        public void MoveMenuCursor(int difference, MenuType menuType)
        {
            int maxLength = 0;
            switch(menuType)
            {
                case MenuType.MainMenu: maxLength = Enum.GetNames(typeof(MainMenuItems)).Length; break;
                case MenuType.SettingsMenu: maxLength = Enum.GetNames(typeof(SettingsMenuItems)).Length; break;
            }
            int newPosition = menuCursorPosition += difference;
            if (newPosition < 0) newPosition = maxLength - 1;
            if (newPosition >= maxLength) newPosition = 0;
            menuCursorPosition = newPosition;
        }
    }
}

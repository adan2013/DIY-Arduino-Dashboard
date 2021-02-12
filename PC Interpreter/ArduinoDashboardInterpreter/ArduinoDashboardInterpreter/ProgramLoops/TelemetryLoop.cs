using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCSSdkClient;
using SCSSdkClient.Object;

namespace ArduinoDashboardInterpreter.ProgramLoops
{
    class TelemetryLoop : ProgramLoop
    {
        const int MAX_SPEED_GAUGE = 140;
        const int MAX_ENGINE_GAUGE = 3000;
        const int MAX_PSI_AIR_PRESSURE_GAUGE = 160;
        const double PSI_TO_BAR = 0.069;
        const int SHORT_DELIVERY_TIME = 120;

        NotificationsController nc;
        SCSSdkTelemetry sdk;
        SCSTelemetry telemetry;
        WorkingState state;
        DateTime lastStateSwitch;

        public enum WorkingState
        {
            TelemetryNotConnected,
            PowerOff,
            InitialTesting,
            Ignition,
            EngineWorking
        }

        public TelemetryLoop(ComConnector serial, ArduinoController arduino, Settings settings) : base(serial, arduino, settings)
        {
            nc = arduino.Screen.notifications;
            nc.ResetNotificationSystem();
            //RESET
            arduino.SetDefaultLedState(true);
            arduino.SetDefaultBacklightState(true);
            arduino.SetDefaultGaugePosition(true);
            arduino.SetDefaultLcdState(true);
            //SENDING
            serial.SendLedUpdate(arduino.GetLedFullState());
            serial.SendBacklightUpdate(arduino.GetBacklightFullState());
            serial.SendGaugeUpdate(arduino.GetGaugeFullState());
            serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryA, arduino.GetRegistryA());
            serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryB, arduino.GetRegistryB());
            serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryC, arduino.GetRegistryC());
            serial.SendPrintLcdCommand(arduino.Screen.ScreenId);
            arduino.MarkChangesAsUpdated();
            base.Start(serial, arduino, settings);
            //TELEMETRY SERVER
            sdk = new SCSSdkTelemetry(20);
            telemetry = new SCSTelemetry();
            sdk.JobStarted += Sdk_JobStarted;
            sdk.JobDelivered += Sdk_JobDelivered;
            sdk.Tollgate += Sdk_Tollgate;
            sdk.RefuelPayed += Sdk_RefuelPayed;
            sdk.Fined += Sdk_Fined;
            sdk.Data += Telemetry_Data;
            if (sdk.Error != null) System.Windows.MessageBox.Show(sdk.Error.Message);
        }

        private bool AcceptSdkGameEvents() => GameIsRunning() && ElectricEnabled();
        private void Sdk_JobStarted(object sender, EventArgs e) => nc.TurnOnNotification(NotificationsController.NotificationType.NewJob, AcceptSdkGameEvents(), false);
        private void Sdk_JobDelivered(object sender, EventArgs e) => nc.TurnOnNotification(NotificationsController.NotificationType.CargoDelivered, AcceptSdkGameEvents(), false);
        private void Sdk_Tollgate(object sender, EventArgs e) => nc.TurnOnNotification(NotificationsController.NotificationType.Tollgate, AcceptSdkGameEvents(), false);
        private void Sdk_RefuelPayed(object sender, EventArgs e) => nc.TurnOnNotification(NotificationsController.NotificationType.TruckRefueled, AcceptSdkGameEvents(), false);
        private void Sdk_Fined(object sender, EventArgs e) => nc.TurnOnNotification(NotificationsController.NotificationType.Fined, AcceptSdkGameEvents(), false);

        public void ShutDownTelemetryServer()
        {
            sdk.pause();
            sdk.Dispose();
        }

        private void Telemetry_Data(SCSTelemetry data, bool newTimestamp)
        {
            if (!newTimestamp) return;
            telemetry = data;
        }

        private bool GameIsRunning() => telemetry.Game != SCSGame.Unknown;

        private bool ElectricEnabled() => telemetry.TruckValues.CurrentValues.ElectricEnabled;

        private bool EngineRunning() => telemetry.TruckValues.CurrentValues.EngineEnabled;

        private bool DashboardLightsOn() => telemetry.TruckValues.CurrentValues.LightsValues.Parking;

        private bool LowLightsOn() => telemetry.TruckValues.CurrentValues.LightsValues.BeamLow;

        private bool HighLightsOn() => telemetry.TruckValues.CurrentValues.LightsValues.BeamHigh;

        private bool ParkingBrakeOn() => telemetry.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake;

        private bool RunOfFuel() => telemetry.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW;

        private bool CruiseControlOn() => telemetry.TruckValues.CurrentValues.DashboardValues.CruiseControl;

        private bool RetarderActivated() => telemetry.TruckValues.CurrentValues.MotorValues.BrakeValues.RetarderLevel > 0 && telemetry.ControlValues.InputValues.Throttle == 0 && telemetry.TruckValues.CurrentValues.DashboardValues.Speed.Kph > 0;

        private bool LowBrakePressure() => telemetry.TruckValues.CurrentValues.DashboardValues.WarningValues.AirPressure;

        private bool BrakesLocked() => telemetry.TruckValues.CurrentValues.DashboardValues.WarningValues.AirPressureEmergency;

        private bool RestNeeded() => telemetry.CommonValues.NextRestStop.Value < 120;

        private ArduinoController.LedState LeftBlinkerState()
        {
            if (telemetry.TruckValues.CurrentValues.LightsValues.BlinkerLeftActive) return ArduinoController.LedState.Blink;
            return telemetry.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn ? ArduinoController.LedState.On : ArduinoController.LedState.Off;
        }

        private ArduinoController.LedState RightBlinkerState()
        {
            if (telemetry.TruckValues.CurrentValues.LightsValues.BlinkerRightActive) return ArduinoController.LedState.Blink;
            return telemetry.TruckValues.CurrentValues.LightsValues.BlinkerRightOn ? ArduinoController.LedState.On : ArduinoController.LedState.Off;
        }

        private string GetGearDashboardValue()
        {
            int gear = telemetry.TruckValues.CurrentValues.DashboardValues.GearDashboards;
            if (gear < 0) return gear.ToString().Replace('-', 'R');
            if (gear == 0) return "N";
            return gear.ToString();
        }

        private string GetEcoShiftValue()
        {
            float rpm = telemetry.TruckValues.CurrentValues.DashboardValues.RPM;
            float throttle = telemetry.ControlValues.InputValues.Throttle;
            int gear = telemetry.TruckValues.CurrentValues.DashboardValues.GearDashboards;
            uint MaxGear = telemetry.TruckValues.ConstantsValues.MotorValues.ForwardGearCount;
            if(throttle > 0 && gear > 0)
            {
                if (rpm > 1550 && gear < MaxGear) return "UP";
                if (rpm < 950 && gear > 1) return "DN";
            }
            return "OK";
        }

        private string GetDashboardClock(Settings settings)
        {
            bool realTime = settings.GetOptionValue(Settings.OptionType.RealTimeClock);
            bool clock24h = settings.GetOptionValue(Settings.OptionType.Clock24h);
            DateTime time = realTime ? DateTime.Now : telemetry.CommonValues.GameTime.Date;
            return time.ToString(clock24h ? "HH:mm" : "hh:mm tt");
        }

        private bool TruckIsDamaged()
        {
            float max = 0;
            max = Math.Max(max, telemetry.TruckValues.CurrentValues.DamageValues.Cabin);
            max = Math.Max(max, telemetry.TruckValues.CurrentValues.DamageValues.Chassis);
            max = Math.Max(max, telemetry.TruckValues.CurrentValues.DamageValues.WheelsAvg);
            max = Math.Max(max, telemetry.TruckValues.CurrentValues.DamageValues.Engine);
            max = Math.Max(max, telemetry.TruckValues.CurrentValues.DamageValues.Transmission);
            return max > 0.08;
        }

        private void SetLedState(ArduinoController arduino, bool initial, bool ignition, Settings settings)
        {
            arduino.SetLedState(ArduinoController.LedType.CheckEngine, initial || TruckIsDamaged() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.GlowPlug, initial ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.LowBeam, LowLightsOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.HighBeam, HighLightsOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Fuel, initial || RunOfFuel() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.LiftAxle, CheckAxleIsLiftOff(true, true) ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.DiffLock, settings.GetOptionValue(Settings.OptionType.DiffLock) ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.LeftBlinker, LeftBlinkerState());
            arduino.SetLedState(ArduinoController.LedType.RightBlinker, RightBlinkerState());
            arduino.SetLedState(ArduinoController.LedType.CruiseControl, CruiseControlOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Rest, RestNeeded() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.WarningBrake, LowBrakePressure() ? ArduinoController.LedState.Blink : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.ParkingBrake, ParkingBrakeOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Battery, ignition ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Oil, ignition ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Retarder, RetarderActivated() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
        }

        private bool CheckAxleIsLiftOff(bool truck, bool trailer)
        {
            if (truck)
            {
                foreach (float lift in telemetry.TruckValues.CurrentValues.WheelsValues.Lift) if (lift > 0) return true;
            }
            if (trailer)
            {
                foreach (float lift in telemetry.TrailerValues[0].Wheelvalues.Lift) if (lift > 0) return true;
            }
            return false;
        }

        private bool CheckAxleIsLiftable(bool truck, bool trailer)
        {
            if (truck)
            {
                foreach (bool liftable in telemetry.TruckValues.ConstantsValues.WheelsValues.Liftable) if (liftable) return true;
            }
            if(trailer)
            {
                foreach (bool liftable in telemetry.TrailerValues[0].WheelsConstant.Liftable) if (liftable) return true;
            }
            return false;
        }

        private String FormatScreenTimeValue(float data)
        {
            if (data == 0) return "---";
            int h = (int)(data / 60);
            int m = (int)(data - h * 60);
            return (h > 0 ? h + "h " : "") + m + "m";
        }

        private String FormatScreenValue(float data, string text)
        {
            if (data == 0) return "---";
            return (int)data + " " + text;
        }

        private void CalculateNotifications()
        {
            //TrailerAttached
            nc.TurnOnNotification(NotificationsController.NotificationType.TrailerAttached, telemetry.TrailerValues[0].Attached);
            nc.UnlockTheNotification(NotificationsController.NotificationType.TrailerAttached, !telemetry.TrailerValues[0].Attached);
            //ShortDeliveryTime
            bool onJob = telemetry.JobValues.CityDestination != "";
            nc.TurnOnNotification(NotificationsController.NotificationType.ShortDeliveryTime, onJob && telemetry.JobValues.RemainingDeliveryTime.Value < SHORT_DELIVERY_TIME);
            nc.UnlockTheNotification(NotificationsController.NotificationType.ShortDeliveryTime, telemetry.JobValues.RemainingDeliveryTime.Value > SHORT_DELIVERY_TIME + 10);
            //NewJob
            //nc.TurnOnNotification(NotificationsController.NotificationType.NewJob, telemetry.SpecialEventsValues.OnJob);
            //nc.UnlockTheNotification(NotificationsController.NotificationType.NewJob, !telemetry.SpecialEventsValues.OnJob);
            //CargoDelivered
            //nc.TurnOnNotification(NotificationsController.NotificationType.CargoDelivered, telemetry.SpecialEventsValues.JobDelivered);
            //nc.UnlockTheNotification(NotificationsController.NotificationType.CargoDelivered, telemetry.SpecialEventsValues.JobDelivered);
            //RestNeeded
            nc.TurnOnNotification(NotificationsController.NotificationType.RestNeeded, RestNeeded());
            nc.UnlockTheNotification(NotificationsController.NotificationType.RestNeeded, !RestNeeded());
            //LowLevelOfFuel
            nc.TurnOnNotification(NotificationsController.NotificationType.LowLevelOfFuel, RunOfFuel());
            nc.UnlockTheNotification(NotificationsController.NotificationType.LowLevelOfFuel, !RunOfFuel());
            //VehicleDamaged
            //TODO vehicle damaged
            //TrailerDamaged
            //TODO trailer damaged
            //BrakeLowPressure
            nc.TurnOnNotification(NotificationsController.NotificationType.LowBrakePressure, LowBrakePressure());
            nc.UnlockTheNotification(NotificationsController.NotificationType.LowBrakePressure, !LowBrakePressure());
            //BrakesLocked
            nc.TurnOnNotification(NotificationsController.NotificationType.BrakesLocked, BrakesLocked());
            nc.UnlockTheNotification(NotificationsController.NotificationType.BrakesLocked, !BrakesLocked());
            //RestTimeLimitExceeded
            nc.TurnOnNotification(NotificationsController.NotificationType.RestTimeLimitExceeded, telemetry.CommonValues.NextRestStop.Value <= 0);
            nc.UnlockTheNotification(NotificationsController.NotificationType.RestTimeLimitExceeded, telemetry.CommonValues.NextRestStop.Value > 10);
            //ReleaseHandbrake
            bool rh = ParkingBrakeOn()
                && telemetry.ControlValues.InputValues.Throttle > 0
                && telemetry.TruckValues.CurrentValues.DashboardValues.GearDashboards != 0
                && telemetry.TruckValues.CurrentValues.DashboardValues.Speed.Kph > 0;
            nc.TurnOnNotification(NotificationsController.NotificationType.ReleaseHandbrake, rh);
            nc.UnlockTheNotification(NotificationsController.NotificationType.ReleaseHandbrake, !rh);
        }

        private void UpdateScreenData(ArduinoController arduino, Settings settings)
        {
            // REG A
            arduino.Screen.gear = GetGearDashboardValue();
            arduino.Screen.ecoShift = settings.GetOptionValue(Settings.OptionType.EcoShift) ? GetEcoShiftValue() : "OK";
            arduino.Screen.clock = GetDashboardClock(settings);
            arduino.Screen.ccSpeed = telemetry.TruckValues.CurrentValues.DashboardValues.CruiseControl ? (int)Math.Round(telemetry.TruckValues.CurrentValues.DashboardValues.CruiseControlSpeed.Kph) : 0;
            // REG B
            arduino.Screen.navTime = FormatScreenTimeValue(telemetry.NavigationValues.NavigationTime / 60);
            arduino.Screen.navDistance = FormatScreenValue(telemetry.NavigationValues.NavigationDistance / 1000, "km");
            arduino.Screen.restTime = FormatScreenTimeValue(telemetry.CommonValues.NextRestStop.Value > 0 ? telemetry.CommonValues.NextRestStop.Value : 0);
            arduino.Screen.jobDeliveryTime = FormatScreenTimeValue(telemetry.JobValues.RemainingDeliveryTime.Value);
            arduino.Screen.jobSource = telemetry.JobValues.CitySource;
            arduino.Screen.jobDestination = telemetry.JobValues.CityDestination;
            arduino.Screen.airPressure = (telemetry.TruckValues.CurrentValues.MotorValues.BrakeValues.AirPressure * PSI_TO_BAR).ToString("N1").Replace(',', '.') + " bar";
            arduino.Screen.oilTemperature = (int)telemetry.TruckValues.CurrentValues.DashboardValues.OilTemperature + " C";
            arduino.Screen.oilPressure = (telemetry.TruckValues.CurrentValues.DashboardValues.OilPressure * PSI_TO_BAR).ToString("N1").Replace(',', '.') + " bar";
            arduino.Screen.waterTemperature = (int)telemetry.TruckValues.CurrentValues.DashboardValues.WaterTemperature + " C";
            arduino.Screen.battery = Math.Round(telemetry.TruckValues.CurrentValues.DashboardValues.BatteryVoltage) + " V";
            arduino.Screen.fuelLeft = ((int)telemetry.TruckValues.CurrentValues.DashboardValues.FuelValue.Amount).ToString();
            arduino.Screen.fuelCapacity = ((int)telemetry.TruckValues.ConstantsValues.CapacityValues.Fuel).ToString();
            arduino.Screen.fuelAvgConsumption = FormatScreenValue(telemetry.TruckValues.CurrentValues.DashboardValues.FuelValue.AverageConsumption * 100, "L/100km");
            arduino.Screen.fuelRange = FormatScreenValue(telemetry.TruckValues.CurrentValues.DashboardValues.FuelValue.Range, "km");
            arduino.Screen.adblue = FormatScreenValue(telemetry.TruckValues.CurrentValues.DashboardValues.AdBlue, "L");
            arduino.Screen.damageEngine = Math.Round(telemetry.TruckValues.CurrentValues.DamageValues.Engine * 100).ToString();
            arduino.Screen.damageTransmission = Math.Round(telemetry.TruckValues.CurrentValues.DamageValues.Transmission * 100).ToString();
            arduino.Screen.damageCabin = Math.Round(telemetry.TruckValues.CurrentValues.DamageValues.Cabin * 100).ToString();
            arduino.Screen.damageChassis = Math.Round(telemetry.TruckValues.CurrentValues.DamageValues.Chassis * 100).ToString();
            arduino.Screen.damageWheels = Math.Round(telemetry.TruckValues.CurrentValues.DamageValues.WheelsAvg * 100).ToString();
            arduino.Screen.trailerDamage = Math.Round(telemetry.TrailerValues[0].DamageValues.Chassis * 100).ToString();
            arduino.Screen.trailerLiftAxle = CheckAxleIsLiftable(false, true) ? "Yes" : "No";
            arduino.Screen.trailerName = telemetry.JobValues.CargoValues.Name;
            arduino.Screen.trailerMass = FormatScreenValue(telemetry.JobValues.CargoValues.Mass, "kg");
            arduino.Screen.trailerAttached = telemetry.TrailerValues[0].Attached ? "Yes" : "No";
            arduino.Screen.currentSpeed = (int)telemetry.TruckValues.CurrentValues.DashboardValues.Speed.Kph;
            // REG C
            arduino.Screen.notificationId = arduino.Screen.notifications.GetCurrentNotification();
            arduino.Screen.retarderCurrent = (int)telemetry.TruckValues.CurrentValues.MotorValues.BrakeValues.RetarderLevel;
            arduino.Screen.retarderMax = (int)telemetry.TruckValues.ConstantsValues.MotorValues.RetarderStepCount;
            arduino.Screen.odometer = (int)telemetry.TruckValues.CurrentValues.DashboardValues.Odometer;
            arduino.Screen.speedLimit = (int)telemetry.NavigationValues.SpeedLimit.Kph;
        }

        private void SwitchState(WorkingState newState)
        {
            state = newState;
            lastStateSwitch = DateTime.Now;
        }

        private void AutoStateSwitch(ArduinoController arduino)
        {
            if((DateTime.Now - lastStateSwitch).TotalMilliseconds > 2000)
            {
                switch (state)
                {
                    case WorkingState.InitialTesting:
                        SwitchState(WorkingState.Ignition);
                        if (!arduino.Screen.RestorePrevScreen()) arduino.Screen.SwitchScreen(ScreenController.ScreenType.Assistant);
                        break;
                    case WorkingState.Ignition:
                        if(EngineRunning()) SwitchState(WorkingState.EngineWorking);
                        break;
                }
            }
        }

        public override void Loop(ComConnector serial, ArduinoController arduino, Settings settings)
        {
            AutoStateSwitch(arduino);
            if(GameIsRunning() && ElectricEnabled())
            {
                switch (state)
                {
                    case WorkingState.TelemetryNotConnected:
                    case WorkingState.PowerOff:
                        arduino.Screen.SwitchScreen(ScreenController.ScreenType.InitialImage);
                        SwitchState(WorkingState.InitialTesting);
                        break;
                    case WorkingState.InitialTesting:
                        SetLedState(arduino, true, true, settings);
                        break;
                    case WorkingState.Ignition:
                        SetLedState(arduino, false, true, settings);
                        break;
                    case WorkingState.EngineWorking:
                        SetLedState(arduino, false, false, settings);
                        break;
                }
                arduino.SetBacklightState(DashboardLightsOn(), true);
                arduino.SetGaugePosition(ArduinoController.GaugeType.Speed, telemetry.TruckValues.CurrentValues.DashboardValues.Speed.Kph * 1000 / MAX_SPEED_GAUGE);
                arduino.SetGaugePosition(ArduinoController.GaugeType.Fuel, telemetry.TruckValues.CurrentValues.DashboardValues.FuelValue.Amount * 1000 / telemetry.TruckValues.ConstantsValues.CapacityValues.Fuel);
                arduino.SetGaugePosition(ArduinoController.GaugeType.Air, telemetry.TruckValues.CurrentValues.MotorValues.BrakeValues.AirPressure * 1000 / MAX_PSI_AIR_PRESSURE_GAUGE);
                arduino.SetGaugePosition(ArduinoController.GaugeType.Engine, telemetry.TruckValues.CurrentValues.DashboardValues.RPM * 1000 / MAX_ENGINE_GAUGE);
                CalculateNotifications();
                UpdateScreenData(arduino, settings);
                arduino.Screen.Loop();
            }
            else
            {
                if(GameIsRunning())
                {
                    nc.ResetNotificationSystem();
                    SwitchState(WorkingState.PowerOff);
                    if((int)arduino.Screen.ScreenId > 2 && (int)arduino.Screen.ScreenId < 10) arduino.Screen.SaveCurrentScreen();
                    arduino.Screen.SwitchScreen(ScreenController.ScreenType.InitialImage);
                    arduino.Screen.Loop();
                    arduino.SetDefaultBacklightState(false);
                }
                else
                {
                    SwitchState(WorkingState.TelemetryNotConnected);
                    arduino.Screen.SwitchScreen(ScreenController.ScreenType.TelemetryNotConnected);
                    arduino.SetBacklightState(false, true);
                }
                arduino.SetDefaultLedState(false);
                arduino.SetDefaultGaugePosition(false);
            }
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

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
            sdk = new SCSSdkTelemetry();
            telemetry = new SCSTelemetry();
            sdk.Data += Telemetry_Data;
            if (sdk.Error != null) System.Windows.MessageBox.Show(sdk.Error.Message);
        }

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

        private bool LeftBlinkerOn() => telemetry.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn;

        private bool RightBlinkerOn() => telemetry.TruckValues.CurrentValues.LightsValues.BlinkerRightOn;

        private bool ParkingBrakeOn() => telemetry.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake;

        private bool RunOfFuel() => telemetry.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW;

        private bool CruiseControlOn() => telemetry.TruckValues.CurrentValues.DashboardValues.CruiseControl;

        private bool RetarderActivated() => telemetry.TruckValues.CurrentValues.MotorValues.BrakeValues.RetarderLevel > 0; //TODO gas position > 0 ???

        private bool LowBrakePressure() => telemetry.TruckValues.CurrentValues.DashboardValues.WarningValues.AirPressure;

        private bool RestNeeded() => telemetry.CommonValues.NextRestStop.Value < 120;

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
            return max > 8;
        }

        private void SetLedState(ArduinoController arduino, bool initial, bool ignition)
        {
            arduino.SetLedState(ArduinoController.LedType.CheckEngine, initial || TruckIsDamaged() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.GlowPlug, initial ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.LowBeam, LowLightsOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.HighBeam, HighLightsOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Fuel, initial || RunOfFuel() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.LiftAxle, false ? ArduinoController.LedState.On : ArduinoController.LedState.Off); //TODO false led
            arduino.SetLedState(ArduinoController.LedType.DiffLock, false ? ArduinoController.LedState.On : ArduinoController.LedState.Off); //TODO false led
            arduino.SetLedState(ArduinoController.LedType.LeftBlinker, LeftBlinkerOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.RightBlinker, RightBlinkerOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.CruiseControl, CruiseControlOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Rest, RestNeeded() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.WarningBrake, LowBrakePressure() ? ArduinoController.LedState.Blink : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.ParkingBrake, ParkingBrakeOn() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Battery, ignition ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Oil, ignition ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
            arduino.SetLedState(ArduinoController.LedType.Retarder, RetarderActivated() ? ArduinoController.LedState.On : ArduinoController.LedState.Off);
        }

        private bool CheckTrailerLiftAxle()
        {
            foreach (bool liftable in telemetry.TrailerValues[0].WheelsConstant.Liftable) if (liftable) return true;
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

        private void UpdateScreenData(ArduinoController arduino, Settings settings)
        {
            // REG A
            arduino.Screen.gear = GetGearDashboardValue();
            arduino.Screen.ecoShift = settings.GetOptionValue(Settings.OptionType.EcoShift) ? GetEcoShiftValue() : "OK";
            arduino.Screen.clock = GetDashboardClock(settings);
            arduino.Screen.ccSpeed = telemetry.TruckValues.CurrentValues.DashboardValues.CruiseControl ? (int)Math.Round(telemetry.TruckValues.CurrentValues.DashboardValues.CruiseControlSpeed.Kph) : 0;
            // REG B
            arduino.Screen.navTime = FormatScreenTimeValue(telemetry.NavigationValues.NavigationTime);
            arduino.Screen.navDistance = FormatScreenValue(telemetry.NavigationValues.NavigationDistance / 1000, "km");
            arduino.Screen.restTime = FormatScreenTimeValue(telemetry.CommonValues.NextRestStop.Value);
            arduino.Screen.jobDeliveryTime = FormatScreenTimeValue(telemetry.JobValues.DeliveryTime.Value);
            arduino.Screen.jobSource = telemetry.JobValues.CitySource;
            arduino.Screen.jobDestination = telemetry.JobValues.CityDestination;
            arduino.Screen.airPressure = (int)telemetry.TruckValues.CurrentValues.MotorValues.BrakeValues.AirPressure + " bar";
            arduino.Screen.oilTemperature = (int)telemetry.TruckValues.CurrentValues.DashboardValues.OilTemperature + " C";
            arduino.Screen.oilPressure = (int)telemetry.TruckValues.CurrentValues.DashboardValues.OilPressure + " bar";
            arduino.Screen.waterTemperature = (int)telemetry.TruckValues.CurrentValues.DashboardValues.WaterTemperature + " C";
            arduino.Screen.battery = Math.Round(telemetry.TruckValues.CurrentValues.DashboardValues.BatteryVoltage, 1) + " V";
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
            arduino.Screen.trailerDamage = Math.Round(telemetry.JobValues.CargoValues.CargoDamage * 100).ToString();
            arduino.Screen.trailerLiftAxle = CheckTrailerLiftAxle() ? "Yes" : "No";
            arduino.Screen.trailerName = telemetry.JobValues.CargoValues.Name;
            arduino.Screen.trailerMass = FormatScreenValue(telemetry.JobValues.CargoValues.Mass, "kg");
            arduino.Screen.trailerAttached = telemetry.TrailerValues[0].Attached ? "Yes" : "No";
            arduino.Screen.currentSpeed = (int)telemetry.TruckValues.CurrentValues.DashboardValues.Speed.Kph;
            // REG C
            arduino.Screen.alertId = ScreenController.AlertType.Off;
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

        private void AutoStateSwitch()
        {
            if((DateTime.Now - lastStateSwitch).TotalMilliseconds > 2000)
            {
                switch (state)
                {
                    case WorkingState.InitialTesting:
                        SwitchState(WorkingState.Ignition);
                        break;
                    case WorkingState.Ignition:
                        if(EngineRunning()) SwitchState(WorkingState.EngineWorking);
                        break;
                }
            }
        }

        public override void Loop(ComConnector serial, ArduinoController arduino, Settings settings)
        {
            AutoStateSwitch();
            if(GameIsRunning() && ElectricEnabled())
            {
                switch (state)
                {
                    case WorkingState.TelemetryNotConnected:
                    case WorkingState.PowerOff:
                        SwitchState(WorkingState.InitialTesting);
                        break;
                    case WorkingState.InitialTesting:
                        SetLedState(arduino, true, true);
                        arduino.Screen.SwitchScreen(ScreenController.ScreenType.InitialImage);
                        break;
                    case WorkingState.Ignition:
                        SetLedState(arduino, false, true);
                        arduino.Screen.SwitchScreen(ScreenController.ScreenType.Assistant);
                        break;
                    case WorkingState.EngineWorking:
                        SetLedState(arduino, false, false);
                        break;
                }
                arduino.SetBacklightState(DashboardLightsOn(), true);
                //TODO GAUGES
                UpdateScreenData(arduino, settings);
                arduino.Screen.Loop();
            }
            else
            {
                if(GameIsRunning())
                {
                    SwitchState(WorkingState.PowerOff);
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
            if (arduino.RegistryAModified) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryA, arduino.GetRegistryA());
            if (arduino.RegistryBModified) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryB, arduino.GetRegistryB());
            if (arduino.RegistryCModified) serial.SendRegistryUpdate(ArduinoController.RegistryType.RegistryC, arduino.GetRegistryC());
            if (arduino.ScreenIdModified)
            {
                serial.SendPrintLcdCommand(arduino.Screen.ScreenId);
            }
            else
            {
                if (arduino.RegistryAModified || arduino.RegistryBModified || arduino.RegistryCModified) serial.SendUpdateLcdCommand();
            }
            arduino.MarkChangesAsUpdated();
            base.Loop(serial, arduino, settings);
        }
    }
}

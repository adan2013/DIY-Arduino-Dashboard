using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Navigation;

namespace ArduinoDashboardInterpreter
{
    [Serializable()]
    public class Settings
    {
        public Settings()
        {
            SetDefaultOptions();
            SetDefaultCustomization();
        }

        #region "SHORTCUTS"

        List<KeyboardShortcut> shortcuts = new List<KeyboardShortcut>();

        public delegate void ShortcutsUpdatedDelegate();
        [field: NonSerialized]
        public event ShortcutsUpdatedDelegate ShortcutsUpdated;

        public delegate void KeyPressedDelegate(KeyboardShortcut shortcut);
        [field: NonSerialized]
        public event KeyPressedDelegate KeyPressed;

        public void StartListening()
        {
            foreach (KeyboardShortcut item in shortcuts) item.RegisterKey();
            GlobalHotKey.HotKeyPressed += HotKeyPressed;
        }

        public void EndListening()
        {
            foreach (KeyboardShortcut item in shortcuts) item.UnregisterKey();
            GlobalHotKey.HotKeyPressed -= HotKeyPressed;
        }

        public void AddShortcut(KeyboardShortcut shortcut)
        {
            DeleteShortcut(shortcut.Target);
            shortcuts.Add(shortcut);
            shortcut.RegisterKey();
            ShortcutsUpdated?.Invoke();
        }

        public void DeleteShortcut(KeyboardShortcut.TargetType target)
        {
            foreach (KeyboardShortcut item in shortcuts)
            {
                if (item.Target == target)
                {
                    item.UnregisterKey();
                    shortcuts.Remove(item);
                    ShortcutsUpdated?.Invoke();
                    break;
                }
            }
        }

        public string GetShortcutString(KeyboardShortcut.TargetType target)
        {
            foreach (KeyboardShortcut item in shortcuts)
            {
                if (item.Target == target) return item.ToString();
            }
            return "none";
        }

        private void HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            foreach (KeyboardShortcut item in shortcuts)
            {
                if (item.ShortcutKey == e.Key && item.ShortcutModifiers == e.Modifiers) KeyPressed?.Invoke(item);
            }
        }
        #endregion

        #region "OPTIONS"

        List<OptionType> options = new List<OptionType>();

        public delegate void OptionsUpdatedDelegate();
        [field: NonSerialized]
        public event OptionsUpdatedDelegate OptionsUpdated;

        public enum OptionType
        {
            KeyboardListener,
            Sound,
            DiffLock,
            Clock24h,
            RealTimeClock,
            EcoShift,
            SpeedLimitWarning
        }

        public void SetDefaultOptions()
        {
            options.Clear();
            options.Add(OptionType.KeyboardListener);
            options.Add(OptionType.Sound);
            options.Add(OptionType.Clock24h);
            options.Add(OptionType.EcoShift);
            options.Add(OptionType.SpeedLimitWarning);
            OptionsUpdated?.Invoke();
        }

        public bool ToggleOption(OptionType option)
        {
            bool current = GetOptionValue(option);
            if(current)
            {
                options.Remove(option);
            }
            else
            {
                options.Add(option);
            }
            OptionsUpdated?.Invoke();
            return !current;
        }

        public bool GetOptionValue(OptionType option)
        {
            return options.Contains(option);
        }
        #endregion

        #region "CUSTOMIZATION"

        ScreenController.InitialImageType initialImage;
        ScreenController.AssistantValueType assistantType1;
        ScreenController.AssistantValueType assistantType2;
        ScreenController.AssistantValueType assistantType3;
        ScreenController.AssistantValueType assistantType4;

        public delegate void CustomizationUpdatedDelegate();
        [field: NonSerialized]
        public event CustomizationUpdatedDelegate CustomizationUpdated;

        public enum CustomizationType
        {
            InitialImage,
            AssistantType1,
            AssistantType2,
            AssistantType3,
            AssistantType4
        }

        public void SetDefaultCustomization()
        {
            initialImage = ScreenController.InitialImageType.Redark;
            assistantType1 = ScreenController.AssistantValueType.Off;
            assistantType2 = ScreenController.AssistantValueType.Off;
            assistantType3 = ScreenController.AssistantValueType.Off;
            assistantType4 = ScreenController.AssistantValueType.Off;
            CustomizationUpdated?.Invoke();
        }

        public void SwitchInitialImage(int difference)
        {
            int maxLength = Enum.GetNames(typeof(ScreenController.InitialImageType)).Length;
            int newValue = (int)initialImage + difference;
            if (newValue < 0) newValue = maxLength - 1;
            if (newValue >= maxLength) newValue = 0;
            initialImage = (ScreenController.InitialImageType)newValue;
            CustomizationUpdated?.Invoke();
        }

        public void SwitchAssistantType(int typeId, int difference)
        {
            int maxLength = Enum.GetNames(typeof(ScreenController.AssistantValueType)).Length;
            int newValue = (int)GetAssistantValueType(typeId) + difference;
            if (newValue < 0) newValue = maxLength - 1;
            if (newValue >= maxLength) newValue = 0;
            SetAssistantValueType(typeId, (ScreenController.AssistantValueType)newValue);
            CustomizationUpdated?.Invoke();
        }

        private ScreenController.AssistantValueType GetAssistantValueType(int id)
        {
            switch (id)
            {
                case 1: return assistantType1;
                case 2: return assistantType2;
                case 3: return assistantType3;
                case 4: return assistantType4;
                default: return ScreenController.AssistantValueType.Off;
            }
        }

        private void SetAssistantValueType(int id, ScreenController.AssistantValueType value)
        {
            switch (id)
            {
                case 1: assistantType1 = value; break;
                case 2: assistantType2 = value; break;
                case 3: assistantType3 = value; break;
                case 4: assistantType4 = value; break;
            }
        }

        public ScreenController.InitialImageType GetInitialImage() => initialImage;

        public ScreenController.AssistantValueType GetAssistantType1() => assistantType1;

        public ScreenController.AssistantValueType GetAssistantType2() => assistantType2;

        public ScreenController.AssistantValueType GetAssistantType3() => assistantType3;

        public ScreenController.AssistantValueType GetAssistantType4() => assistantType4;

        public String GetInitialImageName()
        {
            switch (GetInitialImage())
            {
                case ScreenController.InitialImageType.ScsSoftware: return "SCS Software";
                case ScreenController.InitialImageType.Redark: return "Redark";
                case ScreenController.InitialImageType.Daf: return "Daf";
                case ScreenController.InitialImageType.Man: return "Man";
                case ScreenController.InitialImageType.Mercedes: return "Mercedes";
                case ScreenController.InitialImageType.Renault: return "Renault";
                case ScreenController.InitialImageType.Scania: return "Scania";
                case ScreenController.InitialImageType.Volvo: return "Volvo";
                default: return "";
            }
        }

        public String GetAssistantTypeName(ScreenController.AssistantValueType type)
        {
            switch(type)
            {
                case ScreenController.AssistantValueType.Off: return "OFF";
                case ScreenController.AssistantValueType.NavTime: return "Nav Time";
                case ScreenController.AssistantValueType.NavDistance: return "Nav distance";
                case ScreenController.AssistantValueType.RestTime: return "Rest time";
                case ScreenController.AssistantValueType.JobDeliveryTime: return "Job delivery time";
                case ScreenController.AssistantValueType.AirPressure: return "Air pressure";
                case ScreenController.AssistantValueType.OilTemperature: return "Oil temperature";
                case ScreenController.AssistantValueType.OilPressure: return "Oil pressure";
                case ScreenController.AssistantValueType.WaterTemperature: return "Water temperature";
                case ScreenController.AssistantValueType.Battery: return "Battery";
                case ScreenController.AssistantValueType.FuelLeft: return "Fuel left";
                case ScreenController.AssistantValueType.FuelAvg: return "Fuel average";
                case ScreenController.AssistantValueType.FuelRange: return "Fuel range";
                case ScreenController.AssistantValueType.CurrentSpeed: return "Speed";
                default: return "";
            }
        }
        #endregion
    }
}

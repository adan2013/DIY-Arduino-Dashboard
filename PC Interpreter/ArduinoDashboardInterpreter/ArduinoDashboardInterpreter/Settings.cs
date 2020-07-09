using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter
{
    [Serializable()]
    public class Settings
    {
        public Settings()
        {
            SetDefaultOptions();
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
            SpeedLimitWarning,
            AssistantAutoSwitch
        }

        public void SetDefaultOptions()
        {
            options.Clear();
            options.Add(OptionType.KeyboardListener);
            options.Add(OptionType.Sound);
            options.Add(OptionType.Clock24h);
            options.Add(OptionType.EcoShift);
            options.Add(OptionType.SpeedLimitWarning);
            options.Add(OptionType.AssistantAutoSwitch);
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
    }
}

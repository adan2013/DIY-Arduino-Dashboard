using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter
{
    [Serializable()]
    class Settings
    {
        List<KeyboardShortcut> shortcuts = new List<KeyboardShortcut>();

        public delegate void ShortcutsUpdatedDelegate();
        public event ShortcutsUpdatedDelegate ShortcutsUpdated;

        public delegate void KeyPressedDelegate(KeyboardShortcut shortcut);
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
            foreach(KeyboardShortcut item in shortcuts)
            {
                if(item.Target == target) return item.ToString();
            }
            return "none";
        }

        private void HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            foreach(KeyboardShortcut item in shortcuts)
            {
                if (item.ShortcutKey == e.Key && item.ShortcutModifiers == e.Modifiers) KeyPressed?.Invoke(item);
            }
        }
    }
}

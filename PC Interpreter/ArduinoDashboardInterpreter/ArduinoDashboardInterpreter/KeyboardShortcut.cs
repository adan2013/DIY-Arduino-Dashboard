using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoDashboardInterpreter
{
    [Serializable()]
    class KeyboardShortcut
    {
        public readonly TargetType Target;
        public readonly Keys ShortcutKey;
        public readonly KeyModifiers ShortcutModifiers;
        private int hotkeyID;

        public enum TargetType
        {
            DiffLock,
            Left,
            Ok,
            Right
        }

        public KeyboardShortcut(TargetType Target, Keys ShortcutKey, KeyModifiers ShortcutModifiers)
        {
            this.Target = Target;
            this.ShortcutKey = ShortcutKey;
            this.ShortcutModifiers = ShortcutModifiers;
        }

        public void RegisterKey()
        {
            hotkeyID = GlobalHotKey.RegisterHotKey(ShortcutKey, ShortcutModifiers);
        }

        public void UnregisterKey()
        {
            GlobalHotKey.UnregisterHotKey(hotkeyID);
        }

        public override string ToString()
        {
            string s = "";
            if (ShortcutModifiers == KeyModifiers.Control) s += "Ctrl+";
            if (ShortcutModifiers == KeyModifiers.Alt) s += "Alt+";
            if (ShortcutModifiers == KeyModifiers.Shift) s += "Shift+";
            s += ShortcutKey.ToString();
            return s;
        }
    }
}

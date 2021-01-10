using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoDashboardInterpreter
{
    [Serializable()]
    public class KeyboardShortcut
    {
        public readonly TargetType Target;
        public readonly Keys ShortcutKey;
        public readonly KeyModifiers ShortcutModifiers;

        [field: NonSerialized]
        public GlobalKeyboardHook gkh;

        public enum TargetType
        {
            DiffLock,
            Left,
            Ok,
            Right
        }

        public KeyboardShortcut(GlobalKeyboardHook gkh, TargetType Target, Keys ShortcutKey, KeyModifiers ShortcutModifiers)
        {
            this.gkh = gkh;
            this.Target = Target;
            this.ShortcutKey = ShortcutKey;
            this.ShortcutModifiers = ShortcutModifiers;
        }

        public void RegisterKey()
        {
            gkh.HookedKeys.Add(ShortcutKey);
        }

        public void UnregisterKey()
        {
            gkh.HookedKeys.Remove(ShortcutKey);
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

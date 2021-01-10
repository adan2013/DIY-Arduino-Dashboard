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

        [field: NonSerialized]
        public GlobalKeyboardHook gkh;

        public enum TargetType
        {
            DiffLock,
            Left,
            Ok,
            Right
        }

        public KeyboardShortcut(GlobalKeyboardHook gkh, TargetType Target, Keys ShortcutKey)
        {
            this.gkh = gkh;
            this.Target = Target;
            this.ShortcutKey = ShortcutKey;
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
            return ShortcutKey.ToString();
        }
    }
}

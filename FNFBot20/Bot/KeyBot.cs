using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FNFBot20
{
    public class KeyBot
    {
        public enum BindTarget
        {
            None,
            Play,
            OffsetUp,
            OffsetDown,
            Lane
        }

        public LowLevelKeyboardHook kHook { get; set; }

        public int offset = 25;

        public Keys PlayKey = Keys.F1;
        public Keys OffsetUpKey = Keys.F2;
        public Keys OffsetDownKey = Keys.F3;

        static readonly string KeybindFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keybinds.settings");
        static readonly string OffsetFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bot.settings");

        public BindTarget CurrentBindTarget = BindTarget.None;
        public int CurrentLaneKeyCount { get; private set; }
        public int CurrentLaneIndex { get; private set; } = -1;

        public int DefaultOffset { get; private set; }

        public KeyBot()
        {
            kHook = new LowLevelKeyboardHook();
            try
            {
                if (!File.Exists(OffsetFile))
                    File.WriteAllText(OffsetFile, offset.ToString());
                else
                    offset = Convert.ToInt32(File.ReadAllText(OffsetFile));
            }
            catch
            {
                Form1.WriteToConsole("Failed to load config....");
            }

            DefaultOffset = offset;
            LoadKeybinds();
        }

        void LoadKeybinds()
        {
            PlayKey = Keys.F1;
            OffsetUpKey = Keys.F2;
            OffsetDownKey = Keys.F3;

            if (!File.Exists(KeybindFile))
            {
                SaveKeybinds();
                return;
            }

            var lines = File.ReadAllLines(KeybindFile);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                var name = parts[0].Trim();
                var val = parts[1].Trim();

                if (!Enum.TryParse(val, true, out Keys k)) continue;

                if (string.Equals(name, "Play", StringComparison.OrdinalIgnoreCase))
                    PlayKey = k;
                else if (string.Equals(name, "OffsetUp", StringComparison.OrdinalIgnoreCase))
                    OffsetUpKey = k;
                else if (string.Equals(name, "OffsetDown", StringComparison.OrdinalIgnoreCase))
                    OffsetDownKey = k;
            }
        }

        public void SaveKeybinds()
        {
            var lines = new[]
            {
                "Play=" + PlayKey,
                "OffsetUp=" + OffsetUpKey,
                "OffsetDown=" + OffsetDownKey
            };
            File.WriteAllLines(KeybindFile, lines);
        }

        public void ResetKeybinds()
        {
            PlayKey = Keys.F1;
            OffsetUpKey = Keys.F2;
            OffsetDownKey = Keys.F3;
            CurrentBindTarget = BindTarget.None;
            CurrentLaneIndex = -1;
            SaveKeybinds();
        }

        public void BeginBind(BindTarget target)
        {
            CurrentLaneIndex = -1;
            CurrentBindTarget = target;
            Form1.WriteToConsole("Press a key to bind " + target + ".");
        }

        public void BeginLaneBind(int keyCount, int laneIndex)
        {
            CurrentLaneKeyCount = keyCount;
            CurrentLaneIndex = laneIndex;
            CurrentBindTarget = BindTarget.Lane;
            Form1.WriteToConsole("Press a key for " + keyCount + "K lane " + (laneIndex + 1) + ".");
        }

        public void ResetOffset()
        {
            offset = DefaultOffset;
            try
            {
                File.WriteAllText(OffsetFile, offset.ToString());
            }
            catch
            {
            }

            if (Form1.offset != null)
                Form1.offset.Text = "Offset: " + offset;
            Form1.WriteToConsole("Offset reset to " + offset + ".");
        }

        public void InitHooks()
        {
            kHook.OnKeyPressed += (sender, keys) =>
            {
                if (CurrentBindTarget != BindTarget.None)
                {
                    BindTarget target = CurrentBindTarget;
                    CurrentBindTarget = BindTarget.None;

                    if (target == BindTarget.Lane)
                    {
                        if (Form1.Instance != null && Form1.Instance.bot != null)
                        {
                            Form1.Instance.bot.SetLaneKey(CurrentLaneKeyCount, CurrentLaneIndex, keys);
                            Form1.Instance.RefreshLaneKeyButtons();
                        }

                        Form1.WriteToConsole("Bound " + CurrentLaneKeyCount + "K lane " + (CurrentLaneIndex + 1) + " to " + keys + ".");
                        CurrentLaneIndex = -1;
                        return;
                    }

                    switch (target)
                    {
                        case BindTarget.Play:
                            PlayKey = keys;
                            break;
                        case BindTarget.OffsetUp:
                            OffsetUpKey = keys;
                            break;
                        case BindTarget.OffsetDown:
                            OffsetDownKey = keys;
                            break;
                    }

                    SaveKeybinds();
                    if (Form1.Instance != null)
                        Form1.Instance.UpdateKeybindLabels();
                    Form1.WriteToConsole("Bound " + keys + ".");
                    return;
                }

                if (keys == Keys.F1 && PlayKey != Keys.F1)
                    return;

                if (keys == PlayKey)
                {
                    if (!Bot.SongLoaded)
                    {
                        Form1.WriteToConsole("Please Select A Song First");
                        return;
                    }

                    Bot.Playing = !Bot.Playing;
                    Form1.WriteToConsole("Playing: " + Bot.Playing);
                }
                else if (keys == OffsetUpKey)
                {
                    offset++;
                    Form1.WriteToConsole("Offset: " + offset);
                    Form1.offset.Text = "Offset: " + offset;
                }
                else if (keys == OffsetDownKey)
                {
                    offset--;
                    Form1.WriteToConsole("Offset: " + offset);
                    Form1.offset.Text = "Offset: " + offset;
                }
            };
            kHook.HookKeyboard();
        }

        public void StopHooks()
        {
            kHook.UnHookKeyboard();
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYUP = 0x0002;

        private static bool IsExtendedVirtualKey(int virtualKey)
        {
            switch ((Keys)virtualKey)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Insert:
                case Keys.Delete:
                case Keys.Home:
                case Keys.End:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.RControlKey:
                case Keys.RMenu:
                case Keys.NumLock:
                case Keys.Divide:
                    return true;
                default:
                    return false;
            }
        }

        public void KeyDown(int virtualKey)
        {
            byte key = unchecked((byte)virtualKey);
            byte scan = unchecked((byte)MapVirtualKey((uint)key, 0));
            int flags = IsExtendedVirtualKey(virtualKey) ? KEYEVENTF_EXTENDEDKEY : 0;
            keybd_event(key, scan, flags, 0);
        }

        public void KeyUp(int virtualKey)
        {
            byte key = unchecked((byte)virtualKey);
            byte scan = unchecked((byte)MapVirtualKey((uint)key, 0));
            int flags = KEYEVENTF_KEYUP;
            if (IsExtendedVirtualKey(virtualKey))
                flags |= KEYEVENTF_EXTENDEDKEY;
            keybd_event(key, scan, flags, 0);
        }

        public void KeyPress(byte key, byte scan)
        {
            KeyDown(key);
            Thread.Sleep(25);
            KeyUp(key);
        }
    }

    public class LowLevelKeyboardHook
    {
        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYUP = 0x105;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<Keys> OnKeyPressed;
        public event EventHandler<Keys> OnKeyUnpressed;

        LowLevelKeyboardProc _proc;
        IntPtr _hookID = IntPtr.Zero;

        public LowLevelKeyboardHook()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookID = SetHook(_proc);
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
        }

        IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                OnKeyPressed?.Invoke(this, (Keys)vkCode);
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}

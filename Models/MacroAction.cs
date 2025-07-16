using System;
using System.Runtime.InteropServices;

namespace AutoClicker
{
    // Macro action enumeration
    public enum MacroActionType
    {
        MouseMove,
        LeftClick,
        RightClick,
        Wait
    }

    // Macro action structure
    public struct MacroAction
    {
        public MacroActionType Type;
        public int X, Y;
        public int DelayMs;
        
        public override string ToString()
        {
            return Type switch
            {
                MacroActionType.MouseMove => $"Siirrä hiirtä ({X}, {Y})",
                MacroActionType.LeftClick => $"Vasen klikkaus ({X}, {Y})",
                MacroActionType.RightClick => $"Oikea klikkaus ({X}, {Y})",
                MacroActionType.Wait => $"Odota {DelayMs}ms",
                _ => "Tuntematon toiminto"
            };
        }
    }

    // Windows API Point structure
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    // Windows API Mouse Hook structure
    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    // Windows API Input structures
    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public InputUnion union;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    // Windows API Constants
    public static class WinAPIConstants
    {
        // Input types
        public const uint INPUT_MOUSE = 0;
        public const uint INPUT_KEYBOARD = 1;

        // Mouse event flags
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        // Keyboard event flags
        public const uint KEYEVENTF_KEYUP = 0x0002;

        // Mouse hook constants
        public const int WH_MOUSE_LL = 14;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_RBUTTONDOWN = 0x0204;

        // Hotkey constants
        public const int HOTKEY_ID_START_STOP = 1;
        public const int HOTKEY_ID_RECORD = 2;
        public const int HOTKEY_ID_PLAY_MACRO = 3;
        public const uint MOD_NONE = 0x0000;
        public const uint VK_F1 = 0x70;
        public const uint VK_F2 = 0x71;
        public const uint VK_F3 = 0x72;
        public const int WM_HOTKEY = 0x0312;
    }
}
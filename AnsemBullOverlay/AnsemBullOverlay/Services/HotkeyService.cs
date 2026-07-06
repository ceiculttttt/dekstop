using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using AnsemBullOverlay.Utils;

namespace AnsemBullOverlay.Services;

/// <summary>
/// Registers global (system-wide) hotkeys using RegisterHotKey. Attaches to a host WPF window
/// so the WM_HOTKEY message reaches this process.
/// </summary>
public sealed class HotkeyService : IDisposable
{
    private readonly Window _host;
    private readonly HwndSource _source;
    private readonly Dictionary<int, Action> _actions = new();
    private int _nextId = 9000;

    public HotkeyService(Window host)
    {
        _host = host;
        var helper = new WindowInteropHelper(host);
        var hwnd = helper.EnsureHandle();
        _source = HwndSource.FromHwnd(hwnd) ?? throw new InvalidOperationException("Unable to hook window.");
        _source.AddHook(WndProc);
    }

    /// <summary>Parses strings like "Ctrl+Shift+B" and registers them.</summary>
    public bool Register(string combo, Action callback)
    {
        if (!TryParse(combo, out var mods, out var vk)) return false;
        int id = _nextId++;
        if (!Win32Interop.RegisterHotKey(_source.Handle, id, mods | Win32Interop.MOD_NOREPEAT, vk))
            return false;
        _actions[id] = callback;
        return true;
    }

    private static bool TryParse(string combo, out uint mods, out uint vk)
    {
        mods = 0; vk = 0;
        if (string.IsNullOrWhiteSpace(combo)) return false;
        var parts = combo.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var raw in parts)
        {
            var p = raw.ToLowerInvariant();
            switch (p)
            {
                case "ctrl":
                case "control": mods |= Win32Interop.MOD_CONTROL; break;
                case "shift": mods |= Win32Interop.MOD_SHIFT; break;
                case "alt": mods |= Win32Interop.MOD_ALT; break;
                case "win": mods |= Win32Interop.MOD_WIN; break;
                default:
                    if (Enum.TryParse<Key>(raw, true, out var k))
                        vk = (uint)KeyInterop.VirtualKeyFromKey(k);
                    else if (raw.Length == 1)
                        vk = (uint)KeyInterop.VirtualKeyFromKey((Key)Enum.Parse(typeof(Key), raw.ToUpperInvariant()));
                    break;
            }
        }
        return vk != 0;
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == Win32Interop.WM_HOTKEY && _actions.TryGetValue((int)wParam, out var action))
        {
            action();
            handled = true;
        }
        return nint.Zero;
    }

    public void Dispose()
    {
        foreach (var id in _actions.Keys)
            Win32Interop.UnregisterHotKey(_source.Handle, id);
        _actions.Clear();
        _source.RemoveHook(WndProc);
    }
}

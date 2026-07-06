using System.Runtime.InteropServices;

namespace AnsemBullOverlay.Services;

/// <summary>
/// Low-level global mouse hook. Fires <see cref="MouseDown"/> for any left-click anywhere on
/// the desktop – used to spawn click explosions even though our overlay is click-through.
/// </summary>
public sealed class GlobalMouseHook : IDisposable
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;

    private readonly LowLevelMouseProc _proc;
    private readonly nint _hookId;

    public event Action<int, int>? MouseDown;

    public GlobalMouseHook()
    {
        _proc = HookCallback;
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc,
            GetModuleHandle(curModule.ModuleName!), 0);
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && ((int)wParam == WM_LBUTTONDOWN || (int)wParam == WM_RBUTTONDOWN))
        {
            var mhs = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            MouseDown?.Invoke(mhs.pt.x, mhs.pt.y);
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose() => UnhookWindowsHookEx(_hookId);

    // --- P/Invoke ---
    private delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int x; public int y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public nint dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint GetModuleHandle(string lpModuleName);
}

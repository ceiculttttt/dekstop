using System;
using System.Threading;
using System.Windows;
using AnsemBullOverlay.Services;
using AnsemBullOverlay.ViewModels;
using AnsemBullOverlay.Views;
using Point = System.Windows.Point;

namespace AnsemBullOverlay;

public partial class App : Application
{
    private static Mutex? _singleInstanceMutex;

    public static ConfigService ConfigService { get; private set; } = null!;
    public static TrayService TrayService { get; private set; } = null!;
    public static HotkeyService HotkeyService { get; private set; } = null!;
    public static GlobalMouseHook MouseHook { get; private set; } = null!;
    public static OverlayViewModel OverlayVm { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(true, "AnsemBullOverlay.SingleInstance", out bool isFirst);
        if (!isFirst) { Shutdown(); return; }

        System.Windows.Media.RenderOptions.ProcessRenderMode =
            System.Windows.Interop.RenderMode.Default;

        ConfigService = new ConfigService();
        var config = ConfigService.Load();

        OverlayVm = new OverlayViewModel(config);

        var overlays = OverlayWindow.CreateForAllMonitors(OverlayVm);
        foreach (var w in overlays) w.Show();

        HotkeyService = new HotkeyService(overlays[0]);
        HotkeyService.Register(config.ChargeHotkey, () => OverlayVm.TriggerCharge());
        HotkeyService.Register(config.RocketHotkey, () => OverlayVm.TriggerRocket());
        HotkeyService.Register(config.ToggleHotkey, () => OverlayVm.ToggleVisibility());
        HotkeyService.Register(config.TalkHotkey,   () => OverlayVm.SayRandom());

        TrayService = new TrayService(
            onOpenConfig: ShowConfig,
            onToggle: () => OverlayVm.ToggleVisibility(),
            onCharge: () => OverlayVm.TriggerCharge(),
            onRocket: () => OverlayVm.TriggerRocket(),
            onTalk:   () => OverlayVm.SayRandom(),
            onExit: Shutdown);
        TrayService.Show();

        MouseHook = new GlobalMouseHook();
        MouseHook.MouseDown += (x, y) =>
        {
            Dispatcher.Invoke(() =>
            {
                var world = new Point(x, y);
                if (OverlayVm.IsBullHit(world))
                    OverlayVm.SayRandom();
                else
                    OverlayVm.ClickExplosion(world);
            });
        };

        if (config.AutoStart) AutoStartService.Enable();
    }

    private void ShowConfig()
    {
        var existing = Current.Windows.OfType<ConfigWindow>().FirstOrDefault();
        if (existing != null) { existing.Activate(); return; }
        var win = new ConfigWindow
        {
            DataContext = new ConfigViewModel(ConfigService, OverlayVm)
        };
        win.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            HotkeyService?.Dispose();
            TrayService?.Dispose();
            MouseHook?.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
        }
        catch { }
        base.OnExit(e);
    }
}

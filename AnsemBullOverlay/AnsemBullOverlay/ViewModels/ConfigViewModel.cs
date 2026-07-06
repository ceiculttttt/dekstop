using System.Windows.Input;
using AnsemBullOverlay.Models;
using AnsemBullOverlay.Services;
using AnsemBullOverlay.Utils;

namespace AnsemBullOverlay.ViewModels;

public sealed class ConfigViewModel : ViewModelBase
{
    private readonly ConfigService _service;
    private readonly OverlayViewModel _overlay;

    public AppConfig Config => _overlay.Config;

    public ICommand SaveCommand { get; }
    public ICommand TestChargeCommand { get; }
    public ICommand TestRocketCommand { get; }
    public ICommand TestTalkCommand { get; }
    public ICommand ResetCommand { get; }

    public bool AutoStart
    {
        get => Config.AutoStart;
        set { Config.AutoStart = value; AutoStartService.Set(value); Raise(); }
    }

    public bool EnableBull
    {
        get => Config.EnableBull;
        set { Config.EnableBull = value; Raise(); }
    }
    public bool EnableCandles
    {
        get => Config.EnableCandles;
        set { Config.EnableCandles = value; Raise(); }
    }
    public bool EnableMouseTrail
    {
        get => Config.EnableMouseTrail;
        set { Config.EnableMouseTrail = value; Raise(); }
    }
    public bool EnableClickExplosion
    {
        get => Config.EnableClickExplosion;
        set { Config.EnableClickExplosion = value; Raise(); }
    }

    public double BullScale
    {
        get => Config.BullScale;
        set { Config.BullScale = value; _overlay.ApplyConfigChanged(); Raise(); }
    }
    public double BullSpeed
    {
        get => Config.BullSpeed;
        set { Config.BullSpeed = value; Raise(); }
    }
    public double GlowIntensity
    {
        get => Config.GlowIntensity;
        set { Config.GlowIntensity = value; _overlay.ApplyConfigChanged(); Raise(); }
    }
    public double ParticleDensity
    {
        get => Config.ParticleDensity;
        set { Config.ParticleDensity = value; _overlay.ApplyConfigChanged(); Raise(); }
    }

    public ConfigViewModel(ConfigService service, OverlayViewModel overlay)
    {
        _service = service;
        _overlay = overlay;

        SaveCommand       = new RelayCommand(() => { _service.Save(Config); _overlay.ApplyConfigChanged(); });
        TestChargeCommand = new RelayCommand(() => _overlay.TriggerCharge());
        TestRocketCommand = new RelayCommand(() => _overlay.TriggerRocket());
        TestTalkCommand   = new RelayCommand(() => _overlay.SayRandom());
        ResetCommand      = new RelayCommand(() =>
        {
            var fresh = new AppConfig();
            foreach (var p in typeof(AppConfig).GetProperties())
                if (p.CanWrite) p.SetValue(Config, p.GetValue(fresh));
            foreach (var p in typeof(ConfigViewModel).GetProperties()) Raise(p.Name);
            _overlay.ApplyConfigChanged();
        });
    }
}

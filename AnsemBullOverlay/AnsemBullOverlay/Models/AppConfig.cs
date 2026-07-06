using System.Text.Json.Serialization;

namespace AnsemBullOverlay.Models;

/// <summary>Persisted user configuration (%AppData%/AnsemBullOverlay/config.json).</summary>
public sealed class AppConfig
{
    public bool AutoStart { get; set; } = false;
    public bool EnableMouseTrail { get; set; } = true;
    public bool EnableCandles { get; set; } = true;
    public bool EnableBull { get; set; } = true;
    public bool EnableClickExplosion { get; set; } = true;

    public double GlowIntensity { get; set; } = 1.0;
    public double ParticleDensity { get; set; } = 1.0;
    public double BullSpeed { get; set; } = 1.0;
    public double BullScale { get; set; } = 1.0;

    public string ChargeHotkey { get; set; } = "Ctrl+Shift+B";
    public string RocketHotkey { get; set; } = "Ctrl+Shift+R";
    public string ToggleHotkey { get; set; } = "Ctrl+Shift+H";
    public string TalkHotkey   { get; set; } = "Ctrl+Shift+T";

    [JsonIgnore]
    public double SafeGlowIntensity => System.Math.Clamp(GlowIntensity, 0, 2);
}

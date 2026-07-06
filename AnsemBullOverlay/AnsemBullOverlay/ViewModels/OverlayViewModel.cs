using System.Windows;
using System.Windows.Media;
using AnsemBullOverlay.Models;
using AnsemBullOverlay.Services;

namespace AnsemBullOverlay.ViewModels;

/// <summary>
/// Simulates the bull mascot + particles and orchestrates cross-window events.
/// </summary>
public sealed class OverlayViewModel : ViewModelBase
{
    private readonly Random _rand = new();

    public AppConfig Config { get; }
    public ParticleEngine Particles { get; } = new();

    // Bull state (in virtual-screen / world coords)
    public double BullX { get; private set; }
    public double BullY { get; private set; }
    public double BullSize => 180 * Config.BullScale;
    public int BullFacing { get; private set; } = 1;
    public bool BullCharging { get; private set; }
    public bool BullRocket { get; private set; }

    public Rect VirtualScreen { get; set; } = new(0, 0, 1920, 1080);

    private double _bullTargetX;
    private double _bullTargetY;
    private double _chargeTimer;
    private double _rocketTimer;
    private double _candleTimer;
    private double _idleWanderTimer;
    private bool _isVisible = true;

    public bool IsVisible
    {
        get => _isVisible;
        private set => SetField(ref _isVisible, value);
    }

    public event Action<double>? Tick;
    public event Action<string, Point>? ChatBubbleRequested;

    public OverlayViewModel(AppConfig config)
    {
        Config = config;
        Particles.ParticleDensity = config.ParticleDensity;
        Particles.GlowIntensity = config.GlowIntensity;
        BullX = 200;
        BullY = 400;
        _bullTargetX = BullX + 400;
        _bullTargetY = BullY;
    }

    public void SetVirtualScreen(Rect r)
    {
        VirtualScreen = r;
        BullX = r.X + r.Width / 3;
        BullY = r.Y + r.Height * 0.6;
        PickNewWander();
    }

    public void Update(double dt)
    {
        if (Config.EnableBull) UpdateBull(dt);
        if (Config.EnableCandles) UpdateCandles(dt);

        if (_chargeTimer > 0)
        {
            _chargeTimer -= dt;
            if (_chargeTimer <= 0) BullCharging = false;
        }
        if (_rocketTimer > 0)
        {
            _rocketTimer -= dt;
            if (_rocketTimer <= 0) BullRocket = false;
        }

        Particles.Update(dt);
        Tick?.Invoke(dt);
    }

    private void UpdateBull(double dt)
    {
        double speed = 120 * Config.BullSpeed;
        if (BullCharging) speed = 1400;
        if (BullRocket)   speed = 900;

        double dx = _bullTargetX - BullX;
        double dy = _bullTargetY - BullY;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        if (dist < 12)
        {
            _idleWanderTimer -= dt;
            if (_idleWanderTimer <= 0) PickNewWander();
        }
        else
        {
            double nx = dx / dist, ny = dy / dist;
            BullX += nx * speed * dt;
            BullY += ny * speed * dt;
            BullFacing = nx >= 0 ? 1 : -1;
        }

        if (BullCharging || BullRocket)
        {
            int spawn = BullCharging ? 6 : 3;
            for (int i = 0; i < spawn; i++)
            {
                var vel = new Vector((_rand.NextDouble() - 0.5) * 120,
                                     (_rand.NextDouble() - 0.5) * 120);
                Particles.Spawn(new Point(BullX + BullSize * 0.5, BullY + BullSize * 0.7), vel,
                    BullRocket ? ParticleKind.Rocket : ParticleKind.Spark,
                    0.7, 4 + _rand.NextDouble() * 4,
                    System.Windows.Media.Color.FromRgb(0x39, 0xFF, 0x14));
            }
        }
    }

    private void PickNewWander()
    {
        _bullTargetX = VirtualScreen.X + 80 + _rand.NextDouble() * (VirtualScreen.Width - 260);
        _bullTargetY = VirtualScreen.Y + VirtualScreen.Height * 0.45
                       + _rand.NextDouble() * VirtualScreen.Height * 0.4;
        _idleWanderTimer = 0.5 + _rand.NextDouble() * 1.5;
    }

    private void UpdateCandles(double dt)
    {
        _candleTimer -= dt;
        if (_candleTimer <= 0)
        {
            _candleTimer = Math.Max(0.15, 0.9 / Math.Max(0.25, Config.ParticleDensity));
            Particles.SpawnCandle(VirtualScreen.Width, VirtualScreen.Height);
        }
    }

    // --- Public actions ---

    public void TriggerCharge()
    {
        BullCharging = true;
        _chargeTimer = 2.5;
        _bullTargetX = BullX < VirtualScreen.X + VirtualScreen.Width / 2
                       ? VirtualScreen.X + VirtualScreen.Width - 120
                       : VirtualScreen.X + 120;
        _bullTargetY = BullY;
        Particles.Explode(new Point(BullX + BullSize * 0.5, BullY + BullSize * 0.7), 40, ParticleKind.Spark, 480, 1.0);
    }

    public void TriggerRocket()
    {
        BullRocket = true;
        _rocketTimer = 3.0;
        _bullTargetX = BullX + (BullFacing >= 0 ? 800 : -800);
        _bullTargetY = VirtualScreen.Y + VirtualScreen.Height * 0.15;
        Particles.Explode(new Point(BullX + BullSize * 0.5, BullY + BullSize * 0.9), 30, ParticleKind.Rocket, 220, 1.4);
    }

    public void ClickExplosion(Point p)
    {
        if (!Config.EnableClickExplosion) return;
        Particles.Explode(p, 36, ParticleKind.Diamond, 360, 1.1);
    }

    /// <summary>Checks whether the given screen point lies over the bull mascot.</summary>
    public bool IsBullHit(Point worldPos)
    {
        // Slightly shrink the box vs the visual so background transparency doesn't over-trigger.
        double pad = BullSize * 0.15;
        var rect = new Rect(BullX + pad, BullY + pad, BullSize - pad * 2, BullSize - pad * 2);
        return rect.Contains(worldPos);
    }

    /// <summary>Emits a chat bubble anchored above the bull.</summary>
    public void SayRandom()
    {
        var text = BullSayings.Random();
        var anchor = new Point(BullX + BullSize * 0.35, BullY + BullSize * 0.1);
        ChatBubbleRequested?.Invoke(text, anchor);
    }

    public void ToggleVisibility() => IsVisible = !IsVisible;

    public void ApplyConfigChanged()
    {
        Particles.ParticleDensity = Config.ParticleDensity;
        Particles.GlowIntensity = Config.GlowIntensity;
        Raise(nameof(BullSize));
    }
}

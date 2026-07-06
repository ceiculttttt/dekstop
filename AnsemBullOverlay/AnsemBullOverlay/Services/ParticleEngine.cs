using System.Windows;
using System.Windows.Media;
using AnsemBullOverlay.Models;

namespace AnsemBullOverlay.Services;

/// <summary>
/// Owns the particle pool and renders it via a DrawingVisual for GPU-accelerated
/// composition. Update is O(N) per frame with pooled slots (no allocations).
/// </summary>
public sealed class ParticleEngine
{
    private const int Capacity = 1200;
    private readonly Particle[] _particles = new Particle[Capacity];
    private readonly Random _rand = new();

    // Cached geometries – reused every frame to avoid GC churn.
    private readonly Geometry _diamondGeom;
    private readonly Geometry _sparkGeom;
    private readonly Geometry _rocketGeom;
    private readonly Pen _outlinePen;

    // Pre-frozen brush palette (one per kind) – we modulate alpha via PushOpacity.
    private readonly Brush _diamondBrush;
    private readonly Brush _sparkBrush;
    private readonly Brush _rocketBrush;
    private readonly Brush _candleBrush;

    public double ParticleDensity { get; set; } = 1.0;
    public double GlowIntensity { get; set; } = 1.0;

    public ParticleEngine()
    {
        // A stylized diamond (rhombus with cut) roughly 10 units wide.
        var d = new StreamGeometry();
        using (var ctx = d.Open())
        {
            ctx.BeginFigure(new Point(0, -6), true, true);
            ctx.LineTo(new Point(5, 0), true, false);
            ctx.LineTo(new Point(0, 7), true, false);
            ctx.LineTo(new Point(-5, 0), true, false);
        }
        d.Freeze();
        _diamondGeom = d;

        var s = new EllipseGeometry(new Point(0, 0), 2, 2);
        s.Freeze();
        _sparkGeom = s;

        var r = new StreamGeometry();
        using (var ctx = r.Open())
        {
            ctx.BeginFigure(new Point(0, -8), true, true);
            ctx.LineTo(new Point(4, 4), true, false);
            ctx.LineTo(new Point(0, 2), true, false);
            ctx.LineTo(new Point(-4, 4), true, false);
        }
        r.Freeze();
        _rocketGeom = r;

        _outlinePen = new Pen(new SolidColorBrush(Color.FromArgb(220, 57, 255, 20)), 0.8);
        _outlinePen.Freeze();

        _diamondBrush = MakeFrozen(Color.FromRgb(0xB6, 0xFF, 0xF7));
        _sparkBrush   = MakeFrozen(Color.FromRgb(0x39, 0xFF, 0x14));
        _rocketBrush  = MakeFrozen(Color.FromRgb(0x39, 0xFF, 0x14));
        _candleBrush  = MakeFrozen(Color.FromRgb(0x00, 0xE6, 0x76));
    }

    private static Brush MakeFrozen(Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    public int ActiveCount { get; private set; }

    public void Spawn(Point position, Vector velocity, ParticleKind kind,
                      double life = 1.0, double size = 6, Color? color = null,
                      double angularVel = 0)
    {
        for (int i = 0; i < Capacity; i++)
        {
            if (!_particles[i].IsAlive)
            {
                _particles[i] = new Particle
                {
                    Position = position,
                    Velocity = velocity,
                    Rotation = _rand.NextDouble() * 360,
                    AngularVelocity = angularVel,
                    Size = size,
                    LifeSeconds = life,
                    MaxLifeSeconds = life,
                    Color = color ?? Color.FromRgb(0x39, 0xFF, 0x14),
                    Kind = kind
                };
                return;
            }
        }
    }

    /// <summary>Spawn an outward-radiating burst (used on click / bull impact).</summary>
    public void Explode(Point position, int count, ParticleKind kind, double speed = 320, double life = 1.2)
    {
        count = (int)(count * ParticleDensity);
        for (int i = 0; i < count; i++)
        {
            double a = _rand.NextDouble() * Math.PI * 2;
            double v = speed * (0.4 + _rand.NextDouble() * 0.9);
            var vel = new Vector(Math.Cos(a) * v, Math.Sin(a) * v);
            var col = kind == ParticleKind.Diamond
                ? Color.FromRgb(0xB6, 0xFF, 0xF7)
                : Color.FromRgb(0x39, 0xFF, 0x14);
            Spawn(position, vel, kind, life * (0.6 + _rand.NextDouble() * 0.8),
                  4 + _rand.NextDouble() * 6, col, (_rand.NextDouble() - 0.5) * 720);
        }
    }

    /// <summary>Trailing diamond behind moving mouse cursor.</summary>
    public void TrailDiamond(Point pos)
    {
        var vel = new Vector((_rand.NextDouble() - 0.5) * 40, -20 - _rand.NextDouble() * 30);
        Spawn(pos, vel, ParticleKind.Diamond,
              life: 0.9,
              size: 5 + _rand.NextDouble() * 3,
              color: Color.FromRgb(0xB6, 0xFF, 0xF7),
              angularVel: (_rand.NextDouble() - 0.5) * 360);
    }

    /// <summary>Floating upward candlestick (bullish green).</summary>
    public void SpawnCandle(double screenWidth, double screenHeight)
    {
        var x = _rand.NextDouble() * screenWidth;
        var y = screenHeight + 20;
        Spawn(new Point(x, y),
              new Vector((_rand.NextDouble() - 0.5) * 12, -30 - _rand.NextDouble() * 40),
              ParticleKind.Candle,
              life: 6 + _rand.NextDouble() * 3,
              size: 14 + _rand.NextDouble() * 10,
              color: Color.FromRgb(0x00, 0xE6, 0x76));
    }

    public void Update(double dt)
    {
        int active = 0;
        for (int i = 0; i < Capacity; i++)
        {
            ref var p = ref _particles[i];
            if (!p.IsAlive) continue;
            p.LifeSeconds -= dt;
            if (p.LifeSeconds <= 0) { p.LifeSeconds = 0; continue; }

            // physics
            if (p.Kind != ParticleKind.Candle)
                p.Velocity.Y += 260 * dt; // gravity for sparks/diamonds/rockets
            p.Position += p.Velocity * dt;
            p.Rotation += p.AngularVelocity * dt;
            active++;
        }
        ActiveCount = active;
    }

    public void Render(DrawingContext dc)
    {
        for (int i = 0; i < Capacity; i++)
        {
            ref var p = ref _particles[i];
            if (!p.IsAlive) continue;

            double alpha = Math.Clamp(p.LifeRatio, 0, 1);

            Brush brush = p.Kind switch
            {
                ParticleKind.Diamond => _diamondBrush,
                ParticleKind.Spark   => _sparkBrush,
                ParticleKind.Rocket  => _rocketBrush,
                ParticleKind.Candle  => _candleBrush,
                _                    => _sparkBrush
            };

            dc.PushOpacity(alpha);
            dc.PushTransform(new TranslateTransform(p.Position.X, p.Position.Y));
            dc.PushTransform(new RotateTransform(p.Rotation));
            dc.PushTransform(new ScaleTransform(p.Size / 6.0, p.Size / 6.0));

            switch (p.Kind)
            {
                case ParticleKind.Diamond:
                    dc.DrawGeometry(brush, _outlinePen, _diamondGeom);
                    break;
                case ParticleKind.Spark:
                    dc.DrawGeometry(brush, null, _sparkGeom);
                    break;
                case ParticleKind.Rocket:
                    dc.DrawGeometry(brush, _outlinePen, _rocketGeom);
                    break;
                case ParticleKind.Candle:
                    var body = new Rect(-6 * 0.35, -6 * 1.6, 6 * 0.7, 6 * 3.2);
                    dc.DrawRectangle(brush, _outlinePen, body);
                    dc.DrawLine(_outlinePen, new Point(0, -6 * 2.4), new Point(0, 6 * 2.2));
                    break;
            }

            dc.Pop(); // scale
            dc.Pop(); // rotate
            dc.Pop(); // translate
            dc.Pop(); // opacity
        }
    }
}

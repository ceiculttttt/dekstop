using System.Windows;
using System.Windows.Media;

namespace AnsemBullOverlay.Models;

/// <summary>
/// Lightweight particle record used by the particle engine.
/// Value-type semantics for cache-friendly iteration.
/// </summary>
public struct Particle
{
    public Point Position;
    public Vector Velocity;
    public double Rotation;
    public double AngularVelocity;
    public double Size;
    public double LifeSeconds;
    public double MaxLifeSeconds;
    public Color Color;
    public ParticleKind Kind;

    public double LifeRatio => MaxLifeSeconds <= 0 ? 0 : System.Math.Clamp(LifeSeconds / MaxLifeSeconds, 0, 1);
    public bool IsAlive => LifeSeconds > 0;
}

public enum ParticleKind
{
    Diamond,
    Spark,
    Candle,
    Rocket
}

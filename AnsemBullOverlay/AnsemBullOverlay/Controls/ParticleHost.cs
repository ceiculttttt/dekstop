using System.Windows;
using System.Windows.Media;
using AnsemBullOverlay.Services;

namespace AnsemBullOverlay.Controls;

/// <summary>
/// A lightweight FrameworkElement that renders the ParticleEngine via OnRender.
/// Particles live in virtual-screen coordinates; the host translates by
/// <see cref="WorldOffset"/> so each per-monitor overlay draws its portion correctly.
/// </summary>
public sealed class ParticleHost : FrameworkElement
{
    public static readonly DependencyProperty EngineProperty =
        DependencyProperty.Register(nameof(Engine), typeof(ParticleEngine), typeof(ParticleHost),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public ParticleEngine? Engine
    {
        get => (ParticleEngine?)GetValue(EngineProperty);
        set => SetValue(EngineProperty, value);
    }

    public Vector WorldOffset { get; set; }

    public ParticleHost()
    {
        IsHitTestVisible = false;
        SnapsToDevicePixels = false;
        UseLayoutRounding = false;
        RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (Engine is null) return;
        dc.PushTransform(new TranslateTransform(-WorldOffset.X, -WorldOffset.Y));
        Engine.Render(dc);
        dc.Pop();
    }

    public void Invalidate() => InvalidateVisual();
}

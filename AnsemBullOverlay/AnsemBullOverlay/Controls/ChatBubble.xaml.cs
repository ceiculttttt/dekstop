using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AnsemBullOverlay.Controls;

public partial class ChatBubble : UserControl
{
    public ChatBubble()
    {
        InitializeComponent();
        Opacity = 0;
        RenderTransformOrigin = new Point(0.15, 1);
        RenderTransform = new ScaleTransform(0.6, 0.6);
    }

    public string Message
    {
        get => Text.Text;
        set => Text.Text = value;
    }

    /// <summary>Pop-in, hold, then fade out. Removes self from parent when done.
    /// Lifetime auto-scales with the message length so longer lines stay readable.</summary>
    public void Show(Canvas host, double? lifetimeSeconds = null)
    {
        var scale = (ScaleTransform)RenderTransform;

        // Auto lifetime: baseline 5s + ~55ms per character, capped between 5s and 12s.
        int chars = Text.Text?.Length ?? 0;
        double life = lifetimeSeconds ?? Math.Clamp(5.0 + chars * 0.055, 5.0, 12.0);

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(260))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        var popX = new DoubleAnimation(0.6, 1.0, TimeSpan.FromMilliseconds(300))
        { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.7 } };
        var popY = new DoubleAnimation(0.6, 1.0, TimeSpan.FromMilliseconds(300))
        { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.7 } };
        BeginAnimation(OpacityProperty, fadeIn);
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, popX);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, popY);

        var hold = TimeSpan.FromSeconds(Math.Max(1.0, life - 0.9));
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(900))
        { BeginTime = hold };
        fadeOut.Completed += (_, _) =>
        {
            if (Parent is Canvas c) c.Children.Remove(this);
        };
        BeginAnimation(OpacityProperty, fadeOut);
    }
}

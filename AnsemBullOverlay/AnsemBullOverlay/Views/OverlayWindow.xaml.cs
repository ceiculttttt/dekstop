using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AnsemBullOverlay.Controls;
using AnsemBullOverlay.Utils;
using AnsemBullOverlay.ViewModels;
using WinFormsScreen = System.Windows.Forms.Screen;
using WinFormsSysInfo = System.Windows.Forms.SystemInformation;
using WinFormsCursor = System.Windows.Forms.Cursor;
using DrawingRect = System.Drawing.Rectangle;

namespace AnsemBullOverlay.Views;

public partial class OverlayWindow : Window
{
    private readonly OverlayViewModel _vm;
    private readonly DrawingRect _monitorBounds;
    private DateTime _lastFrame = DateTime.Now;
    private DateTime _lastTrail = DateTime.MinValue;
    private bool _isPrimary;
    private Vector _worldOffset;
    private double _bobPhase;

    private static readonly ImageSource CursorTrailImage = LoadCursor();
    private static ImageSource LoadCursor()
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = new Uri("pack://application:,,,/Assets/Bull/bull-cursor.png", UriKind.Absolute);
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    private OverlayWindow(OverlayViewModel vm, DrawingRect monitor)
    {
        InitializeComponent();
        _vm = vm;
        _monitorBounds = monitor;
        DataContext = vm;

        Left = monitor.Left;
        Top = monitor.Top;
        Width = monitor.Width;
        Height = monitor.Height;

        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        CompositionTarget.Rendering += OnRenderFrame;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(OverlayViewModel.IsVisible))
                Visibility = vm.IsVisible ? Visibility.Visible : Visibility.Hidden;
        };

        vm.ChatBubbleRequested += OnChatBubbleRequested;
    }

    /// <summary>Creates one overlay per connected monitor for full multi-monitor support.</summary>
    public static List<OverlayWindow> CreateForAllMonitors(OverlayViewModel vm)
    {
        var list = new List<OverlayWindow>();
        foreach (var screen in WinFormsScreen.AllScreens)
        {
            list.Add(new OverlayWindow(vm, screen.Bounds));
        }
        var virt = WinFormsSysInfo.VirtualScreen;
        vm.SetVirtualScreen(new Rect(virt.X, virt.Y, virt.Width, virt.Height));
        return list;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        Win32Interop.MakeClickThrough(this, true);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _worldOffset = new Vector(Left, Top);
        Particles.Engine = _vm.Particles;
        Particles.WorldOffset = _worldOffset;
    }

    private void OnRenderFrame(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        double dt = (now - _lastFrame).TotalSeconds;
        if (dt <= 0) dt = 1.0 / 60.0;
        if (dt > 0.1) dt = 0.1;
        _lastFrame = now;

        if (IsPrimary())
        {
            SampleGlobalCursorTrail();
            _vm.Update(dt);
        }

        Particles.Invalidate();
        UpdateBullPosition(dt);
    }

    private bool IsPrimary()
    {
        if (_isPrimary) return true;
        var all = System.Windows.Application.Current.Windows.OfType<OverlayWindow>().ToList();
        _isPrimary = all.Count > 0 && ReferenceEquals(all[0], this);
        return _isPrimary;
    }

    private void SampleGlobalCursorTrail()
    {
        if (!_vm.Config.EnableMouseTrail) return;
        var now = DateTime.Now;
        if ((now - _lastTrail).TotalMilliseconds < 50) return;
        _lastTrail = now;

        var p = WinFormsCursor.Position;
        var world = new Point(p.X, p.Y);
        var target = System.Windows.Application.Current.Windows
            .OfType<OverlayWindow>()
            .FirstOrDefault(w => world.X >= w.Left && world.Y >= w.Top &&
                                 world.X < w.Left + w.Width && world.Y < w.Top + w.Height);
        target?.SpawnCursorTrail(new Point(world.X - target.Left, world.Y - target.Top));
    }

    private void SpawnCursorTrail(Point local)
    {
        var img = new Image
        {
            Source = CursorTrailImage,
            Width = 30 + Random.Shared.NextDouble() * 12,
            Height = 30 + Random.Shared.NextDouble() * 12,
            Opacity = 0.9,
            IsHitTestVisible = false,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        img.RenderTransform = new RotateTransform((Random.Shared.NextDouble() - 0.5) * 40);
        Canvas.SetLeft(img, local.X - img.Width / 2);
        Canvas.SetTop(img, local.Y - img.Height / 2);
        CursorTrailLayer.Children.Add(img);

        var fade = new DoubleAnimation(0.9, 0, TimeSpan.FromMilliseconds(750));
        var drift = new DoubleAnimation(local.Y - img.Height / 2,
                                        local.Y - img.Height / 2 - 40,
                                        TimeSpan.FromMilliseconds(750));
        fade.Completed += (_, _) => CursorTrailLayer.Children.Remove(img);
        img.BeginAnimation(OpacityProperty, fade);
        img.BeginAnimation(Canvas.TopProperty, drift);
    }

    private void UpdateBullPosition(double dt)
    {
        double x = _vm.BullX - Left;
        double y = _vm.BullY - Top;
        double size = _vm.BullSize;

        Bull.Width = size;
        Bull.Height = size;

        Canvas.SetLeft(Bull, x);
        Canvas.SetTop(Bull, y);

        // Facing – flip horizontally when moving left.
        BullFlip.CenterX = size / 2;
        BullFlip.CenterY = size / 2;
        BullFlip.ScaleX = _vm.BullFacing >= 0 ? 1 : -1;

        // Bob
        _bobPhase += dt * (_vm.BullCharging ? 14 : 6);
        BullBob.Y = Math.Sin(_bobPhase) * (_vm.BullCharging ? 6 : 3);

        // Glow pulse
        BullGlow.BlurRadius = 22 + (_vm.BullCharging ? 30 : 8) * _vm.Config.GlowIntensity;
        BullGlow.Opacity = Math.Clamp(0.5 + _vm.Config.GlowIntensity * 0.4, 0, 1);

        // Cull
        Bull.Visibility = (x > -size && x < ActualWidth + size &&
                           y > -size && y < ActualHeight + size)
                          ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnChatBubbleRequested(string text, Point worldPos)
    {
        // Only the overlay whose monitor contains the bubble anchor renders it.
        if (worldPos.X < Left || worldPos.X >= Left + Width ||
            worldPos.Y < Top  || worldPos.Y >= Top + Height) return;

        var bubble = new ChatBubble { Message = text };
        // Measure once so we can offset horizontally to keep the tail near the anchor.
        bubble.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double bubbleWidth = bubble.DesiredSize.Width;
        double bubbleHeight = bubble.DesiredSize.Height;

        double localX = worldPos.X - Left - 30;   // tail sits ~30px from left
        double localY = worldPos.Y - Top - bubbleHeight - 20;
        if (localX < 8) localX = 8;
        if (localX + bubbleWidth > ActualWidth - 8) localX = ActualWidth - bubbleWidth - 8;
        if (localY < 8) localY = 8;

        Canvas.SetLeft(bubble, localX);
        Canvas.SetTop(bubble, localY);
        ChatLayer.Children.Add(bubble);
        bubble.Show(ChatLayer);
    }
}

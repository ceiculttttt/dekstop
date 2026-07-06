using System.Drawing;
using System.Windows.Forms;

namespace AnsemBullOverlay.Services;

/// <summary>System tray icon with context menu (WinForms NotifyIcon hosted in WPF).</summary>
public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _icon;

    public TrayService(Action onOpenConfig,
                       Action onToggle,
                       Action onCharge,
                       Action onRocket,
                       Action onTalk,
                       Action onExit)
    {
        _icon = new NotifyIcon
        {
            Icon = BuildIcon(),
            Text = "Ansem Bull Overlay",
            Visible = false
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Bull Charge    (Ctrl+Shift+B)", null, (_, _) => onCharge());
        menu.Items.Add("Rocket Boost   (Ctrl+Shift+R)", null, (_, _) => onRocket());
        menu.Items.Add("Say Something  (Ctrl+Shift+T)", null, (_, _) => onTalk());
        menu.Items.Add("Toggle Overlay (Ctrl+Shift+H)", null, (_, _) => onToggle());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Configure…", null, (_, _) => onOpenConfig());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => onExit());

        _icon.ContextMenuStrip = menu;
        _icon.DoubleClick += (_, _) => onOpenConfig();
    }

    public void Show() => _icon.Visible = true;

    private static Icon BuildIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.Transparent);
            using var body = new SolidBrush(System.Drawing.Color.FromArgb(255, 8, 10, 12));
            using var glow = new SolidBrush(System.Drawing.Color.FromArgb(120, 57, 255, 20));
            using var neon = new SolidBrush(System.Drawing.Color.FromArgb(255, 57, 255, 20));
            g.FillEllipse(glow, 1, 4, 30, 26);
            g.FillEllipse(body, 6, 12, 20, 12);
            g.FillEllipse(body, 2, 8, 12, 12);
            var horns = new PointF[] { new(3, 9), new(0, 2), new(6, 7) };
            g.FillPolygon(neon, horns);
            var horns2 = new PointF[] { new(11, 9), new(14, 2), new(9, 7) };
            g.FillPolygon(neon, horns2);
            g.FillEllipse(neon, 6, 12, 3, 3);
        }
        var hIcon = bmp.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}

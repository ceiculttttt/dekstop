# ANSEM Bull Overlay — WPF Desktop Companion

A neon, always-on-top desktop overlay featuring a **charging black bull mascot** that reacts when you click it, complete with a bull-head cursor trail, floating candlesticks, and rocket boosts.

Built in **C# 12 / .NET 8 WPF** with clean **MVVM**.

---

## ✨ Features

| | |
|---|---|
| **Bull mascot** | Fierce charging bull sprite wanders your desktop, faces movement direction, bobs with each step, glows neon green |
| **Click the bull** | Left-click the mascot anywhere on your desktop → speech bubble pops up with a random line ("You can touch me 😉", "GM, degen.", "Charge it. 🚀", ...) |
| **Bull-head cursor trail** | Small bull-head sprites follow your mouse with fade + drift |
| **Charge** `Ctrl+Shift+B` | Bull dashes across the screen with a spark trail |
| **Rocket boost** `Ctrl+Shift+R` | Launches skyward with green rocket flames |
| **Say something** `Ctrl+Shift+T` | Manual speech bubble trigger |
| **Toggle overlay** `Ctrl+Shift+H` | Hide/show |
| **Floating candlesticks** | Bullish green candles drift up from the bottom |
| **Click explosion** | Left/right click anywhere else spawns a diamond particle burst |
| **Neon glow** with pulse | Dynamic drop-shadow that intensifies during charges |
| **60 FPS** GPU-composited via `CompositionTarget.Rendering` |
| **System tray** with menu + Configure panel |
| **Auto-start** with Windows |
| **Multi-monitor** — one transparent overlay per screen sharing one world |
| **Per-monitor v2 DPI** via manifest |
| **Config panel** — MVVM two-way sliders for scale/speed/glow/density |

---

## 🚀 Simplest way to run

1. Unzip the folder anywhere.
2. Double-click **`Build-Exe.bat`**  
   → Auto-installs .NET 8 SDK (via `winget`) if missing.  
   → Builds a self-contained portable `AnsemBullOverlay.exe`.  
   → Copies it to your **Desktop** and offers to launch.
3. From then on: just double-click the exe on your Desktop.

The .exe is portable — copy it to a USB stick and it runs on any Windows 10/11 PC with **zero installs** after that first build.

---

## 🖥 If you'd rather use Visual Studio

1. Install **Visual Studio 2022** with the *.NET desktop development* workload.
2. Open `AnsemBullOverlay.sln`.
3. Press **F5**.

---

## 🎮 Controls

| | |
|---|---|
| **Click the bull** | Random speech bubble |
| `Ctrl + Shift + B` | Bull charges across the screen |
| `Ctrl + Shift + R` | Rocket boost |
| `Ctrl + Shift + T` | Make the bull talk |
| `Ctrl + Shift + H` | Show / hide overlay |
| Right-click tray 🐂 | Full menu |
| Double-click tray | Configuration panel |
| Right-click tray → Exit | Quit |

---

## 🗂 Architecture (MVVM)

```
AnsemBullOverlay/
├─ App.xaml(.cs)                    # Composition root, single-instance guard
├─ app.manifest                     # PerMonitorV2 DPI
├─ Assets/Bull/                     # bull-mascot.png, bull-cursor.png
├─ Themes/NeonTheme.xaml            # Neon palette, control styles, glow effects
├─ Models/
│  ├─ AppConfig.cs
│  └─ Particle.cs
├─ ViewModels/
│  ├─ ViewModelBase.cs
│  ├─ OverlayViewModel.cs           # Bull sim + chat bubble events
│  └─ ConfigViewModel.cs
├─ Views/
│  ├─ OverlayWindow.xaml(.cs)       # Transparent per-monitor overlay
│  └─ ConfigWindow.xaml(.cs)
├─ Controls/
│  ├─ ChatBubble.xaml(.cs)          # Neon speech bubble w/ pop-in animation
│  └─ ParticleHost.cs
├─ Services/
│  ├─ ConfigService.cs              # JSON load/save
│  ├─ AutoStartService.cs           # HKCU Run key
│  ├─ HotkeyService.cs              # RegisterHotKey P/Invoke
│  ├─ GlobalMouseHook.cs            # WH_MOUSE_LL – detects bull clicks
│  ├─ TrayService.cs
│  ├─ ParticleEngine.cs             # Pooled particles, single-visual render
│  └─ BullSayings.cs                # Random chat lines
└─ Utils/
   ├─ Win32Interop.cs               # Click-through + hotkey helpers
   └─ RelayCommand.cs
```

---

## ⚙️ Where settings live

```
%AppData%\AnsemBullOverlay\config.json
```

---

## 📦 License

MIT — Ansem Bull Labs, 2026 🟢🐂

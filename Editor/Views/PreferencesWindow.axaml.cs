using System;
using System.IO;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml;
using Editor.Localization;
using Editor.Models;
using Editor.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Editor.Views;

public partial class PreferencesWindow : ReactiveWindow<PreferencesWindowViewModel>
{
    private const int GwlStyle = -16;
    private const long WsMinimizeBox = 0x00020000L;
    private const long WsMaximizeBox = 0x00010000L;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    public PreferencesWindow()
    {
        InitializeComponent();
#if DEBUG
        Application.Current!.AttachDeveloperTools();
#endif
#pragma warning disable IL2026
        this.WhenActivated(d =>
        {
            var vm = ViewModel!;

            d.Add(vm.SaveAction.RegisterHandler(Save));
            d.Add(vm.CancelAction.RegisterHandler(Cancel));
        });
#pragma warning restore IL2026
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        var handle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero) return;

        var style = GetWindowLongPtr(handle, GwlStyle);
        SetWindowLongPtr(handle, GwlStyle, style & ~(nint)(WsMinimizeBox | WsMaximizeBox));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Save(IInteractionContext<Preferences, Unit> arg)
    {
        var filePath = "preferences.json";
        using var metaJsonFile = File.Create(filePath);
        var preferences = arg.Input;

        JsonSerializer.Serialize(metaJsonFile, preferences, PreferencesJsonContext.Default.Preferences);

        LocalizationManager.LoadLanguage(preferences.Language);
        Close(preferences);
        arg.SetOutput(Unit.Default);
    }

    private void Cancel(IInteractionContext<Preferences, Unit> arg)
    {
        Close();
        arg.SetOutput(Unit.Default);
    }
}
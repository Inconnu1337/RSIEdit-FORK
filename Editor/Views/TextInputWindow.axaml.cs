using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml;
using Editor.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Editor.Views;

public partial class TextInputWindow : ReactiveWindow<TextInputWindowViewModel>
{
    public TextInputWindow()
    {
        InitializeComponent();
#if DEBUG
        Application.Current!.AttachDeveloperTools();
#endif

#pragma warning disable IL2026
        this.WhenActivated(d =>
        {
            d.Add(ViewModel!.ConfirmAction.RegisterHandler(Confirm));
            d.Add(ViewModel!.DeclineAction.RegisterHandler(Decline));
        });
#pragma warning restore IL2026
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Confirm(IInteractionContext<string, Unit> arg)
    {
        Close(true);
        arg.SetOutput(Unit.Default);
    }

    private void Decline(IInteractionContext<Unit, Unit> arg)
    {
        Close(false);
        arg.SetOutput(Unit.Default);
    }
}
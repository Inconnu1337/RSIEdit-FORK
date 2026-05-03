using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml;
using Editor.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Editor.Views;

public partial class ConfirmationWindow : ReactiveWindow<ConfirmationWindowViewModel>
{
    public ConfirmationWindow()
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

    private void Confirm(IInteractionContext<Unit, Unit> interaction)
    {
        Close(true);
        interaction.SetOutput(Unit.Default);
    }

    private void Decline(IInteractionContext<Unit, Unit> interaction)
    {
        Close(false);
        interaction.SetOutput(Unit.Default);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
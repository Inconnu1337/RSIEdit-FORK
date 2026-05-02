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
        this.WhenActivated(d =>
        {
            d.Add(ViewModel!.ConfirmAction.RegisterHandler(Confirm));
            d.Add(ViewModel!.DeclineAction.RegisterHandler(Decline));
        });
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
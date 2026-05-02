using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml;
using Editor.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Editor.Views;

public partial class TextReplaceWindow : ReactiveWindow<TextReplaceWindowViewModel>
{
    public TextReplaceWindow()
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Confirm(IInteractionContext<(string, string), Unit> arg)
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Editor.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();
#if DEBUG
        Application.Current!.AttachDeveloperTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
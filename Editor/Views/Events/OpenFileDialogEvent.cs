using Avalonia.Interactivity;

namespace Editor.Views.Events;

public class OpenFileDialogEvent : RoutedEventArgs
{
    public string[] Files { get; set; } = [];
}

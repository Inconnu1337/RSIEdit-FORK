using Avalonia.Interactivity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Editor.Views.Events;

public class SaveFileDialogEvent : RoutedEventArgs
{
    public SaveFileDialogEvent(Image<Rgba32> png)
    {
        Png = png;
    }

    public Image<Rgba32> Png { get; }
}

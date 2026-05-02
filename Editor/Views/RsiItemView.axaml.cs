using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Editor.ViewModels;
using Editor.Views.Events;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Editor.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class RsiItemView : ReactiveUserControl<RsiItemViewModel>
{
    public RsiItemView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            d.Add(this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(new AnonymousObserver<RsiItemViewModel?>(vm =>
                {
                    if (vm != null)
                    {
                        d.Add(vm.ImportImageInteraction.RegisterHandler(ImportImage));
                        d.Add(vm.ExportPngInteraction.RegisterHandler(ExportPng));
                        d.Add(vm.ErrorDialog.RegisterHandler(ShowError));
                        d.Add(vm.CloseInteraction.RegisterHandler(Close));
                        d.Add(vm.VisibleStates.Subscribe(new AnonymousObserver<RsiStateViewModel>(s => d.Add(s.Image.Preview))));
                        d.Add(vm);
                    }
                })));
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ShowError(IInteractionContext<ErrorWindowViewModel, Unit> interaction)
    {
        var args = new ShowErrorEvent(interaction.Input) {RoutedEvent = MainWindow.ShowErrorEvent};
        RaiseEvent(args);

        interaction.SetOutput(Unit.Default);
    }

    private async Task ImportImage(IInteractionContext<Unit, string> interaction)
    {
        var topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Image Files")
                {
                    Patterns = RsiItemViewModel.ValidExtensions.Select(e => $"*.{e}").ToList()
                }
            }
        });
        interaction.SetOutput(files.Count > 0 ? files[0].TryGetLocalPath() ?? string.Empty : string.Empty);
    }

    private async Task ExportPng(IInteractionContext<Image<Rgba32>, Unit> interaction)
    {
        if (ViewModel is null)
        {
            interaction.SetOutput(Unit.Default);
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this)!;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = "png",
            SuggestedFileName = ViewModel.SelectedStates[0].Image.State.Name ?? string.Empty,
        });

        if (file != null)
        {
            await using var stream = await file.OpenWriteAsync();
            await interaction.Input.SaveAsPngAsync(stream);
        }

        interaction.SetOutput(Unit.Default);
    }

    private void Close(IInteractionContext<RsiItemViewModel, Unit> interaction)
    {
        var args = new CloseRsiEvent(interaction.Input) {RoutedEvent = MainWindow.CloseRsiEvent};
        RaiseEvent(args);

        interaction.SetOutput(Unit.Default);
    }
}

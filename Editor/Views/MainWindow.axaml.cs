using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Editor.Models;
using Editor.ViewModels;
using Editor.Views.Events;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SixLabors.ImageSharp;

namespace Editor.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        Application.Current!.AttachDeveloperTools();
#endif
        this.WhenActivated(d =>
        {
            var vm = ViewModel!;

            d.Add(vm.NewRsiAction.RegisterHandler(NewRsi));
            d.Add(vm.OpenRsiDialog.RegisterHandler(OpenRsi));
            d.Add(vm.OpenAllInDialog.RegisterHandler(OpenAllIn));
            d.Add(vm.SaveRsiDialog.RegisterHandler(SaveRsi));
            d.Add(vm.ImportImageDialog.RegisterHandler(ImportImage));
            d.Add(vm.ImportDmiDialog.RegisterHandler(ImportDmi));
            d.Add(vm.ImportDmiFolderDialog.RegisterHandler(ImportDmiFolder));
            d.Add(vm.PreferencesAction.RegisterHandler(OpenPreferences));
            d.Add(vm.ErrorDialog.RegisterHandler(ShowError));
            d.Add(vm.ChangeAllLicensesAction.RegisterHandler(ChangeAllLicenses));
            d.Add(vm.ChangeAllCopyrightsAction.RegisterHandler(ChangeAllCopyrights));
            d.Add(vm.ReplaceAllStateNamesAction.RegisterHandler(ReplaceAllStateNames));
        });

        ShowErrorEvent.AddClassHandler<MainWindow>(OnShowError);
        AskConfirmationEvent.AddClassHandler<MainWindow>(OnAskConfirmation);
        CloseRsiEvent.AddClassHandler<MainWindow>(OnCloseRsi);
        GetMainWindowEvent.AddClassHandler<MainWindow>(OnGetMainWindow);

        AddHandler(DragDrop.DropEvent, DropEvent);
    }

    public static RoutedEvent<ShowErrorEvent> ShowErrorEvent { get; } =
        RoutedEvent.Register<MainWindow, ShowErrorEvent>(nameof(ShowErrorEvent), RoutingStrategies.Bubble);

    public static RoutedEvent<AskConfirmationEvent> AskConfirmationEvent { get; } =
        RoutedEvent.Register<MainWindow, AskConfirmationEvent>(nameof(AskConfirmationEvent), RoutingStrategies.Bubble);

    public static RoutedEvent<CloseRsiEvent> CloseRsiEvent { get; } =
        RoutedEvent.Register<MainWindow, CloseRsiEvent>(nameof(CloseRsiEvent), RoutingStrategies.Bubble);

    public static RoutedEvent<GetMainWindowEvent> GetMainWindowEvent { get; } =
        RoutedEvent.Register<MainWindow, GetMainWindowEvent>(nameof(GetMainWindowEvent), RoutingStrategies.Bubble);

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task<bool> TryOpenConfirmation(string text, bool modified = true)
    {
        if (!modified || ViewModel?.CurrentOpenRsi == null)
        {
            return true;
        }

        var newVm = new ConfirmationWindowViewModel(text);
        var confirmed = await new ConfirmationWindow() {ViewModel = newVm}.ShowDialog<bool>(this);

        return confirmed;
    }

    public bool DoNewRsi()
    {
        if (ViewModel == null)
        {
            return false;
        }

        return true;
    }

    private void NewRsi(IInteractionContext<Unit, bool> interaction)
    {
        var confirm = DoNewRsi();
        interaction.SetOutput(confirm);
    }

    private async Task OpenRsi(IInteractionContext<Unit, string?> interaction)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Open RSI", AllowMultiple = false });
        interaction.SetOutput(folders.Count > 0 ? folders[0].TryGetLocalPath() : null);
    }

    private async Task OpenAllIn(IInteractionContext<Unit, string?> interaction)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Open All RSIs", AllowMultiple = false });
        interaction.SetOutput(folders.Count > 0 ? folders[0].TryGetLocalPath() : null);
    }

    private async Task SaveRsi(IInteractionContext<Unit, string?> interaction)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Save RSI", AllowMultiple = false });
        interaction.SetOutput(folders.Count > 0 ? folders[0].TryGetLocalPath() : null);
    }

    private async Task ImportDmi(IInteractionContext<Unit, string?> interaction)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import DMI",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("DMI Files") { Patterns = new List<string> { "*.dmi" } }
            }
        });
        interaction.SetOutput(files.Count > 0 ? files[0].TryGetLocalPath() : string.Empty);
    }

    private async Task ImportImage(IInteractionContext<Unit, string?> interaction)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Image",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Image Files") { Patterns = new List<string> { "*.dmi", "*.gif", "*.png" } }
            }
        });
        interaction.SetOutput(files.Count > 0 ? files[0].TryGetLocalPath() : string.Empty);
    }

    private async Task ImportDmiFolder(IInteractionContext<Unit, string?> interaction)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Convert directory", AllowMultiple = false });
        interaction.SetOutput(folders.Count > 0 ? folders[0].TryGetLocalPath() : null);
    }

    private async Task OpenPreferences(IInteractionContext<Unit, Unit> arg)
    {
        if (ViewModel == null)
        {
            return;
        }

        var vm = new PreferencesWindowViewModel(ViewModel.Preferences);
        var dialog = new PreferencesWindow() {DataContext = vm};
        var preferences = await dialog.ShowDialog<Preferences?>(this);

        if (preferences?.EasterEggs == true)
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://Editor/Assets/joke-logo.ico")));
            Background = new ImageBrush(new Bitmap(AssetLoader.Open(new Uri("avares://Editor/Assets/joke-background.png"))));
        }
        else
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://Editor/Assets/logo.ico")));
            Background = null;
        }

        arg.SetOutput(Unit.Default);
    }

    private async Task ShowError(IInteractionContext<ErrorWindowViewModel, Unit> interaction)
    {
        var dialog = new ErrorWindow {DataContext = interaction.Input};
        await dialog.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private async Task ChangeAllLicenses(IInteractionContext<Unit, string?> arg)
    {
        var vm = new TextInputWindowViewModel("Change all licenses", "Change all open RSI licenses to:");
        var dialog = new TextInputWindow {DataContext = vm};

        if (!await dialog.ShowDialog<bool>(this))
        {
            arg.SetOutput(null);
            return;
        }

        arg.SetOutput(vm.SubmittedText);
    }

    private async Task ChangeAllCopyrights(IInteractionContext<Unit, string?> arg)
    {
        var vm = new TextInputWindowViewModel("Change all copyrights", "Change all open RSI copyrights to:");
        var dialog = new TextInputWindow {DataContext = vm};

        if (!await dialog.ShowDialog<bool>(this))
        {
            arg.SetOutput(null);
            return;
        }

        arg.SetOutput(vm.SubmittedText);
    }

    private async Task ReplaceAllStateNames(IInteractionContext<string, (string, string)?> arg)
    {
        var vm = new TextReplaceWindowViewModel(arg.Input);
        var dialog = new TextReplaceWindow {DataContext = vm};

        if (!await dialog.ShowDialog<bool>(this))
        {
            arg.SetOutput(null);
            return;
        }

        arg.SetOutput((vm.Replace, vm.With));
    }

    private async void DropEvent(object? sender, DragEventArgs e)
    {
        var storageItems = e.DataTransfer.TryGetFiles();
        if (ViewModel == null || storageItems == null)
        {
            return;
        }

        var rsiDmiToOpen = new List<string>();

        foreach (var item in storageItems)
        {
            var path = item.TryGetLocalPath();
            if (path == null) continue;

            if (item is IStorageFolder)
            {
                rsiDmiToOpen.Add(path);
                continue;
            }

            switch (Path.GetExtension(path))
            {
                case ".dmi":
                    rsiDmiToOpen.Add(path);
                    break;
                case ".png":
                    ViewModel.CurrentOpenRsi?.CreateNewState(path);
                    break;
            }
        }

        foreach (var rsiOrDmi in rsiDmiToOpen)
        {
            switch (Path.GetExtension(rsiOrDmi))
            {
                case ".dmi":
                    await ViewModel.ImportImage(rsiOrDmi);
                    break;
                default:
                    await ViewModel.OpenRsi(rsiOrDmi);
                    break;
            }
        }
    }

    private async void OnShowError(MainWindow window, ShowErrorEvent args)
    {
        var dialog = new ErrorWindow {DataContext = args.ViewModel};
        await dialog.ShowDialog(this);
    }

    private void OnAskConfirmation(MainWindow window, AskConfirmationEvent args)
    {
        var dialog = new ConfirmationWindow {DataContext = args.ViewModel};
        args.Confirmed = dialog.ShowDialog<bool>(this).Result;
    }

    private async void OnCloseRsi(MainWindow window, CloseRsiEvent args)
    {
        if (ViewModel != null && await TryOpenConfirmation("Are you sure you want to close the current RSI without saving?", args.ViewModel.Modified))
        {
            ViewModel?.CloseRsi(args.ViewModel);
        }
    }

    private void OnGetMainWindow(MainWindow window, GetMainWindowEvent args)
    {
        args.MainWindow = window;
    }
}

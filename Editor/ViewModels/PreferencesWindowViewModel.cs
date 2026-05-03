using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Editor.Models;
using ReactiveUI;

namespace Editor.ViewModels;

public class PreferencesWindowViewModel : ViewModelBase
{
    private string? _defaultLicense;
    private string? _defaultCopyright;
    private string? _gitHubToken;
    private bool _revealGitHubToken;
    private bool _minifyJson;
    private int _indentation;
    private int _selectedLanguageIndex;

    public PreferencesWindowViewModel(Preferences preferences)
    {
        Preferences = preferences;
        DefaultLicense = Preferences.DefaultLicense;
        DefaultCopyright = Preferences.DefaultCopyright;
        GitHubToken = Preferences.GitHubToken;
        MinifyJson = Preferences.MinifyJson;
        Indentation = Preferences.Indentation;
        _selectedLanguageIndex = preferences.Language == "ru-RU" ? 1 : 0;
    }

    public Preferences Preferences { get; }

    public Interaction<Preferences, Unit> SaveAction { get; } = new();

    public Interaction<Preferences, Unit> CancelAction { get; } = new();

    public string? DefaultLicense
    {
        get => _defaultLicense;
        set
        {
            if (value == string.Empty)
            {
                value = null;
            }

            this.RaiseAndSetIfChanged(ref _defaultLicense, value);
        }
    }

    public string? DefaultCopyright
    {
        get => _defaultCopyright;
        set
        {
            if (value == string.Empty)
            {
                value = null;
            }

            this.RaiseAndSetIfChanged(ref _defaultCopyright, value);
        }
    }

    public string? GitHubToken
    {
        get => _gitHubToken;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = null;
            }

            this.RaiseAndSetIfChanged(ref _gitHubToken, value);
        }
    }

    public bool RevealGitHubToken
    {
        get => _revealGitHubToken;
        set => this.RaiseAndSetIfChanged(ref _revealGitHubToken, value);
    }

    public bool MinifyJson
    {
        get => _minifyJson;
        set => this.RaiseAndSetIfChanged(ref _minifyJson, value);
    }

    public int Indentation
    {
        get => _indentation;
        set => this.RaiseAndSetIfChanged(ref _indentation, value);
    }

    public int SelectedLanguageIndex
    {
        get => _selectedLanguageIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedLanguageIndex, value);
    }

    public async Task Save()
    {
        Preferences.DefaultLicense = DefaultLicense;
        Preferences.DefaultCopyright = DefaultCopyright;
        Preferences.GitHubToken = GitHubToken;
        Preferences.MinifyJson = MinifyJson;
        Preferences.Indentation = Indentation;
        Preferences.Language = SelectedLanguageIndex == 1 ? "ru-RU" : "en-US";

        await SaveAction.Handle(Preferences);
    }

    public async Task Cancel()
    {
        await CancelAction.Handle(Preferences);
    }
}

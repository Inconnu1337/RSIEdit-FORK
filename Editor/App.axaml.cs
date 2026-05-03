using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Editor.Converters;
using Editor.Localization;
using Editor.Models;
using Editor.ViewModels;
using Editor.Views;
using Splat;

namespace Editor;

public class App : Application
{
    private const string OnlineRepositoryLicenses = "https://github.com/space-wizards/RSIEdit/raw/master/Editor/Assets/repository-licenses.json";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private new void RegisterServices()
    {
        Locator.CurrentMutable.RegisterLazySingleton(() =>
        {
            Preferences preferences;
            var filePath = "preferences.json";

            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    preferences = JsonSerializer.Deserialize(json, PreferencesJsonContext.Default.Preferences) ?? new Preferences();
                }
                catch (Exception e)
                {
                    Logger.Sink?.Log(LogEventLevel.Error, "MAIN", null, e.ToString());
                    preferences = new Preferences();
                    File.WriteAllText(filePath, string.Empty);
                }
            }
            else
            {
                preferences = new Preferences();
            }

            return preferences;
        });

        Locator.CurrentMutable.RegisterLazySingleton(() =>
        {
            var repoLicensesFile = AssetLoader.Open(new Uri("avares://Editor/Assets/repository-licenses.json"));
            return ParseRepositoryLicenses(repoLicensesFile);
        });

        Locator.CurrentMutable.RegisterLazySingleton(async () =>
        {
            var http = new HttpClient(new HttpClientHandler { UseProxy = false });
            var response = await http.GetAsync(OnlineRepositoryLicenses);
            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();
            return ParseRepositoryLicenses(stream);
        });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        RegisterServices();

        var prefs = Locator.Current.GetService<Preferences>()
            ?? throw new Exception("Preferences not registered");
        LocalizationManager.LoadLanguage(prefs.Language);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private RepositoryLicenses? ParseRepositoryLicenses(Stream stream)
    {
        var options = new JsonSerializerOptions { Converters = { new ListStringTupleConverter() } };
        var context = new RepoLicensesSourceGenerationContext(options);
        var repoLicensesList = JsonSerializer.Deserialize(stream, context.ListValueTupleStringString);
        if (repoLicensesList == null)
            return null;

        var repoLicenses = new RepositoryLicenses();
        repoLicenses.Repositories.AddRange(repoLicensesList);
        foreach (var (repo, license) in repoLicenses.Repositories)
        {
            repoLicenses.RepositoriesRegex.Add((new Regex(repo), license));
        }

        return repoLicenses;
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless;
using ReactiveUI.Avalonia;
using NUnit.Framework;

namespace Test;

[SetUpFixture]
public class GlobalSetup
{
    public static TestApp? App;

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        var tcs = new TaskCompletionSource<SynchronizationContext>();

        var app = AppBuilder
            .Configure<TestApp>()
            .UsePlatformDetect()
            .UseReactiveUI(_ => { })
            .AfterSetup(builder =>
            {
                App = (TestApp?) builder.Instance;
                tcs.SetResult(SynchronizationContext.Current!);
            })
            .UseHeadless(new Avalonia.Headless.AvaloniaHeadlessPlatformOptions());

        var thread = new Thread(() => app.StartWithClassicDesktopLifetime(Array.Empty<string>()))
        {
            IsBackground = true
        };

        thread.Start();

        SynchronizationContext.SetSynchronizationContext(tcs.Task.Result);
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        await AvaloniaTest.Post(() =>
        {
            App?.Shutdown();
        });
    }
}

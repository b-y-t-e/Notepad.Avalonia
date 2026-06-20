using global::Avalonia;
using global::Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Notepad.Avalonia.Tests.TestAppBuilder))]

namespace Notepad.Avalonia.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public class App : Application
{
}

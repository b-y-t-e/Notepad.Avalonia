using global::Avalonia;
using global::Avalonia.Headless;
using global::Avalonia.Themes.Simple;

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
    // Templated controls (ScrollViewer in particular) need a theme, otherwise they
    // have no presenter and silently do nothing in tests.
    public override void Initialize() => Styles.Add(new SimpleTheme());
}

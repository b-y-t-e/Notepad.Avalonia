using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Markup.Xaml;
using global::Avalonia.Themes.Fluent;

namespace Notepad.Avalonia.Demo;

public class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MvvmWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

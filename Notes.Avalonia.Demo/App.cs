using System.Linq;
using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Markup.Xaml;
using global::Avalonia.Themes.Fluent;

namespace Notes.Avalonia.Demo;

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
            bool mvvm = desktop.Args?.Contains("--mvvm") == true;
            desktop.MainWindow = mvvm ? new MvvmWindow() : new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

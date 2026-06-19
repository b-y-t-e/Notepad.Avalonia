using System;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Media.Imaging;
using global::Avalonia.Platform;
using Notes.Avalonia.Controls;
using Notes.Avalonia.Model;

namespace Notes.Avalonia.Demo;

public class MainWindow : Window
{
    private readonly NoteEditor _editor;
    private readonly TextBlock _statusBar;

    public MainWindow()
    {
        Title = "Note Editor — Demo";
        Width = 800;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _editor = new NoteEditor
        {
            DefaultFont = new FontFamily("Segoe UI"),
            DefaultFontSize = 15,
            Margin = new Thickness(0)
        };

        _statusBar = new TextBlock
        {
            Text = "Ready. Type text. Ctrl+V pastes text/images. Shift+arrows to select.",
            Margin = new Thickness(8, 4),
            FontSize = 12,
            Foreground = Brushes.Gray
        };

        var addButton = new Button
        {
            Content = "+ Add note",
            Margin = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        addButton.Click += (_, _) =>
        {
            _editor.AddItem("");
            _editor.Caret = new CursorPosition(_editor.Document.Items.Count - 1, 0);
            _editor.SelectionAnchor = _editor.Caret;
            _editor.Focus();
            _editor.InvalidateVisual();
        };

        var fontCombo = new ComboBox
        {
            Items = { "Segoe UI", "Consolas", "Arial", "Courier New", "Times New Roman" },
            SelectedIndex = 0,
            Margin = new Thickness(4),
            Width = 160
        };
        fontCombo.SelectionChanged += (_, _) =>
        {
            if (fontCombo.SelectedItem is string fontName)
            {
                _editor.DefaultFont = new FontFamily(fontName);
                _editor.Focus();
            }
        };

        var fontSizeCombo = new ComboBox
        {
            Items = { "12", "14", "16", "18", "20", "24", "28" },
            SelectedIndex = 2,
            Margin = new Thickness(4),
            Width = 70
        };
        fontSizeCombo.SelectionChanged += (_, _) =>
        {
            if (fontSizeCombo.SelectedItem is string sizeStr && double.TryParse(sizeStr, out var size))
            {
                _editor.DefaultFontSize = size;
                _editor.Focus();
            }
        };

        var imageDisplayCombo = new ComboBox
        {
            Items = { "Inline", "Block" },
            SelectedIndex = 1,
            Margin = new Thickness(4),
            Width = 100
        };
        imageDisplayCombo.SelectionChanged += (_, _) =>
        {
            if (imageDisplayCombo.SelectedItem is string mode)
            {
                _editor.ImageDisplay = mode == "Block"
                    ? ImageDisplayMode.Block
                    : ImageDisplayMode.Inline;
                _editor.Focus();
            }
        };

        var themeCombo = new ComboBox
        {
            Items = { "Light", "Dark" },
            SelectedIndex = 0,
            Margin = new Thickness(4),
            Width = 80
        };
        themeCombo.SelectionChanged += (_, _) =>
        {
            if (themeCombo.SelectedItem is string t)
            {
                _editor.ColorTheme = t == "Dark" ? EditorTheme.Dark : EditorTheme.Light;
                _editor.Focus();
            }
        };

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(4),
            Children = { addButton, fontCombo, fontSizeCombo, imageDisplayCombo, themeCombo }
        };

        var scrollViewer = new ScrollViewer
        {
            Content = _editor,
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        var mainPanel = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        DockPanel.SetDock(_statusBar, Dock.Bottom);
        mainPanel.Children.Add(toolbar);
        mainPanel.Children.Add(_statusBar);
        mainPanel.Children.Add(scrollViewer);

        Content = mainPanel;

        var dogBitmap = new Bitmap(AssetLoader.Open(new Uri("avares://Notes.Avalonia.Demo/Assets/dog.png")));
        _editor.ImageStore["dog"] = dogBitmap;

        _editor.Document.Items.Clear();
        _editor.Document.Items.Add(_editor.ParseTextToItem("Meeting notes — discuss Q3 roadmap ![dog](dog)"));
        _editor.Document.Items.Add(new NoteItem("Remember to review the design document"));
        _editor.Document.Items.Add(new NoteItem("Ideas for the new feature: drag & drop, themes, export"));
        _editor.Document.Items.Add(new NoteItem("Paste an image here with Ctrl+V"));
        _editor.Document.Items.Add(_editor.ParseTextToItem("Photo reference: ![dog](dog)"));
        _editor.Document.Items.Add(new NoteItem("Shopping list: bread, milk, eggs, coffee"));
        _editor.InvalidateVisual();
    }
}

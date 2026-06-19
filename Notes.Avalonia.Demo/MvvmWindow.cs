using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Data;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Media.Imaging;
using global::Avalonia.Platform;
using Notes.Avalonia.Controls;

namespace Notes.Avalonia.Demo;

public class MvvmWindow : Window
{
    public MvvmWindow()
    {
        Title = "Note Editor — MVVM Demo";
        Width = 900;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var vm = new NoteViewModel();
        DataContext = vm;

        var editor = new NoteEditor
        {
            DefaultFont = new FontFamily("Segoe UI"),
            DefaultFontSize = 15,
            [!NoteEditor.ItemsProperty] = new Binding("Items")
        };

        editor.ImageStore["star"] = CreateSampleBitmap(Colors.Gold);
        editor.ImageStore["photo"] = CreateSampleBitmap(Colors.LimeGreen);

        editor.ItemsChanged += (_, _) =>
        {
            if (DataContext is NoteViewModel v)
            {
                v.StatusText = $"Editor changed — notes: {v.ItemCount}";
            }
        };

        var addButton = new Button { Content = "+ Add note", Margin = new Thickness(4) };
        addButton[!Button.CommandProperty] = new Binding("AddItemCommand");

        var clearButton = new Button { Content = "Clear all", Margin = new Thickness(4) };
        clearButton[!Button.CommandProperty] = new Binding("ClearAllCommand");

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(4),
            Children = { addButton, clearButton }
        };

        var scrollViewer = new ScrollViewer
        {
            Content = editor,
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        var itemCountText = new TextBlock { Margin = new Thickness(8, 4), FontSize = 12 };
        itemCountText[!TextBlock.TextProperty] = new Binding("ItemCount") { StringFormat = "Notes: {0}" };

        var statusText = new TextBlock
        {
            Margin = new Thickness(8, 4),
            FontSize = 12,
            Foreground = Brushes.Gray
        };
        statusText[!TextBlock.TextProperty] = new Binding("StatusText");

        var statusBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { itemCountText, statusText }
        };

        var vmItemsList = new ListBox
        {
            Width = 250,
            FontSize = 12,
            [!ListBox.ItemsSourceProperty] = new Binding("Items")
        };
        vmItemsList.ItemTemplate = new global::Avalonia.Controls.Templates.FuncDataTemplate<Notes.Avalonia.Model.NoteItemData>(
            (item, _) =>
            {
                var tb = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    MaxWidth = 230,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                tb[!TextBlock.TextProperty] = new Binding("Text");
                return tb;
            });

        var vmPanel = new DockPanel { Width = 250 };
        var vmHeader = new TextBlock
        {
            Text = "ViewModel Notes",
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(8, 4),
            FontSize = 13
        };
        DockPanel.SetDock(vmHeader, Dock.Top);
        vmPanel.Children.Add(vmHeader);
        vmPanel.Children.Add(vmItemsList);

        var editorPanel = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        editorPanel.Children.Add(toolbar);
        editorPanel.Children.Add(scrollViewer);

        var splitPanel = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*, Auto, 250")
        };
        Grid.SetColumn(editorPanel, 0);

        var splitter = new GridSplitter
        {
            Width = 4,
            Background = Brushes.LightGray
        };
        Grid.SetColumn(splitter, 1);
        Grid.SetColumn(vmPanel, 2);

        splitPanel.Children.Add(editorPanel);
        splitPanel.Children.Add(splitter);
        splitPanel.Children.Add(vmPanel);

        var mainPanel = new DockPanel();
        DockPanel.SetDock(statusBar, Dock.Bottom);
        mainPanel.Children.Add(statusBar);
        mainPanel.Children.Add(splitPanel);

        Content = mainPanel;
    }

    private static WriteableBitmap CreateSampleBitmap(Color bgColor)
    {
        var bmp = new WriteableBitmap(
            new PixelSize(32, 32),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using (var buf = bmp.Lock())
        {
            unsafe
            {
                byte* ptr = (byte*)buf.Address;
                byte r = bgColor.R, g = bgColor.G, b = bgColor.B;
                for (int i = 0; i < 32 * 32; i++)
                {
                    ptr[i * 4 + 0] = b;
                    ptr[i * 4 + 1] = g;
                    ptr[i * 4 + 2] = r;
                    ptr[i * 4 + 3] = 255;
                }
            }
        }

        return bmp;
    }
}

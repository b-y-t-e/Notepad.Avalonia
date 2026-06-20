using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using global::Avalonia;
using global::Avalonia.Media;
using global::Avalonia.Media.Imaging;
using global::Avalonia.Platform;
using Notepad.Avalonia.Model;

namespace Notepad.Avalonia.Demo;

public class DemoViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ImageEntry> Images { get; } = new();

    private string? _markdownText;
    public string? MarkdownText
    {
        get => _markdownText;
        set { if (_markdownText != value) { _markdownText = value; OnPropertyChanged(); UpdateStatus(); } }
    }

    private string _statusText = "MVVM Demo — edit the note, changes sync with ViewModel.";
    public string StatusText
    {
        get => _statusText;
        set { if (_statusText != value) { _statusText = value; OnPropertyChanged(); } }
    }

    public DemoViewModel()
    {
        var dogBitmap = new Bitmap(
            AssetLoader.Open(
                new Uri("avares://Notepad.Avalonia.Demo/Assets/dog.png")));

        Images.Add(new ImageEntry("dog", dogBitmap));
        Images.Add(new ImageEntry("star", CreateSolidBitmap(Colors.Gold)));
        Images.Add(new ImageEntry("check", CreateSolidBitmap(Colors.LimeGreen)));

        MarkdownText = "Morning walk in the park ![dog](dog)\nDesign ideas for the dashboard\nImages: ![star](star) great ![check](check) done\nBrush that shiny black coat ![dog](dog)\nPaste an image here (Ctrl+V)";
    }

    private static WriteableBitmap CreateSolidBitmap(Color color)
    {
        const int size = 32;
        var bmp = new WriteableBitmap(
            new PixelSize(size, size),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var buf = bmp.Lock();
        var pixels = new byte[size * size * 4];
        for (int i = 0; i < size * size; i++)
        {
            pixels[i * 4 + 0] = color.B;
            pixels[i * 4 + 1] = color.G;
            pixels[i * 4 + 2] = color.R;
            pixels[i * 4 + 3] = 255;
        }
        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, buf.Address, pixels.Length);

        return bmp;
    }

    private void UpdateStatus()
    {
        var lineCount = string.IsNullOrEmpty(_markdownText) ? 0 : _markdownText.Split('\n').Length;
        StatusText = $"Lines: {lineCount}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

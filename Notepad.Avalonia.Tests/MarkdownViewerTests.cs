using System.Collections.ObjectModel;
using System.Linq;
using global::Avalonia;
using global::Avalonia.Headless.NUnit;
using global::Avalonia.Media;
using global::Avalonia.Media.Imaging;
using global::Avalonia.Platform;
using NUnit.Framework;
using Notepad.Avalonia.Controls;
using Notepad.Avalonia.Model;

namespace Notepad.Avalonia.Tests;

[TestFixture]
public class MarkdownViewerTests
{
    private static MarkdownViewer CreateViewer(string markdown, double width = 600, double height = 400)
    {
        var viewer = new MarkdownViewer { MarkdownText = markdown };
        Layout(viewer, width, height);
        return viewer;
    }

    private static void Layout(MarkdownViewer viewer, double width, double height)
    {
        viewer.Measure(new Size(width, height));
        viewer.Arrange(new Rect(0, 0, width, height));
    }

    [AvaloniaTest]
    public void EmptyViewerHasEmptyText()
    {
        var viewer = CreateViewer(string.Empty);
        Assert.That(viewer.PlainText, Is.EqualTo(string.Empty));
        Assert.That(viewer.HasSelection, Is.False);
        Assert.That(viewer.SelectedText, Is.EqualTo(string.Empty));
    }

    [AvaloniaTest]
    public void PlainTextStripsMarkup()
    {
        var viewer = CreateViewer("# Title\n\nSome **bold** and `code`.");

        Assert.That(viewer.PlainText, Is.EqualTo("Title\n\nSome bold and code."));
    }

    [AvaloniaTest]
    public void PlainTextIncludesListMarkers()
    {
        var viewer = CreateViewer("- one\n- two");

        Assert.That(viewer.PlainText, Is.EqualTo("• one\n• two"));
    }

    [AvaloniaTest]
    public void TableCellsAreTabSeparated()
    {
        var viewer = CreateViewer("| a | b |\n|---|---|\n| 1 | 2 |");

        Assert.That(viewer.PlainText, Is.EqualTo("a\tb\n1\t2"));
    }

    [AvaloniaTest]
    public void SelectAllSelectsWholeDocument()
    {
        var viewer = CreateViewer("# Title\n\nBody text");

        viewer.SelectAll();

        Assert.That(viewer.HasSelection);
        Assert.That(viewer.SelectedText, Is.EqualTo(viewer.PlainText));
    }

    [AvaloniaTest]
    public void ClearSelectionRemovesSelection()
    {
        var viewer = CreateViewer("Body text");
        viewer.SelectAll();

        viewer.ClearSelection();

        Assert.That(viewer.HasSelection, Is.False);
    }

    [AvaloniaTest]
    public void SelectionSpansMultipleBlocks()
    {
        var viewer = CreateViewer("First\n\nSecond");
        viewer.SelectAll();

        Assert.That(viewer.SelectedText, Is.EqualTo("First\n\nSecond"));
        Assert.That(viewer.SelectedText, Does.Contain("\n"));
    }

    [AvaloniaTest]
    public void SelectionChangedFiresOnSelectAll()
    {
        var viewer = CreateViewer("Body text");
        int raised = 0;
        viewer.SelectionChanged += (_, _) => raised++;

        viewer.SelectAll();

        Assert.That(raised, Is.EqualTo(1));
    }

    [AvaloniaTest]
    public void ChangingMarkdownRebuildsContent()
    {
        var viewer = CreateViewer("first");
        Assert.That(viewer.PlainText, Is.EqualTo("first"));

        viewer.MarkdownText = "# second";
        Layout(viewer, 600, 400);

        Assert.That(viewer.PlainText, Is.EqualTo("second"));
    }

    [AvaloniaTest]
    public void SelectionIsClampedWhenContentShrinks()
    {
        var viewer = CreateViewer("a long paragraph of text");
        viewer.SelectAll();

        viewer.MarkdownText = "x";
        Layout(viewer, 600, 400);

        Assert.That(viewer.SelectedText.Length, Is.LessThanOrEqualTo(viewer.PlainText.Length));
    }

    [AvaloniaTest]
    public void ImageAltTextIsUsedWhenImageIsMissing()
    {
        var viewer = CreateViewer("![a dog](dog)");

        Assert.That(viewer.PlainText, Is.EqualTo("a dog"));
    }

    [AvaloniaTest]
    public void BoundImageIsResolved()
    {
        var images = new ObservableCollection<ImageEntry>
        {
            new("dog", CreateBitmap())
        };
        var viewer = new MarkdownViewer { MarkdownText = "![](dog)", Images = images };
        Layout(viewer, 600, 400);

        // The object-replacement char marks a resolved, rendered image.
        Assert.That(viewer.PlainText, Is.EqualTo("￼"));
    }

    [AvaloniaTest]
    public void ImageResolverIsUsedAsFallback()
    {
        var bitmap = CreateBitmap();
        var viewer = new MarkdownViewer
        {
            MarkdownText = "![](anything)",
            ImageResolver = _ => bitmap
        };
        Layout(viewer, 600, 400);

        Assert.That(viewer.PlainText, Is.EqualTo("￼"));
    }

    [AvaloniaTest]
    public void LongDocumentRendersWithoutError()
    {
        var markdown = string.Join("\n\n", System.Linq.Enumerable.Range(0, 200)
            .Select(i => $"## Section {i}\n\nParagraph {i} with **bold**, `code` and a [link](https://example.com/{i})."));

        var viewer = CreateViewer(markdown, 500, 300);

        Assert.That(viewer.PlainText, Does.Contain("Section 199"));
        Assert.That(() => Layout(viewer, 320, 300), Throws.Nothing);
    }

    [AvaloniaTest]
    public void NarrowWidthWrapsWithoutLosingText()
    {
        var viewer = CreateViewer("A fairly long paragraph that must wrap onto several lines when narrow.", 120, 400);

        Assert.That(viewer.PlainText,
            Is.EqualTo("A fairly long paragraph that must wrap onto several lines when narrow."));
    }

    [AvaloniaTest]
    public void ThemeSwitchUpdatesBrushes()
    {
        var viewer = CreateViewer("text");

        viewer.ColorTheme = EditorTheme.Dark;

        Assert.That(((SolidColorBrush)viewer.BackgroundBrush).Color, Is.EqualTo(Color.FromRgb(30, 30, 30)));
    }

    private static Bitmap CreateBitmap()
    {
        var pixelSize = new PixelSize(16, 16);
        var bitmap = new WriteableBitmap(pixelSize, new Vector(96, 96),
            PixelFormat.Bgra8888, AlphaFormat.Premul);
        return bitmap;
    }
}

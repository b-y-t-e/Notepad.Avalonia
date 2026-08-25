using System;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Headless;
using global::Avalonia.Headless.NUnit;
using global::Avalonia.Input;
using global::Avalonia.Threading;
using NUnit.Framework;
using Notepad.Avalonia.Controls;

namespace Notepad.Avalonia.Tests;

/// <summary>
/// A control placed inside an outer ScrollViewer must not make it jump when the
/// user clicks to select, while keyboard navigation must still scroll it into
/// view and an explicit BringIntoView() must keep working.
/// </summary>
[TestFixture]
public class BringIntoViewTests
{
    private const string LongMarkdown =
        "# Heading\n\n" +
        "Paragraph text that is long enough to give the control real height.\n\n" +
        "- one\n- two\n- three\n\n" +
        "Another paragraph so the control is taller than the scroll viewport.\n";

    private const double LeadingHeight = 200;
    private const double StartOffset = 100;

    // Target sits below a 200px filler, so at offset 100 its top is 100px down the
    // 300px window: visible enough to click, tall enough that bring-into-view scrolls.
    private static (Window window, ScrollViewer scroll, T target) Host<T>(T target) where T : Control
    {
        var stack = new StackPanel
        {
            Children =
            {
                new Border { Height = LeadingHeight },
                target,
                new Border { Height = 800 }
            }
        };

        var scroll = new ScrollViewer { Content = stack };
        var window = new Window { Width = 500, Height = 300, Content = scroll };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, scroll, target);
    }

    private static double ScrollTo(ScrollViewer scroll, double y)
    {
        scroll.Offset = new Vector(0, y);
        Dispatcher.UIThread.RunJobs();
        return scroll.Offset.Y;
    }

    private static void ClickInside(Window window)
    {
        var point = new Point(10, LeadingHeight - StartOffset + 10);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void ScrollViewerActuallyScrolls()
    {
        // Guards every other test here: a ScrollViewer that cannot scroll would
        // make the "did not jump" assertions pass vacuously.
        var (_, scroll, _) = Host(new MarkdownViewer { MarkdownText = LongMarkdown });

        Assert.That(ScrollTo(scroll, StartOffset), Is.EqualTo(StartOffset).Within(0.5));
    }

    [AvaloniaTest]
    public void ViewerKeyboardFocusStillScrollsIntoView()
    {
        var (_, scroll, viewer) = Host(new MarkdownViewer { MarkdownText = LongMarkdown });
        double before = ScrollTo(scroll, StartOffset);

        viewer.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();

        Assert.That(Math.Abs(scroll.Offset.Y - before), Is.GreaterThan(1),
            "Tab navigation must still bring the control into view");
    }

    [AvaloniaTest]
    public void EditorKeyboardFocusStillScrollsIntoView()
    {
        var editor = new NoteEditor();
        editor.SetText(LongMarkdown);
        var (_, scroll, _) = Host(editor);
        double before = ScrollTo(scroll, StartOffset);

        editor.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();

        Assert.That(Math.Abs(scroll.Offset.Y - before), Is.GreaterThan(1),
            "Tab navigation must still bring the control into view");
    }

    [AvaloniaTest]
    public void ViewerClickDoesNotScrollOuterViewer()
    {
        var (window, scroll, viewer) = Host(new MarkdownViewer { MarkdownText = LongMarkdown });
        double before = ScrollTo(scroll, StartOffset);

        ClickInside(window);

        Assert.That(viewer.IsFocused, Is.True, "the click must still focus the control");
        Assert.That(scroll.Offset.Y, Is.EqualTo(before).Within(0.5));
    }

    [AvaloniaTest]
    public void EditorClickDoesNotScrollOuterViewer()
    {
        var editor = new NoteEditor();
        editor.SetText(LongMarkdown);
        var (window, scroll, _) = Host(editor);
        double before = ScrollTo(scroll, StartOffset);

        ClickInside(window);

        Assert.That(editor.IsFocused, Is.True, "the click must still focus the control");
        Assert.That(scroll.Offset.Y, Is.EqualTo(before).Within(0.5));
    }

    [AvaloniaTest]
    public void ExplicitBringIntoViewStillWorksAfterAClick()
    {
        var (window, scroll, viewer) = Host(new MarkdownViewer { MarkdownText = LongMarkdown });
        ScrollTo(scroll, StartOffset);
        ClickInside(window);

        double before = ScrollTo(scroll, 0);
        viewer.BringIntoView();
        Dispatcher.UIThread.RunJobs();

        // Suppression is scoped to the Focus() call, so a host asking for
        // bring-into-view while the control is focused must not be swallowed.
        Assert.That(Math.Abs(scroll.Offset.Y - before), Is.GreaterThan(1));
    }
}

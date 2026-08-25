using System.Threading.Tasks;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Headless;
using global::Avalonia.Headless.NUnit;
using global::Avalonia.Input;
using global::Avalonia.Input.Platform;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using NUnit.Framework;
using Notepad.Avalonia.Controls;

namespace Notepad.Avalonia.Tests;

/// <summary>
/// Focus/selection behaviour, matching Avalonia's TextBox and SelectableTextBlock:
/// the selection is dropped when the control loses focus unless opted out.
/// </summary>
[TestFixture]
public class FocusSelectionTests
{
    private static (T control, Button other) Host<T>(T control) where T : Control
    {
        var other = new Button { Content = "other" };
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = new StackPanel { Children = { control, other } }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (control, other);
    }

    [AvaloniaTest]
    public void ViewerClearsSelectionOnLostFocusByDefault()
    {
        var (viewer, other) = Host(new MarkdownViewer { MarkdownText = "hello world" });

        viewer.Focus();
        viewer.SelectAll();
        Assert.That(viewer.HasSelection, Is.True);

        other.Focus();

        Assert.That(viewer.HasSelection, Is.False);
        Assert.That(viewer.SelectedText, Is.EqualTo(string.Empty));
    }

    [AvaloniaTest]
    public void ViewerKeepsSelectionWhenClearingIsDisabled()
    {
        var (viewer, other) = Host(new MarkdownViewer
        {
            MarkdownText = "hello world",
            ClearSelectionOnLostFocus = false
        });

        viewer.Focus();
        viewer.SelectAll();
        other.Focus();

        Assert.That(viewer.HasSelection, Is.True);
        Assert.That(viewer.SelectedText, Is.EqualTo("hello world"));
    }

    [AvaloniaTest]
    public void EditorClearsSelectionOnLostFocusByDefault()
    {
        var (editor, other) = Host(new NoteEditor());
        editor.SetText("hello world");

        editor.Focus();
        editor.SelectAll();
        Assert.That(editor.HasSelection, Is.True);

        other.Focus();

        Assert.That(editor.HasSelection, Is.False);
    }

    [AvaloniaTest]
    public void EditorKeepsSelectionWhenClearingIsDisabled()
    {
        var (editor, other) = Host(new NoteEditor { ClearSelectionOnLostFocus = false });
        editor.SetText("hello world");

        editor.Focus();
        editor.SelectAll();
        other.Focus();

        Assert.That(editor.HasSelection, Is.True);
        Assert.That(editor.GetSelectedText(), Is.EqualTo("hello world"));
    }

    [AvaloniaTest]
    public void ViewerKeepsSelectionWhileItsContextMenuIsOpen()
    {
        var (viewer, _) = Host(new MarkdownViewer { MarkdownText = "hello world" });
        viewer.Focus();
        viewer.SelectAll();

        // Opening the menu moves focus away from the control.
        viewer.ContextMenu!.Open(viewer);
        Dispatcher.UIThread.RunJobs();

        Assert.That(viewer.IsFocused, Is.False, "sanity: the menu takes focus");
        Assert.That(viewer.SelectedText, Is.EqualTo("hello world"),
            "the Copy command needs the selection to still be there");
    }

    [AvaloniaTest]
    public void ViewerDropsSelectionWhenTheContextMenuClosesWithFocusElsewhere()
    {
        var (viewer, other) = Host(new MarkdownViewer { MarkdownText = "hello world" });
        viewer.Focus();
        viewer.SelectAll();
        viewer.ContextMenu!.Open(viewer);
        Dispatcher.UIThread.RunJobs();

        other.Focus();
        viewer.ContextMenu.Close();
        Dispatcher.UIThread.RunJobs();

        // No further LostFocus is raised here, so closing the menu has to finish
        // the job or the selection stays stranded on an unfocused control.
        Assert.That(viewer.IsFocused, Is.False);
        Assert.That(viewer.HasSelection, Is.False);
    }

    [AvaloniaTest]
    public void ViewerKeepsSelectionWhenTheContextMenuClosesBackOntoIt()
    {
        var (viewer, _) = Host(new MarkdownViewer { MarkdownText = "hello world" });
        viewer.Focus();
        viewer.SelectAll();
        viewer.ContextMenu!.Open(viewer);
        Dispatcher.UIThread.RunJobs();

        viewer.ContextMenu.Close();
        viewer.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.That(viewer.SelectedText, Is.EqualTo("hello world"));
    }

    [AvaloniaTest]
    public void EditorDropsSelectionWhenAHostContextMenuClosesWithFocusElsewhere()
    {
        var editor = new NoteEditor { ContextMenu = new ContextMenu() };
        var (_, other) = Host(editor);
        editor.SetText("hello world");
        editor.Focus();
        editor.SelectAll();

        editor.ContextMenu!.Open(editor);
        Dispatcher.UIThread.RunJobs();
        other.Focus();
        editor.ContextMenu.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.That(editor.HasSelection, Is.False);
    }

    /// <summary>
    /// The real path behind the context menu: select with the mouse, right-click,
    /// pick Copy. Copying falls back to the whole document when nothing is
    /// selected, so losing the selection here would silently copy everything.
    /// </summary>
    [AvaloniaTest]
    public async Task CopyFromTheContextMenuCopiesTheSelectionNotTheWholeDocument()
    {
        var viewer = new MarkdownViewer { MarkdownText = "alpha bravo charlie delta" };
        var window = new Window { Width = 400, Height = 200, Content = viewer };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        window.MouseDown(new Point(14, 20), MouseButton.Left);
        window.MouseMove(new Point(60, 20));
        window.MouseUp(new Point(60, 20), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        string selected = viewer.SelectedText;
        Assert.That(selected, Is.Not.Empty);
        Assert.That(selected, Is.Not.EqualTo(viewer.PlainText), "the drag must select only part of it");

        viewer.ContextMenu!.Open(viewer);
        Dispatcher.UIThread.RunJobs();

        // Worst-case ordering: the menu closes before the item's Click is handled.
        var copyItem = (MenuItem)((object[])viewer.ContextMenu.ItemsSource!)[0];
        viewer.ContextMenu.Close();
        copyItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        var clipboard = TopLevel.GetTopLevel(viewer)!.Clipboard!;
        Assert.That(await clipboard.TryGetTextAsync(), Is.EqualTo(selected));
    }

    [AvaloniaTest]
    public void ClearingAfterAClosedContextMenuIsDeferred()
    {
        var (viewer, other) = Host(new MarkdownViewer { MarkdownText = "hello world" });
        viewer.Focus();
        viewer.SelectAll();
        viewer.ContextMenu!.Open(viewer);
        Dispatcher.UIThread.RunJobs();
        other.Focus();

        viewer.ContextMenu.Close();

        // Menu commands run synchronously in this window, so the selection has to
        // outlive the close itself; only the following dispatcher turn drops it.
        Assert.That(viewer.SelectedText, Is.EqualTo("hello world"));

        Dispatcher.UIThread.RunJobs();
        Assert.That(viewer.HasSelection, Is.False);
    }

    /// <summary>
    /// A menu shared by several controls notifies all of them when it closes.
    /// Each one only ever acts on its own focus state, so the extra notifications
    /// are harmless: the unfocused owner is tidied up, the focused one is left alone.
    /// </summary>
    [AvaloniaTest]
    public void SharedContextMenuOnlyAffectsTheUnfocusedOwner()
    {
        var menu = new ContextMenu();
        var first = new MarkdownViewer { MarkdownText = "first document", ContextMenu = menu };
        var second = new MarkdownViewer { MarkdownText = "second document", ContextMenu = menu };
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = new StackPanel { Children = { first, second } }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        first.Focus();
        first.SelectAll();
        menu.Open(first);
        Dispatcher.UIThread.RunJobs();

        second.Focus();
        second.SelectAll();
        menu.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.That(first.HasSelection, Is.False, "the unfocused owner is tidied up");
        Assert.That(second.SelectedText, Is.EqualTo("second document"), "the focused owner keeps its selection");
    }
}

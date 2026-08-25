using global::Avalonia.Controls;
using global::Avalonia.Headless.NUnit;
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
}

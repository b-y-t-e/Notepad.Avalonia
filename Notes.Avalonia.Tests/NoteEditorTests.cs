using global::Avalonia.Headless.NUnit;
using global::Avalonia.Media;
using global::Avalonia.Media.Imaging;
using NUnit.Framework;
using Notes.Avalonia.Controls;
using Notes.Avalonia.Model;

namespace Notes.Avalonia.Tests;

[TestFixture]
public class NoteEditorTests
{
    [AvaloniaTest]
    public void NewEditorHasOneEmptyItem()
    {
        var editor = new NoteEditor();
        Assert.That(editor.Document.Items.Count, Is.EqualTo(1));
        Assert.That(editor.GetText(), Is.EqualTo(string.Empty));
    }

    [AvaloniaTest]
    public void InsertTextAtCaret()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Hello");
        Assert.That(editor.GetText(), Is.EqualTo("Hello"));
    }

    [AvaloniaTest]
    public void InsertMultipleTexts()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Hello");
        editor.InsertTextAtCaret(" World");
        Assert.That(editor.GetText(), Is.EqualTo("Hello World"));
    }

    [AvaloniaTest]
    public void SplitItemCreatesNewNoteItem()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Line1");
        editor.SplitItemAtCaret();
        editor.InsertTextAtCaret("Line2");

        Assert.That(editor.Document.Items.Count, Is.EqualTo(2));
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("Line1"));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("Line2"));
    }

    [AvaloniaTest]
    public void SplitInMiddleOfText()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("HelloWorld");
        editor.Caret = new CursorPosition(0, 5);
        editor.SelectionAnchor = editor.Caret;
        editor.SplitItemAtCaret();

        Assert.That(editor.Document.Items.Count, Is.EqualTo(2));
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("Hello"));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("World"));
    }

    [AvaloniaTest]
    public void SetTextCreatesMultipleItems()
    {
        var editor = new NoteEditor();
        editor.SetText("Item 1\nItem 2\nItem 3");

        Assert.That(editor.Document.Items.Count, Is.EqualTo(3));
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("Item 1"));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("Item 2"));
        Assert.That(editor.Document.Items[2].PlainText, Is.EqualTo("Item 3"));
    }

    [AvaloniaTest]
    public void SetTextWithCRLF()
    {
        var editor = new NoteEditor();
        editor.SetText("a\r\nb\r\nc");
        Assert.That(editor.Document.Items.Count, Is.EqualTo(3));
    }

    [AvaloniaTest]
    public void PasteMultilineTextCreatesNewItems()
    {
        var editor = new NoteEditor();
        editor.PasteMultilineText("First note\nSecond note\nThird note");

        Assert.That(editor.Document.Items.Count, Is.EqualTo(3));
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("First note"));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("Second note"));
        Assert.That(editor.Document.Items[2].PlainText, Is.EqualTo("Third note"));
    }

    [AvaloniaTest]
    public void PasteMultilineIntoExistingItems()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Existing");
        editor.SplitItemAtCaret();
        editor.InsertTextAtCaret("Also existing");

        editor.Caret = new CursorPosition(0, 8);
        editor.SelectionAnchor = editor.Caret;
        editor.PasteMultilineText("\nNew note 1\nNew note 2");

        Assert.That(editor.Document.Items.Count, Is.EqualTo(4));
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("Existing"));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("New note 1"));
    }

    [AvaloniaTest]
    public void InsertImageAtCaret()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("before");
        editor.InsertImageAtCaret(CreateTestBitmap());

        var item = editor.Document.Items[0];
        Assert.That(item.Elements.Count, Is.EqualTo(2));
        Assert.That(item.Elements[0].Type, Is.EqualTo(ContentElementType.Text));
        Assert.That(item.Elements[1].Type, Is.EqualTo(ContentElementType.Image));
    }

    [AvaloniaTest]
    public void InsertImageInMiddleOfText()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("ABCD");
        editor.Caret = new CursorPosition(0, 2);
        editor.SelectionAnchor = editor.Caret;
        editor.InsertImageAtCaret(CreateTestBitmap());

        var item = editor.Document.Items[0];
        Assert.That(item.Elements.Count, Is.EqualTo(3));
        Assert.That(item.Elements[0].Text, Is.EqualTo("AB"));
        Assert.That(item.Elements[1].Type, Is.EqualTo(ContentElementType.Image));
        Assert.That(item.Elements[2].Text, Is.EqualTo("CD"));
    }

    [AvaloniaTest]
    public void TextAndImageInSameLine()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Photo: ");
        editor.InsertImageAtCaret(CreateTestBitmap());
        editor.InsertTextAtCaret(" caption");

        var item = editor.Document.Items[0];
        Assert.That(item.Elements.Count, Is.EqualTo(3));
        Assert.That(item.PlainText, Does.Contain("Photo:"));
        Assert.That(item.PlainText, Does.Contain("caption"));
        Assert.That(item.TextLength, Is.EqualTo(16));
    }

    // ---- Selection tests ----

    [AvaloniaTest]
    public void SelectionWithinSingleItem()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Hello World");

        editor.SelectionAnchor = new CursorPosition(0, 0);
        editor.Caret = new CursorPosition(0, 5);

        Assert.That(editor.HasSelection, Is.True);
        Assert.That(editor.GetSelectedText(), Is.EqualTo("Hello"));
    }

    [AvaloniaTest]
    public void SelectionAcrossMultipleItems()
    {
        var editor = new NoteEditor();
        editor.SetText("First line\nSecond line\nThird line");

        editor.SelectionAnchor = new CursorPosition(0, 6);
        editor.Caret = new CursorPosition(2, 5);

        var selected = editor.GetSelectedText();
        Assert.That(selected, Does.Contain("line"));
        Assert.That(selected, Does.Contain("Second line"));
        Assert.That(selected, Does.Contain("Third"));
    }

    [AvaloniaTest]
    public void SelectionIncludingImages()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("AB");
        editor.InsertImageAtCaret(CreateTestBitmap());
        editor.InsertTextAtCaret("CD");

        editor.SelectionAnchor = new CursorPosition(0, 0);
        editor.Caret = new CursorPosition(0, 5);

        var text = editor.GetSelectedText();
        Assert.That(text.Length, Is.EqualTo(5));
    }

    [AvaloniaTest]
    public void SelectAllCoverEverything()
    {
        var editor = new NoteEditor();
        editor.SetText("One\nTwo\nThree");
        editor.SelectAll();

        Assert.That(editor.HasSelection, Is.True);
        var text = editor.GetSelectedText();
        Assert.That(text, Does.Contain("One"));
        Assert.That(text, Does.Contain("Two"));
        Assert.That(text, Does.Contain("Three"));
    }

    [AvaloniaTest]
    public void DeleteSelectionWithinItem()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Hello World");

        editor.SelectionAnchor = new CursorPosition(0, 5);
        editor.Caret = new CursorPosition(0, 11);
        editor.DeleteSelection();

        Assert.That(editor.GetText(), Is.EqualTo("Hello"));
    }

    [AvaloniaTest]
    public void DeleteSelectionAcrossItems()
    {
        var editor = new NoteEditor();
        editor.SetText("First\nSecond\nThird");

        editor.SelectionAnchor = new CursorPosition(0, 3);
        editor.Caret = new CursorPosition(2, 3);
        editor.DeleteSelection();

        Assert.That(editor.Document.Items.Count, Is.EqualTo(1));
        Assert.That(editor.GetText(), Is.EqualTo("Firrd"));
    }

    [AvaloniaTest]
    public void DeleteSelectionWithImages()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("AB");
        editor.InsertImageAtCaret(CreateTestBitmap());
        editor.InsertTextAtCaret("CD");

        editor.SelectionAnchor = new CursorPosition(0, 2);
        editor.Caret = new CursorPosition(0, 5);
        editor.DeleteSelection();

        Assert.That(editor.GetText(), Is.EqualTo("AB"));
    }

    // ---- Caret tests ----

    [AvaloniaTest]
    public void CaretStartsAtOrigin()
    {
        var editor = new NoteEditor();
        Assert.That(editor.Caret.ItemIndex, Is.EqualTo(0));
        Assert.That(editor.Caret.Offset, Is.EqualTo(0));
    }

    [AvaloniaTest]
    public void CaretAdvancesAfterInsert()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("ABC");
        Assert.That(editor.Caret.Offset, Is.EqualTo(3));
    }

    [AvaloniaTest]
    public void CaretAfterSplit()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Hello");
        editor.SplitItemAtCaret();
        Assert.That(editor.Caret.ItemIndex, Is.EqualTo(1));
        Assert.That(editor.Caret.Offset, Is.EqualTo(0));
    }

    [AvaloniaTest]
    public void CaretAfterImageInsert()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("AB");
        editor.InsertImageAtCaret(CreateTestBitmap());
        Assert.That(editor.Caret.Offset, Is.EqualTo(3));
    }

    // ---- Font property tests ----

    [AvaloniaTest]
    public void DefaultFontSizeProperty()
    {
        var editor = new NoteEditor();
        Assert.That(editor.DefaultFontSize, Is.EqualTo(14.0));
        editor.DefaultFontSize = 20;
        Assert.That(editor.DefaultFontSize, Is.EqualTo(20.0));
    }

    [AvaloniaTest]
    public void DefaultFontFamilyProperty()
    {
        var editor = new NoteEditor();
        var courier = new FontFamily("Courier New");
        editor.DefaultFont = courier;
        Assert.That(editor.DefaultFont.Name, Is.EqualTo("Courier New"));
    }

    [AvaloniaTest]
    public void InsertedTextUsesDefaultFont()
    {
        var editor = new NoteEditor();
        editor.DefaultFont = new FontFamily("Courier New");
        editor.DefaultFontSize = 18;
        editor.InsertTextAtCaret("test");

        var el = editor.Document.Items[0].Elements[0];
        Assert.That(el.Font, Is.EqualTo(FontFamily.Default));
        Assert.That(el.FontSize, Is.EqualTo(0));
        Assert.That(editor.Document.DefaultFont.Name, Is.EqualTo("Courier New"));
        Assert.That(editor.Document.DefaultFontSize, Is.EqualTo(18));
    }

    // ---- Rich copy/paste ----

    [AvaloniaTest]
    public void CopyPastePreservesImages()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("AB");
        editor.InsertImageAtCaret(CreateTestBitmap());
        editor.InsertTextAtCaret("CD");

        editor.SelectionAnchor = new CursorPosition(0, 0);
        editor.Caret = new CursorPosition(0, 5);
        editor.CopyToClipboard();

        editor.SplitItemAtCaret();

        editor.SelectionAnchor = editor.Caret;
        editor.PasteRichFromInternalClipboard();

        var pastedItem = editor.Document.Items[1];
        Assert.That(pastedItem.Elements.Count, Is.EqualTo(3));
        Assert.That(pastedItem.Elements[0].Type, Is.EqualTo(ContentElementType.Text));
        Assert.That(pastedItem.Elements[0].Text, Is.EqualTo("AB"));
        Assert.That(pastedItem.Elements[1].Type, Is.EqualTo(ContentElementType.Image));
        Assert.That(pastedItem.Elements[2].Type, Is.EqualTo(ContentElementType.Text));
        Assert.That(pastedItem.Elements[2].Text, Is.EqualTo("CD"));
    }

    [AvaloniaTest]
    public void CopyPasteAcrossItemsPreservesImages()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Line1");
        editor.InsertImageAtCaret(CreateTestBitmap());
        editor.SplitItemAtCaret();
        editor.InsertTextAtCaret("Line2");

        editor.SelectionAnchor = new CursorPosition(0, 0);
        editor.Caret = new CursorPosition(1, 5);
        editor.CopyToClipboard();

        editor.Caret = new CursorPosition(1, 5);
        editor.SelectionAnchor = editor.Caret;
        editor.SplitItemAtCaret();

        editor.PasteRichFromInternalClipboard();

        Assert.That(editor.Document.Items.Count, Is.EqualTo(4));
        var pasted0 = editor.Document.Items[2];
        Assert.That(pasted0.Elements.Any(e => e.Type == ContentElementType.Image), Is.True);
    }

    // ---- AddItem API ----

    [AvaloniaTest]
    public void AddItemApi()
    {
        var editor = new NoteEditor();
        editor.AddItem("First note");
        editor.AddItem("Second note");

        Assert.That(editor.Document.Items.Count, Is.EqualTo(3));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("First note"));
        Assert.That(editor.Document.Items[2].PlainText, Is.EqualTo("Second note"));
    }

    // ---- Text wrapping tests ----

    [AvaloniaTest]
    public void LongTextWrapsWithinNarrowEditor()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.Measure(new global::Avalonia.Size(150, 600));

        editor.InsertTextAtCaret("This is a very long text that should wrap to multiple lines in a narrow editor");

        editor.Measure(new global::Avalonia.Size(150, 600));

        var item = editor.Document.Items[0];
        Assert.That(item.PlainText.Length, Is.GreaterThan(20));
        Assert.That(editor.GetText(), Does.Contain("very long text"));
    }

    [AvaloniaTest]
    public void LongTextRemainsOneItemAfterWrapping()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("AAAA BBBB CCCC DDDD EEEE FFFF GGGG HHHH");
        editor.Measure(new global::Avalonia.Size(150, 600));

        Assert.That(editor.Document.Items.Count, Is.EqualTo(1));
        Assert.That(editor.Caret.ItemIndex, Is.EqualTo(0));
        Assert.That(editor.Caret.Offset, Is.EqualTo(39));
    }

    [AvaloniaTest]
    public void SelectionAcrossWrappedLinesWorks()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("AAAA BBBB CCCC DDDD EEEE FFFF");
        editor.Measure(new global::Avalonia.Size(150, 600));

        editor.SelectionAnchor = new CursorPosition(0, 5);
        editor.Caret = new CursorPosition(0, 25);

        Assert.That(editor.HasSelection, Is.True);
        Assert.That(editor.GetSelectedText(), Is.EqualTo("BBBB CCCC DDDD EEEE "));
    }

    [AvaloniaTest]
    public void DeleteSelectionInWrappedTextWorks()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("AAAA BBBB CCCC DDDD");
        editor.Measure(new global::Avalonia.Size(150, 600));

        editor.SelectionAnchor = new CursorPosition(0, 5);
        editor.Caret = new CursorPosition(0, 15);
        editor.DeleteSelection();

        Assert.That(editor.GetText(), Is.EqualTo("AAAA DDDD"));
    }

    [AvaloniaTest]
    public void InsertTextInWrappedItemWorks()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("AAAA CCCC");
        editor.Measure(new global::Avalonia.Size(150, 600));

        editor.Caret = new CursorPosition(0, 5);
        editor.SelectionAnchor = editor.Caret;
        editor.InsertTextAtCaret("BBBB ");

        Assert.That(editor.GetText(), Is.EqualTo("AAAA BBBB CCCC"));
    }

    [AvaloniaTest]
    public void SplitItemInWrappedTextWorks()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("AAAA BBBB CCCC DDDD");
        editor.Measure(new global::Avalonia.Size(150, 600));

        editor.Caret = new CursorPosition(0, 10);
        editor.SelectionAnchor = editor.Caret;
        editor.SplitItemAtCaret();

        Assert.That(editor.Document.Items.Count, Is.EqualTo(2));
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("AAAA BBBB "));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("CCCC DDDD"));
    }

    [AvaloniaTest]
    public void ImageInWrappedLineDoesNotCrash()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("Before image ");
        editor.InsertImageAtCaret(CreateTestBitmap());
        editor.InsertTextAtCaret(" After image text that is long enough to wrap");
        editor.Measure(new global::Avalonia.Size(150, 600));

        Assert.That(editor.Document.Items[0].Elements.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(editor.GetText(), Does.Contain("Before image"));
    }

    [AvaloniaTest]
    public void MoveCaretDownNavigatesWrappedLines()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("AAAA BBBB CCCC DDDD EEEE FFFF");
        editor.Measure(new global::Avalonia.Size(150, 600));

        editor.Caret = new CursorPosition(0, 2);
        editor.SelectionAnchor = editor.Caret;

        editor.MoveCaretVertical(1, false);

        Assert.That(editor.Caret.ItemIndex, Is.EqualTo(0));
        Assert.That(editor.Caret.Offset, Is.GreaterThan(2));
    }

    [AvaloniaTest]
    public void MoveCaretUpNavigatesWrappedLines()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("AAAA BBBB CCCC DDDD EEEE FFFF");
        editor.Measure(new global::Avalonia.Size(150, 600));

        int endOffset = editor.Document.Items[0].TextLength;
        editor.Caret = new CursorPosition(0, endOffset);
        editor.SelectionAnchor = editor.Caret;

        editor.MoveCaretVertical(-1, false);

        Assert.That(editor.Caret.ItemIndex, Is.EqualTo(0));
        Assert.That(editor.Caret.Offset, Is.LessThan(endOffset));
    }

    [AvaloniaTest]
    public void MoveCaretDownFromLastWrappedLineGoesToNextItem()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.SetText("AAAA BBBB CCCC DDDD EEEE FFFF\nSecond");
        editor.Measure(new global::Avalonia.Size(150, 600));

        int lastOffset = editor.Document.Items[0].TextLength;
        editor.Caret = new CursorPosition(0, lastOffset);
        editor.SelectionAnchor = editor.Caret;

        editor.MoveCaretVertical(1, false);

        Assert.That(editor.Caret.ItemIndex, Is.EqualTo(1));
    }

    [AvaloniaTest]
    public void MoveCaretUpFromFirstWrappedLineGoesToPreviousItem()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.SetText("First\nAAAA BBBB CCCC DDDD EEEE FFFF");
        editor.Measure(new global::Avalonia.Size(150, 600));

        editor.Caret = new CursorPosition(1, 2);
        editor.SelectionAnchor = editor.Caret;

        editor.MoveCaretVertical(-1, false);

        Assert.That(editor.Caret.ItemIndex, Is.EqualTo(0));
    }

    [AvaloniaTest]
    public void MoveCaretVerticalWithShiftExtendsSelection()
    {
        var editor = new NoteEditor();
        editor.DefaultFontSize = 14;
        editor.InsertTextAtCaret("AAAA BBBB CCCC DDDD EEEE FFFF");
        editor.Measure(new global::Avalonia.Size(150, 600));

        editor.Caret = new CursorPosition(0, 2);
        editor.SelectionAnchor = new CursorPosition(0, 2);

        editor.MoveCaretVertical(1, true);

        Assert.That(editor.HasSelection, Is.True);
        Assert.That(editor.SelectionAnchor.Offset, Is.EqualTo(2));
        Assert.That(editor.Caret.Offset, Is.GreaterThan(2));
    }

    // ---- Undo / Redo tests ----

    [AvaloniaTest]
    public void UndoRevertsTextInsert()
    {
        var editor = new NoteEditor();
        editor.SaveUndoState();
        editor.InsertTextAtCaret("Hello");

        Assert.That(editor.GetText(), Is.EqualTo("Hello"));

        editor.Undo();

        Assert.That(editor.GetText(), Is.EqualTo(""));
    }

    [AvaloniaTest]
    public void RedoRestoresUndoneChange()
    {
        var editor = new NoteEditor();
        editor.SaveUndoState();
        editor.InsertTextAtCaret("Hello");
        editor.Undo();

        Assert.That(editor.GetText(), Is.EqualTo(""));

        editor.Redo();

        Assert.That(editor.GetText(), Is.EqualTo("Hello"));
    }

    [AvaloniaTest]
    public void UndoRevertsSplitItem()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("AAAA BBBB");
        editor.Caret = new CursorPosition(0, 5);
        editor.SelectionAnchor = editor.Caret;

        editor.SaveUndoState();
        editor.SplitItemAtCaret();

        Assert.That(editor.Document.Items.Count, Is.EqualTo(2));

        editor.Undo();

        Assert.That(editor.Document.Items.Count, Is.EqualTo(1));
        Assert.That(editor.GetText(), Is.EqualTo("AAAA BBBB"));
    }

    [AvaloniaTest]
    public void UndoRevertsDeleteSelection()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Hello World");
        editor.SelectionAnchor = new CursorPosition(0, 0);
        editor.Caret = new CursorPosition(0, 5);

        editor.SaveUndoState();
        editor.DeleteSelection();

        Assert.That(editor.GetText(), Is.EqualTo(" World"));

        editor.Undo();

        Assert.That(editor.GetText(), Is.EqualTo("Hello World"));
    }

    [AvaloniaTest]
    public void MultipleUndosWorkInOrder()
    {
        var editor = new NoteEditor();

        editor.SaveUndoState();
        editor.InsertTextAtCaret("A");
        editor.SaveUndoState();
        editor.InsertTextAtCaret("B");
        editor.SaveUndoState();
        editor.InsertTextAtCaret("C");

        Assert.That(editor.GetText(), Is.EqualTo("ABC"));

        editor.Undo();
        Assert.That(editor.GetText(), Is.EqualTo("AB"));

        editor.Undo();
        Assert.That(editor.GetText(), Is.EqualTo("A"));

        editor.Undo();
        Assert.That(editor.GetText(), Is.EqualTo(""));
    }

    [AvaloniaTest]
    public void NewEditAfterUndoClearsRedoStack()
    {
        var editor = new NoteEditor();

        editor.SaveUndoState();
        editor.InsertTextAtCaret("A");
        editor.SaveUndoState();
        editor.InsertTextAtCaret("B");

        editor.Undo();
        Assert.That(editor.GetText(), Is.EqualTo("A"));

        editor.SaveUndoState();
        editor.InsertTextAtCaret("C");
        Assert.That(editor.GetText(), Is.EqualTo("AC"));

        editor.Redo();
        Assert.That(editor.GetText(), Is.EqualTo("AC"));
    }

    [AvaloniaTest]
    public void UndoOnEmptyStackDoesNothing()
    {
        var editor = new NoteEditor();
        editor.InsertTextAtCaret("Hello");

        editor.Undo();

        Assert.That(editor.GetText(), Is.EqualTo("Hello"));
    }

    // ---- MVVM API Tests ----

    [AvaloniaTest]
    public void SetItemsPopulatesDocument()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("First note"),
            new("Second note"),
        };
        editor.Items = items;

        Assert.That(editor.Document.Items.Count, Is.EqualTo(2));
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("First note"));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("Second note"));
    }

    [AvaloniaTest]
    public void EditSyncsBackToItems()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("Hello"),
        };
        editor.Items = items;

        editor.Caret = new CursorPosition(0, 5);
        editor.SelectionAnchor = new CursorPosition(0, 5);
        editor.InsertTextAtCaret(" World");

        Assert.That(items[0].Text, Is.EqualTo("Hello World"));
    }

    [AvaloniaTest]
    public void ItemsChangedEventFires()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("Test"),
        };
        editor.Items = items;

        int fireCount = 0;
        editor.ItemsChanged += (_, _) => fireCount++;

        editor.Caret = new CursorPosition(0, 4);
        editor.SelectionAnchor = new CursorPosition(0, 4);
        editor.InsertTextAtCaret("!");

        Assert.That(fireCount, Is.GreaterThan(0));
    }

    [AvaloniaTest]
    public void ViewModelTextChangeUpdatesDocument()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("Original"),
        };
        editor.Items = items;

        items[0].Text = "Changed";

        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("Changed"));
    }

    [AvaloniaTest]
    public void CollectionAddSyncsToDocument()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("First"),
        };
        editor.Items = items;

        items.Add(new NoteItemData("Second"));

        Assert.That(editor.Document.Items.Count, Is.EqualTo(2));
        Assert.That(editor.Document.Items[1].PlainText, Is.EqualTo("Second"));
    }

    [AvaloniaTest]
    public void CollectionRemoveSyncsToDocument()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("First"),
            new("Second"),
        };
        editor.Items = items;

        items.RemoveAt(0);

        Assert.That(editor.Document.Items.Count, Is.EqualTo(1));
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("Second"));
    }

    [AvaloniaTest]
    public void ImageMarkdownParsedCorrectly()
    {
        var editor = new NoteEditor();
        var bmp = CreateTestBitmap();
        editor.ImageStore["photo1"] = bmp;

        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("Before ![alt](photo1) after"),
        };
        editor.Items = items;

        Assert.That(editor.Document.Items[0].Elements.Count, Is.EqualTo(3));
        Assert.That(editor.Document.Items[0].Elements[0].Type, Is.EqualTo(ContentElementType.Text));
        Assert.That(editor.Document.Items[0].Elements[0].Text, Is.EqualTo("Before "));
        Assert.That(editor.Document.Items[0].Elements[1].Type, Is.EqualTo(ContentElementType.Image));
        Assert.That(editor.Document.Items[0].Elements[1].ImageKey, Is.EqualTo("photo1"));
        Assert.That(editor.Document.Items[0].Elements[1].ImageAltText, Is.EqualTo("alt"));
        Assert.That(editor.Document.Items[0].Elements[2].Type, Is.EqualTo(ContentElementType.Text));
        Assert.That(editor.Document.Items[0].Elements[2].Text, Is.EqualTo(" after"));
    }

    [AvaloniaTest]
    public void ImageSerializedBackToMarkdown()
    {
        var editor = new NoteEditor();
        var bmp = CreateTestBitmap();
        editor.ImageStore["pic1"] = bmp;

        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("Note ![receipt](pic1) done"),
        };
        editor.Items = items;

        Assert.That(items[0].Text, Is.EqualTo("Note ![receipt](pic1) done"));

        editor.Caret = new CursorPosition(0, 0);
        editor.SelectionAnchor = new CursorPosition(0, 0);
        editor.InsertTextAtCaret("X");

        Assert.That(items[0].Text, Is.EqualTo("XNote ![receipt](pic1) done"));
    }

    [AvaloniaTest]
    public void UnknownImageKeyKeptAsText()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("Text ![alt](missing_key) more"),
        };
        editor.Items = items;

        Assert.That(editor.Document.Items[0].Elements.All(
            e => e.Type == ContentElementType.Text), Is.True);
        Assert.That(editor.Document.Items[0].PlainText, Is.EqualTo("Text ![alt](missing_key) more"));
    }

    [AvaloniaTest]
    public void SplitItemSyncsNewItemToCollection()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("HelloWorld"),
        };
        editor.Items = items;

        editor.Caret = new CursorPosition(0, 5);
        editor.SelectionAnchor = new CursorPosition(0, 5);
        editor.SplitItemAtCaret();

        Assert.That(items.Count, Is.EqualTo(2));
        Assert.That(items[0].Text, Is.EqualTo("Hello"));
        Assert.That(items[1].Text, Is.EqualTo("World"));
    }

    [AvaloniaTest]
    public void NullItemsWorksLikeBeforeMvvm()
    {
        var editor = new NoteEditor();
        Assert.That(editor.Items, Is.Null);

        editor.InsertTextAtCaret("No MVVM");
        Assert.That(editor.GetText(), Is.EqualTo("No MVVM"));
    }

    [AvaloniaTest]
    public void PastedImageGetsAutoRegisteredInImageStore()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("Before"),
        };
        editor.Items = items;

        var bmp = CreateTestBitmap();
        editor.Caret = new CursorPosition(0, 6);
        editor.SelectionAnchor = new CursorPosition(0, 6);
        editor.InsertImageAtCaret(bmp);

        Assert.That(items[0].Text, Does.Contain("!["));
        Assert.That(items[0].Text, Does.Contain("]("));
        Assert.That(editor.ImageStore.Count, Is.EqualTo(1));
    }

    [AvaloniaTest]
    public void MultipleImagesInSingleItem()
    {
        var editor = new NoteEditor();
        var bmp1 = CreateTestBitmap();
        var bmp2 = CreateTestBitmap();
        editor.ImageStore["a"] = bmp1;
        editor.ImageStore["b"] = bmp2;

        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("![x](a) mid ![y](b)"),
        };
        editor.Items = items;

        Assert.That(editor.Document.Items[0].Elements.Count, Is.EqualTo(3));
        Assert.That(editor.Document.Items[0].Elements[0].Type, Is.EqualTo(ContentElementType.Image));
        Assert.That(editor.Document.Items[0].Elements[1].Type, Is.EqualTo(ContentElementType.Text));
        Assert.That(editor.Document.Items[0].Elements[1].Text, Is.EqualTo(" mid "));
        Assert.That(editor.Document.Items[0].Elements[2].Type, Is.EqualTo(ContentElementType.Image));
    }

    [AvaloniaTest]
    public void DeleteSelectionAcrossItemsMergesAndSyncs()
    {
        var editor = new NoteEditor();
        var items = new System.Collections.ObjectModel.ObservableCollection<NoteItemData>
        {
            new("First"),
            new("Second"),
        };
        editor.Items = items;

        editor.Caret = new CursorPosition(0, 5);
        editor.SelectionAnchor = new CursorPosition(1, 0);
        editor.DeleteSelection();

        Assert.That(items.Count, Is.EqualTo(1));
        Assert.That(items[0].Text, Is.EqualTo("FirstSecond"));
    }

    // ---- DeleteRange bug regression test ----

    [AvaloniaTest]
    public void DeleteRange_DoesNotDeleteAdjacentImage()
    {
        var editor = new NoteEditor();
        editor.Document.Items.Clear();

        var item = new NoteItem();
        item.Elements.Add(ContentElement.CreateText("AB"));
        item.Elements.Add(ContentElement.CreateImage(CreateTestBitmap()));
        item.Elements.Add(ContentElement.CreateText("CD"));
        editor.Document.Items.Add(item);

        editor.Caret = new CursorPosition(0, 1);
        editor.SelectionAnchor = new CursorPosition(0, 0);
        editor.DeleteSelection();

        var resultItem = editor.Document.Items[0];
        Assert.That(resultItem.Elements.Count, Is.EqualTo(3));
        Assert.That(resultItem.Elements[0].Text, Is.EqualTo("B"));
        Assert.That(resultItem.Elements[1].Type, Is.EqualTo(ContentElementType.Image));
        Assert.That(resultItem.Elements[2].Text, Is.EqualTo("CD"));
    }

    [AvaloniaTest]
    public void DeleteRange_SpanningTextAndImage()
    {
        var editor = new NoteEditor();
        editor.Document.Items.Clear();

        var item = new NoteItem();
        item.Elements.Add(ContentElement.CreateText("AB"));
        item.Elements.Add(ContentElement.CreateImage(CreateTestBitmap()));
        item.Elements.Add(ContentElement.CreateText("CD"));
        editor.Document.Items.Add(item);

        editor.Caret = new CursorPosition(0, 3);
        editor.SelectionAnchor = new CursorPosition(0, 1);
        editor.DeleteSelection();

        var resultItem = editor.Document.Items[0];
        Assert.That(resultItem.PlainText, Is.EqualTo("ACD"));
    }

    // ---- Ctrl+Arrow word navigation tests ----

    [AvaloniaTest]
    public void CtrlRight_MovesToNextWordBoundary()
    {
        var editor = new NoteEditor();
        editor.Document.Items.Clear();
        editor.Document.Items.Add(new NoteItem("Hello World"));
        editor.Caret = new CursorPosition(0, 0);
        editor.SelectionAnchor = editor.Caret;

        editor.MoveCaretByWord(1, false);

        Assert.That(editor.Caret.Offset, Is.EqualTo(6));
    }

    [AvaloniaTest]
    public void CtrlLeft_MovesToPreviousWordBoundary()
    {
        var editor = new NoteEditor();
        editor.Document.Items.Clear();
        editor.Document.Items.Add(new NoteItem("Hello World"));
        editor.Caret = new CursorPosition(0, 11);
        editor.SelectionAnchor = editor.Caret;

        editor.MoveCaretByWord(-1, false);

        Assert.That(editor.Caret.Offset, Is.EqualTo(6));
    }

    [AvaloniaTest]
    public void CtrlShiftRight_ExtendsSelection()
    {
        var editor = new NoteEditor();
        editor.Document.Items.Clear();
        editor.Document.Items.Add(new NoteItem("Hello World"));
        editor.Caret = new CursorPosition(0, 0);
        editor.SelectionAnchor = editor.Caret;

        editor.MoveCaretByWord(1, true);

        Assert.That(editor.Caret.Offset, Is.EqualTo(6));
        Assert.That(editor.SelectionAnchor.Offset, Is.EqualTo(0));
        Assert.That(editor.HasSelection, Is.True);
    }

    private static Bitmap CreateTestBitmap()
    {
        return new WriteableBitmap(
            new global::Avalonia.PixelSize(10, 10),
            new global::Avalonia.Vector(96, 96),
            global::Avalonia.Platform.PixelFormat.Bgra8888,
            global::Avalonia.Platform.AlphaFormat.Premul);
    }
}

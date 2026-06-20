using global::Avalonia.Headless.NUnit;
using NUnit.Framework;
using Notepad.Avalonia.Model;

namespace Notepad.Avalonia.Tests;

[TestFixture]
public class DocumentModelTests
{
    [Test]
    public void NewDocumentHasOneItem()
    {
        var doc = new NoteDocument();
        Assert.That(doc.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetAllTextReturnsEmptyForNewDocument()
    {
        var doc = new NoteDocument();
        Assert.That(doc.GetAllText(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void NoteItemPlainText()
    {
        var item = new NoteItem("Hello World");
        Assert.That(item.PlainText, Is.EqualTo("Hello World"));
    }

    [Test]
    public void NoteItemTextLength()
    {
        var item = new NoteItem("ABC");
        Assert.That(item.TextLength, Is.EqualTo(3));
    }

    [Test]
    public void NoteItemEmptyTextLength()
    {
        var item = new NoteItem();
        Assert.That(item.TextLength, Is.EqualTo(0));
    }

    [Test]
    public void ContentElementCreateText()
    {
        var el = ContentElement.CreateText("hello");
        Assert.That(el.Type, Is.EqualTo(ContentElementType.Text));
        Assert.That(el.Text, Is.EqualTo("hello"));
    }

    [Test]
    public void ContentElementClone()
    {
        var el = ContentElement.CreateText("test");
        el.Bold = true;
        var clone = el.Clone();
        Assert.That(clone.Text, Is.EqualTo("test"));
        Assert.That(clone.Bold, Is.True);
        clone.Text = "changed";
        Assert.That(el.Text, Is.EqualTo("test"));
    }

    [Test]
    public void ResolveOffsetSimpleText()
    {
        var item = new NoteItem("ABCDE");
        var (elIdx, localOff) = item.ResolveOffset(3);
        Assert.That(elIdx, Is.EqualTo(0));
        Assert.That(localOff, Is.EqualTo(3));
    }

    [Test]
    public void ResolveOffsetZero()
    {
        var item = new NoteItem("Hello");
        var (elIdx, localOff) = item.ResolveOffset(0);
        Assert.That(elIdx, Is.EqualTo(0));
        Assert.That(localOff, Is.EqualTo(0));
    }

    [Test]
    public void ResolveOffsetEnd()
    {
        var item = new NoteItem("ABC");
        var (elIdx, localOff) = item.ResolveOffset(3);
        Assert.That(elIdx, Is.EqualTo(0));
        Assert.That(localOff, Is.EqualTo(3));
    }

    [Test]
    public void GlobalOffsetRoundTrip()
    {
        var item = new NoteItem("ABCDE");
        var (elIdx, localOff) = item.ResolveOffset(3);
        int global = item.GlobalOffset(elIdx, localOff);
        Assert.That(global, Is.EqualTo(3));
    }

    [Test]
    public void DocumentMultipleItems()
    {
        var doc = new NoteDocument();
        doc.Items.Clear();
        doc.Items.Add(new NoteItem("Line 1"));
        doc.Items.Add(new NoteItem("Line 2"));
        doc.Items.Add(new NoteItem("Line 3"));

        var text = doc.GetAllText();
        Assert.That(text, Does.Contain("Line 1"));
        Assert.That(text, Does.Contain("Line 2"));
        Assert.That(text, Does.Contain("Line 3"));
    }

    [Test]
    public void CursorPositionStart()
    {
        var pos = CursorPosition.Start;
        Assert.That(pos.ItemIndex, Is.EqualTo(0));
        Assert.That(pos.Offset, Is.EqualTo(0));
    }

    [Test]
    public void SelectionRangeIsEmpty()
    {
        var sel = new SelectionRange(CursorPosition.Start, CursorPosition.Start);
        Assert.That(sel.IsEmpty, Is.True);
    }

    [Test]
    public void SelectionRangeNotEmpty()
    {
        var sel = new SelectionRange(
            new CursorPosition(0, 0),
            new CursorPosition(0, 5));
        Assert.That(sel.IsEmpty, Is.False);
    }

    [Test]
    public void SelectionRangeOrdered()
    {
        var sel = new SelectionRange(
            new CursorPosition(1, 3),
            new CursorPosition(0, 1));
        var (first, last) = sel.Ordered();
        Assert.That(first.ItemIndex, Is.EqualTo(0));
        Assert.That(last.ItemIndex, Is.EqualTo(1));
    }

    [Test]
    public void DefaultFontSettings()
    {
        var doc = new NoteDocument();
        Assert.That(doc.DefaultFontSize, Is.EqualTo(14));
    }

    [AvaloniaTest]
    public void NoteItemWithMixedContent()
    {
        var item = new NoteItem();
        item.Elements.Add(ContentElement.CreateText("Hello "));
        var bmp = CreateTestBitmap();
        item.Elements.Add(ContentElement.CreateImage(bmp));
        item.Elements.Add(ContentElement.CreateText(" World"));

        Assert.That(item.TextLength, Is.EqualTo(13));
        Assert.That(item.PlainText, Does.Contain("Hello"));
        Assert.That(item.PlainText, Does.Contain("World"));
    }

    [AvaloniaTest]
    public void ResolveOffsetWithImage()
    {
        var item = new NoteItem();
        item.Elements.Add(ContentElement.CreateText("AB"));
        item.Elements.Add(ContentElement.CreateImage(CreateTestBitmap()));
        item.Elements.Add(ContentElement.CreateText("CD"));

        var (elIdx, localOff) = item.ResolveOffset(3);
        Assert.That(elIdx, Is.EqualTo(2));
        Assert.That(localOff, Is.EqualTo(0));
    }

    // ---- Word boundary tests ----

    [Test]
    public void FindWordBoundaryRight_SimpleWord()
    {
        var item = new NoteItem("Hello World");
        Assert.That(item.FindWordBoundaryRight(0), Is.EqualTo(6));
    }

    [Test]
    public void FindWordBoundaryRight_FromMiddleOfWord()
    {
        var item = new NoteItem("Hello World");
        Assert.That(item.FindWordBoundaryRight(2), Is.EqualTo(6));
    }

    [Test]
    public void FindWordBoundaryRight_FromSpace()
    {
        var item = new NoteItem("Hello World");
        Assert.That(item.FindWordBoundaryRight(5), Is.EqualTo(6));
    }

    [Test]
    public void FindWordBoundaryRight_AtEnd()
    {
        var item = new NoteItem("Hello");
        Assert.That(item.FindWordBoundaryRight(5), Is.EqualTo(5));
    }

    [Test]
    public void FindWordBoundaryRight_Punctuation()
    {
        var item = new NoteItem("foo.bar");
        Assert.That(item.FindWordBoundaryRight(0), Is.EqualTo(3));
    }

    [AvaloniaTest]
    public void FindWordBoundaryRight_Image()
    {
        var item = new NoteItem();
        item.Elements.Add(ContentElement.CreateText("AB"));
        item.Elements.Add(ContentElement.CreateImage(CreateTestBitmap()));
        item.Elements.Add(ContentElement.CreateText("CD"));
        Assert.That(item.FindWordBoundaryRight(2), Is.EqualTo(3));
    }

    [Test]
    public void FindWordBoundaryLeft_SimpleWord()
    {
        var item = new NoteItem("Hello World");
        Assert.That(item.FindWordBoundaryLeft(11), Is.EqualTo(6));
    }

    [Test]
    public void FindWordBoundaryLeft_FromSpace()
    {
        var item = new NoteItem("Hello World");
        Assert.That(item.FindWordBoundaryLeft(6), Is.EqualTo(0));
    }

    [Test]
    public void FindWordBoundaryLeft_AtStart()
    {
        var item = new NoteItem("Hello");
        Assert.That(item.FindWordBoundaryLeft(0), Is.EqualTo(0));
    }

    [Test]
    public void FindWordBoundaryLeft_Punctuation()
    {
        var item = new NoteItem("foo.bar");
        Assert.That(item.FindWordBoundaryLeft(7), Is.EqualTo(4));
    }

    [AvaloniaTest]
    public void FindWordBoundaryLeft_Image()
    {
        var item = new NoteItem();
        item.Elements.Add(ContentElement.CreateText("AB"));
        item.Elements.Add(ContentElement.CreateImage(CreateTestBitmap()));
        item.Elements.Add(ContentElement.CreateText("CD"));
        Assert.That(item.FindWordBoundaryLeft(3), Is.EqualTo(2));
    }

    private static global::Avalonia.Media.Imaging.Bitmap CreateTestBitmap()
    {
        return new global::Avalonia.Media.Imaging.WriteableBitmap(
            new global::Avalonia.PixelSize(10, 10),
            new global::Avalonia.Vector(96, 96),
            global::Avalonia.Platform.PixelFormat.Bgra8888,
            global::Avalonia.Platform.AlphaFormat.Premul);
    }
}

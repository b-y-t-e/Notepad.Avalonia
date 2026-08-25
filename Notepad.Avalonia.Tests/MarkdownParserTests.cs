using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Notepad.Avalonia.Model;

namespace Notepad.Avalonia.Tests;

[TestFixture]
public class MarkdownParserTests
{
    private static string Plain(IEnumerable<MarkdownInline> inlines)
    {
        var sb = new StringBuilder();
        foreach (var inline in inlines)
            sb.Append(inline.LineBreak ? "\n" : inline.Text);
        return sb.ToString();
    }

    [Test]
    public void ParsesAtxHeadings()
    {
        var blocks = MarkdownParser.Parse("# Title\n\n### Third");

        Assert.That(blocks.Count, Is.EqualTo(2));
        Assert.That(blocks[0].Type, Is.EqualTo(MarkdownBlockType.Heading));
        Assert.That(blocks[0].HeadingLevel, Is.EqualTo(1));
        Assert.That(Plain(blocks[0].Inlines), Is.EqualTo("Title"));
        Assert.That(blocks[1].HeadingLevel, Is.EqualTo(3));
        Assert.That(Plain(blocks[1].Inlines), Is.EqualTo("Third"));
    }

    [Test]
    public void ParsesSetextHeading()
    {
        var blocks = MarkdownParser.Parse("Title\n=====\n");

        Assert.That(blocks[0].Type, Is.EqualTo(MarkdownBlockType.Heading));
        Assert.That(blocks[0].HeadingLevel, Is.EqualTo(1));
        Assert.That(Plain(blocks[0].Inlines), Is.EqualTo("Title"));
    }

    [Test]
    public void DashUnderParagraphIsSetextNotThematicBreak()
    {
        var blocks = MarkdownParser.Parse("Title\n-----\n");

        Assert.That(blocks.Count, Is.EqualTo(1));
        Assert.That(blocks[0].Type, Is.EqualTo(MarkdownBlockType.Heading));
        Assert.That(blocks[0].HeadingLevel, Is.EqualTo(2));
    }

    [Test]
    public void DashAfterBlankLineIsThematicBreak()
    {
        var blocks = MarkdownParser.Parse("Title\n\n-----\n");

        Assert.That(blocks[0].Type, Is.EqualTo(MarkdownBlockType.Paragraph));
        Assert.That(blocks[1].Type, Is.EqualTo(MarkdownBlockType.ThematicBreak));
    }

    [Test]
    public void ParsesIndentedCodeBlock()
    {
        var blocks = MarkdownParser.Parse("text\n\n    var x = 1;\n    var y = 2;\n");

        Assert.That(blocks[1].Type, Is.EqualTo(MarkdownBlockType.CodeBlock));
        Assert.That(blocks[1].Code, Is.EqualTo("var x = 1;\nvar y = 2;"));
    }

    [Test]
    public void ParsesEntities()
    {
        var inlines = MarkdownParser.Parse("a &amp; b &lt; c")[0].Inlines;
        Assert.That(Plain(inlines), Is.EqualTo("a & b < c"));
    }

    [Test]
    public void ParsesCenterAlignedTableColumn()
    {
        var table = MarkdownParser.Parse("| a | b | c |\n|:--|:-:|--:|\n| 1 | 2 | 3 |")[0].Table!;

        Assert.That(table.Alignments, Is.EqualTo(new[]
        {
            MarkdownColumnAlignment.Left,
            MarkdownColumnAlignment.Center,
            MarkdownColumnAlignment.Right
        }));
    }

    [Test]
    public void ParsesEmphasis()
    {
        var inlines = MarkdownParser.Parse("plain **bold** *italic* ***both*** ~~gone~~")[0].Inlines;

        Assert.That(inlines.Any(i => i.Text == "bold" && i.Bold && !i.Italic));
        Assert.That(inlines.Any(i => i.Text == "italic" && i.Italic && !i.Bold));
        Assert.That(inlines.Any(i => i.Text == "both" && i.Italic && i.Bold));
        Assert.That(inlines.Any(i => i.Text == "gone" && i.Strikethrough));
    }

    [Test]
    public void UnderscoreInsideWordIsNotEmphasis()
    {
        var inlines = MarkdownParser.Parse("snake_case_name")[0].Inlines;
        Assert.That(Plain(inlines), Is.EqualTo("snake_case_name"));
        Assert.That(inlines.Any(i => i.Italic), Is.False);
    }

    [Test]
    public void ParsesCodeSpanAndKeepsMarkupLiteral()
    {
        var inlines = MarkdownParser.Parse("use `a * b` here")[0].Inlines;

        var code = inlines.Single(i => i.Code);
        Assert.That(code.Text, Is.EqualTo("a * b"));
    }

    [Test]
    public void UnmatchedEmphasisStaysLiteral()
    {
        var inlines = MarkdownParser.Parse("2 * 3 = 6")[0].Inlines;
        Assert.That(Plain(inlines), Is.EqualTo("2 * 3 = 6"));
    }

    [Test]
    public void ParsesInlineLink()
    {
        var inlines = MarkdownParser.Parse("see [docs](https://example.com) now")[0].Inlines;

        var link = inlines.Single(i => i.LinkUrl != null);
        Assert.That(link.Text, Is.EqualTo("docs"));
        Assert.That(link.LinkUrl, Is.EqualTo("https://example.com"));
    }

    [Test]
    public void ParsesReferenceLink()
    {
        var blocks = MarkdownParser.Parse("see [docs][ref]\n\n[ref]: https://example.com");
        var link = blocks[0].Inlines.Single(i => i.LinkUrl != null);

        Assert.That(link.Text, Is.EqualTo("docs"));
        Assert.That(link.LinkUrl, Is.EqualTo("https://example.com"));
        Assert.That(blocks.Count, Is.EqualTo(1), "the definition line must not render");
    }

    [Test]
    public void ParsesAutolink()
    {
        var link = MarkdownParser.Parse("<https://example.com>")[0].Inlines.Single();
        Assert.That(link.LinkUrl, Is.EqualTo("https://example.com"));
    }

    [Test]
    public void ParsesImage()
    {
        var image = MarkdownParser.Parse("![a dog](dog)")[0].Inlines.Single();

        Assert.That(image.IsImage);
        Assert.That(image.ImageKey, Is.EqualTo("dog"));
        Assert.That(image.ImageAlt, Is.EqualTo("a dog"));
    }

    [Test]
    public void ParsesFencedCodeBlockWithLanguage()
    {
        var blocks = MarkdownParser.Parse("```csharp\nvar x = 1;\nvar y = 2;\n```");

        Assert.That(blocks[0].Type, Is.EqualTo(MarkdownBlockType.CodeBlock));
        Assert.That(blocks[0].CodeLanguage, Is.EqualTo("csharp"));
        Assert.That(blocks[0].Code, Is.EqualTo("var x = 1;\nvar y = 2;"));
    }

    [Test]
    public void CodeBlockContentIsNotParsedAsMarkdown()
    {
        var blocks = MarkdownParser.Parse("```\n# not a heading\n**not bold**\n```");

        Assert.That(blocks.Count, Is.EqualTo(1));
        Assert.That(blocks[0].Code, Does.Contain("**not bold**"));
    }

    [Test]
    public void ParsesThematicBreak()
    {
        var blocks = MarkdownParser.Parse("a\n\n---\n\nb");

        Assert.That(blocks[1].Type, Is.EqualTo(MarkdownBlockType.ThematicBreak));
    }

    [Test]
    public void ParsesUnorderedList()
    {
        var blocks = MarkdownParser.Parse("- one\n- two\n- three");

        Assert.That(blocks.Count, Is.EqualTo(3));
        Assert.That(blocks.All(b => b.IsListItem && b.ListDepth == 1));
        Assert.That(Plain(blocks[2].Inlines), Is.EqualTo("three"));
    }

    [Test]
    public void ParsesNestedList()
    {
        var blocks = MarkdownParser.Parse("- one\n  - nested\n- two");

        Assert.That(blocks[0].ListDepth, Is.EqualTo(1));
        Assert.That(blocks[1].ListDepth, Is.EqualTo(2));
        Assert.That(blocks[2].ListDepth, Is.EqualTo(1));
    }

    [Test]
    public void OrderedListNumbersIncrement()
    {
        var blocks = MarkdownParser.Parse("1. one\n1. two\n1. three");

        Assert.That(blocks.Select(b => b.Marker), Is.EqualTo(new[] { "1.", "2.", "3." }));
        Assert.That(blocks.All(b => b.Ordered));
    }

    [Test]
    public void ParsesTaskList()
    {
        var blocks = MarkdownParser.Parse("- [x] done\n- [ ] todo");

        Assert.That(blocks[0].IsTask && blocks[0].TaskChecked);
        Assert.That(Plain(blocks[0].Inlines), Is.EqualTo("done"));
        Assert.That(blocks[1].IsTask && !blocks[1].TaskChecked);
    }

    [Test]
    public void ParsesBlockquoteDepth()
    {
        var blocks = MarkdownParser.Parse("> quoted\n\n>> deeper");

        Assert.That(blocks[0].QuoteDepth, Is.EqualTo(1));
        Assert.That(blocks[1].QuoteDepth, Is.EqualTo(2));
    }

    [Test]
    public void ParsesTable()
    {
        var blocks = MarkdownParser.Parse("| a | b |\n|---|--:|\n| 1 | 2 |");

        Assert.That(blocks[0].Type, Is.EqualTo(MarkdownBlockType.Table));
        var table = blocks[0].Table!;
        Assert.That(table.Rows.Count, Is.EqualTo(2));
        Assert.That(table.Rows[0].IsHeader);
        Assert.That(table.ColumnCount, Is.EqualTo(2));
        Assert.That(table.Alignments[1], Is.EqualTo(MarkdownColumnAlignment.Right));
        Assert.That(Plain(table.Rows[1].Cells[1].Inlines), Is.EqualTo("2"));
    }

    [Test]
    public void SoftBreaksBecomeLineBreaksWhenEnabled()
    {
        var withBreaks = MarkdownParser.Parse("one\ntwo", softBreaksAsLineBreaks: true)[0].Inlines;
        Assert.That(withBreaks.Any(i => i.LineBreak));

        var joined = MarkdownParser.Parse("one\ntwo", softBreaksAsLineBreaks: false)[0].Inlines;
        Assert.That(joined.Any(i => i.LineBreak), Is.False);
        Assert.That(Plain(joined), Is.EqualTo("one two"));
    }

    [Test]
    public void TwoTrailingSpacesForceLineBreak()
    {
        var inlines = MarkdownParser.Parse("one  \ntwo", softBreaksAsLineBreaks: false)[0].Inlines;
        Assert.That(inlines.Any(i => i.LineBreak));
    }

    [Test]
    public void BackslashEscapeKeepsLiteral()
    {
        var inlines = MarkdownParser.Parse(@"\*not italic\*")[0].Inlines;
        Assert.That(Plain(inlines), Is.EqualTo("*not italic*"));
        Assert.That(inlines.Any(i => i.Italic), Is.False);
    }

    [Test]
    public void ParsesEmptyInputToNoBlocks()
    {
        Assert.That(MarkdownParser.Parse(null), Is.Empty);
        Assert.That(MarkdownParser.Parse(string.Empty), Is.Empty);
        Assert.That(MarkdownParser.Parse("   \n\n  "), Is.Empty);
    }
}

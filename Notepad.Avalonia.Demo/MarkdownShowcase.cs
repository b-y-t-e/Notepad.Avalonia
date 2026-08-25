using System.Text;

namespace Notepad.Avalonia.Demo;

/// <summary>
/// A large markdown document exercising every construct the viewer renders, plus
/// enough generated sections to make scrolling and virtualization visible.
/// </summary>
public static class MarkdownShowcase
{
    public static string Build(int generatedSections = 40)
    {
        var sb = new StringBuilder();
        sb.Append(Syntax);

        for (int i = 1; i <= generatedSections; i++)
        {
            sb.AppendLine();
            sb.AppendLine($"## Generated section {i}");
            sb.AppendLine();
            sb.AppendLine($"Paragraph {i} shows how the viewer reflows a longer body of text. It mixes "
                + $"**bold run {i}**, *italic run {i}*, `code_{i}()` and a [link {i}](https://example.com/{i}) "
                + "so that measurement, wrapping and hit testing are all exercised on the same line.");
            sb.AppendLine();
            sb.AppendLine($"- Bullet {i}.1 with a trailing note");
            sb.AppendLine($"- Bullet {i}.2 containing ~~a struck out phrase~~");
            sb.AppendLine($"  - Nested bullet {i}.2.1");
            sb.AppendLine();

            if (i % 4 == 0)
            {
                sb.AppendLine("```csharp");
                sb.AppendLine($"public int Section{i}() => {i} * 2;");
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (i % 5 == 0)
            {
                sb.AppendLine($"> Quoted remark for section {i}, long enough to wrap onto a second line "
                    + "so the quote bar spans the whole block.");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private const string Syntax = """
# Markdown showcase

A read-only rendering of **every** supported construct. Select any text with the
mouse and press `Ctrl+C` to copy it — selection spans blocks, lists and tables.

## Headings

# Heading 1
## Heading 2
### Heading 3
#### Heading 4
##### Heading 5
###### Heading 6

Setext heading
==============

Another setext
--------------

## Inline formatting

Plain text, **bold**, *italic*, ***bold italic***, ~~strikethrough~~,
`inline code`, and a mix of **bold with *nested italic* inside**.

Escapes stay literal: \*not italic\*, \_not italic\_, \`not code\`.

Entities: &amp; &lt; &gt; &hellip; &mdash;

A hard line break follows this line
and the text continues on a new line.

## Links and images

An inline [link to example.com](https://example.com "with a title"), an autolink
<https://avaloniaui.net>, an email <hello@example.com>, and a reference
[link to the docs][docs].

Images resolve by key from the bound `Images` collection:

![dog](dog)

An unresolved key falls back to its alt text: ![missing image](no-such-key)

[docs]: https://example.com/docs

## Lists

- First bullet
- Second bullet with **bold** text
  - Nested bullet
    - Third level
- Back to the first level

1. Ordered one
2. Ordered two
   1. Nested ordered
   2. Nested ordered again
3. Ordered three

- [x] Completed task
- [ ] Pending task
- [ ] Another pending task

## Blockquotes

> A single quoted paragraph long enough to wrap onto a second rendered line.
>
> > A nested quote at depth two.

> Quotes can contain **formatting**, `code` and [links](https://example.com).

## Code

An inline `Dispatcher.UIThread.Post(...)` call, then a fenced block:

```csharp
public override void Render(DrawingContext context)
{
    context.FillRectangle(BackgroundBrush, new Rect(Bounds.Size));
    using (context.PushTransform(Matrix.CreateTranslation(0, -_offset)))
        RenderContent(context);
    RenderScrollBar(context);
}
```

Indented code blocks work too:

    var indented = true;
    Console.WriteLine(indented);

## Tables

| Construct   | Supported | Notes                         |
|:------------|:---------:|------------------------------:|
| Headings    | yes       | ATX and setext, levels 1-6    |
| Lists       | yes       | ordered, unordered, tasks     |
| Tables      | yes       | GFM pipes, with alignment     |
| Code        | yes       | fenced and indented           |
| Images      | yes       | resolved from `Images`        |

## Thematic breaks

---

***

___

## Long paragraph

This final paragraph is deliberately long so that greedy line breaking, caret hit
testing and multi-line selection can all be checked against real wrapped text.
Drag across it, then drag past the top or bottom edge of the control to watch the
view auto-scroll while the selection keeps growing.

""";
}

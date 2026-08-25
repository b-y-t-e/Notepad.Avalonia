# Notepad.Avalonia

Two Avalonia UI controls for a single markdown note:

- **`NoteEditor`** — editable source view with inline image pasting.
- **`MarkdownViewer`** — read-only rendered view with full markdown support, free
  text selection and copy.

## Features

### NoteEditor

- Markdown text editing for a single note
- Inline image support (paste from clipboard or programmatic insert)
- Full undo/redo with intelligent coalescing
- Text selection (single and multi-line)
- Clipboard integration (text + images)
- Text wrapping
- Light/Dark theme support
- MVVM-ready via `MarkdownText` property binding
- Optimized for large documents (FormattedText caching, incremental layout, viewport culling)

### MarkdownViewer

Renders markdown instead of showing its source. Read-only, but every rendered
character can be selected and copied.

- Blocks: ATX and setext headings, paragraphs, thematic breaks, fenced and
  indented code blocks, nested blockquotes, nested ordered/unordered/task lists,
  GFM pipe tables (with column alignment)
- Inline: `**bold**`, `*italic*`, `***both***`, `~~strikethrough~~`, `` `code` ``,
  links (inline, reference and autolinks), images, hard line breaks, backslash
  escapes and HTML entities
- Selection: drag, Shift+click, double-click (word), triple-click (line),
  `Ctrl+A`, auto-scroll while dragging past the edge
- Copy: `Ctrl+C` / context menu — copies the rendered text, not the markup
- Links: `LinkClicked` event, opens http/https/mailto in the browser by default
- Built-in scrollbar with viewport virtualization and light/dark themes

## Quick Start

```csharp
var editor = new NoteEditor
{
    DefaultFont = new FontFamily("Segoe UI"),
    DefaultFontSize = 15,
    MarkdownText = "Hello world\nSecond line with ![alt](imageKey)"
};
```

```csharp
var viewer = new MarkdownViewer
{
    DefaultFontSize = 15,
    MarkdownText = "# Title\n\nSome **bold** text and a [link](https://example.com)."
};

viewer.SelectAll();
var text = viewer.SelectedText;   // rendered text, without the markup
```

Give the viewer a finite height (a `DockPanel`/`Grid` cell); like `NoteEditor` it
scrolls its own content, so do **not** wrap it in a `ScrollViewer`.

## MVVM Usage

```xml
<notepad:NoteEditor MarkdownText="{Binding MarkdownText}"
                    Images="{Binding Images}" />

<notepad:MarkdownViewer MarkdownText="{Binding MarkdownText}"
                        Images="{Binding Images}" />
```

```csharp
public class MyViewModel : INotifyPropertyChanged
{
    public string? MarkdownText { get; set; }
    public ObservableCollection<ImageEntry> Images { get; } = new();
}
```

## Installation

```
dotnet add package Notepad.Avalonia
```

## License

MIT

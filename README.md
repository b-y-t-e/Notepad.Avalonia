# Notepad.Avalonia

A single-note markdown editor control for Avalonia UI with inline image pasting support.

## Features

- Markdown text editing for a single note
- Inline image support (paste from clipboard or programmatic insert)
- Full undo/redo with intelligent coalescing
- Text selection (single and multi-line)
- Clipboard integration (text + images)
- Text wrapping
- Light/Dark theme support
- MVVM-ready via `MarkdownText` property binding
- Optimized for large documents (FormattedText caching, incremental layout, viewport culling)

## Quick Start

```csharp
var editor = new NoteEditor
{
    DefaultFont = new FontFamily("Segoe UI"),
    DefaultFontSize = 15,
    MarkdownText = "Hello world\nSecond line with ![alt](imageKey)"
};
```

## MVVM Usage

```xml
<notepad:NoteEditor MarkdownText="{Binding MarkdownText}"
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

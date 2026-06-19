# Notes.Avalonia

A custom rich-text note editor control for Avalonia UI with inline image pasting support.

## Features

- Rich text editing with multiple note lines
- Inline image support (paste from clipboard or programmatic insert)
- Full undo/redo with intelligent coalescing
- Text selection (single and multi-line)
- Clipboard integration (text + images)
- Text wrapping
- Light/Dark theme support
- MVVM-ready via `Items` property binding

## Quick Start

```csharp
var editor = new NoteEditor
{
    DefaultFont = new FontFamily("Segoe UI"),
    DefaultFontSize = 15
};

// Add notes programmatically
editor.AddItem("My first note");
editor.AddItem("Note with image — paste with Ctrl+V");

// Insert image
editor.InsertImageAtCaret(bitmap);
```

## MVVM Usage

```csharp
var items = new ObservableCollection<NoteItemData>
{
    new("First note"),
    new("Second note with ![alt](imageKey)")
};
editor.Items = items;
```

## Installation

```
dotnet add package Notes.Avalonia
```

## License

MIT

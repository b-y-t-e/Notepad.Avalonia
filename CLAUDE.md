# Notepad.Avalonia

Avalonia UI single-note markdown editor control with image pasting for .NET 10.

## Build & Run

```bash
dotnet build Notepad.Avalonia.slnx
dotnet run -p Notepad.Avalonia.Demo
dotnet test Notepad.Avalonia.Tests
```

## Project Structure

- **Notepad.Avalonia/** — core library: `NoteEditor` and `MarkdownViewer` controls, `DocumentModel`, `MarkdownParser`, `ImageEntry`
- **Notepad.Avalonia.Demo/** — demo app with MVVM binding (`MvvmWindow`, `DemoViewModel`)
- **Notepad.Avalonia.Tests/** — NUnit tests (headless Avalonia)

## Public API

### NoteEditor

- `MarkdownText` (string, TwoWay) — the primary text property, holds the full note as markdown
- `Images` (IEnumerable<ImageEntry>) — bindable image collection for MVVM
- `IsDirty` / `DirtyChanged` / `MarkClean()` — dirty tracking
- `ImagePasted` event — fires when user pastes an image
- `ContentChanged` / `ContentDetailChanged` events — content change notifications

### MarkdownViewer

Read-only renderer for the same markdown. Full block/inline support (headings,
lists, tasks, quotes, code, tables, links, images), free text selection
(drag, Shift+click, double/triple click, `Ctrl+A`) and copy (`Ctrl+C`).

- `MarkdownText` / `Images` — same binding contract as `NoteEditor`
- `PlainText` / `SelectedText` / `SelectAll()` / `CopySelection()` — selection offsets index into `PlainText`
- `LinkClicked` / `SelectionChanged` events
- `MarkdownParser.Parse(...)` is public if you need the block model without the control

## Layout

- `NoteEditor` and `MarkdownViewer` have a **built-in vertical scrollbar** and virtualize their content. Give them a finite height (e.g. a `DockPanel`/`Grid` cell).
- **Do not wrap them in an external `ScrollViewer`.** That gives the control an unbounded height, which disables both the built-in scrollbar and viewport virtualization (all items render every frame). This usage is not supported.

## Conventions

- All code, comments, commit messages, branch names, PR descriptions, and UI strings must be in **English**, regardless of the language the user communicates in.
- Commit messages follow conventional commits format: `type(scope): description`

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
- `ClearSelectionOnLostFocus` (default true, as in Avalonia's `TextBox`) / `InactiveSelectionBrush`; the caret is drawn only while focused

### MarkdownViewer

Read-only renderer for the same markdown. Full block/inline support (headings,
lists, tasks, quotes, code, tables, links, images), free text selection
(drag, Shift+click, double/triple click, `Ctrl+A`) and copy (`Ctrl+C`).

- `MarkdownText` / `Images` — same binding contract as `NoteEditor`
- `PlainText` / `SelectedText` / `SelectAll()` / `CopySelection()` — selection offsets index into `PlainText`
- `LinkClicked` / `SelectionChanged` events
- `ClearSelectionOnLostFocus` (default true, as in Avalonia's `TextBox`) / `InactiveSelectionBrush`
- `MarkdownParser.Parse(...)` is public if you need the block model without the control

## Layout

- `NoteEditor` and `MarkdownViewer` have a **built-in vertical scrollbar** and virtualize their content. Give them a finite height (e.g. a `DockPanel`/`Grid` cell).
- Both suppress the focus-driven bring-into-view for **pointer** focus, so clicking to select inside an outer scroll area does not make it jump; `Tab` navigation still scrolls them into view. The framework focuses on pointer press *before* `PointerPressed` reaches the control (`GotFocus` -> `RequestBringIntoView` -> `PointerPressed`), so the suppression hangs off `OnGotFocus`, not off the `Focus()` call.
- Wrapping them in an external `ScrollViewer` gives them an unbounded height: the built-in scrollbar and viewport virtualization are disabled and every block renders each frame. That is the supported way to stack several notes in one scroll area, but a single long document should get a finite height instead.

## Conventions

- All code, comments, commit messages, branch names, PR descriptions, and UI strings must be in **English**, regardless of the language the user communicates in.
- Commit messages follow conventional commits format: `type(scope): description`

# Notepad.Avalonia

Avalonia UI single-note markdown editor control with image pasting for .NET 10.

## Build & Run

```bash
dotnet build Notepad.Avalonia.slnx
dotnet run -p Notepad.Avalonia.Demo
dotnet test Notepad.Avalonia.Tests
```

## Project Structure

- **Notepad.Avalonia/** — core library: `NoteEditor` control, `DocumentModel`, `ImageEntry`
- **Notepad.Avalonia.Demo/** — demo app with MVVM binding (`MvvmWindow`, `DemoViewModel`)
- **Notepad.Avalonia.Tests/** — NUnit tests (headless Avalonia)

## Public API

- `MarkdownText` (string, TwoWay) — the primary text property, holds the full note as markdown
- `Images` (IEnumerable<ImageEntry>) — bindable image collection for MVVM
- `IsDirty` / `DirtyChanged` / `MarkClean()` — dirty tracking
- `ImagePasted` event — fires when user pastes an image
- `ContentChanged` / `ContentDetailChanged` events — content change notifications

## Layout

- `NoteEditor` has a **built-in vertical scrollbar** and virtualizes its content. Give it a finite height (e.g. a `DockPanel`/`Grid` cell).
- **Do not wrap it in an external `ScrollViewer`.** That gives the control an unbounded height, which disables both the built-in scrollbar and viewport virtualization (all items render every frame). This usage is not supported.

## Conventions

- All code, comments, commit messages, branch names, PR descriptions, and UI strings must be in **English**, regardless of the language the user communicates in.
- Commit messages follow conventional commits format: `type(scope): description`

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

## Conventions

- All code, comments, commit messages, branch names, PR descriptions, and UI strings must be in **English**, regardless of the language the user communicates in.
- Commit messages follow conventional commits format: `type(scope): description`

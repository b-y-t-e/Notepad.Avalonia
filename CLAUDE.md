# Notes.Avalonia

Avalonia UI custom note editor control with image pasting for .NET 10.

## Build & Run

```bash
dotnet build Notes.Avalonia.slnx
dotnet run -p Notes.Avalonia.Demo
dotnet test Notes.Avalonia.Tests
```

## Project Structure

- **Notes.Avalonia/** — core library: `NoteEditor` control, `DocumentModel`, `NoteItemData`
- **Notes.Avalonia.Demo/** — demo app with direct API usage (`MainWindow`) and MVVM binding (`MvvmWindow`)
- **Notes.Avalonia.Tests/** — NUnit tests (headless Avalonia)

## Conventions

- All code, comments, commit messages, branch names, PR descriptions, and UI strings must be in **English**, regardless of the language the user communicates in.
- Commit messages follow conventional commits format: `type(scope): description`

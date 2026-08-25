using global::Avalonia.Controls;

namespace Notepad.Avalonia.Demo;

public partial class MvvmWindow : Window
{
    public MvvmWindow()
    {
        InitializeComponent();

        this.FindControl<Button>("ShowcaseButton")!.Click += (_, _) =>
            (DataContext as DemoViewModel)?.PreviewMarkdownShowcase();

        this.FindControl<Button>("SampleNoteButton")!.Click += (_, _) =>
            (DataContext as DemoViewModel)?.PreviewSampleNote();

        this.FindControl<Button>("EditorTextButton")!.Click += (_, _) =>
            (DataContext as DemoViewModel)?.PreviewEditorText();
    }
}

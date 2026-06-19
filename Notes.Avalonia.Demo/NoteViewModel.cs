using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Notes.Avalonia.Model;

namespace Notes.Avalonia.Demo;

public class NoteViewModel : INotifyPropertyChanged
{
    public ObservableCollection<NoteItemData> Items { get; } = new();

    public ICommand AddItemCommand { get; }
    public ICommand ClearAllCommand { get; }

    private string _statusText = "MVVM Demo — edit notes, changes sync with ViewModel.";
    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText != value)
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
    }

    private int _itemCount;
    public int ItemCount
    {
        get => _itemCount;
        private set
        {
            if (_itemCount != value)
            {
                _itemCount = value;
                OnPropertyChanged();
            }
        }
    }

    public NoteViewModel()
    {
        AddItemCommand = new RelayCommand(() =>
        {
            Items.Add(new NoteItemData($"New note #{Items.Count + 1}"));
            UpdateCounts();
        });

        ClearAllCommand = new RelayCommand(() =>
        {
            Items.Clear();
            UpdateCounts();
        });

        Items.Add(new NoteItemData("Meeting notes for Monday"));
        Items.Add(new NoteItemData("Design ideas for the dashboard"));
        Items.Add(new NoteItemData("Code review comments"));
        Items.Add(new NoteItemData("MVVM binding example"));
        Items.Add(new NoteItemData("Images in text: ![star](star) great ![photo](photo) done"));
        Items.Add(new NoteItemData("Paste an image here (Ctrl+V)"));

        Items.CollectionChanged += OnCollectionChanged;
        foreach (var item in Items)
            item.PropertyChanged += OnItemPropertyChanged;

        UpdateCounts();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateCounts();

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (NoteItemData item in e.OldItems)
                item.PropertyChanged -= OnItemPropertyChanged;
        if (e.NewItems != null)
            foreach (NoteItemData item in e.NewItems)
                item.PropertyChanged += OnItemPropertyChanged;
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        ItemCount = Items.Count;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
}

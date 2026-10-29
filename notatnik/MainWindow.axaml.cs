using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace notatnik;

public partial class MainWindow : Window
{
    private readonly string _filePath;
    private readonly List<Note> _allNotes = new();
    private readonly ObservableCollection<Note> _displayedNotes = new();
    private Note? _selectedNote;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public MainWindow()
    {
        InitializeComponent();

        _filePath = ResolveStoragePath();

        NotesListBox.ItemsSource = _displayedNotes;

        StorageInfoTextBlock.Text = $"Plik: {Path.GetFileName(_filePath)}";
        ToolTip.SetTip(StorageInfoTextBlock, $"Pełna ścieżka: {_filePath}");

        LoadNotesFromFile();
    }

    private static string ResolveStoragePath()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var projectFile = Path.Combine(currentDir, "notatnik.csproj");
        if (File.Exists(projectFile) || File.Exists(Path.Combine(currentDir, "notatki.json")))
        {
            return Path.Combine(currentDir, "notatki.json");
        }

        return Path.Combine(AppContext.BaseDirectory, "notatki.json");
    }

    /// <summary>
    /// Wczytuje notatki z pliku JSON.
    /// </summary>
    private void LoadNotesFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var loadedNotes = JsonSerializer.Deserialize<List<Note>>(json, JsonOptions);

                _allNotes.Clear();
                if (loadedNotes != null)
                {
                    _allNotes.AddRange(loadedNotes.OrderByDescending(n => n.ModifiedAt ?? n.CreatedAt));
                }
            }
        }
        catch (Exception ex)
        {
            SetStatus($"❌ Błąd podczas wczytywania notatek: {ex.Message}", Brushes.Red);
        }

        ApplyFilter();
    }

    /// <summary>
    /// Zapisuje aktualną listę notatek do pliku JSON.
    /// </summary>
    private bool SaveNotesToFile()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(_allNotes, JsonOptions);
            File.WriteAllText(_filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            SetStatus($"❌ Błąd zapisu do pliku: {ex.Message}", Brushes.Red);
            return false;
        }
    }

    /// <summary>
    /// Obsługa przycisku zapisu notatki.
    /// </summary>
    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text?.Trim() ?? string.Empty;
        var content = ContentTextBox.Text?.Trim() ?? string.Empty;

        // Walidacja - tytuł jest wymagany
        if (string.IsNullOrWhiteSpace(title))
        {
            SetStatus("⚠️ Wpisz tytuł notatki przed zapisaniem!", Brushes.OrangeRed);
            TitleTextBox.Focus();
            return;
        }

        if (_selectedNote != null)
        {
            // Aktualizacja istniejącej notatki
            _selectedNote.Title = title;
            _selectedNote.Content = content;
            _selectedNote.ModifiedAt = DateTime.Now;

            // Sortowanie notatek wg najnowszej edycji
            _allNotes.Remove(_selectedNote);
            _allNotes.Insert(0, _selectedNote);

            if (SaveNotesToFile())
            {
                SetStatus("✅ Zaktualizowano notatkę i zapisano do pliku!", Brushes.ForestGreen);
            }
        }
        else
        {
            // Utworzenie nowej notatki
            var newNote = new Note
            {
                Id = Guid.NewGuid(),
                Title = title,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _allNotes.Insert(0, newNote);
            _selectedNote = newNote;

            if (SaveNotesToFile())
            {
                SetStatus("✅ Notatka zapisana do pliku!", Brushes.ForestGreen);
            }

            EditorHeaderTextBlock.Text = "Edycja notatki";
            DeleteButton.IsVisible = true;
            ClearButton.Content = "Nowa notatka";
        }

        ApplyFilter();
        NotesListBox.SelectedItem = _selectedNote;
    }

    /// <summary>
    /// Obsługa przycisku nowej notatki / wyczyszczenia formularza.
    /// </summary>
    private void OnNewNoteClick(object? sender, RoutedEventArgs e)
    {
        ClearForm();
        TitleTextBox.Focus();
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    /// <summary>
    /// Czyści formularz i przełącza w tryb tworzenia nowej notatki.
    /// </summary>
    private void ClearForm()
    {
        _selectedNote = null;
        NotesListBox.SelectedItem = null;

        TitleTextBox.Text = string.Empty;
        ContentTextBox.Text = string.Empty;
        EditorHeaderTextBlock.Text = "Nowa notatka";
        DeleteButton.IsVisible = false;
        ClearButton.Content = "Wyczyść formularz";
        SetStatus("Formularz wyczyszczony. Gotowy na nową notatkę.", Brushes.Gray);
    }

    /// <summary>
    /// Obsługa usuwania zaznaczonej notatki.
    /// </summary>
    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedNote == null) return;

        var noteToRemove = _selectedNote;
        _allNotes.Remove(noteToRemove);

        if (SaveNotesToFile())
        {
            SetStatus($"🗑️ Usunięto notatkę \"{noteToRemove.Title}\".", Brushes.DodgerBlue);
        }

        ClearForm();
        ApplyFilter();
    }

    /// <summary>
    /// Obsługa zmiany wyboru notatki na liście.
    /// </summary>
    private void OnNotesListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NotesListBox.SelectedItem is Note note)
        {
            _selectedNote = note;
            TitleTextBox.Text = note.Title;
            ContentTextBox.Text = note.Content;

            EditorHeaderTextBlock.Text = "Edycja notatki";
            DeleteButton.IsVisible = true;
            ClearButton.Content = "Nowa notatka";
            SetStatus($"Wczytano notatkę: {note.Title}", Brushes.Gray);
        }
    }

    /// <summary>
    /// Filtrowanie listy notatek po wpisaniu tekstu w wyszukiwarkę.
    /// </summary>
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filter = SearchTextBox.Text?.Trim().ToLowerInvariant() ?? string.Empty;

        var matchingNotes = string.IsNullOrEmpty(filter)
            ? _allNotes
            : _allNotes.Where(n =>
                n.Title.ToLowerInvariant().Contains(filter) ||
                n.Content.ToLowerInvariant().Contains(filter)).ToList();

        _displayedNotes.Clear();
        foreach (var note in matchingNotes)
        {
            _displayedNotes.Add(note);
        }

        NotesCountTextBlock.Text = _allNotes.Count.ToString();
        EmptyNotesPlaceholder.IsVisible = _displayedNotes.Count == 0;
    }

    /// <summary>
    /// Ustawia komunikat o statusie.
    /// </summary>
    private void SetStatus(string message, IBrush brush)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = brush;
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace notatnik;

public partial class MainWindow : Window
{
    private readonly string _filePath;
    private readonly List<Note> _allNotes = new();
    private readonly ObservableCollection<Note> _displayedNotes = new();
    private Note? _selectedNote;

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    // Wzorzec nagłówka wpisu w Markdown: ## RRRR-MM-DD GG:MM — Tytuł notatki
    private static readonly Regex HeaderRegex = new(
        @"^##\s+(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2})\s+[—–-]\s*(.*)$",
        RegexOptions.Compiled);

    public MainWindow()
    {
        InitializeComponent();

        _filePath = ResolveStoragePath();

        NotesListBox.ItemsSource = _displayedNotes;

        StorageInfoTextBlock.Text = $"Plik: {Path.GetFileName(_filePath)}";
        ToolTip.SetTip(StorageInfoTextBlock, $"Pełna ścieżka: {_filePath}");

        LoadNotesFromFile();
    }

    /// <summary>
    /// Ustala ścieżkę do pliku notatki.md w folderze projektu.
    /// </summary>
    private static string ResolveStoragePath()
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Jeśli uruchomiono bezpośrednio w folderze projektu notatnik
        var projectFile = Path.Combine(currentDir, "notatnik.csproj");
        if (File.Exists(projectFile) || File.Exists(Path.Combine(currentDir, "notatki.md")))
        {
            return Path.Combine(currentDir, "notatki.md");
        }

        // Jeśli uruchomiono z katalogu nadrzędnego repozytorium
        var subDirProject = Path.Combine(currentDir, "notatnik", "notatnik.csproj");
        if (File.Exists(subDirProject))
        {
            return Path.Combine(currentDir, "notatnik", "notatki.md");
        }

        return Path.Combine(AppContext.BaseDirectory, "notatki.md");
    }

    /// <summary>
    /// Wczytuje notatki z pliku Markdown.
    /// </summary>
    private void LoadNotesFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var markdown = File.ReadAllText(_filePath, Encoding.UTF8);
                var loadedNotes = ParseMarkdown(markdown);

                _allNotes.Clear();
                _allNotes.AddRange(loadedNotes.OrderByDescending(n => n.CreatedAt));
            }
        }
        catch (Exception ex)
        {
            SetStatus($"❌ Błąd podczas wczytywania notatek: {ex.Message}", Brushes.Red);
        }

        ApplyFilter();
    }

    /// <summary>
    /// Parsuje zawartość pliku Markdown do obiektów Note.
    /// </summary>
    private static List<Note> ParseMarkdown(string markdown)
    {
        var notes = new List<Note>();
        if (string.IsNullOrWhiteSpace(markdown)) return notes;

        var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        Note? currentNote = null;
        var contentLines = new List<string>();

        void FlushCurrentNote()
        {
            if (currentNote != null)
            {
                while (contentLines.Count > 0 && string.IsNullOrWhiteSpace(contentLines[0]))
                    contentLines.RemoveAt(0);
                while (contentLines.Count > 0 && string.IsNullOrWhiteSpace(contentLines[^1]))
                    contentLines.RemoveAt(contentLines.Count - 1);

                currentNote.Content = string.Join(Environment.NewLine, contentLines);
                notes.Add(currentNote);
                currentNote = null;
                contentLines.Clear();
            }
        }

        foreach (var line in lines)
        {
            var match = HeaderRegex.Match(line);
            if (match.Success)
            {
                FlushCurrentNote();

                var dateStr = match.Groups[1].Value.Trim();
                var titleStr = match.Groups[2].Value.Trim();

                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    parsedDate = DateTime.Now;
                }

                currentNote = new Note
                {
                    Id = Guid.NewGuid(),
                    Title = titleStr,
                    CreatedAt = parsedDate
                };
            }
            else if (currentNote != null)
            {
                contentLines.Add(line);
            }
        }

        FlushCurrentNote();
        return notes;
    }

    /// <summary>
    /// Zapisuje aktualną listę notatek do pliku notatki.md w formacie Markdown.
    /// Najnowsza notatka znajduje się na górze pliku.
    /// Między wpisami znajduje się jedna pusta linia.
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

            var sb = new StringBuilder();
            var sortedNotes = _allNotes.OrderByDescending(n => n.CreatedAt).ToList();

            for (int i = 0; i < sortedNotes.Count; i++)
            {
                var note = sortedNotes[i];
                sb.AppendLine($"## {note.CreatedAt:yyyy-MM-dd HH:mm} — {note.Title}");

                if (!string.IsNullOrWhiteSpace(note.Content))
                {
                    sb.AppendLine(note.Content.TrimEnd());
                }

                // Pusta linia między wpisami
                if (i < sortedNotes.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            File.WriteAllText(_filePath, sb.ToString(), Utf8NoBom);
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
            _selectedNote.CreatedAt = DateTime.Now;

            // Sortowanie: najnowsza na górze
            _allNotes.Remove(_selectedNote);
            _allNotes.Insert(0, _selectedNote);

            if (SaveNotesToFile())
            {
                SetStatus("✅ Zaktualizowano notatkę i zapisano do pliku notatki.md!", Brushes.ForestGreen);
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
                SetStatus("✅ Notatka zapisana do pliku notatki.md!", Brushes.ForestGreen);
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